using System;
using System.Management;
using System.Collections.Generic;

namespace Era.Synth.Control.Panel
{
    public class DeviceList : List<Device>
    {
        private static List<Device> _devices;

        public DeviceList()
        {
            _devices = new List<Device>();
        }

        public static List<Device> getAllDevices()
        {
            if (_devices == null) { _devices = new List<Device>(); }
            _devices.Clear();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_SerialPort");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    Device _device = new Device();
                    _device.Description = queryObj["Description"].ToString();
                    _device.DevicePort = queryObj["DeviceID"].ToString();
                    _device.MaxBaudRate = Convert.ToInt32(queryObj["MaxBaudRate"]);
                    _device.Name = queryObj["Name"].ToString();
                    _device.DeviceID = queryObj["PNPDeviceID"].ToString();
                    _device.Selected = false;
                    _device.Active = false;
                    _device.DisplayName = _device.DevicePort;

                    if (_device.Description != "") { _device.DisplayName += " - " + _device.Description; }

                    if (queryObj["Status"].ToString() == "OK") { _device.Status = true; }
                    else { _device.Status = false; }
                    _devices.Add(_device);
                }
            }
            catch
            {
                foreach (string port in System.IO.Ports.SerialPort.GetPortNames())
                {
                    Device _device = new Device() { Description = "", DevicePort = port, MaxBaudRate = 0, Name = "", DeviceID = "", Status = true, Active = false, Selected = false, DisplayName = port };
                    _devices.Add(_device);
                }
            }
            return _devices;
        }
    }
}