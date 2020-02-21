using MAD.API.Pardot.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace MAD.API.Pardot.Api
{
    public class AccountResponse : ApiResponse
    {
        public Account Account { get; set; }
    }
}
