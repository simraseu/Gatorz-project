using Gatorz.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Gatorz.Services
{

    public interface ITokenService
    {
        Task<string> GetTokenAsync();
    }


    public class AmadeusTokenService : ITokenService
    {
        private readonly IHttpClientFactory _factory;
        private readonly AmadeusSettings _settings;
        private (string Token, DateTime Expiry) _cache;
        private readonly ILogger<AmadeusTokenService> _logger;

        public AmadeusTokenService(IHttpClientFactory factory, IOptions<AmadeusSettings> options, ILogger<AmadeusTokenService> logger)
        {
            _factory = factory;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<string> GetTokenAsync()
        {
            _logger.LogInformation("GetTokenAsync called");

            // Check if we have a valid cached token
            if (!string.IsNullOrEmpty(_cache.Token) && _cache.Expiry > DateTime.UtcNow.AddMinutes(1))
            {
                _logger.LogInformation("Returning cached token");
                return _cache.Token;
            }

            try
            {
                _logger.LogInformation("Getting new token from Amadeus");
                var client = _factory.CreateClient("AmadeusAuth");

                _logger.LogInformation($"Using Amadeus auth URL: {client.BaseAddress}");
                _logger.LogInformation($"Using client ID: {_settings.ClientId.Substring(0, 4)}*** (truncated)");

                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = _settings.ClientId,
                    ["client_secret"] = _settings.ClientSecret
                });

                _logger.LogInformation("Sending token request to Amadeus...");
                var response = await client.PostAsync("", content);
                _logger.LogInformation($"Amadeus auth response status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Amadeus auth error: {errorContent}");
                    throw new HttpRequestException($"Failed to get Amadeus token. Status code: {response.StatusCode}, Response: {errorContent}");
                }

                var json = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Parsing auth response, length: {json.Length}");
                dynamic data = JsonConvert.DeserializeObject(json);

                if (data?.access_token == null)
                {
                    _logger.LogError("No access_token in response");
                    throw new InvalidOperationException("No access_token found in Amadeus response");
                }

                _cache.Token = (string)data.access_token;
                int expiresIn = (int)data.expires_in;
                _cache.Expiry = DateTime.UtcNow.AddSeconds(expiresIn);

                _logger.LogInformation($"New token obtained, expires in {expiresIn} seconds");
                return _cache.Token;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting token: {ex.Message}");
                throw;
            }
        }
    }
}