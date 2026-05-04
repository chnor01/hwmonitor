using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LibreHardwareMonitor.Hardware;

namespace hwmonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
        };
        
        private void getMetrics()
        {
            var allowedSensorTypes = new[] { SensorType.Load, SensorType.Temperature, SensorType.Data, SensorType.SmallData, SensorType.Power};
            var allowedHardwareTypes = new[] { HardwareType.Cpu, HardwareType.GpuAmd, HardwareType.Memory };

            float cpuLoadTotal = 0;
            float cpuMaxCoreLoad = 0;
            float cpuTemp = 0;
            float cpuPower = 0;

            float ramUsedGB = 0;
            float ramAvailGB = 0;
            float ramUsedPercentage = 0;

            float gpuCoreLoad = 0;
            float gpuMemoryLoad = 0;
            float gpuCoreTemp = 0;
            float gpuMemoryTemp = 0;
            float gpuHotspotTemp = 0;
            float gpuPower = 0;
            float gpuMemoryUsedMB = 0;
            float gpuMemoryFreeMB = 0;


            foreach (IHardware hardware in computer.Hardware)
            {
                if (hardware.Name.Contains("Virtual") || hardware.Name.Contains("Graphics"))
                    continue;

                if (!allowedHardwareTypes.Contains(hardware.HardwareType))
                    continue;

                hardware.Update();

                Debug.WriteLine($"\n{hardware.HardwareType}: {hardware.Name}");


                if (hardware.HardwareType == HardwareType.Cpu)
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (!sensor.Value.HasValue) continue;

                        switch (sensor.Name)
                        {
                            case "CPU Total": cpuLoadTotal = sensor.Value.Value; break;
                            case "CPU Core Max": cpuMaxCoreLoad = sensor.Value.Value; break;
                            case "Core (Tctl/Tdie)": cpuTemp = sensor.Value.Value; break;
                            case "Package": cpuPower = sensor.Value.Value; break;
                        }
                    }

                else if (hardware.HardwareType == HardwareType.GpuAmd)
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (!sensor.Value.HasValue) continue;

                        switch (sensor.Name)
                        {
                            case "GPU Core":
                                if (sensor.SensorType == SensorType.Load)
                                    gpuCoreLoad = sensor.Value.Value;
                                if (sensor.SensorType == SensorType.Temperature)
                                    gpuCoreTemp = sensor.Value.Value; 
                                break;
                            case "GPU Memory":
                                if (sensor.SensorType == SensorType.Load) gpuMemoryLoad = sensor.Value.Value;
                                if (sensor.SensorType == SensorType.Temperature) gpuMemoryTemp = sensor.Value.Value;
                                break;
                            case "GPU Hot Spot": gpuHotspotTemp = sensor.Value.Value; break;
                            case "GPU Package": gpuPower = sensor.Value.Value; break;
                            case "GPU Memory Used": gpuMemoryUsedMB = sensor.Value.Value; break;
                            case "GPU Memory Free": gpuMemoryFreeMB = sensor.Value.Value; break;
                        }
                    }


                else if (hardware.HardwareType == HardwareType.Memory)
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (!sensor.Value.HasValue) continue;

                        switch (sensor.Name)
                        {
                            case "Memory Used": ramUsedGB = sensor.Value.Value; break;
                            case "Memory Available": ramAvailGB = sensor.Value.Value; break;
                            case "Memory": ramUsedPercentage = sensor.Value.Value; break;

                        }
                    }
            }

            Debug.WriteLine($"=== CPU ===");
            Debug.WriteLine($"  Total Load:    {cpuLoadTotal:F1}%");
            Debug.WriteLine($"  Max Core Load: {cpuMaxCoreLoad:F1}%");
            Debug.WriteLine($"  Temperature:   {cpuTemp:F1}°C");
            Debug.WriteLine($"  Power:         {cpuPower:F1}W");

            Debug.WriteLine($"\n=== Memory ===");
            Debug.WriteLine($"  Used:          {ramUsedGB:F1} GB");
            Debug.WriteLine($"  Available:     {ramAvailGB:F1} GB");
            Debug.WriteLine($"  Load:          {ramUsedPercentage:F1}%");

            Debug.WriteLine($"\n=== GPU ===");
            Debug.WriteLine($"  Core Load:     {gpuCoreLoad:F1}%");
            Debug.WriteLine($"  Memory Load:   {gpuMemoryLoad:F1}%");
            Debug.WriteLine($"  Core Temp:     {gpuCoreTemp:F1}°C");
            Debug.WriteLine($"  Memory Temp:   {gpuMemoryTemp:F1}°C");
            Debug.WriteLine($"  Hotspot Temp:  {gpuHotspotTemp:F1}°C");
            Debug.WriteLine($"  Power:         {gpuPower:F1}W");
            Debug.WriteLine($"  Memory Used:   {gpuMemoryUsedMB:F0} MB");
            Debug.WriteLine($"  Memory Free:   {gpuMemoryFreeMB:F0} MB");
        }
        

        public MainWindow()
        {
            InitializeComponent();
            computer.Open();
            getMetrics();
            computer.Close();
        }


    }
}