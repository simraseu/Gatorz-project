using Gatorz.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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

        public AmadeusTokenService(IHttpClientFactory factory, IOptions<AmadeusSettings> options)
        {
            _factory = factory;
            _settings = options.Value;
        }

        public async Task<string> GetTokenAsync()
        {
            if (!string.IsNullOrEmpty(_cache.Token) && _cache.Expiry > DateTime.UtcNow.AddMinutes(1))
                return _cache.Token;

            var client = _factory.CreateClient("AmadeusAuth");
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _settings.ClientId,
                ["client_secret"] = _settings.ClientSecret
            });

            var response = await client.PostAsync("", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(json);

            _cache.Token = (string)data.access_token;
            int expiresIn = (int)data.expires_in;
            _cache.Expiry = DateTime.UtcNow.AddSeconds(expiresIn);

            return _cache.Token;
        }
    }
}