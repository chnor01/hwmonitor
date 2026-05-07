using LibreHardwareMonitor.Hardware;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace hwmonitor
{
    public partial class MainWindow : Window
    {
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
        public ISeries[] CpuLoadSeries { get; set; }
        public ISeries[] CpuTempPowerSeries { get; set; }
        public ISeries[] GpuLoadSeries { get; set; }
        public ISeries[] GpuTempSeries { get; set; }
        public ISeries[] GpuPowerSeries { get; set; }
        public ISeries[] RamSeries { get; set; }
        public Axis[] XAxes { get; set; } = new Axis[]
        {
            new Axis { MinLimit = 0, MaxLimit = 60, Labeler = value => $"{value}s", ForceStepToMin = true, MinStep = 10 }
        };
        public Axis[] YAxes0to100Percent { get; set; } = new Axis[]
        {
            new Axis { MinLimit = 0, MaxLimit = 100, Labeler = value => $"{value}%", ForceStepToMin = true, MinStep = 25 }
        };
        public Axis[] YAxes0to150Temp { get; set; } = new Axis[]
        {
            new Axis { MinLimit = 0, MaxLimit = 150, Labeler = value => $"{value}°C", ForceStepToMin = true, MinStep = 50 }
        };
        public Axis[] YAxes0to300Watts { get; set; } = new Axis[]
        {
            new Axis { MinLimit = 0, MaxLimit = 300, Labeler = value => $"{value}W", ForceStepToMin = true, MinStep = 100 }
        };


        private DispatcherTimer _timer;
        private const int MaxPoints = 60;

        Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
        };


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            CpuLoadSeries = new ISeries[]
            { 
                new LineSeries<float> { Values = _cpuLoadTotal, Name = "CPU Total", Fill = new SolidColorPaint(SKColors.Crimson.WithAlpha(120)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.Crimson) { StrokeThickness = 2 }, LineSmoothness = 1 }
            };

            CpuTempPowerSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _cpuTemp, Name = "CPU Temp °C", Fill = new SolidColorPaint(SKColors.Crimson.WithAlpha(120)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.Crimson) { StrokeThickness = 2 }, LineSmoothness = 1 },
                new LineSeries<float> { Values = _cpuPower, Name = "CPU Power W", Fill = new SolidColorPaint(SKColors.Crimson.WithAlpha(120)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.Crimson) { StrokeThickness = 2 }, LineSmoothness = 1 },
            };

            GpuLoadSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _gpuCoreLoad, Name = "GPU Core Load %", Fill = new SolidColorPaint(SKColors.Crimson.WithAlpha(120)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.Crimson) { StrokeThickness = 2 }, LineSmoothness = 1 }
            };

            GpuTempSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _gpuCoreTemp,    Name = "GPU Core °C",    Fill = new SolidColorPaint(SKColors.Crimson.WithAlpha(150)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.Crimson) { StrokeThickness = 2 }, LineSmoothness = 1  },
                new LineSeries<float> { Values = _gpuMemoryTemp,  Name = "GPU Memory °C",  Fill = new SolidColorPaint(SKColors.Crimson.WithAlpha(120)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.Crimson) { StrokeThickness = 2 }, LineSmoothness = 1  },
                new LineSeries<float> { Values = _gpuHotspotTemp, Name = "GPU Hotspot °C", Fill = new SolidColorPaint(SKColors.Crimson.WithAlpha(90)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.Crimson) { StrokeThickness = 2 }, LineSmoothness = 1  }
            };
            
            GpuPowerSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _gpuPower, Name = "GPU Power W", Fill = new SolidColorPaint(SKColors.Crimson.WithAlpha(120)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.Crimson) { StrokeThickness = 2 }, LineSmoothness = 1  }
            };
            
            RamSeries = new ISeries[]
            {
                new LineSeries<float> { Values = _ramUsedPercentage, Name = "RAM %", Fill = new SolidColorPaint(SKColors.Crimson.WithAlpha(120)), GeometrySize = 0, Stroke = new SolidColorPaint(SKColors.Crimson) { StrokeThickness = 2 }, LineSmoothness = 1 }
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
                if (hardware.Name.Contains("Virtual") || hardware.Name.Contains("Graphics"))
                    continue;

                hardware.Update();


                if (hardware.HardwareType == HardwareType.Cpu)
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (!sensor.Value.HasValue) continue;

                        switch (sensor.Name)
                        {
                            case "CPU Total": addPoint(_cpuLoadTotal, sensor.Value.Value); break;
                            case "Core (Tctl/Tdie)": addPoint(_cpuTemp, sensor.Value.Value); break;
                            case "Package": addPoint(_cpuPower, sensor.Value.Value); break;
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
                                    addPoint(_gpuCoreLoad, sensor.Value.Value);
                                if (sensor.SensorType == SensorType.Temperature)
                                    addPoint(_gpuCoreTemp, sensor.Value.Value);
                                break;
                            case "GPU Memory":
                                if (sensor.SensorType == SensorType.Temperature)
                                    addPoint(_gpuMemoryTemp, sensor.Value.Value);
                                break;
                            case "GPU Hot Spot": addPoint(_gpuHotspotTemp, sensor.Value.Value); break;
                            case "GPU Package": addPoint(_gpuPower, sensor.Value.Value); break;
                            case "GPU Memory Used": addPoint(_gpuMemoryUsed, sensor.Value.Value); break;
                        }
                    }

                else if (hardware.HardwareType == HardwareType.Memory)
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (!sensor.Value.HasValue) continue;

                        switch (sensor.Name)
                        {
                            case "Memory": addPoint(_ramUsedPercentage, sensor.Value.Value); break;

                        }
                    }
            }
        }
        private void addPoint(ObservableCollection<float> collection, float value)
        {
            collection.Add(value);
            if (collection.Count > MaxPoints)
                collection.Clear();
        }
    }
}