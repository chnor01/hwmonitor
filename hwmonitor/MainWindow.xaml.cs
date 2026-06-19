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
using System.Text.Json;
using System.IO;


namespace hwmonitor
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // collections storing hardware sensordata, for metrics charts.
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
        private ObservableCollection<float> _storageCompTemp = new();
        private ObservableCollection<float> _storageReadRate = new();
        private ObservableCollection<float> _storageWriteRate = new();

        // strings for metrics, displayed in main window on each chart
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

        private string _storageCompTempText = "0°C";
        public string StorageCompTempText { get => _storageCompTempText; set { _storageCompTempText = value; OnPropertyChanged(nameof(StorageCompTempText)); } }

        private string _storageReadRateText = "0MB/s";
        public string StorageReadRateText { get => _storageReadRateText; set { _storageReadRateText = value; OnPropertyChanged(nameof(StorageReadRateText)); } }

        private string _storageWriteRateText = "0MB/s";
        public string StorageWriteRateText { get => _storageWriteRateText; set { _storageWriteRateText = value; OnPropertyChanged(nameof(StorageWriteRateText)); } }

        // all metrics charts
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
        public ISeries[] StorageCompTempSeries { get; set; }
        public ISeries[] StorageReadRateSeries { get; set; }
        public ISeries[] StorageWriteRateSeries { get; set; }

        // alert collections
        public ObservableCollection<AlertEntry> CpuAlerts { get; set; } = new();
        public ObservableCollection<AlertEntry> GpuAlerts { get; set; } = new();
        public ObservableCollection<AlertEntry> RamAlerts { get; set; } = new();
        public ObservableCollection<AlertEntry> StorageAlerts { get; set; } = new();

        // axes for charts
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
        public Axis[] YAxesReadWrite7000MBs { get; set; } = new Axis[]
        {
            new Axis { MinLimit = 0, MaxLimit = 7000, Labeler = value => $"{value}MB/s", ForceStepToMin = true, MinStep = 1750, IsVisible = false }
        };
        public Axis[] YAxesStorageTemp0to90C { get; set; } = new Axis[]
        {
            new Axis { MinLimit = 0, MaxLimit = 90, Labeler = value => $"{value}°C", ForceStepToMin = true, MinStep = 25, IsVisible = false }
        };

        private AppSettings _appSettings;

        private HardwareInfo _hardwareInfo;

        public HardwareInfo HardwareInfo => _hardwareInfo;

        private SessionStats _sessionStats = new SessionStats();
        public SessionStats SessionStats => _sessionStats;

        private DispatcherTimer _sessionTimer;
        private DateTime _sessionStart;
        public string SessionTime => (DateTime.Now - _sessionStart).ToString(@"hh\:mm\:ss");

        private Dictionary<string, DateTime> _lastMetricNotifications = new Dictionary<string, DateTime>();

        private InfluxDBClient _influxClient;

        private CancellationTokenSource _cts = new CancellationTokenSource();

        // max points per chart
        private const int MaxPoints = 60;

        // max amount of alerts per hardware type
        private const int MaxAlerts = 20;

        // time in seconds between each alert
        private const int AlertWaitSeconds = 10;

        // instance to fetch the metrics
        Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsStorageEnabled = true,
            IsControllerEnabled = false,
            IsNetworkEnabled = false,
            IsMotherboardEnabled = false,
        };

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // env variables for influxdb
            var host = Environment.GetEnvironmentVariable("INFLUXDB3_HOST")
                ?? throw new InvalidOperationException("INFLUXDB3_HOST is not set");
            var token = Environment.GetEnvironmentVariable("INFLUXDB3_AUTH_TOKEN")
                ?? throw new InvalidOperationException("INFLUXDB3_AUTH_TOKEN is not set");
            var database = Environment.GetEnvironmentVariable("INFLUXDB3_DATABASE");

            _influxClient = new InfluxDBClient(host: host, token: token, database: database);

            StartSessionTimer();
            _appSettings = AppSettings.Load();
            computer.Open();
            _hardwareInfo = HardwareInfo.Load(computer);
            Task.Run(() => MetricsLoop(_cts.Token));

            CpuLoadSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _cpuLoadTotal, Name = "CPU Load", Fill = new SolidColorPaint(SKColors.DeepSkyBlue.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 2 }, LineSmoothness = 1, YToolTipLabelFormatter = p => $"{p.Model:F1}%" }
            };

            CpuTempSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _cpuTemp, Name = "CPU Temp", Fill = new SolidColorPaint(SKColors.DeepSkyBlue.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.DeepSkyBlue) { StrokeThickness = 2 }, LineSmoothness = 1, YToolTipLabelFormatter = p => $"{p.Model:F1}°C"}
            };

            CpuPowerSeries = new ISeries[]
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

            StorageCompTempSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _storageCompTemp, Name = "Storage Composite Temp", Fill = new SolidColorPaint(SKColors.Orange.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 2 }, LineSmoothness = 1, YToolTipLabelFormatter = p => $"{p.Model:F1}°C"}
            };

            StorageReadRateSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _storageReadRate, Name = "Storage Read Rate", Fill = new SolidColorPaint(SKColors.Orange.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 2 }, LineSmoothness = 1, YToolTipLabelFormatter = p => $"{p.Model:F1}MB/s"}
            };

            StorageWriteRateSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _storageWriteRate, Name = "Storage Write Rate", Fill = new SolidColorPaint(SKColors.Orange.WithAlpha(75)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 2 }, LineSmoothness = 1, YToolTipLabelFormatter = p => $"{p.Model:F1}MB/s"}
            };

        }

        private void StartSessionTimer()
        {
            _sessionStart = DateTime.Now;
            _sessionTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _sessionTimer.Tick += (s, e) => OnPropertyChanged(nameof(SessionTime));
            _sessionTimer.Start();
        }

        // periodically fetch sensordata and update the mainwindow UI
        private async Task MetricsLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var data = await Task.Run(() => ReadSensors(), token);
                Dispatcher.Invoke(() => UpdateUI(data));
                await Task.Delay(_appSettings.PollingIntervalMs, token);
            }
        }

        // fetch sensordata
        private SensorData ReadSensors()
        {
            var data = new SensorData();
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
                            case "CPU Total": data.CpuLoadTotal = sensor.Value.Value; break;
                            case "Core (Tctl/Tdie)": data.CpuTemp = sensor.Value.Value; break;
                            case "Package": data.CpuPower = sensor.Value.Value; break;
                        }
                    }

                else if (hardware.HardwareType == HardwareType.GpuAmd)
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (!sensor.Value.HasValue) continue;
                        switch (sensor.Name)
                        {
                            case "GPU Core":
                                if (sensor.SensorType == SensorType.Load) data.GpuCoreLoad = sensor.Value.Value;
                                if (sensor.SensorType == SensorType.Temperature) data.GpuCoreTemp = sensor.Value.Value;
                                break;
                            case "GPU Memory":
                                if (sensor.SensorType == SensorType.Temperature) data.GpuMemoryTemp = sensor.Value.Value;
                                break;
                            case "GPU Hot Spot": data.GpuHotspotTemp = sensor.Value.Value; break;
                            case "GPU Package": data.GpuPower = sensor.Value.Value; break;
                            case "GPU Memory Total": data.GpuMemoryTotalMB = sensor.Value.Value; break;
                            case "GPU Memory Used":
                                data.GpuMemoryUsedMB = sensor.Value.Value;
                                data.GpuMemoryPercent = (sensor.Value.Value / data.GpuMemoryTotalMB) * 100;
                                break;
                        }
                    }

                else if (hardware.HardwareType == HardwareType.Memory)
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (!sensor.Value.HasValue) continue;
                        switch (sensor.Name)
                        {
                            case "Memory Used": data.RamUsedGB = sensor.Value.Value; break;
                            case "Memory Available": data.RamAvailGB = sensor.Value.Value; break;
                            case "Memory": data.RamPercent = sensor.Value.Value; break;
                        }
                    }

                else if (hardware.HardwareType == HardwareType.Storage)
                {
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (!sensor.Value.HasValue) continue;
                        switch (sensor.Name)
                        {
                            case "Composite Temperature": data.StorageCompTemp = sensor.Value.Value; break;
                            case "Read Rate": data.StorageReadRate = (sensor.Value.Value / 1_000_000); break;
                            case "Write Rate": data.StorageWriteRate = (sensor.Value.Value / 1_000_000); break;
                        }
                    }
                }
            }

            return data;

        }

        // add a sensordata point to its collection
        private void addPoint(ObservableCollection<float> collection, float value)
        {
            collection.Add(value);
            if (collection.Count > MaxPoints)
                collection.Clear();
        }

        // add all fetched sensordata to their respective collection and update the metrics UI text strings
        private void UpdateUI(SensorData data)
        {
            addPoint(_cpuLoadTotal, data.CpuLoadTotal);
            addPoint(_cpuTemp, data.CpuTemp);
            addPoint(_cpuPower, data.CpuPower);
            CpuLoadText = $"{data.CpuLoadTotal:F1}%";
            CpuTempText = $"{data.CpuTemp:F1}°C";
            CpuPowerText = $"{data.CpuPower:F1}W";

            addPoint(_gpuCoreLoad, data.GpuCoreLoad);
            addPoint(_gpuCoreTemp, data.GpuCoreTemp);
            addPoint(_gpuMemoryTemp, data.GpuMemoryTemp);
            addPoint(_gpuHotspotTemp, data.GpuHotspotTemp);
            addPoint(_gpuPower, data.GpuPower);
            addPoint(_gpuMemoryUsed, data.GpuMemoryPercent);
            GpuCoreLoadText = $"{data.GpuCoreLoad:F1}%";
            GpuCoreTempText = $"{data.GpuCoreTemp:F1}°C";
            GpuMemoryTempText = $"{data.GpuMemoryTemp:F1}°C";
            GpuHotspotTempText = $"{data.GpuHotspotTemp:F1}°C";
            GpuPowerText = $"{data.GpuPower:F1}W";
            GpuMemoryUsedGBText = $"{data.GpuMemoryUsedMB / 1024:F2}/{data.GpuMemoryTotalMB / 1024:F0}GB";

            addPoint(_ramUsedPercentage, data.RamPercent);
            RamUsedGBText = $"{data.RamUsedGB:F1}/{data.RamUsedGB + data.RamAvailGB:F0}GB";

            addPoint(_storageCompTemp, data.StorageCompTemp);
            addPoint(_storageReadRate, data.StorageReadRate);
            addPoint(_storageWriteRate, data.StorageWriteRate);
            StorageCompTempText = $"{data.StorageCompTemp:F1}°C";
            StorageReadRateText = $"{data.StorageReadRate:F1}MB/s";
            StorageWriteRateText = $"{data.StorageWriteRate:F1}MB/s";

            _sessionStats.Update(data);
            CheckAlerts(data);

            //_ = WriteMetricsToInflux(data);

        }

        // check if sensordata is above alert settings warning/critical thresholds. send alerts if true
        private void CheckAlerts(SensorData data)
        {
            if (_appSettings.CpuLoadAlertEnabled)
            {
                if (data.CpuLoadTotal >= _appSettings.CpuLoadCritical)
                    SendNotification("CPU", "Critical", $"CPU utilization is {data.CpuLoadTotal:F1}%", CpuAlerts);
                else if (data.CpuLoadTotal >= _appSettings.CpuLoadWarning)
                    SendNotification("CPU", "Warning", $"CPU utilization is {data.CpuLoadTotal:F1}%", CpuAlerts);
            }
            if (_appSettings.CpuTempAlertEnabled)
            {
                if (data.CpuTemp >= _appSettings.CpuTempCritical)
                    SendNotification("CPU", "Critical", $"CPU temperature is {data.CpuTemp:F1}°C", CpuAlerts);
                else if (data.CpuTemp >= _appSettings.CpuTempWarning)
                    SendNotification("CPU", "Warning", $"CPU temperature is {data.CpuTemp:F1}°C", CpuAlerts);
            }

            if (_appSettings.CpuPowerAlertEnabled)
            {
                if (data.CpuPower >= _appSettings.CpuPowerCritical)
                    SendNotification("CPU", "Critical", $"CPU power consumption is {data.CpuPower:F1}W", CpuAlerts);
                else if (data.CpuPower >= _appSettings.CpuPowerWarning)
                    SendNotification("CPU", "Warning", $"CPU power consumption is {data.CpuPower:F1}W", CpuAlerts);
            }

            if (_appSettings.GpuLoadAlertEnabled)
            {
                if (data.GpuCoreLoad >= _appSettings.GpuLoadCritical)
                    SendNotification("GPU", "Critical", $"GPU utilization is {data.GpuCoreLoad:F1}%", GpuAlerts);
                else if (data.GpuCoreLoad >= _appSettings.GpuLoadWarning)
                    SendNotification("GPU", "Warning", $"GPU utilization is {data.GpuCoreLoad:F1}%", GpuAlerts);
            }

            if (_appSettings.GpuTempAlertEnabled)
            {
                if (data.GpuCoreTemp >= _appSettings.GpuTempCritical)
                    SendNotification("GPU", "Critical", $"GPU core temperature is {data.GpuCoreTemp:F1}°C", GpuAlerts);
                else if (data.GpuCoreTemp >= _appSettings.GpuTempWarning)
                    SendNotification("GPU", "Warning", $"GPU core temperature is {data.GpuCoreTemp:F1}°C", GpuAlerts);
            }

            if (_appSettings.GpuPowerAlertEnabled)
            {
                if (data.GpuPower >= _appSettings.GpuPowerCritical)
                    SendNotification("GPU", "Critical", $"GPU power consumption is {data.GpuPower:F1}W", GpuAlerts);
                else if (data.GpuPower >= _appSettings.GpuPowerWarning)
                    SendNotification("GPU", "Warning", $"GPU power consumption is {data.GpuPower:F1}W", GpuAlerts);
            }

            if (_appSettings.RamAlertEnabled)
            {
                if (data.RamPercent >= _appSettings.RamCritical)
                    SendNotification("RAM", "Critical", $"RAM usage is {data.RamPercent:F1}%", RamAlerts);
                else if (data.RamPercent >= _appSettings.RamWarning)
                    SendNotification("RAM", "Warning", $"RAM usage is {data.RamPercent:F1}%", RamAlerts);
            }

            if (_appSettings.StorageTempAlertEnabled)
            {
                if (data.StorageCompTemp >= _appSettings.StorageTempCritical)
                    SendNotification("Storage", "Critical", $"Storage composite temperature is {data.StorageCompTemp:F1}°C", StorageAlerts);
                else if (data.StorageCompTemp >= _appSettings.StorageTempWarning)
                    SendNotification("Storage", "Warning", $"Storage composite temperature is {data.StorageCompTemp:F1}°C", StorageAlerts);
            }
        }

        // write sensordata to influxdb
        private async Task WriteMetricsToInflux(SensorData data)
        {
            var point = PointData.Measurement("pcmetrics")
                .SetTag("host", Environment.MachineName)
                .SetField("cpu_load", data.CpuLoadTotal)
                .SetField("cpu_temp", data.CpuTemp)
                .SetField("cpu_power", data.CpuPower)
                .SetField("gpu_load", data.GpuCoreLoad)
                .SetField("gpu_core_temp", data.GpuCoreTemp)
                .SetField("gpu_memory_temp", data.GpuMemoryTemp)
                .SetField("gpu_hotspot_temp", data.GpuHotspotTemp)
                .SetField("gpu_power", data.GpuPower)
                .SetField("gpu_memory_percent", data.GpuMemoryPercent)
                .SetField("ram_percent", data.RamPercent)
                .SetField("storage_temp", data.StorageCompTemp)
                .SetField("storage_read", data.StorageReadRate)
                .SetField("storage_write", data.StorageWriteRate)
                .SetTimestamp(DateTime.UtcNow);

            await _influxClient.WritePointAsync(point);
        }

        // cleanup when closing mainwindow
        protected override void OnClosed(EventArgs e)
        {
            _cts.Cancel();
            computer.Close();
            base.OnClosed(e);
        }

        // add alertentry to its respective collection. returns if time since last alert is under wait time (independent per alert). remove oldest alert if over max alert count
        private void SendNotification(string hardwareType, string title, string message, ObservableCollection<AlertEntry> alerts)
        {
            if (_lastMetricNotifications.TryGetValue(hardwareType, out DateTime lastTime))
            {
                if ((DateTime.UtcNow - lastTime).TotalSeconds < AlertWaitSeconds)
                    return;
            }

            _lastMetricNotifications[hardwareType] = DateTime.UtcNow;

            alerts.Insert(0, new AlertEntry
            {
                Time = DateTime.UtcNow.ToString("HH:mm:ss"),
                Title = title,
                Message = message,
                Color = title.Contains("Critical") ? "#d61313" : "#d47d0d"
            });

            if (alerts.Count > MaxAlerts)
                alerts.RemoveAt(alerts.Count - 1);
        }

        // load alert settings when opening the settings window. load new settings when saving settings
        private void OpenSettingsWindow(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(_appSettings.Clone());
            settingsWindow.OnSettingsSaved += () => _appSettings = AppSettings.Load();
            settingsWindow.Closed += (s, e) => _appSettings = AppSettings.Load();
            settingsWindow.Show();
        }

        // methods to clear alerts collections
        private void ClearCpuAlerts(object sender, RoutedEventArgs e) => CpuAlerts.Clear();
        private void ClearGpuAlerts(object sender, RoutedEventArgs e) => GpuAlerts.Clear();
        private void ClearRamAlerts(object sender, RoutedEventArgs e) => RamAlerts.Clear();
        private void ClearStorageAlerts(object sender, RoutedEventArgs e) => StorageAlerts.Clear();


    }
}