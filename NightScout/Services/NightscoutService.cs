using System.Configuration;
using Newtonsoft.Json;
using NightScout.Models;

namespace NightScout.Services;

public class NightscoutService: IDisposable
{
	private readonly HttpClient _httpClient;
	private readonly string _baseUrl;
	private readonly string _token;
	private bool _disposedValue;

	public NightscoutService()
	{
		_httpClient = new HttpClient();
		_baseUrl = ConfigurationManager.AppSettings["NightscoutUrl"] ?? throw new InvalidOperationException("NightscoutUrl not configured");
		_token = ConfigurationManager.AppSettings["NightscoutToken"] ?? throw new InvalidOperationException("NightscoutToken not configured");
	}

	public async Task<GlucoseReading?> GetLatestGlucoseReadingAsync()
	{
		try
		{
			var url = $"{_baseUrl.TrimEnd('/')}/api/v1/entries.json?count=1&token={_token}";
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

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				_httpClient?.Dispose();
			}

			_disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
