using LibreHardwareMonitor.Hardware;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace hwmonitor
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<float> _cpuLoadTotal = new();
        public ISeries[] Series { get; set; }
        public Axis[] XAxes { get; set; } = new Axis[]
        {
            new Axis { MinLimit = 0, MaxLimit = 60 }
        };
        public Axis[] YAxes { get; set; } = new Axis[]
        {
            new Axis { MinLimit = 0, MaxLimit = 100 }
        };

        private DispatcherTimer _timer;
        private const int MaxPoints = 60;

        Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
        };

        private void getMetrics()
        {
            var allowedSensorTypes = new[] { SensorType.Load, SensorType.Temperature, SensorType.Data, SensorType.SmallData, SensorType.Power };
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
        }


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Series = new ISeries[]
            {
                new LineSeries<float>
                {
                    Values = _cpuLoadTotal,
                    Name = "CPU Total",
                    Fill = null,
                    GeometrySize = 0,
                }
            };
            computer.Open();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => UpdateMetrics();
            _timer.Start();

        }
        private void UpdateMetrics()
        {
            foreach (IHardware hardware in computer.Hardware)
            {
                if (hardware.HardwareType != HardwareType.Cpu) continue;

                hardware.Update();

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.Name == "CPU Total" && sensor.Value.HasValue)
                    {
                        _cpuLoadTotal.Add(sensor.Value.Value);

                        if (_cpuLoadTotal.Count > MaxPoints)
                            _cpuLoadTotal.RemoveAt(0);
                    }
                }
            }



        }
    }
}