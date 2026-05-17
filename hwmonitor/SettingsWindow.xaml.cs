using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MediaColor = System.Windows.Media.Color;


namespace hwmonitor
{
    public partial class SettingsWindow : Window
    {
        private AlertSettings _settings;
        public AlertSettings Settings => _settings;


        public SettingsWindow(AlertSettings settings)
        {
            InitializeComponent();
            _settings = settings;
            DataContext = _settings;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            _settings.Save();
            SaveButton.Content = "Saved!";
            await Task.Delay(1500);
            SaveButton.Content = "Save";

        }
        private async void Undo_Click(object sender, RoutedEventArgs e)
        {
            _settings = AlertSettings.Load();
            CancelButton.Content = "Undone!";
            await Task.Delay(1500);
            CancelButton.Content = "Undo";
        }

    }
}
