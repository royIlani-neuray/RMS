using System.Text.Json.Serialization;
using WebService.Context;
using WebService.Entites;
using WebService.Radar;

namespace WebService.Actions.Radar;

public class AddRadarDeviceArgs 
    {
        [JsonPropertyName("device_id")]
        public string Id { get; set; } = String.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = String.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = String.Empty;

        [JsonPropertyName("template_id")]
        public string TemplateId { get; set; } = String.Empty;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new HttpRequestException("Missing radar name.");
            if (string.IsNullOrWhiteSpace(Id))
                throw new HttpRequestException("Missing radar id.");
            if (!Guid.TryParse(Id, out _))
                throw new BadRequestException("invalid device id provided.");            
            if (!String.IsNullOrEmpty(TemplateId) && !Guid.TryParse(TemplateId, out _))
                throw new BadRequestException("invalid template id provided.");            
        }
    }

public class AddRadarAction : IAction 
{
    AddRadarDeviceArgs args;

    public AddRadarAction(AddRadarDeviceArgs args)
    {
        this.args = args;
    }

    public void Run()
    {
        args.Validate();

        System.Console.WriteLine($"Registering radar device - {args.Id}");

        RadarDevice device = new RadarDevice();
        device.Id = args.Id;
        device.Name = args.Name;
        device.Description = args.Description;

        if (!String.IsNullOrEmpty(args.TemplateId))
        {
            var template = TemplateContext.Instance.GetTemplate(args.TemplateId);
            device.ConfigScript = template.ConfigScript;
        }

        try
        {
            device.deviceMapping = DeviceMapper.Instance.GetMappedDevice(device.Id);
        }
        catch {}
        
        DeviceContext.Instance.AddDevice(device);

        System.Console.WriteLine($"Radar device registered.");
    }
}