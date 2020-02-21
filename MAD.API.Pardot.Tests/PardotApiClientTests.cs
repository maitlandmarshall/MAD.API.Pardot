using MAD.API.Pardot.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MAD.API.Pardot.Tests
{
    [TestClass]
    public class PardotApiClientTests
    {
        private static PardotApiClient GetClient()
        {
            string keys = File.ReadAllText("PardotApiKeys.txt");
            Dictionary<string, object> keysObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(keys);

            string email = keysObj[nameof(email)] as string;
            string password = keysObj[nameof(password)] as string;
            string userKey = keysObj[nameof(userKey)] as string;

            return new PardotApiClient(email, password, userKey);
        }

        [TestMethod]
        public async Task LoginAndGetApiKeyTest()
        {
            PardotApiClient client = GetClient();
            await client.LoginAndGetApiKey();
        }

        [TestMethod]
        public async Task GetAccountTest()
        {
            PardotApiClient client = GetClient();

            Account account = await client.GetAccount();

            Assert.IsNotNull(account);
        }


        #region BULK QUERY TESTS WITHOUT DATE PARAMS

        [TestMethod]
        public async Task GetCampaignsTest()
        {
            IEnumerable<Campaign> campaigns = await this.BulkQueryTest<Campaign>();

            Assert.IsNotNull(campaigns);
        }

        [TestMethod]
        public async Task GetCustomRedirectsTest()
        {
            IEnumerable<CustomRedirect> customRedirects = await this.BulkQueryTest<CustomRedirect>();

            Assert.IsNotNull(customRedirects);
        }

        [TestMethod]
        public async Task GetEmailClickTest()
        {
            IEnumerable<EmailClick> emailClicks = await this.BulkQueryTest<EmailClick>();

            Assert.IsNotNull(emailClicks);
        }

        [TestMethod]
        public async Task GetOpportunitiesTest()
        {
            IEnumerable<Opportunity> opportunitites = await this.BulkQueryTest<Opportunity>();

            Assert.IsNotNull(opportunitites);
        }

        [TestMethod]
        public async Task GetProspectsTest()
        {
            IEnumerable<Prospect> prospects = await this.BulkQueryTest<Prospect>();

            Assert.IsNotNull(prospects);
        }

        [TestMethod]
        public async Task GetProspectAccountsTest()
        {
            IEnumerable<ProspectAccount> prospectAccounts = await this.BulkQueryTest<ProspectAccount>();

            Assert.IsNotNull(prospectAccounts);
        }

        [TestMethod]
        public async Task GetUsersTest()
        {
            IEnumerable<User> users = await this.BulkQueryTest<User>();

            Assert.IsNotNull(users);
        }

        [TestMethod]
        public async Task GetVisitorsTest()
        {
            IEnumerable<Visitor> visitors = await this.BulkQueryTest<Visitor>();

            Assert.IsNotNull(visitors);
        }

        #endregion

        #region BULK QUERY TESTS WITH DATE PARAMS

        [TestMethod]
        public async Task GetCampaignsWithDateTest()
        {
            IEnumerable<Campaign> campaigns = await this.BulkQueryTest<Campaign>(true);

            Assert.IsNotNull(campaigns);
        }

        [TestMethod]
        public async Task GetCustomRedirectsWithDateTest()
        {
            IEnumerable<CustomRedirect> customRedirects = await this.BulkQueryTest<CustomRedirect>(true);

            Assert.IsNotNull(customRedirects);
        }

        [TestMethod]
        public async Task GetEmailClickWithDateTest()
        {
            IEnumerable<EmailClick> emailClicks = await this.BulkQueryTest<EmailClick>(true);

            Assert.IsNotNull(emailClicks);
        }

        [TestMethod]
        public async Task GetOpportunitiesWithDateTest()
        {
            IEnumerable<Opportunity> opportunitites = await this.BulkQueryTest<Opportunity>(true);

            Assert.IsNotNull(opportunitites);
        }

        [TestMethod]
        public async Task GetProspectsWithDateTest()
        {
            IEnumerable<Prospect> prospects = await this.BulkQueryTest<Prospect>(true);

            Assert.IsNotNull(prospects);
        }

        [TestMethod]
        public async Task GetProspectAccountsWithDateTest()
        {
            IEnumerable<ProspectAccount> prospectAccounts = await this.BulkQueryTest<ProspectAccount>(true);

            Assert.IsNotNull(prospectAccounts);
        }

        [TestMethod]
        public async Task GetUsersWithDateTest()
        {
            IEnumerable<User> users = await this.BulkQueryTest<User>(true);

            Assert.IsNotNull(users);
        }

        [TestMethod]
        public async Task GetVisitorsWithDateTest()
        {
            IEnumerable<Visitor> visitors = await this.BulkQueryTest<Visitor>(true);

            Assert.IsNotNull(visitors);
        }

        [TestMethod]
        public void TestFailResponse()
        {
            string failResponseJson = "{\"@attributes\":{\"stat\":\"fail\",\"version\":1,\"err_code\":1},\"err\":\"Invalid API key or user key\"}";
            string successResponseJson = "{\"@attributes\":{\"stat\":\"ok\",\"version\":1}}";

            JObject failResponse = JsonConvert.DeserializeObject<JObject>(failResponseJson);
            JObject successResponse = JsonConvert.DeserializeObject<JObject>(successResponseJson);

            this.ParseResponse(failResponse);
            this.ParseResponse(successResponse);
        }

        private void ParseResponse(JObject successOrFailResponse)
        {
            JToken errCode = successOrFailResponse["@attributes"]["err_code"];

            if (errCode == null)
                return;

            int errorCode = errCode.Value<int>();

            Assert.IsTrue(errorCode > 0);
        }

        #endregion


        private async Task<IEnumerable<ResponseType>> BulkQueryTest<ResponseType>(bool isDateTest = false) where ResponseType : IEntity
        {
            PardotApiClient client = GetClient();

            if (isDateTest)
            {
                return await client.PerformBulkQuery<ResponseType>(new Api.BulkQueryParameters
                {
                    CreatedAfter = DateTime.Now.AddDays(-1),
                    UpdatedAfter = DateTime.Now.AddDays(-1)
                });
            }
            else
            {
                IEnumerable<ResponseType> result = await client.PerformBulkQuery<ResponseType>(new Api.BulkQueryParameters { Take = 600 });
                Assert.IsTrue(result.Count() <= 600);

                return result;
            }


        }
    }
}
