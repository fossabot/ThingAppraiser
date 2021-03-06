﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Acolyte.Assertions;
using ThingAppraiser.Configuration;
using ThingAppraiser.Logging;
using ThingAppraiser.Models.WebService;

namespace ThingAppraiser.DesktopApp.Models.DataSuppliers
{
    internal sealed class ServiceProxy : IDisposable
    {
        private static readonly ILogger _logger = LoggerFactory.CreateLoggerFor<ServiceProxy>();

        private readonly string _baseAddress;

        private readonly string _apiUrl;

        private readonly HttpClient _client;

        private bool _disposed;


        public ServiceProxy()
        {
            _baseAddress = ConfigOptions.ThingAppraiserService.CommunicationServiceBaseAddress;
            _apiUrl = ConfigOptions.ThingAppraiserService.CommunicationServiceApiUrl;

            _logger.Info($"ThingAppraiser service url: {_baseAddress}");

            _client = new HttpClient { BaseAddress = new Uri(_baseAddress) };
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json")
            );
        }

        public async Task<ProcessingResponse?> SendPostRequest(RequestParams requestParams)
        {
            requestParams.ThrowIfNull(nameof(requestParams));

            _logger.Info($"Service method '{nameof(SendPostRequest)}' is called.");

            using HttpResponseMessage response = await _client.PostAsJsonAsync(
                _apiUrl, requestParams
            );

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<ProcessingResponse>();
                return result;
            }

            return null;
        }

        #region IDisposable Implementation

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _client.Dispose();
        }

        #endregion
    }
}