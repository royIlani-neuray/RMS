using WebService.Entites;
using System.Text.Json.Serialization;

namespace WebService.Actions.Radar;

public class SetTracksReportsArgs
{
    [JsonPropertyName("send_tracks_report")]
    public bool? SendTracksReport { get; set; }

    public void Validate()
    { 
        if (SendTracksReport == null)
            throw new HttpRequestException("Missing send_tracks_report argument");        
    }
}

public class SetTracksReportsAction : RadarDeviceAction 
{
    private SetTracksReportsArgs args;

    public SetTracksReportsAction(string deviceId, SetTracksReportsArgs args) : base(deviceId) 
    {
        this.args = args;
    }

    protected override void RunDeviceAction(RadarDevice radarDevice)
    {
        radarDevice.SendTracksReport = args.SendTracksReport!.Value;
    }

} 