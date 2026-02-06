using System.Configuration;
using Newtonsoft.Json;
using NightScout.Models;

namespace NightScout.Services
{
    public class NightscoutService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public NightscoutService()
        {
            _httpClient = new HttpClient();
            _baseUrl = ConfigurationManager.AppSettings["NightscoutUrl"] ?? throw new InvalidOperationException("NightscoutUrl not configured");
        }

        public async Task<GlucoseReading?> GetLatestGlucoseReadingAsync()
        {
            try
            {
                var url = $"{_baseUrl.TrimEnd('/')}/api/v1/entries.json?count=1";
                var response = await _httpClient.GetStringAsync(url);
                var readings = JsonConvert.DeserializeObject<GlucoseReading[]>(response);
                
                return readings?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Log error or handle as needed
                System.Diagnostics.Debug.WriteLine($"Error fetching glucose reading: {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}