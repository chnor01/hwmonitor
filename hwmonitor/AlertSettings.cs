using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.IO;

namespace hwmonitor
{
        public class AlertSettings
        {
            public float CpuTempWarning { get; set; } = 80;
            public float CpuTempCritical { get; set; } = 95;
            public float CpuLoadWarning { get; set; } = 80;
            public float CpuLoadCritical { get; set; } = 95;

            public float GpuTempWarning { get; set; } = 80;
            public float GpuTempCritical { get; set; } = 95;
            public float GpuLoadWarning { get; set; } = 80;
            public float GpuLoadCritical { get; set; } = 95;

            public float GpuPowerWarning { get; set; } = 250;
            public float GpuPowerCritical { get; set; } = 300;

            public float RamWarning { get; set; } = 80;
            public float RamCritical { get; set; } = 95;

        public AlertSettings Clone() => new AlertSettings
        {
            CpuTempWarning = CpuTempWarning,
            CpuTempCritical = CpuTempCritical,
            CpuLoadWarning = CpuLoadWarning,
            CpuLoadCritical = CpuLoadCritical,
            GpuTempWarning = GpuTempWarning,
            GpuTempCritical = GpuTempCritical,
            GpuLoadWarning = GpuLoadWarning,
            GpuLoadCritical = GpuLoadCritical,
            GpuPowerWarning = GpuPowerWarning,
            GpuPowerCritical = GpuPowerCritical,
            RamWarning = RamWarning,
            RamCritical = RamCritical
        };

        private static string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "alertSettings.json");
        public static AlertSettings Load() {

            if (!File.Exists(SettingsPath))
                return new AlertSettings();

        string jsonString = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AlertSettings>(jsonString) ?? new AlertSettings();
            }

        public void Save()
        {
            string jsonString = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, jsonString);
        }
        }
}
