﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MAD.API.Pardot.Domain
{
    public class Campaign : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Cost { get; set; }
    }
}
