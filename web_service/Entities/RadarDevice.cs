namespace WebService.Entites;

public class RadarDevice {

    public enum DeviceState {
        Unknown,
        Disconnected,
        Connected,
        Active
    };

    public DeviceState State { get; set; }
    public String Name { get; set; }
    public String Description { get; set; }
    public String Model { get; set; }
    public String Id { get; set; }
    public bool Enabled {get; set; }

    public class RadarDeviceBrief 
    {
        public DeviceState State { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public String Model { get; set; }
        public String Id { get; set; }
        public bool Enabled {get; set; }

        public RadarDeviceBrief(RadarDevice device)
        {
            State = device.State;
            Name = device.Name;
            Model = device.Model;
            Description = device.Description;
            Id = device.Id;
            Enabled = device.Enabled;
        }
    }

    public RadarDevice()
    {
        Name = "";
        Description = "";
        Model = "";
        Id = "";
        Enabled = false;
        State = DeviceState.Unknown;
    }
}