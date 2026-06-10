using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Diagnostics;


namespace hwmonitor
{
    public class SessionStats : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        private Stopwatch _stopwatch = Stopwatch.StartNew();
        public string SessionTime => _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");


        // Session tick count
        private int _tickCount = 0;

        // CPU Load
        private float _cpuLoadMin = float.MaxValue;
        private float _cpuLoadMax = 0;
        private float _cpuLoadSum = 0;
        public float CpuLoadMin { get => _tickCount == 0 ? 0 : _cpuLoadMin; private set { _cpuLoadMin = value; OnPropertyChanged(nameof(CpuLoadMin)); } }
        public float CpuLoadMax { get => _cpuLoadMax; private set { _cpuLoadMax = value; OnPropertyChanged(nameof(CpuLoadMax)); } }
        public float CpuLoadAvg { get => _tickCount == 0 ? 0 : _cpuLoadSum / _tickCount; }


        // CPU Temp
        private float _cpuTempMin = float.MaxValue;
        private float _cpuTempMax = 0;
        private float _cpuTempSum = 0;
        public float CpuTempMin { get => _tickCount == 0 ? 0 : _cpuTempMin; private set { _cpuTempMin = value; OnPropertyChanged(nameof(CpuTempMin)); } }
        public float CpuTempMax { get => _cpuTempMax; private set { _cpuTempMax = value; OnPropertyChanged(nameof(CpuTempMax)); } }
        public float CpuTempAvg { get => _tickCount == 0 ? 0 : _cpuTempSum / _tickCount; }


        // CPU Power
        private float _cpuPowerMin = float.MaxValue;
        private float _cpuPowerMax = 0;
        private float _cpuPowerSum = 0;
        public float CpuPowerMin { get => _tickCount == 0 ? 0 : _cpuPowerMin; private set { _cpuPowerMin = value; OnPropertyChanged(nameof(CpuPowerMin)); } }
        public float CpuPowerMax { get => _cpuPowerMax; private set { _cpuPowerMax = value; OnPropertyChanged(nameof(CpuPowerMax)); } }
        public float CpuPowerAvg { get => _tickCount == 0 ? 0 : _cpuPowerSum / _tickCount; }


        // GPU Load
        private float _gpuLoadMin = float.MaxValue;
        private float _gpuLoadMax = 0;
        private float _gpuLoadSum = 0;
        public float GpuLoadMin { get => _tickCount == 0 ? 0 : _gpuLoadMin; private set { _gpuLoadMin = value; OnPropertyChanged(nameof(GpuLoadMin)); } }
        public float GpuLoadMax { get => _gpuLoadMax; private set { _gpuLoadMax = value; OnPropertyChanged(nameof(GpuLoadMax)); } }
        public float GpuLoadAvg { get => _tickCount == 0 ? 0 : _gpuLoadSum / _tickCount; }


        // GPU Core Temp
        private float _gpuCoreTempMin = float.MaxValue;
        private float _gpuCoreTempMax = 0;
        private float _gpuCoreTempSum = 0;
        public float GpuCoreTempMin { get => _tickCount == 0 ? 0 : _gpuCoreTempMin; private set { _gpuCoreTempMin = value; OnPropertyChanged(nameof(GpuCoreTempMin)); } }
        public float GpuCoreTempMax { get => _gpuCoreTempMax; private set { _gpuCoreTempMax = value; OnPropertyChanged(nameof(GpuCoreTempMax)); } }
        public float GpuCoreTempAvg { get => _tickCount == 0 ? 0 : _gpuCoreTempSum / _tickCount; }


        // GPU Memory Temp
        private float _gpuMemoryTempMin = float.MaxValue;
        private float _gpuMemoryTempMax = 0;
        private float _gpuMemoryTempSum = 0;
        public float GpuMemoryTempMin { get => _gpuMemoryTempMin; private set { _gpuMemoryTempMin = value; OnPropertyChanged(nameof(GpuMemoryTempMin)); } }
        public float GpuMemoryTempMax { get => _gpuMemoryTempMax; private set { _gpuMemoryTempMax = value; OnPropertyChanged(nameof(GpuMemoryTempMax)); } }
        public float GpuMemoryTempAvg { get => _tickCount == 0 ? 0 : _gpuMemoryTempSum / _tickCount; }

