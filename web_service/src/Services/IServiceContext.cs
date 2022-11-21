namespace WebService.Services;
public interface IServiceContext
{
    public enum ServiceState {
        Initialized,
        Active,
        Error
    }

    public ServiceState State { get; set; }
}