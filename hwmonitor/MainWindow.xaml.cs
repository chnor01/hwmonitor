using LibreHardwareMonitor.Hardware;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using InfluxDB3.Client;
using InfluxDB3.Client.Write;


namespace hwmonitor
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private ObservableCollection<float> _cpuLoadTotal = new();
        private ObservableCollection<float> _cpuTemp = new();
        private ObservableCollection<float> _cpuPower = new();
        private ObservableCollection<float> _gpuCoreLoad = new();
        private ObservableCollection<float> _gpuCoreTemp = new();
        private ObservableCollection<float> _gpuMemoryUsed = new();
        private ObservableCollection<float> _gpuMemoryTemp = new();
        private ObservableCollection<float> _gpuHotspotTemp = new();
        private ObservableCollection<float> _gpuPower = new();
        private ObservableCollection<float> _ramUsedPercentage = new();

        private float gpuMemoryTotalMB = 0;
        private float ramUsedGB = 0;
        private float ramAvailGB = 0;
        private float gpuMemoryPercent = 0;

        private string _cpuLoadText = "0%";
        public string CpuLoadText { get => _cpuLoadText; set { _cpuLoadText = value; OnPropertyChanged(nameof(CpuLoadText)); } }
        
        private string _cpuTempText = "0°C";
        public string CpuTempText { get => _cpuTempText; set { _cpuTempText = value; OnPropertyChanged(nameof(CpuTempText)); } }
        
        private string _cpuPowerText = "0W";
        public string CpuPowerText { get => _cpuPowerText; set { _cpuPowerText = value; OnPropertyChanged(nameof(CpuPowerText)); } }
        
        private string _gpuCoreLoadText = "0%";
        public string GpuCoreLoadText { get => _gpuCoreLoadText; set { _gpuCoreLoadText = value; OnPropertyChanged(nameof(GpuCoreLoadText)); } }
        
        private string _gpuCoreTempText = "0°C";
        public string GpuCoreTempText { get => _gpuCoreTempText; set { _gpuCoreTempText = value; OnPropertyChanged(nameof(GpuCoreTempText)); } }
        
        private string _gpuMemoryTempText = "0°C";
        public string GpuMemoryTempText { get => _gpuMemoryTempText; set { _gpuMemoryTempText = value; OnPropertyChanged(nameof(GpuMemoryTempText)); } }
        
        private string _gpuHotspotTempText = "0°C";
        public string GpuHotspotTempText { get => _gpuHotspotTempText; set { _gpuHotspotTempText = value; OnPropertyChanged(nameof(GpuHotspotTempText)); } }

        private string _gpuPowerText = "0W";
        public string GpuPowerText { get => _gpuPowerText; set { _gpuPowerText = value; OnPropertyChanged(nameof(GpuPowerText)); } }

        private string _gpuMemoryUsedGBText = "0GB";
        public string GpuMemoryUsedGBText { get => _gpuMemoryUsedGBText; set { _gpuMemoryUsedGBText = value; OnPropertyChanged(nameof(GpuMemoryUsedGBText)); } }

        private string _ramUsedGBText = "0GB";
        public string RamUsedGBText { get => _ramUsedGBText; set { _ramUsedGBText = value; OnPropertyChanged(nameof(RamUsedGBText)); } }

        public ISeries[] CpuLoadSeries { get; set; }
        public ISeries[] CpuTempSeries { get; set; }
        public ISeries[] CpuPowerSeries { get; set; }
        public ISeries[] GpuLoadSeries { get; set; }
        public ISeries[] GpuCoreTempSeries { get; set; }
        public ISeries[] GpuMemoryTempSeries { get; set; }
        public ISeries[] GpuHotspotTempSeries { get; set; }
        public ISeries[] GpuPowerSeries { get; set; }
        public ISeries[] GpuMemoryUsageSeries { get; set; }
        public ISeries[] RamSeries { get; set; }
        public Axis[] XAxes { get; set; } = new Axis[]
        {
            new Axis { MinLimit = 0, MaxLimit = 60, Labeler = value => $"{value}s", ForceStepToMin = true, MinStep = 15, IsVisible = false }
        };
        public Axis[] YAxes0to100Percent { get; set; } = new Axis[]
        {
            new Axis { MinLimit = 0, MaxLimit = 100, Labeler = value => $"{value}%", ForceStepToMin = true, MinStep = 25, IsVisible = false }
        };
        public Axis[] YAxes0to120Temp { get; set; } = new Axis[]
        {
            new Axis { MinLimit = 0, MaxLimit = 120, Labeler = value => $"{value}°C", ForceStepToMin = true, MinStep = 30, IsVisible = false }
        };
        public Axis[] YAxes0to160Watts { get; set; } = new Axis[]
        {
            new Axis { MinLimit = 0, MaxLimit = 160, Labeler = value => $"{value}W", ForceStepToMin = true, MinStep = 40, IsVisible = false }
        };
        public Axis[] YAxes0to200Watts { get; set; } = new Axis[]
        {
            new Axis { MinLimit = 0, MaxLimit = 200, Labeler = value => $"{value}W", ForceStepToMin = true, MinStep = 50, IsVisible = false }
        };


        private InfluxDBClient _influxClient;

        private DispatcherTimer _timer;
        private const int MaxPoints = 60;

        Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsStorageEnabled = false,
            IsControllerEnabled = false,
            IsNetworkEnabled = false,
            IsMotherboardEnabled = false,
        };


        public MainWindow()
        {
            var host = Environment.GetEnvironmentVariable("INFLUXDB3_HOST")
                ?? throw new InvalidOperationException("INFLUXDB3_HOST is not set");
            var token = Environment.GetEnvironmentVariable("INFLUXDB3_AUTH_TOKEN")
                ?? throw new InvalidOperationException("INFLUXDB3_AUTH_TOKEN is not set");
            var database = Environment.GetEnvironmentVariable("INFLUXDB3_DATABASE");

            _influxClient = new InfluxDBClient(host: host, token: token, database: database);

            InitializeComponent();
            DataContext = this;

            CpuLoadSeries = new ISeries[]
            { 
                new LineSeries<float> { Values = _cpuLoadTotal, Name = "CPU Load", Fill = new SolidColorPaint(SKColors.DeepSkyBlue.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 2 }, LineSmoothness = 1, YToolTipLabelFormatter = p => $"{p.Model:F1}%" }
            };

            CpuTempSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _cpuTemp, Name = "CPU Temp", Fill = new SolidColorPaint(SKColors.DeepSkyBlue.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 2 }, LineSmoothness = 1, YToolTipLabelFormatter = p => $"{p.Model:F1}°C"}
            };

            CpuPowerSeries = new ISeries [] 
            {
            new LineSeries<float> { Values = _cpuPower, Name = "CPU Power", Fill = new SolidColorPaint(SKColors.DeepSkyBlue.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 2 }, LineSmoothness = 1, YToolTipLabelFormatter = p => $"{p.Model:F1}W" }
            };

            GpuLoadSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _gpuCoreLoad, Name = "GPU Core Load", Fill = new SolidColorPaint(SKColors.MediumPurple.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.MediumPurple) { StrokeThickness = 2 }, LineSmoothness = 1 }
            };

            GpuCoreTempSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _gpuCoreTemp,    Name = "GPU Core Temp", Fill = new SolidColorPaint(SKColors.MediumPurple.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.MediumPurple) { StrokeThickness = 2 }, LineSmoothness = 1  }
            };

            GpuMemoryTempSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _gpuMemoryTemp,  Name = "GPU Memory Temp", Fill = new SolidColorPaint(SKColors.MediumPurple.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.MediumPurple) { StrokeThickness = 2 }, LineSmoothness = 1  }
            };

            GpuHotspotTempSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _gpuHotspotTemp, Name = "GPU Hotspot Temp", Fill = new SolidColorPaint(SKColors.MediumPurple.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.MediumPurple) { StrokeThickness = 2 }, LineSmoothness = 1  }
            };

            GpuPowerSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _gpuPower, Name = "GPU Power", Fill = new SolidColorPaint(SKColors.MediumPurple.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.MediumPurple) { StrokeThickness = 2 }, LineSmoothness = 1  }
            };

            GpuMemoryUsageSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _gpuMemoryUsed, Name = "GPU Memory", Fill = new SolidColorPaint(SKColors.MediumPurple.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.MediumPurple) { StrokeThickness = 2 }, LineSmoothness = 1, YToolTipLabelFormatter = p => $"{p.Model:F1}%" }
            };

            RamSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _ramUsedPercentage, Name = "RAM Usage", Fill = new SolidColorPaint(SKColors.MediumSpringGreen.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.MediumSpringGreen) { StrokeThickness = 2 }, LineSmoothness = 1 , YToolTipLabelFormatter = p => $"{p.Model:F1}%"}
            };

            computer.Open();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += async (s, e) =>
            {
                UpdateMetrics();
                //await WriteMetricsToInflux();
            };
            _timer.Start();

        }
        private void UpdateMetrics()
        {
            foreach (IHardware hardware in computer.Hardware)
            {
                if (hardware.Name.Contains("Virtual") || hardware.Name.Contains("Graphics"))
                    continue;

                hardware.Update();

                if (hardware.HardwareType == HardwareType.Cpu)
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (!sensor.Value.HasValue) continue;

                        switch (sensor.Name)
                        {
                            case "CPU Total": addPoint(_cpuLoadTotal, sensor.Value.Value); CpuLoadText = $"{sensor.Value.Value:F1}%"; break;
                            case "Core (Tctl/Tdie)": addPoint(_cpuTemp, sensor.Value.Value); CpuTempText = $"{sensor.Value.Value:F1}°C"; break;
                            case "Package": addPoint(_cpuPower, sensor.Value.Value); CpuPowerText = $"{sensor.Value.Value:F1}W"; break;
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
                                {
                                    addPoint(_gpuCoreLoad, sensor.Value.Value);
                                    GpuCoreLoadText = $"{sensor.Value.Value:F1}%";
                                }
                                if (sensor.SensorType == SensorType.Temperature)
                                {
                                    addPoint(_gpuCoreTemp, sensor.Value.Value);
                                    GpuCoreTempText = $"{sensor.Value.Value:F1}°C";
                                }
                                break;
                            case "GPU Memory":
                                if (sensor.SensorType == SensorType.Temperature)
                                {
                                    addPoint(_gpuMemoryTemp, sensor.Value.Value);
                                    GpuMemoryTempText = $"{sensor.Value.Value:F1}°C";
                                }
                                break;
                            case "GPU Hot Spot": addPoint(_gpuHotspotTemp, sensor.Value.Value); GpuHotspotTempText = $"{sensor.Value.Value:F1}°C"; break;
                            case "GPU Package": addPoint(_gpuPower, sensor.Value.Value); GpuPowerText = $"{sensor.Value.Value:F1}W"; break;
                            case "GPU Memory Total": gpuMemoryTotalMB = sensor.Value.Value; break;
                            case "GPU Memory Used":
                                gpuMemoryPercent = (sensor.Value.Value / gpuMemoryTotalMB) * 100;
                                addPoint(_gpuMemoryUsed, gpuMemoryPercent); 
                                GpuMemoryUsedGBText = $"{sensor.Value.Value / 1024:F2}/{gpuMemoryTotalMB / 1024:F0}GB"; 
                                break;
                        }
                    }

                else if (hardware.HardwareType == HardwareType.Memory)
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        //Debug.WriteLine($"{sensor.SensorType}: {sensor.Name} -- {sensor.Value}");
                        if (!sensor.Value.HasValue) continue;

                        switch (sensor.Name)
                        {
                            case "Memory Used": ramUsedGB = sensor.Value.Value; break;
                            case "Memory Available": ramAvailGB = sensor.Value.Value; RamUsedGBText = $"{ramUsedGB:F1}/{ramUsedGB + ramAvailGB:F0}GB"; break;
                            case "Memory": addPoint(_ramUsedPercentage, sensor.Value.Value); break;
                        }
                    }
            }
        }
        private async Task WriteMetricsToInflux()
        {
            if (_cpuLoadTotal.Count == 0) return;

            var point = PointData.Measurement("pcmetrics")
                .SetTag("host", Environment.MachineName)
                .SetField("cpu_load", _cpuLoadTotal.Last())
                .SetField("cpu_temp", _cpuTemp.Last())
                .SetField("cpu_power", _cpuPower.Last())
                .SetField("gpu_load", _gpuCoreLoad.Last())
                .SetField("gpu_core_temp", _gpuCoreTemp.Last())
                .SetField("gpu_memory_temp", _gpuMemoryTemp.Last())
                .SetField("gpu_hotspot_temp", _gpuHotspotTemp.Last())
                .SetField("gpu_power", _gpuPower.Last())
                .SetField("gpu_memory_percent", _gpuMemoryUsed.Last())
                .SetField("ram_percent", _ramUsedPercentage.Last())
                .SetTimestamp(DateTime.UtcNow); 

            await _influxClient.WritePointAsync(point);
        }
        
        private void addPoint(ObservableCollection<float> collection, float value)
        {
            collection.Add(value);
            if (collection.Count > MaxPoints)
                collection.Clear();
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            computer.Close();
            base.OnClosed(e);
        }
    }
}