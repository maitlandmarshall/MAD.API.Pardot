using MAD.API.Pardot.Api;
using MAD.API.Pardot.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("MAD.API.Pardot.Tests")]
namespace MAD.API.Pardot
{
    public sealed class PardotApiClient : IDisposable
    {
        private const int MaxQueryResults = 200;
        private const string ApiBaseUri = "https://pi.pardot.com/api/";

        private static BlockingCollection<object> ConcurrentRequests = new BlockingCollection<object>(5);

        public string ApiKey { get; set; }
        public string Email { get; }
        public string Password { get; }
        public string UserKey { get; }

        private readonly HttpClient httpClient;

        public PardotApiClient(string email, string password, string userKey)
        {
            this.Email = email ?? throw new ArgumentNullException(nameof(email));
            this.Password = password ?? throw new ArgumentNullException(nameof(password));
            this.UserKey = userKey ?? throw new ArgumentNullException(nameof(userKey));

            this.httpClient = new HttpClient();
            this.httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            this.httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
            this.httpClient.Timeout = TimeSpan.FromMinutes(10);
            this.httpClient.BaseAddress = new Uri(ApiBaseUri);
        }

        private string BuildRequestBody((string argName, object argValue)[] args)
        {
            string[] argumentsFormatted = args
                    .Where(y => y.argValue != null)
                    .Select(y =>
                    {
                        object argValue = y.argValue;
                        string argName = y.argName;

                        string argFinalValue;

                        switch (argValue)
                        {
                            case DateTime argValueDateTime:
                                argFinalValue = argValueDateTime.ToString("yyyy-MM-ddTHH:mm:ss");

                                break;
                            default:
                                argFinalValue = argValue.ToString();

                                break;
                        }

                        return $"{argName}={argFinalValue}";
                    }).ToArray();

            string argSegment = String.Join("&", argumentsFormatted);

            return $"{argSegment}&format=json";
        }

        internal HttpRequestMessage CreateRequestMessage(string requestUri)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            if (!String.IsNullOrEmpty(this.ApiKey))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue($"Pardot", $"api_key={this.ApiKey}, user_key={this.UserKey}");

            return request;
        }

        internal async Task<string> GetWebRequestResponseAsString(HttpRequestMessage request, (string argName, object argValue)[] args)
        {
            const int maxRequestAttempts = 3;
            int requestAttempts = 1;

            while (true)
            {
                ConcurrentRequests.Add(new object());

                request.Content = new StringContent(this.BuildRequestBody(args), Encoding.UTF8, "application/x-www-form-urlencoded");

                using (HttpResponseMessage response = await this.httpClient.SendAsync(request))
                {
                    try
                    {
                        response.EnsureSuccessStatusCode();

                        string responseContent = await response.Content.ReadAsStringAsync();
                        return responseContent;
                    }
                    catch
                    {
                        if (requestAttempts >= maxRequestAttempts)
                            throw;

                        await Task.Delay(5000);
                        request = this.CreateRequestMessage(request.RequestUri.AbsoluteUri);
                    }
                    finally
                    {
                        requestAttempts++;
                        ConcurrentRequests.Take();
                    }
                }
            }
        }

