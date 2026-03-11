using Newtonsoft.Json;

namespace NightScout.Models;

public class GlucoseReading
{
	[JsonProperty("sgv")]
	public int BloodGlucose { get; set; }

	[JsonProperty("delta")]
	public double Delta { get; set; }

	[JsonProperty("direction")]
	public string? Direction { get; set; }

	[JsonProperty("dateString")]
	public string? DateString { get; set; }

	[JsonProperty("date")]
	public long Date { get; set; }

	public DateTime DateTime => DateTimeOffset.FromUnixTimeMilliseconds(Date).ToLocalTime().DateTime;

	public double BloodGlucoseMmol => Math.Round(BloodGlucose / 18.0182, 1);

	public string DeltaDirection => Delta >= 0 ? "+" : "";
	public double DeltaMmol => Math.Round(Delta / 18.0182, 1);

	public string DirectionArrow => Direction switch
	{
		"Flat" => "→",
		"SingleUp" => "↗",
		"DoubleUp" => "⬆",
		"SingleDown" => "↘",
		"DoubleDown" => "⬇",
		"FortyFiveUp" => "↗",
		"FortyFiveDown" => "↘",
		_ => "?"
	};
}
