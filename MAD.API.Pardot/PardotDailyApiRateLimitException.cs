using MAD.API.Pardot.Api;
using System;
using System.Runtime.Serialization;

namespace MAD.API.Pardot
{
    [Serializable]
    public class PardotApiDailyRateLimitException : PardotApiException
    {
        public PardotApiDailyRateLimitException(ApiResponse apiResponse) : base(apiResponse)
        {
        }
    }
}