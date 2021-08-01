using System;
using Newtonsoft.Json;

public struct PlayerProcess
{
	[JsonProperty("hash")]
	public string Hash { get; set; }

	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("title")]
	public string Title { get; set; }

	[JsonProperty("class")]
	public string Class { get; set; }

	[JsonProperty("origin")]
	public string Origin { get; set; }

	[JsonProperty("path")]
	public string Path { get; set; }

	[JsonProperty("size")]
	public int Size { get; set; }

	[JsonProperty("secure")]
	public bool Secure { get; set; }
}
