using System.Text.Json.Serialization;

namespace WebService.Services;

public class RegisteredService 
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = String.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = String.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = String.Empty;

    [JsonPropertyName("host")]
    public string Host { get; set; } = String.Empty;

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

}