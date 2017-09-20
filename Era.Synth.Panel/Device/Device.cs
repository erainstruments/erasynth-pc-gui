namespace Era.Synth.Control.Panel
{
    public class Device
    {
        public Device() { }

        public string DevicePort { get; set; }

        public string Description { get; set; }

        public int MaxBaudRate { get; set; }

        public string Name { get; set; }

        public string DeviceID { get; set; }

        public bool Status { get; set; }

        public bool Active { get; set; }

        public bool Selected { get; set; }

        public string DisplayName { get; set; }
    }
}