        internal async Task<ResponseType> ExecuteApiRequest<ResponseType>(string relativeUri, params (string argName, object argValue)[] args)
        {
            if (String.IsNullOrEmpty(this.ApiKey) && relativeUri.Contains("login") == false)
                await this.LoginAndGetApiKey();

            using HttpRequestMessage request = this.CreateRequestMessage(relativeUri);
            string responseContent = await this.GetWebRequestResponseAsString(request, args);

            try
            {
                ResponseType result = JsonConvert.DeserializeObject<ResponseType>(responseContent);

                if (result is ApiResponse queryResponse && queryResponse.Attributes.ErrorCode.HasValue)
                {
                    if (queryResponse.Attributes.ErrorCode == 1)
                    {
                        this.ApiKey = null;

                        return await this.ExecuteApiRequest<ResponseType>(relativeUri, args);
                    }
                    else
                    {
                        throw new Exception(queryResponse.Error);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(responseContent, ex);
            }

        }

        internal async Task<LoginResponse> LoginAndGetApiKey()
        {
            LoginResponse response = await this.ExecuteApiRequest<LoginResponse>(
                relativeUri: $"login/version/4",
                ("email", this.Email),
                ("password", this.Password),
                ("user_key", this.UserKey)
            );

            this.ApiKey = response.ApiKey;

            return response;
        }

        public async Task<Account> GetAccount()
        {
            AccountResponse response = await this.ExecuteApiRequest<AccountResponse>("account/version/4/do/read");

            return response.Account;
        }

        public async Task<Email> GetEmail(int emailId)
        {
            if (emailId == 0)
                throw new Exception($"{nameof(emailId)} must be greater than 0");

            EmailResponse response = await this.ExecuteApiRequest<EmailResponse>($"email/version/4/do/read/id/{emailId}");

            return response.Email;
        }

        public async Task<IEnumerable<Visit>> GetVisits(params int[] visitorIds)
        {
            List<Visit> totalVisits = new List<Visit>();
            QueryResponse<Visit> paginatedResponse;

            do
            {
                paginatedResponse = await this.ExecuteApiRequest<QueryResponse<Visit>>("visit/version/4/do/query",
                    ("visitor_ids", String.Join(",", visitorIds)),
                    ("offset", totalVisits.Count));

                if (paginatedResponse.Result?.Items == null)
                    break;

                totalVisits.AddRange(paginatedResponse.Result.Items);

                if (paginatedResponse.Result.Items.Count < MaxQueryResults)
                    break;

            } while (paginatedResponse.Result.TotalResults.Value > totalVisits.Count);

            return totalVisits;
        }

        public Task<IEnumerable<TargetType>> PerformBulkQuery<TargetType>(BulkQueryParameters parameters = null) where TargetType : IEntity
        {
            return this.PerformBulkQuery<TargetType>(typeof(TargetType).Name, parameters);
        }

        public async Task<IEnumerable<TargetType>> PerformBulkQuery<TargetType>(string pardotApiEndpointName, BulkQueryParameters parameters = null) where TargetType : IEntity
        {
            List<TargetType> totalResult = new List<TargetType>();
            int lastPaginatedResultCount = 0;

            DateTime? createdBefore = parameters?.CreatedBefore;
            DateTime? createdAfter = parameters?.CreatedAfter;
            DateTime? updatedBefore = parameters?.UpdatedBefore;
            DateTime? updatedAfter = parameters?.UpdatedAfter;
            int? idGreaterThan = parameters?.IdGreaterThan;
            int? idLessThan = parameters?.IdLessThan;
            int? take = parameters?.Take;
            string sortBy = parameters?.SortBy ?? "id";
            SortOrder sortOrder = parameters?.SortOrder ?? SortOrder.Ascending;

            bool isImmutableEntity = typeof(IImmutableEntity).IsAssignableFrom(typeof(TargetType));
            bool isMutableEntity = typeof(IMutableEntity).IsAssignableFrom(typeof(TargetType));

            do
            {
                QueryResponse<TargetType> queryResponse = await this.ExecuteApiRequest<QueryResponse<TargetType>>($"{pardotApiEndpointName}/version/4/do/query",
                    ("output", "bulk"),
                    ("created_before", createdBefore),
                    ("created_after", createdAfter),
                    ("updated_before", updatedBefore),
                    ("updated_after", updatedAfter),
                    ("id_greater_than", idGreaterThan),
                    ("id_less_than", idLessThan),
                    ("sort_by", sortBy),
                    ("sort_order", sortOrder.ToString().ToLower())
                    );

                List<TargetType> items = queryResponse.Result?.Items;

                if (items is null)
                    continue;

                lastPaginatedResultCount = items.Count;

                if (lastPaginatedResultCount == 0)
                    break;

                totalResult.AddRange(items);

                if (!createdBefore.HasValue
                    && !createdAfter.HasValue
                    && !updatedBefore.HasValue
                    && !updatedAfter.HasValue
                    && !idGreaterThan.HasValue
                    && !idLessThan.HasValue)
                {
                    idGreaterThan = items.Max(y => y.Id);
                }
                else
                {
                    if (createdBefore.HasValue && isImmutableEntity)
                    {
                        createdBefore = items.Cast<IImmutableEntity>().Min(y => y.CreatedAt);
                    }

                    if (updatedBefore.HasValue && isMutableEntity)
                    {
                        updatedBefore = items.Cast<IMutableEntity>().Min(y => y.UpdatedAt);
                    }

                    if (createdAfter.HasValue && isImmutableEntity)
                    {
                        createdAfter = items.Cast<IImmutableEntity>().Max(y => y.CreatedAt);
                    }

                    if (updatedAfter.HasValue && isMutableEntity)
                    {
                        updatedAfter = items.Cast<IMutableEntity>().Max(y => y.CreatedAt);
                    }

                    if (idGreaterThan.HasValue)
                        idGreaterThan = items.Max(y => y.Id);

                    if (idLessThan.HasValue)
                        idLessThan = items.Min(y => y.Id);
                }

            } while (lastPaginatedResultCount == MaxQueryResults && (!take.HasValue || totalResult.Count < take.Value));

            return totalResult;
        }

        public void Dispose()
        {
            this.httpClient?.Dispose();
        }
    }
}
