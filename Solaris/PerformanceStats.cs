using LibreHardwareMonitor.Hardware;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Solaris
{
    internal class PerformanceStats
    {
        public static Computer? ThisPc;
        private static string _cpuLoad = "0 %";
        private static string _netUpload = "0 KB/s";
        private static string _netDownload = "0 KB/s";
        private const int Delay = 500;
        private static readonly MainWindow MainWindow = (MainWindow)Application.Current.MainWindow;

        public static void StartHardwareMonitor()
        {
            int roundValue;

            ThisPc = new Computer
            {
                IsCpuEnabled = true,
                IsNetworkEnabled = true
            };
            ThisPc.Open();

            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    foreach (IHardware hardware in ThisPc.Hardware)
                    {
                        hardware.Update();

                        if (hardware.Sensors.Length > 0)
                        {
                            foreach (ISensor sensor in hardware.Sensors)
                            {
                                if (sensor.SensorType == SensorType.Load && sensor.Name.Equals("CPU Total", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (sensor.Value != null)
                                    {
                                        roundValue = (int)Math.Round(sensor.Value.GetValueOrDefault());
                                        _cpuLoad = roundValue + " %";
                                    }
                                    else
                                    {
                                        _cpuLoad = "0 %";
                                    }
                                }

                                if (sensor.SensorType == SensorType.Throughput && sensor.Name.Equals("Upload Speed", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (sensor.Value != null)
                                    {
                                        roundValue = (int)Math.Round(sensor.Value.GetValueOrDefault());
                                        if (roundValue >= 1000000000.0)
                                        {
                                            _netUpload = string.Format("{0:##.##}", roundValue / 1000000000.0) + " GB/s";
                                        }
                                        else if (roundValue >= 1000000.0)
                                        {
                                            _netUpload = string.Format("{0:##.##}", roundValue / 1000000.0) + " MB/s";
                                        }
                                        else if (roundValue >= 1000.0)
                                        {
                                            _netUpload = string.Format("{0:##.##}", roundValue / 1000) + " KB/s";
                                        }
                                        else if (roundValue > 0 && roundValue < 1000.0)
                                        {
                                            _netUpload = "0 KB/s";
                                        }
                                    }
                                    else
                                    {
                                        _netUpload = "0 KB/s";
                                    }
                                }
                                if (sensor.SensorType == SensorType.Throughput && sensor.Name.Equals("Download Speed", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (sensor.Value != null)
                                    {
                                        roundValue = (int)Math.Round(sensor.Value.GetValueOrDefault());
                                        if (roundValue >= 1000000000.0)
                                        {
                                            _netDownload = string.Format("{0:##.##}", roundValue / 1000000000.0) + " GB/s";
                                        }
                                        else if (roundValue >= 1000000.0)
                                        {
                                            _netDownload = string.Format("{0:##.##}", roundValue / 1000000.0) + " MB/s";
                                        }
                                        else if (roundValue >= 1000.0)
                                        {
                                            _netDownload = string.Format("{0:##.##}", roundValue / 1000) + " KB/s";
                                        }
                                        else if (roundValue > 0 && roundValue < 1000.0)
                                        {
                                            _netDownload = "0 KB/s";
                                        }
                                    }
                                    else
                                    {
                                        _netDownload = "0 KB/s";
                                    }
                                }
                            }
                        }
                    }
                    await Task.Delay(Delay).ConfigureAwait(false);
                    UpdateHardwareData();
                    await Task.Delay(Delay).ConfigureAwait(false);
                }
            });
        }

        private static void UpdateHardwareData()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                UpdateHardwareData(_cpuLoad, _netUpload, _netDownload);
            });
        }

        public static void UpdateHardwareData(string cpuLoad, string netUpload, string netDownload)
        {
            MainWindow.CPUusage.Content = cpuLoad;
            MainWindow.UploadSpeed.Content = netUpload;
            MainWindow.DownloadSpeed.Content = netDownload;
        }
    }
}