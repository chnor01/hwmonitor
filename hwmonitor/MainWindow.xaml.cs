using LibreHardwareMonitor.Hardware;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;


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

        private SKColor CpuColor = SKColors.DeepSkyBlue;
        private SKColor GpuColor = SKColors.MediumPurple;
        private SKColor RamColor = SKColors.MediumSpringGreen;
        private SKColor StorageColor = SKColors.Orange;

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

        private Dictionary<string, DateTime> _lastMetricNotifications = new Dictionary<string, DateTime>();

        private CancellationTokenSource _cts = new CancellationTokenSource();

        // max points per chart
        private const int MaxPoints = 60;

        // max amount of alerts per hardware type
        private const int MaxAlerts = 20;

        // instance to fetch the metrics
        Computer _computer = new Computer
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
            InitializeChartSeries();
            DataContext = this;

            _appSettings = AppSettings.Load();
            _computer.Open();
            _hardwareInfo = HardwareInfo.Load(_computer);
            Task.Run(() => MetricsLoop(_cts.Token));

        }

        // create a line chart
        private static ISeries[] CreateLineSeries(ObservableCollection<float> values, string name, SKColor color, string tooltipUnit)
        {
            return new ISeries[]
            {
                new LineSeries<float>
                {
                    Values = values,
                    Name = name,
                    Fill = new SolidColorPaint(color.WithAlpha(75)),
                    Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
                    GeometrySize = 0,
                    LineSmoothness = 1,
                    YToolTipLabelFormatter = p => $"{p.Model:F1}{tooltipUnit}",
                }
            };
        }

        // create line charts for all metrics
        private void InitializeChartSeries()
        {
            CpuLoadSeries = CreateLineSeries(_cpuLoadTotal, "CPU Load", CpuColor, "%");
            CpuTempSeries = CreateLineSeries(_cpuTemp, "CPU Temp", CpuColor, "°C");
            CpuPowerSeries = CreateLineSeries(_cpuPower, "CPU Power", CpuColor, "W");

            GpuLoadSeries = CreateLineSeries(_gpuCoreLoad, "GPU Core Load", GpuColor, "%");
            GpuCoreTempSeries = CreateLineSeries(_gpuCoreTemp, "GPU Core Temp", GpuColor, "°C");
            GpuMemoryTempSeries = CreateLineSeries(_gpuMemoryTemp, "GPU Memory Temp", GpuColor, "°C");
            GpuHotspotTempSeries = CreateLineSeries(_gpuHotspotTemp, "GPU Hotspot Temp", GpuColor, "°C");
            GpuPowerSeries = CreateLineSeries(_gpuPower, "GPU Power", GpuColor, "W");
            GpuMemoryUsageSeries = CreateLineSeries(_gpuMemoryUsed, "GPU Memory", GpuColor, "%");

            RamSeries = CreateLineSeries(_ramUsedPercentage, "RAM Usage", RamColor, "%");

            StorageCompTempSeries = CreateLineSeries(_storageCompTemp, "Storage Composite Temp", StorageColor, "°C");
            StorageReadRateSeries = CreateLineSeries(_storageReadRate, "Storage Read Rate", StorageColor, "MB/s");
            StorageWriteRateSeries = CreateLineSeries(_storageWriteRate, "Storage Write Rate", StorageColor, "MB/s");
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
            foreach (IHardware hardware in _computer.Hardware)
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
        private void AddPoint(ObservableCollection<float> collection, float value)
        {
            collection.Add(value);
            if (collection.Count > MaxPoints)
                collection.RemoveAt(0);
        }

        // add all fetched sensordata to their respective collection and update the metrics UI text strings
        private void UpdateUI(SensorData data)
        {
            AddPoint(_cpuLoadTotal, data.CpuLoadTotal);
            AddPoint(_cpuTemp, data.CpuTemp);
            AddPoint(_cpuPower, data.CpuPower);
            CpuLoadText = $"{data.CpuLoadTotal:F1}%";
            CpuTempText = $"{data.CpuTemp:F1}°C";
            CpuPowerText = $"{data.CpuPower:F1}W";

            AddPoint(_gpuCoreLoad, data.GpuCoreLoad);
            AddPoint(_gpuCoreTemp, data.GpuCoreTemp);
            AddPoint(_gpuMemoryTemp, data.GpuMemoryTemp);
            AddPoint(_gpuHotspotTemp, data.GpuHotspotTemp);
            AddPoint(_gpuPower, data.GpuPower);
            AddPoint(_gpuMemoryUsed, data.GpuMemoryPercent);
            GpuCoreLoadText = $"{data.GpuCoreLoad:F1}%";
            GpuCoreTempText = $"{data.GpuCoreTemp:F1}°C";
            GpuMemoryTempText = $"{data.GpuMemoryTemp:F1}°C";
            GpuHotspotTempText = $"{data.GpuHotspotTemp:F1}°C";
            GpuPowerText = $"{data.GpuPower:F1}W";
            GpuMemoryUsedGBText = $"{data.GpuMemoryUsedMB / 1024:F2}/{data.GpuMemoryTotalMB / 1024:F0}GB";

            AddPoint(_ramUsedPercentage, data.RamPercent);
            RamUsedGBText = $"{data.RamUsedGB:F1}/{data.RamUsedGB + data.RamAvailGB:F0}GB";

            AddPoint(_storageCompTemp, data.StorageCompTemp);
            AddPoint(_storageReadRate, data.StorageReadRate);
            AddPoint(_storageWriteRate, data.StorageWriteRate);
            StorageCompTempText = $"{data.StorageCompTemp:F1}°C";
            StorageReadRateText = $"{data.StorageReadRate:F1}MB/s";
            StorageWriteRateText = $"{data.StorageWriteRate:F1}MB/s";

            _sessionStats.Update(data);
            CheckAlerts(data);


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
                    SendNotification("Storage", "Critical", $"Storage temperature is {data.StorageCompTemp:F1}°C", StorageAlerts);
                else if (data.StorageCompTemp >= _appSettings.StorageTempWarning)
                    SendNotification("Storage", "Warning", $"Storage temperature is {data.StorageCompTemp:F1}°C", StorageAlerts);
            }
        }


        // cleanup when closing mainwindow
        protected override void OnClosed(EventArgs e)
        {
            _cts.Cancel();
            _computer.Close();
            base.OnClosed(e);
        }

        // add alertentry to its respective collection. returns if time since last alert is under wait time (independent per alert). remove oldest alert if over max alert count
        private void SendNotification(string hardwareType, string title, string message, ObservableCollection<AlertEntry> alerts)
        {
            if (_lastMetricNotifications.TryGetValue(hardwareType, out DateTime lastTime))
            {
                if ((DateTime.UtcNow - lastTime).TotalSeconds < _appSettings.AlertCooldownSeconds)
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

        // methods to reset session stats for each hardware type
        private void ResetCpuStats(object sender, RoutedEventArgs e) => _sessionStats.ResetCpuStats();
        private void ResetGpuStats(object sender, RoutedEventArgs e) => _sessionStats.ResetGpuStats();
        private void ResetRamStats(object sender, RoutedEventArgs e) => _sessionStats.ResetRamStats();
        private void ResetStorageStats(object sender, RoutedEventArgs e) => _sessionStats.ResetStorageStats();


    }
}