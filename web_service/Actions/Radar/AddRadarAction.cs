using System.Text.Json.Serialization;
using WebService.Context;
using WebService.Entites;
using WebService.Radar;
using WebService.Utils;

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

        [JsonPropertyName("send_tracks_report")]
        public bool SendTracksReport { get; set; } = false;

        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        [JsonPropertyName("radar_position")]
        public RadarSettings.SensorPositionParams? RadarPosition { get; set; }

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
            if (RadarPosition == null)
               throw new BadRequestException("Radar position must be provided.");
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
        device.SendTracksReport = args.SendTracksReport;

        if (!String.IsNullOrEmpty(args.TemplateId))
        {
            var template = TemplateContext.Instance.GetTemplate(args.TemplateId);          
            device.ConfigScript = new List<string>(template.ConfigScript);
            ConfigScriptUtils.UpdateSensorPosition(device.ConfigScript, args.RadarPosition!);
            RadarConfigParser configParser = new RadarConfigParser(device.ConfigScript);
            device.radarSettings = configParser.GetRadarSettings();  
        }

        try
        {
            device.deviceMapping = DeviceMapper.Instance.GetMappedDevice(device.Id);
        }
        catch {}
        
        DeviceContext.Instance.AddDevice(device);

        System.Console.WriteLine($"Radar device registered.");

        if (args.Enabled)
        {
            Task enableDeviceTask = new Task(() => 
            {
                try
                {
                    var action = new EnableRadarAction(device.Id);
                    action.Run();
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine($"Error: could not enable radar device! - {ex.Message}");
                }
            });
            enableDeviceTask.Start();
        }
    }
}