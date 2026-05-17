using System;
using System.Collections.Generic;
using System.Text;

namespace hwmonitor
{
    public class SensorData
    {
        public float CpuLoadTotal { get; set; }
        public float CpuTemp { get; set; }
        public float CpuPower { get; set; }

        public float GpuCoreLoad { get; set; }
        public float GpuCoreTemp { get; set; }
        public float GpuMemoryTemp { get; set; }
        public float GpuHotspotTemp { get; set; }
        public float GpuPower { get; set; }
        public float GpuMemoryUsedMB { get; set; }
        public float GpuMemoryTotalMB { get; set; }
        public float GpuMemoryPercent { get; set; }

        public float RamUsedGB { get; set; }
        public float RamAvailGB { get; set; }
        public float RamPercent { get; set; }


    }
}
