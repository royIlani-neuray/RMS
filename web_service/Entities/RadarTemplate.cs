
using System.Text.Json.Serialization;

namespace WebService.Entites;

public class RadarTemplate 
{
    [JsonPropertyName("template_id")]
    public String Id { get; set; }

    [JsonPropertyName("name")]
    public String Name { get; set; }

    [JsonPropertyName("description")]
    public String Description { get; set; }

    [JsonPropertyName("model")]
    public String Model { get; set; }

    [JsonPropertyName("application")]
    public String Application { get; set; }

    [JsonPropertyName("config_script")]
    public List<string> ConfigScript { get; set; }

    [JsonIgnore]
    public ReaderWriterLockSlim templateLock;

    public class RadarTemplateBrief 
    {
        [JsonPropertyName("template_id")]
        public String Id { get; set; }

        [JsonPropertyName("name")]
        public String Name { get; set; }

        [JsonPropertyName("description")]
        public String Description { get; set; }

        [JsonPropertyName("model")]
        public String Model { get; set; }

        [JsonPropertyName("application")]
        public String Application { get; set; }

        public RadarTemplateBrief(RadarTemplate template)
        {
            Name = template.Name;
            Description = template.Description;
            Id = template.Id;
            Model = template.Model;
            Application = template.Application;
        }
    }

    public RadarTemplate()
    {
        Id = Guid.NewGuid().ToString();
        Name = String.Empty;
        Description = String.Empty;
        Model = String.Empty;
        Application = String.Empty;
        ConfigScript = new List<string>();
        templateLock = new ReaderWriterLockSlim();
    }
}