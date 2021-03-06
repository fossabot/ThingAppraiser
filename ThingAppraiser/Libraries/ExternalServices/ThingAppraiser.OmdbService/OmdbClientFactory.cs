﻿using Acolyte.Assertions;

namespace ThingAppraiser.OmdbService
{
    public static class OmdbClientFactory
    {
        public static IOmdbClient CreateClient(string apiKey)
        {
            apiKey.ThrowIfNullOrWhiteSpace(nameof(apiKey));

            return new OmdbClient(apiKey);
        }
    }
}
