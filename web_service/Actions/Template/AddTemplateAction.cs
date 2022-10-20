using System.Text.Json.Serialization;
using WebService.Context;
using WebService.Entites;
using WebService.Radar;

namespace WebService.Actions.Template;

public class AddTemplateArgs 
    {
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

        public AddTemplateArgs()
        {
            Name = String.Empty;
            Description = String.Empty;
            Model = String.Empty;
            Application = String.Empty;
            ConfigScript = new List<string>();
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new HttpRequestException("Template name not defined");
            if (string.IsNullOrWhiteSpace(Application))
                throw new HttpRequestException("Template application not defined");
            if (string.IsNullOrWhiteSpace(Model))
                throw new HttpRequestException("Template model not defined");
        }
    }

public class AddTemplateAction : IAction 
{
    AddTemplateArgs args;

    public AddTemplateAction(AddTemplateArgs args)
    {
        this.args = args;
    }

    public void Run()
    {
        args.Validate();

        RadarTemplate template = new RadarTemplate();
        template.Name = args.Name;
        template.Description = args.Description;
        template.Model = args.Model;
        template.Application = args.Application;
        template.ConfigScript = args.ConfigScript;

        System.Console.WriteLine($"Adding new template - [{template.Name}]");
 
        TemplateContext.Instance.AddTemplate(template);

        System.Console.WriteLine($"Radar template added.");
    }
}