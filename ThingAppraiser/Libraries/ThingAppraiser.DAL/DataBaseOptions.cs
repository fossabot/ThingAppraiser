﻿using Acolyte.Assertions;
using ThingAppraiser.Configuration;

namespace ThingAppraiser.DAL
{
    public sealed class DataBaseOptions : IOptions
    {
        public string ConnectionString { get; set; }


        public DataBaseOptions()
        {
            ConnectionString = string.Empty;
        }

        public DataBaseOptions(string dbConnectionString)
        {
            ConnectionString = dbConnectionString.ThrowIfNullOrEmpty(nameof(dbConnectionString));
        }
    }
}
