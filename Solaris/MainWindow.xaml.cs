using MahApps.Metro.Controls.Dialogs;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Solaris
{
    public partial class MainWindow
    {
        private static long calculatedSeconds;
        private static long calculatedMinutes;
        private static long calculatedHours;
        private static long adjustedSeconds;
        private static long adjustedMinutes;
        private static long adjustedHours;
        private DispatcherTimer countDown = null!;
        private bool isTimerRunning;
        public bool isPerfStatsRunning;

        public MainWindow()
        {
            InitializeComponent();
            dropdownFilesize.ItemsSource = CalculatorFilesize;
            dropDownalreadydownloaded.ItemsSource = CalculatorAlreadyDownloaded;
            dropDownspeed.ItemsSource = CalculatorSpeed;
            dropDownshutdown.ItemsSource = ShutdownType;

            var varSize = CalculatorFilesize.FindIndex(a => a.Contains("GB"));
            dropdownFilesize.SelectedIndex = varSize;

            var varCompleted = CalculatorAlreadyDownloaded.FindIndex(a => a.Contains('%'));
            dropDownalreadydownloaded.SelectedIndex = varCompleted;

            var varSpeed = CalculatorSpeed.FindIndex(a => a.Contains("MB"));
            dropDownspeed.SelectedIndex = varSpeed;

            var varshutdownType = ShutdownType.FindIndex(a => a.Contains("Shutdown"));
            dropDownshutdown.SelectedIndex = varshutdownType;

            string timeValue = AddExtraTimeTB.Text = Properties.Settings.Default.SettingAddTimeValue;

            Window window = (Window)this;
            if (Properties.Settings.Default.SettingKeepOnTop == true)
            {
                window.Topmost = true;
            }

            if (Properties.Settings.Default.SettingTooltips == true)
            {
                SetValue(ToolTipEnabledProperty, true);
            }
            else if (Properties.Settings.Default.SettingTooltips == false)
            {
                SetValue(ToolTipEnabledProperty, false);
            }

            VersionLabel.Content += GetVersion;

            InfoFlyout.IsOpenChanged += InfoFlyoutIsOpenChanged;
            SettingsFlyout.IsOpenChanged += SettingsFlyoutIsOpenChanged;
        }

        public static readonly DependencyProperty ToolTipEnabledProperty = DependencyProperty.RegisterAttached(
            "IsToolTipEnabled",
            typeof(Boolean),
            typeof(MainWindow),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits, (sender, e) =>
            {
                if (sender is FrameworkElement element)
                {
                    element.SetValue(ToolTipService.IsEnabledProperty, e.NewValue);
                }
            }));

        private void InfoFlyoutIsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (InfoFlyout.IsOpen)
            {
                MainGrid.Visibility = Visibility.Collapsed;
                PerfStatsGrid.Visibility = Visibility.Collapsed;
                ButtonPerfStats.Visibility = Visibility.Collapsed;
                ButtonInfo.Visibility = Visibility.Collapsed;
                ButtonSettings.Visibility = Visibility.Collapsed;
                ButtonExit.Visibility = Visibility.Collapsed;
                MainInfoGrid.Visibility = Visibility.Visible;
            }
            else
            {
                MainGrid.Visibility = Visibility.Visible;
                PerfStatsGrid.Visibility = Visibility.Visible;
                ButtonPerfStats.Visibility = Visibility.Visible;
                ButtonInfo.Visibility = Visibility.Visible;
                ButtonSettings.Visibility = Visibility.Visible;
                ButtonExit.Visibility = Visibility.Visible;
                MainInfoGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void SettingsFlyoutIsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (SettingsFlyout.IsOpen)
            {
                MainGrid.Visibility = Visibility.Collapsed;
                PerfStatsGrid.Visibility = Visibility.Collapsed;
                ButtonPerfStats.Visibility = Visibility.Collapsed;
                ButtonInfo.Visibility = Visibility.Collapsed;
                ButtonSettings.Visibility = Visibility.Collapsed;
                ButtonExit.Visibility = Visibility.Collapsed;
                MainSettingsGrid.Visibility = Visibility.Visible;

                if (Properties.Settings.Default.SettingKeepOnTop == true)
                {
                    KeepOnTopCB.IsChecked = true;
                }

                if (Properties.Settings.Default.SettingTooltips == true)
                {
                    TooltipsCB.IsChecked = true;
                }

                if (Properties.Settings.Default.SettingNotifications == true)
                {
                    ShowNotifCB.IsChecked = true;
                }

                if (Properties.Settings.Default.SettingAddTime == true)
                {
                    AddExtraTimeCB.IsChecked = true;
                    labelDisableAddTime.Visibility = Visibility.Collapsed;
                }
                else
                {
                    labelDisableAddTime.Visibility = Visibility.Visible;
                }
            }
            else
            {
                MainSettingsGrid.Visibility = Visibility.Collapsed;
                PerfStatsGrid.Visibility = Visibility.Visible;
                ButtonPerfStats.Visibility = Visibility.Visible;
                ButtonInfo.Visibility = Visibility.Visible;
                ButtonSettings.Visibility = Visibility.Visible;
                ButtonExit.Visibility = Visibility.Visible;
                MainGrid.Visibility = Visibility.Visible;
            }
        }        

        internal static List<string> CalculatorFilesize
        {
            get
            {
                var sizeunit = new List<string>
                {
                    "MB",
                    "GB"
                };
                return sizeunit;
            }
        }

        internal static List<string> CalculatorAlreadyDownloaded
        {
            get
            {
                var calculationtype = new List<string>
                {
                    "%",
                    "MB"
                };
                return calculationtype;
            }
        }

        internal static List<string> CalculatorSpeed
        {
            get
            {
                var speedunit = new List<string>
                {
                    "KB/s",
                    "MB/s"
                };
                return speedunit;
            }
        }

        internal static List<string> ShutdownType
        {
            get
            {
                var shutdownType = new List<string>
                {
                    "Shutdown",
                    "Restart"
                };
                return shutdownType;
            }
        }

        private void CalculateStart_Click(object sender, RoutedEventArgs e)
        {
            int size = int.Parse(inputFilesize.Text);
            int completed = int.Parse(inputCompleted.Text);
            int speed = int.Parse(inputSpeed.Text);

            if (!size.Equals(0) && (!speed.Equals(0)))
            {
                string sizeUnit = dropdownFilesize.Text;
                string calculationType = dropDownalreadydownloaded.Text;
                string speedUnit = dropDownspeed.Text;

                long megaBytes = 1000000;
                long gigaBytes = 1000000000;
                long kilaBytesPerSecond = 1000;
                long MegaBytesPerSecond = 1000000;

                long sizeVariable = 0;
                long speedVariable = 0;

                if (sizeUnit.Contains("MB"))
                {
                    sizeVariable = megaBytes;
                }
                if (sizeUnit.Contains("GB"))
                {
                    sizeVariable = gigaBytes;
                }
                if (speedUnit.Contains("KB"))
                {
                    speedVariable = kilaBytesPerSecond;
                }
                if (speedUnit.Contains("MB"))
                {
                    speedVariable = MegaBytesPerSecond;
                }

                long sizefinal = size * sizeVariable;

                if (!completed.Equals(null))
                {
                    if (calculationType.Contains('%'))
                    {
                        if (completed > 0)
                        {
                            long result = sizefinal - (sizefinal * completed / 100);
                            sizefinal = result;
                        }
                        if (completed > 100)
                        {
                            this.ShowMessageAsync("", "Percentage cannot be above 100");
                            ETA.Content = "";
                            CompletionTime.Content = "";
                            return;
                        }
                    }

                    if (calculationType.Contains("MB"))
                    {
                        if (completed > 0)
                        {
                            long adjustedValue = completed * megaBytes;

                            if (adjustedValue >= sizefinal)
                            {
                                this.ShowMessageAsync("", "Downloaded Size cannot be more than Total File Size");
                                ETA.Content = "";
                                CompletionTime.Content = "";
                                return;
                            }

                            long results = sizefinal - adjustedValue;
                            sizefinal = results;
                        }
                    }
                }

                long speedfinal = speed * speedVariable;
                long totalSeconds = sizefinal / speedfinal;

                if (Properties.Settings.Default.SettingAddTime == true)
                {
                    long additionalMinutes = Convert.ToInt32(Properties.Settings.Default.SettingAddTimeValue);
                    long calculatedSeconds = additionalMinutes * 60;
                    long calculatedTotal = totalSeconds + calculatedSeconds;
                    adjustedHours = calculatedTotal / 3600;
                    adjustedMinutes = (calculatedTotal % 3600) / 60;
                    adjustedSeconds = (calculatedTotal % 60);
                }

                long hours = totalSeconds / 3600;
                long minutes = (totalSeconds % 3600) / 60;
                long seconds = (totalSeconds % 60);

                string eta;
                string time;

                if (hours > 0)
                {
                    eta = hours + "hr, " + minutes + "min, " + seconds + "sec";
                    calculatedHours = hours;
                    calculatedMinutes = minutes;
                    calculatedSeconds = seconds;
                }
                else if (minutes > 0)
                {
                    eta = minutes + "min, " + seconds + "sec";
                    calculatedHours = 0;
                    calculatedMinutes = minutes;
                    calculatedSeconds = seconds;
                }
                else
                {
                    eta = seconds + "sec";
                    calculatedHours = 0;
                    calculatedMinutes = 0;
                    calculatedSeconds = seconds;
                }

                ETA.Content = eta;
                time = DateTime.Now.AddSeconds(totalSeconds).ToString("h:mm:ss tt - dd MMM");
                CompletionTime.Content = time;
            }
            else
            {
                ETA.Content = "";
                CompletionTime.Content = "";
            }
        }

        private void CalculateReset_Click(object sender, RoutedEventArgs e)
        {
            string adjusted = "0";
            inputFilesize.Text = adjusted;
            inputCompleted.Text = adjusted;
            inputSpeed.Text = adjusted;

            calculatedHours = 0;
            calculatedMinutes = 0;
            calculatedSeconds = 0;

            ETA.Content = "";
            CompletionTime.Content = "";
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBoxSender = (TextBox)sender;

            if (!System.Text.RegularExpressions.Regex.IsMatch(textBoxSender.Text, "^[0-9]*$"))
            {
                if (textBoxSender.Text.Equals("0"))
                {
                    return;
                }
                textBoxSender.Text = "0";
                textBoxSender.SelectionStart = textBoxSender.Text.Length;
            }
        }

        private void Calculator_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var senderTextBox = (TextBox)sender;
            string maxTextBoxNumber = "";

            if (senderTextBox.Name.Contains("inputFilesize"))
            {
                senderTextBox = inputFilesize;
                maxTextBoxNumber = "999";
            }
            if (senderTextBox.Name.Contains("inputCompleted"))
            {
                maxTextBoxNumber = "99";
            }
            if (senderTextBox.Name.Contains("inputSpeed"))
            {
                maxTextBoxNumber = "999";
            }

            if (e.Delta > 0)
            {
                // The user scrolled up.
                if (senderTextBox.Text.Equals(maxTextBoxNumber))
                {
                    string final = "0";
                    senderTextBox.Text = final.ToString();
                    return;
                }
                int senderTimer = int.Parse((string)senderTextBox.Text);
                int adjustedsenderTimer = senderTimer + 1;
                senderTextBox.Text = adjustedsenderTimer.ToString();
            }
            else
            {
                // The user scrolled down.
                if (senderTextBox.Text.Equals("0"))
                {
                    string final = maxTextBoxNumber;
                    senderTextBox.Text = final.ToString();
                    return;
                }
                int senderTimer = int.Parse((string)senderTextBox.Text);
                int adjustedsenderTimer = senderTimer - 1;
                senderTextBox.Text = adjustedsenderTimer.ToString();
            }
        }

        private void ETA_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isTimerRunning)
            {
                return;
            }

            if (ETA.Content.ToString() == "")
            {
                return;
            }

            long selectedHours;
            long selectedMinutes;
            long selectedSeconds;
            string message;

            if (Properties.Settings.Default.SettingAddTime == true)
            {
                selectedHours = adjustedHours;
                selectedMinutes = adjustedMinutes;
                selectedSeconds = adjustedSeconds;
                message = "(Additional +" + Properties.Settings.Default.SettingAddTimeValue + " mins added to imported time)";
            }
            else
            {
                selectedHours = calculatedHours;
                selectedMinutes = calculatedMinutes;
                selectedSeconds = calculatedSeconds;
                message = "";
            }

            if (!selectedHours.Equals(null))
            {
                if (selectedHours < 24)
                {
                    if (selectedHours.ToString().Length.Equals(2))
                    {
                        TimerHours.Content = selectedHours.ToString();
                    }
                    else if (selectedHours.ToString().Length.Equals(1))
                    {
                        string adjusted = "0" + selectedHours;
                        TimerHours.Content = adjusted.ToString();
                    }
                }
                else
                {
                    this.ShowMessageAsync("", "Shutdown Timer cannot be set for longer than 24 hours");
                    ETA.Content = "";
                    CompletionTime.Content = "";
                    return;
                }
            }
            if (!selectedMinutes.Equals(null))
            {
                if (selectedMinutes.ToString().Length.Equals(2))
                {
                    TimerMinutes.Content = selectedMinutes.ToString();
                }
                else
                {
                    string adjusted = "0" + selectedMinutes;
                    TimerMinutes.Content = adjusted.ToString();
                }
            }
            if (!selectedSeconds.Equals(null))
            {
                if (selectedSeconds.ToString().Length.Equals(2))
                {
                    TimerSeconds.Content = selectedSeconds.ToString();
                }
                else
                {
                    string adjusted = "0" + selectedSeconds;
                    TimerSeconds.Content = adjusted.ToString();
                }
            }
            AdditionalMessage.Content = message;
        }

        private void TimerStartStop_Click(object sender, RoutedEventArgs e)
        {
            int hours = Convert.ToInt32(TimerHours.Content);
            int minutes = Convert.ToInt32(TimerMinutes.Content);
            int seconds = Convert.ToInt32(TimerSeconds.Content);
            int totalSeconds = (hours * 3600) + (minutes * 60) + seconds;
            TimeSpan runningTime;
            runningTime = TimeSpan.FromSeconds(totalSeconds);

            if (totalSeconds == 0)
            {
                return;
            }

            if (isTimerRunning)
            {
                countDown.Stop();
                isTimerRunning = false;
                TimerStartStop.Foreground = Brushes.White;
                StartStopIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
                Indicator1.Visibility = Visibility.Collapsed;
                Indicator2.Visibility = Visibility.Collapsed;
                Indicator3.Visibility = Visibility.Collapsed;
                labelDisableButtons.Visibility = Visibility.Collapsed;

                if (Properties.Settings.Default.SettingNotifications == true)
                {
                    new ToastContentBuilder()
                            .AddArgument("action", "viewConversation")
                            .AddArgument("conversationId", 9813)
                            .AddText("Shutdown Timer Stopped")
                            .Show();
                }

                return;
            }

            countDown = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                TimerStartStop.Foreground = Brushes.Red;
                StartStopIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
                Indicator1.Visibility = Visibility.Visible;
                Indicator2.Visibility = Visibility.Visible;
                Indicator3.Visibility = Visibility.Visible;
                labelDisableButtons.Visibility = Visibility.Visible;

                string totalTime = runningTime.ToString(@"hh\:mm\:ss");
                string[] totalTimeSplit = totalTime.Split(':');
                string hoursLeft = totalTimeSplit[0];
                string minutesLeft = totalTimeSplit[1];
                string secondsLeft = totalTimeSplit[2];

                if (hoursLeft.Length.Equals(1))
                {
                    hoursLeft = "0" + hoursLeft;
                }
                if (minutesLeft.Length.Equals(1))
                {
                    minutesLeft = "0" + minutesLeft;
                }
                if (secondsLeft.Length.Equals(1))
                {
                    secondsLeft = "0" + secondsLeft;
                }

                TimerHours.Content = hoursLeft;
                TimerMinutes.Content = minutesLeft;
                TimerSeconds.Content = secondsLeft;

                if (runningTime == TimeSpan.Zero)
                {
                    countDown.Stop();
                    isTimerRunning = false;

                    if (dropDownshutdown.Text == "Shutdown")
                    {
                        string command = "/s";
                        ExecuteShutdown(command);
                    }
                    if (dropDownshutdown.Text == "Restart")
                    {
                        string command = "/r";
                        ExecuteShutdown(command);
                    }
                }
                runningTime = runningTime.Add(TimeSpan.FromSeconds(-1));
            }, Application.Current.Dispatcher);

            countDown.Start();
            isTimerRunning = true;

            if (Properties.Settings.Default.SettingNotifications == true)
            {
                new ToastContentBuilder()
                   .AddArgument("action", "viewConversation")
                   .AddArgument("conversationId", 9813)
                   .AddText("Shutdown Timer Started")
                   .Show();
            }
        }

        private static void ExecuteShutdown(string command)
        {
            Process process = new();
            ProcessStartInfo startInfo = new()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = "/C shutdown " + command + " /t 1"
            };
            process.StartInfo = startInfo;
            process.Start();
        }

        private void TimerDelete_Click(object sender, RoutedEventArgs e)
        {
            if (isTimerRunning)
            {
                return;
            }

            string adjusted = "00";
            TimerHours.Content = adjusted;
            TimerMinutes.Content = adjusted;
            TimerSeconds.Content = adjusted;
            AdditionalMessage.Content = "";
        }

        private void PredefinedTimes_Click(object sender, RoutedEventArgs e)
        {
            string adjusted = "00";
            TimerHours.Content = adjusted;
            TimerMinutes.Content = adjusted;
            TimerSeconds.Content = adjusted;
            var buttonSender = (Button)sender;

            if (buttonSender.Content.Equals("60"))
            {
                TimerHours.Content = "01";
            }
            else
            {
                TimerMinutes.Content = buttonSender.Content;
            }
        }

        private void Timer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (isTimerRunning)
            {
                return;
            }

            var senderLabel = (Label)sender;
            string maxLabelNumber = "";

            if (senderLabel.Name.Contains("Hours"))
            {
                senderLabel = TimerHours;
                maxLabelNumber = "23";
            }
            if (senderLabel.Name.Contains("Minutes"))
            {
                senderLabel = TimerMinutes;
                maxLabelNumber = "59";
            }
            if (senderLabel.Name.Contains("Seconds"))
            {
                senderLabel = TimerSeconds;
                maxLabelNumber = "59";
            }

            if (e.Delta > 0)
            {
                // The user scrolled up.
                if (senderLabel.Content.Equals(maxLabelNumber))
                {
                    string final = "00";
                    senderLabel.Content = final.ToString();
                    return;
                }

                int senderTimer = int.Parse((string)senderLabel.Content);
                int adjustedsenderTimer = senderTimer + 1;
                if (adjustedsenderTimer.ToString().Length.Equals(1))
                {
                    string final = "0" + adjustedsenderTimer;
                    senderLabel.Content = final.ToString();
                }
                else
                {
                    senderLabel.Content = adjustedsenderTimer.ToString();
                }
            }
            else
            {
                // The user scrolled down.
                if (senderLabel.Content.Equals("00"))
                {
                    string final = maxLabelNumber;
                    senderLabel.Content = final.ToString();
                    return;
                }

                int senderTimer = int.Parse((string)senderLabel.Content);
                int adjustedsenderTimer = senderTimer - 1;
                if (adjustedsenderTimer.ToString().Length.Equals(1))
                {
                    string final = "0" + adjustedsenderTimer;
                    senderLabel.Content = final.ToString();
                }
                else
                {
                    senderLabel.Content = adjustedsenderTimer.ToString();
                }
            }
        }

        private void ButtonInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoFlyout.IsOpen = true;
        }

        private void ButtonCloseInfo_Click(object sender, RoutedEventArgs e)
        {
            InfoFlyout.IsOpen = false;
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = true;
        }

        private void ButtonCloseSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = false;
            string timeValue = AddExtraTimeTB.Text;
            Properties.Settings.Default.SettingAddTimeValue = timeValue;
            Properties.Settings.Default.Save();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void KeepOnTopCB_Click(object sender, RoutedEventArgs e)
        {
            Window window = (Window)this;

            if (KeepOnTopCB.IsChecked == true)
            {
                Properties.Settings.Default.SettingKeepOnTop = true;
                Properties.Settings.Default.Save();
                window.Topmost = true;
            }
            else
            {
                Properties.Settings.Default.SettingKeepOnTop = false;
                Properties.Settings.Default.Save();
                window.Topmost = false;
            }
        }

        private void TooltipsCB_Click(object sender, RoutedEventArgs e)
        {
            if (TooltipsCB.IsChecked == true)
            {
                Properties.Settings.Default.SettingTooltips = true;
                Properties.Settings.Default.Save();
                SetValue(ToolTipEnabledProperty, true);
            }
            else
            {
                Properties.Settings.Default.SettingTooltips = false;
                Properties.Settings.Default.Save();
                SetValue(ToolTipEnabledProperty, false);
            }
        }

        private void ShowNotifCB_Click(object sender, RoutedEventArgs e)
        {
            if (ShowNotifCB.IsChecked == true)
            {
                Properties.Settings.Default.SettingNotifications = true;
                Properties.Settings.Default.Save();
            }
            else
            {
                Properties.Settings.Default.SettingNotifications = false;
                Properties.Settings.Default.Save();
            }
        }

        private void AddExtraTimeCB_Click(object sender, RoutedEventArgs e)
        {
            if (AddExtraTimeCB.IsChecked == true)
            {
                Properties.Settings.Default.SettingAddTime = true;
                Properties.Settings.Default.Save();
                labelDisableAddTime.Visibility = Visibility.Collapsed;
            }
            else
            {
                Properties.Settings.Default.SettingAddTime = false;
                Properties.Settings.Default.Save();
                labelDisableAddTime.Visibility = Visibility.Visible;
            }
        }

        public static string GetVersion => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString();

        private void ButtonPerfStats_Click(object sender, RoutedEventArgs e)
        {
            if (PerfStatsFlyout.IsOpen)
            {
                try
                {
                    PerformanceStats.ThisPc?.Close();
                }
                catch { }

                PerfStatsFlyout.IsOpen = false;
                Application.Current.MainWindow.Height = 481;
                PerfStatsGrid.Visibility = Visibility.Collapsed;
                isPerfStatsRunning = false;
                Indicator3.Margin = new Thickness(2, 474, -3, 38);
            }
            else
            {
                Application.Current.MainWindow.Height = 530;
                PerfStatsGrid.Visibility = Visibility.Visible;
                PerfStatsFlyout.IsOpen = true;
                isPerfStatsRunning = true;
                Indicator3.Margin = new Thickness(0, 523, -1, -10);
                StartPerfStats();
            }
        }

        private static async void StartPerfStats()
        {
            await PutTaskDelay();
            PerformanceStats.StartHardwareMonitor();
        }

        private static async Task PutTaskDelay()
        {
            await Task.Delay(500);
        }
    }
}