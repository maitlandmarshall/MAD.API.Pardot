﻿using MAD.API.Pardot.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MAD.API.Pardot.Api
{
    public class EmailResponse : ApiResponse
    {
        public Email Email { get; set; }
    }
}
