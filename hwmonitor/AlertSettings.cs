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
            public float CpuTempCritical { get; set; } = 85;
            public float CpuLoadWarning { get; set; } = 90;
            public float CpuLoadCritical { get; set; } = 95;
            public float CpuPowerWarning { get; set; } = 105;
            public float CpuPowerCritical { get; set; } = 115;

            public float GpuTempWarning { get; set; } = 80;
            public float GpuTempCritical { get; set; } = 95;
            public float GpuLoadWarning { get; set; } = 80;
            public float GpuLoadCritical { get; set; } = 95;
            public float GpuPowerWarning { get; set; } = 170;
            public float GpuPowerCritical { get; set; } = 190;

            public float RamWarning { get; set; } = 85;
            public float RamCritical { get; set; } = 95;

            public float StorageTempWarning { get; set; } = 83;
            public float StorageTempCritical { get; set; } = 88;

            public bool CpuTempAlertEnabled { get; set; } = true;
            public bool CpuLoadAlertEnabled { get; set; } = true;
            public bool CpuPowerAlertEnabled { get; set; } = true;

            public bool GpuTempAlertEnabled { get; set; } = true;
            public bool GpuLoadAlertEnabled { get; set; } = true;
            public bool GpuPowerAlertEnabled { get; set; } = true;

            public bool RamAlertEnabled { get; set; } = true;

            public bool StorageTempAlertEnabled { get; set; } = true;

        public AlertSettings Clone() => new AlertSettings
        {
            CpuTempWarning = CpuTempWarning,
            CpuTempCritical = CpuTempCritical,
            CpuLoadWarning = CpuLoadWarning,
            CpuLoadCritical = CpuLoadCritical,
            CpuPowerWarning = CpuPowerWarning,
            CpuPowerCritical = CpuPowerCritical,
            GpuTempWarning = GpuTempWarning,
            GpuTempCritical = GpuTempCritical,
            GpuLoadWarning = GpuLoadWarning,
            GpuLoadCritical = GpuLoadCritical,
            GpuPowerWarning = GpuPowerWarning,
            GpuPowerCritical = GpuPowerCritical,
            RamWarning = RamWarning,
            RamCritical = RamCritical,
            StorageTempWarning = StorageTempWarning,
            StorageTempCritical = StorageTempCritical,
            CpuTempAlertEnabled = CpuTempAlertEnabled,
            CpuLoadAlertEnabled = CpuLoadAlertEnabled,
            CpuPowerAlertEnabled = CpuPowerAlertEnabled,
            GpuTempAlertEnabled = GpuTempAlertEnabled,
            GpuLoadAlertEnabled = GpuLoadAlertEnabled,
            GpuPowerAlertEnabled = GpuPowerAlertEnabled,
            RamAlertEnabled = RamAlertEnabled,
            StorageTempAlertEnabled = StorageTempAlertEnabled,
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
