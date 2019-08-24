﻿using System;
using System.Collections.Generic;
using System.Linq;
using ThingAppraiser.Communication;
using ThingAppraiser.Logging;
using ThingAppraiser.Models.Data;
using ThingAppraiser.SteamService;
using ThingAppraiser.SteamService.Models;

namespace ThingAppraiser.Crawlers.Steam
{
    /// <summary>
    /// Concrete crawler for Steam service.
    /// </summary>
    public sealed class SteamCrawler : Crawler
    {
        /// <summary>
        /// Logger instance for current class.
        /// </summary>
        private static readonly ILogger _logger =
            LoggerFactory.CreateLoggerFor<SteamCrawler>();

        /// <summary>
        /// Adapter class to make a calls to Steam API.
        /// </summary>
        private readonly ISteamApiClient _steamApiClient;

        /// <inheritdoc />
        public override string Tag { get; } = nameof(SteamCrawler);

        /// <inheritdoc />
        public override Type TypeId { get; } = typeof(SteamGameInfo);


        /// <summary>
        /// Initializes instance according to parameter values.
        /// </summary>
        /// <param name="apiKey">Key to get access to Steam service.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="apiKey" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="apiKey" /> presents empty strings or contains only whitespaces.
        /// </exception>
        public SteamCrawler(string apiKey)
        {
            apiKey.ThrowIfNullOrWhiteSpace(nameof(apiKey));

            _steamApiClient = SteamApiClientFactory.CreateClient(apiKey);
        }

        #region Crawler Overridden Methods

        /// <inheritdoc />
        public override List<BasicInfo> GetResponse(List<string> entities, bool outputResults)
        {
            if (SteamAppsStorage.IsEmpty)
            {
                SteamBriefInfoContainer steamApps = _steamApiClient.GetAppListAsync().Result;
                SteamAppsStorage.FillStorage(steamApps);
            }

            // Use HashSet to avoid duplicated data which can produce errors in further work.
            var searchResults = new HashSet<BasicInfo>();
            foreach (string game in entities)
            {
                int? appId = SteamAppsStorage.TryGetAppIdByName(game);

                if (!appId.HasValue)
                {
                    string message = $"{game} was not find in Steam responses storage.";
                    _logger.Warn(message);
                    GlobalMessageHandler.OutputMessage(message);

                    continue;
                }

                var response = _steamApiClient.TryGetSteamAppAsync(
                    appId.Value, SteamCountryCode.Russia, SteamResponseLanguage.English
                ).Result;

                if (response is null)
                {
                    string message = $"{game} was not processed.";
                    _logger.Warn(message);
                    GlobalMessageHandler.OutputMessage(message);

                    continue;
                }

                if (outputResults)
                {
                    GlobalMessageHandler.OutputMessage($"Got {response} from \"{Tag}\".");
                }

                searchResults.Add(response);
            }
            return searchResults.ToList();
        }

        #endregion
    }
}