        // GPU Hotspot Temp
        private float _gpuHotspotTempMin = float.MaxValue;
        private float _gpuHotspotTempMax = 0;
        private float _gpuHotspotTempSum = 0;
        public float GpuHotspotTempMin { get => _tickCount == 0 ? 0 : _gpuHotspotTempMin; private set { _gpuHotspotTempMin = value; OnPropertyChanged(nameof(GpuHotspotTempMin)); } }
        public float GpuHotspotTempMax { get => _gpuHotspotTempMax; private set { _gpuHotspotTempMax = value; OnPropertyChanged(nameof(GpuHotspotTempMax)); } }
        public float GpuHotspotTempAvg { get => _tickCount == 0 ? 0 : _gpuHotspotTempSum / _tickCount; }


        // GPU Power
        private float _gpuPowerMin = float.MaxValue;
        private float _gpuPowerMax = 0;
        private float _gpuPowerSum = 0;
        public float GpuPowerMin { get => _tickCount == 0 ? 0 : _gpuPowerMin; private set { _gpuPowerMin = value; OnPropertyChanged(nameof(GpuPowerMin)); } }
        public float GpuPowerMax { get => _gpuPowerMax; private set { _gpuPowerMax = value; OnPropertyChanged(nameof(GpuPowerMax)); } }
        public float GpuPowerAvg { get => _tickCount == 0 ? 0 : _gpuPowerSum / _tickCount; }

        // GPU Memory
        private float _gpuMemoryMin = float.MaxValue;
        private float _gpuMemoryMax = 0;
        private float _gpuMemorySum = 0;
        public float GpuMemoryMin { get => _tickCount == 0 ? 0 : _gpuMemoryMin; private set { _gpuMemoryMin = value; OnPropertyChanged(nameof(GpuMemoryMin)); } }
        public float GpuMemoryMax { get => _gpuMemoryMax; private set { _gpuMemoryMax = value; OnPropertyChanged(nameof(GpuMemoryMax)); } }
        public float GpuMemoryAvg { get => _tickCount == 0 ? 0 : _gpuMemorySum / _tickCount; }

        // RAM
        private float _ramMin = float.MaxValue;
        private float _ramMax = 0;
        private float _ramSum = 0;
        public float RamMin { get => _tickCount == 0 ? 0 : _ramMin; private set { _ramMin = value; OnPropertyChanged(nameof(RamMin)); } }
        public float RamMax { get => _ramMax; private set { _ramMax = value; OnPropertyChanged(nameof(RamMax)); } }
        public float RamAvg { get => _tickCount == 0 ? 0 : _ramSum / _tickCount; }

        // Storage Read Rate
        private float _storageReadMin = float.MaxValue;
        private float _storageReadMax = 0;
        private float _storageReadSum = 0;
        public float StorageReadMin { get => _tickCount == 0 ? 0 : _storageReadMin; private set { _storageReadMin = value; OnPropertyChanged(nameof(StorageReadMin)); } }
        public float StorageReadMax { get => _storageReadMax; private set { _storageReadMax = value; OnPropertyChanged(nameof(StorageReadMax)); } }
        public float StorageReadAvg { get => _tickCount == 0 ? 0 : _storageReadSum / _tickCount; }

        // Storage Write Rate
        private float _storageWriteMin = float.MaxValue;
        private float _storageWriteMax = 0;
        private float _storageWriteSum = 0;
        public float StorageWriteMin { get => _tickCount == 0 ? 0 : _storageWriteMin; private set { _storageWriteMin = value; OnPropertyChanged(nameof(StorageWriteMin)); } }
        public float StorageWriteMax { get => _storageWriteMax; private set { _storageWriteMax = value; OnPropertyChanged(nameof(StorageWriteMax)); } }
        public float StorageWriteAvg { get => _tickCount == 0 ? 0 : _storageWriteSum / _tickCount; }

        // Storage Temp
        private float _storageTempMin = float.MaxValue;
        private float _storageTempMax = 0;
        private float _storageTempSum = 0;
        public float StorageTempMin { get => _tickCount == 0 ? 0 : _storageTempMin; private set { _storageTempMin = value; OnPropertyChanged(nameof(StorageTempMin)); } }
        public float StorageTempMax { get => _storageTempMax; private set { _storageTempMax = value; OnPropertyChanged(nameof(StorageTempMax)); } }
        public float StorageTempAvg { get => _tickCount == 0 ? 0 : _storageTempSum / _tickCount; }


