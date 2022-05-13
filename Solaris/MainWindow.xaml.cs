using MahApps.Metro.Controls.Dialogs;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private DispatcherTimer countDown = null!;
        private bool isTimerRunning;

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

        private void ETA_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (isTimerRunning)
            {
                return;
            }

            if (!calculatedHours.Equals(null))
            {
                if (calculatedHours < 24)
                {
                    if (calculatedHours.ToString().Length.Equals(2))
                    {
                        TimerHours.Content = calculatedHours.ToString();
                    }
                    else if (calculatedHours.ToString().Length.Equals(1))
                    {
                        string adjusted = "0" + calculatedHours;
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
            if (!calculatedMinutes.Equals(null))
            {
                if (calculatedMinutes.ToString().Length.Equals(2))
                {
                    TimerMinutes.Content = calculatedMinutes.ToString();
                }
                else
                {
                    string adjusted = "0" + calculatedMinutes;
                    TimerMinutes.Content = adjusted.ToString();
                }
            }
            if (!calculatedSeconds.Equals(null))
            {
                if (calculatedSeconds.ToString().Length.Equals(2))
                {
                    TimerSeconds.Content = calculatedSeconds.ToString();
                }
                else
                {
                    string adjusted = "0" + calculatedSeconds;
                    TimerSeconds.Content = adjusted.ToString();
                }
            }
        }

        private void TimerStartStop_Click(object sender, RoutedEventArgs e)
        {
            TimerHours.Focus();

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
                TimerStartStop.Content = "Start";
                Indicator1.Visibility = Visibility.Collapsed;
                Indicator2.Visibility = Visibility.Collapsed;
                Indicator3.Visibility = Visibility.Collapsed;

                new ToastContentBuilder()
                    .AddArgument("action", "viewConversation")
                    .AddArgument("conversationId", 9813)
                    .AddText("Shutdown Timer Stopped")
                    .Show();
                return;
            }

            countDown = new DispatcherTimer(new TimeSpan(0, 0, 1), DispatcherPriority.Normal, delegate
            {
                TimerStartStop.Foreground = Brushes.Red;
                TimerStartStop.Content = "Stop";
                Indicator1.Visibility = Visibility.Visible;
                Indicator2.Visibility = Visibility.Visible;
                Indicator3.Visibility = Visibility.Visible;

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

            new ToastContentBuilder()
                   .AddArgument("action", "viewConversation")
                   .AddArgument("conversationId", 9813)
                   .AddText("Shutdown Timer Started")
                   .Show();
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

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}