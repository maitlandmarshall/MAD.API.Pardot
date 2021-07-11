using MAD.API.Pardot.Api;
using System;
using System.Runtime.Serialization;

namespace MAD.API.Pardot
{
    [Serializable]
    public class PardotApiException : Exception
    {
        public ApiResponse ApiResponse { get; }

        public PardotApiException(ApiResponse apiResponse) : base (apiResponse.Error)
        {
            this.ApiResponse = apiResponse ?? throw new ArgumentNullException(nameof(apiResponse));
        }
    }
}