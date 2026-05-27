using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Text;

namespace hwmonitor
{
    public class HardwareInfo
    {
        public string CpuName { get; set; } = "";
        public int CpuCores { get; set; }
        public int CpuThreads { get; set; }
        public int CpuBaseClock { get; set; }

        public string GpuName { get; set; } = "";
        public int GpuMemoryGB { get; set; }

        public int RamGB { get; set; }
        public int RamSpeed { get; set; }

        public string StorageName { get; set; } = "";
        public int StorageGB { get; set; }

        public static HardwareInfo Load(Computer computer)
        {
            var info = new HardwareInfo();
            foreach (IHardware hardware in computer.Hardware)
            {
                hardware.Update();
                
                if (hardware.HardwareType == HardwareType.Cpu)
                    info.CpuName = hardware.Name;

                else if (hardware.HardwareType == HardwareType.GpuAmd)
                {
                    info.GpuName = hardware.Name;
                    foreach (ISensor sensor in hardware.Sensors)
                        if (sensor.Name == "GPU Memory Total" && sensor.Value.HasValue)
                            info.GpuMemoryGB = (int)Math.Round(sensor.Value.Value / 1024);
                }

                else if (hardware.HardwareType == HardwareType.Storage)
                {
                    info.StorageName = hardware.Name;
                    foreach (ISensor sensor in hardware.Sensors)
                        if (sensor.Name == "Total Space" && sensor.Value.HasValue)
                            info.StorageGB = (int)sensor.Value.Value;
                }

            }
            try
            {
                var cpu = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject obj in cpu.Get())
                {
                    info.CpuCores = Convert.ToInt32(obj["NumberOfCores"]);
                    info.CpuThreads = Convert.ToInt32(obj["NumberOfLogicalProcessors"]);
                    info.CpuBaseClock = Convert.ToInt32(obj["MaxClockSpeed"]);
                }

                var ram = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                long totalBytes = 0;
                foreach (ManagementObject obj in ram.Get())
                {
                    totalBytes += Convert.ToInt64(obj["Capacity"]);
                    info.RamSpeed = Convert.ToInt32(obj["Speed"]);
                }
                info.RamGB = (int)(totalBytes / 1_073_741_824);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"WMI error: {ex.Message}");
            }
            return info;
        }
    }
}
