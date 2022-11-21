using WebService.Entites;
using WebService.Tracking;

namespace WebService.Services;

public interface IRadarService 
{
    public string ServiceId { get; }

    public RadarServiceSettings? Settings {get; set; }
    public IServiceContext CreateServiceContext(RadarDevice device, Dictionary<string,string> serviceOptions);

    public void DisposeServiceContext(IServiceContext serviceContext);

    public void HandleFrame(FrameData frame, IServiceContext serviceContext);
    
}