        public void Update(SensorData data)
        {
            // CPU Load
            if (data.CpuLoadTotal > _cpuLoadMax) CpuLoadMax = data.CpuLoadTotal;
            else if (data.CpuLoadTotal < _cpuLoadMin) CpuLoadMin = data.CpuLoadTotal;
            _cpuLoadSum += data.CpuLoadTotal;

            // CPU Power
            if (data.CpuPower > _cpuPowerMax) CpuPowerMax = data.CpuPower;
            else if (data.CpuPower < _cpuPowerMin) CpuPowerMin = data.CpuPower;
            _cpuPowerSum += data.CpuPower;

            // CPU Temp
            if (data.CpuTemp > _cpuTempMax) CpuTempMax = data.CpuTemp;
            else if (data.CpuTemp < _cpuTempMin) CpuTempMin = data.CpuTemp;
            _cpuTempSum += data.CpuTemp;

            // GPU Load
            if (data.GpuCoreLoad > _gpuLoadMax) GpuLoadMax = data.GpuCoreLoad;
            else if (data.GpuCoreLoad < _gpuLoadMin) GpuLoadMin = data.GpuCoreLoad;
            _gpuLoadSum += data.GpuCoreLoad;

            // GPU Core Temp
            if (data.GpuCoreTemp > _gpuCoreTempMax) GpuCoreTempMax = data.GpuCoreTemp;
            else if (data.GpuCoreTemp < _gpuCoreTempMin) GpuCoreTempMin = data.GpuCoreTemp;
            _gpuCoreTempSum += data.GpuCoreTemp;

            // GPU Memory Temp
            if (data.GpuMemoryTemp > _gpuMemoryTempMax) GpuMemoryTempMax = data.GpuMemoryTemp;
            else if (data.GpuMemoryTemp < _gpuMemoryTempMin) GpuMemoryTempMin = data.GpuMemoryTemp;
            _gpuMemoryTempSum += data.GpuMemoryTemp;

            // GPU Hotspot Temp
            if (data.GpuHotspotTemp > _gpuHotspotTempMax) GpuHotspotTempMax = data.GpuHotspotTemp;
            else if (data.GpuHotspotTemp < _gpuHotspotTempMin) GpuHotspotTempMin = data.GpuHotspotTemp;
            _gpuHotspotTempSum += data.GpuHotspotTemp;

            // GPU Power
            if (data.GpuPower > _gpuPowerMax) GpuPowerMax = data.GpuPower;
            else if (data.GpuPower < _gpuPowerMin) GpuPowerMin = data.GpuPower;
            _gpuPowerSum += data.GpuPower;

            // GPU Memory
            if (data.GpuMemoryPercent > _gpuMemoryMax) GpuMemoryMax = data.GpuMemoryPercent;
            else if (data.GpuMemoryPercent < _gpuMemoryMin) GpuMemoryMin = data.GpuMemoryPercent;
            _gpuMemorySum += data.GpuMemoryPercent;

            // RAM
            if (data.RamPercent > _ramMax) RamMax = data.RamPercent;
            else if (data.RamPercent < _ramMin) RamMin = data.RamPercent;
            _ramSum += data.RamPercent;

            // Storage Read Rate
            if (data.StorageReadRate > _storageReadMax) StorageReadMax = data.StorageReadRate;
            else if (data.StorageReadRate < _storageReadMin) StorageReadMin = data.StorageReadRate;
            _storageReadSum += data.StorageReadRate;

            // Storage Write Rate
            if (data.StorageWriteRate > _storageWriteMax) StorageWriteMax = data.StorageWriteRate;
            else if (data.StorageWriteRate < _storageWriteMin) StorageWriteMin = data.StorageWriteRate;
            _storageWriteSum += data.StorageWriteRate;

            // Storage Temp
            if (data.StorageCompTemp > _storageTempMax) StorageTempMax = data.StorageCompTemp;
            else if (data.StorageCompTemp < _storageTempMin) StorageTempMin = data.StorageCompTemp;
            _storageTempSum += data.StorageCompTemp;

            // notify all avg properties
            OnPropertyChanged(nameof(CpuLoadAvg));
            OnPropertyChanged(nameof(CpuPowerAvg));
            OnPropertyChanged(nameof(CpuTempAvg));
            OnPropertyChanged(nameof(GpuLoadAvg));
            OnPropertyChanged(nameof(GpuCoreTempAvg));
            OnPropertyChanged(nameof(GpuMemoryTempAvg));
            OnPropertyChanged(nameof(GpuHotspotTempAvg));
            OnPropertyChanged(nameof(GpuPowerAvg));
            OnPropertyChanged(nameof(GpuMemoryAvg));
            OnPropertyChanged(nameof(RamAvg));
            OnPropertyChanged(nameof(StorageReadAvg));
            OnPropertyChanged(nameof(StorageWriteAvg));
            OnPropertyChanged(nameof(StorageTempAvg));

            OnPropertyChanged(nameof(SessionTime));

            _tickCount += 1;
        }

    }
}
