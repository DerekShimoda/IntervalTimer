using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IntervalTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        bool TimerRunning { get; set; } = false;
        bool StopwatchRunning { get; set; } = false;
        int _StopwatchTime = 0;
        public int StopwatchTime
        {
            get
            {
                return _StopwatchTime;
            }
            set
            {
                _StopwatchTime = value;
                OnPropertyChanged();
            }
        }
        private double _CountDown = 0;
        public double CountDown 
        {

            get
            {
                return _CountDown;
            }
            set
            {
                _CountDown = value;
                OnPropertyChanged();
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            Dispatcher mainThreadDispatcher = Dispatcher.CurrentDispatcher;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void btnStartTimer_Click(object sender, RoutedEventArgs e)
        {
            string inputString = this.txtIntervalTime.Text;
            if (!ValidateNumString(inputString))
            {
                await InvalidInput();
                return;
            }

            double intervalTime = Convert.ToDouble(inputString);
            
            TimerRunning = true;
            btnStartTimer.Visibility = Visibility.Collapsed;
            btnStopTimer.Visibility = Visibility.Visible;
            await Timer(intervalTime);
            
        }

        private void btnStopTimer_Click(object sender, RoutedEventArgs e)
        {
            TimerRunning = false;
            btnStartTimer.Visibility = Visibility.Visible;
            btnStopTimer.Visibility = Visibility.Collapsed;
        }

        private Task InvalidInput()
        {
            Storyboard showWarning = Resources["animate"] as Storyboard;
            showWarning.Begin(invalidInputWarning);
            return Task.CompletedTask;
        }

        private bool ValidateNumString(string input)
        {
            double asDouble = 0.0;
            if (input[^1] == '.')
            {
                return false;
            }
            else if (double.TryParse(input, out asDouble))
            {
                return true;
            }
            
            else
            {
                return false;
            }
        }

        Stopwatch stopwatch = new Stopwatch();

        private async Task Timer(double intervalTimeSec)
        {
            txtIntervalTime.Visibility = Visibility.Collapsed;
            txtCountDown.Visibility = Visibility.Visible;
            new Thread(async () =>
            {
                while (TimerRunning)
                {
                    bool success = await RunNewTimer(intervalTimeSec);
                    if (success) PlaySound();

                }
                Application.Current.Dispatcher.Invoke((Action)delegate ()
                {
                    txtIntervalTime.Visibility = Visibility.Visible;
                    txtCountDown.Visibility = Visibility.Collapsed;
                }, null);

            }).Start();
        }


        private async Task<bool> RunNewTimer(double intervalTimeSec)
        {
            stopwatch.Restart();
            while (stopwatch.Elapsed.TotalSeconds < intervalTimeSec && TimerRunning)
            {
                Dispatcher.Invoke(() =>
                {
                    CountDown = Math.Round(intervalTimeSec - stopwatch.Elapsed.TotalSeconds, 2);
                });
            }
            if (stopwatch.Elapsed.TotalSeconds < intervalTimeSec)
            {
                return false;
            }
            return true;
        }        

        private async Task PlaySound()
        {
            SystemSounds.Hand.Play();
            await Task.Delay(1000);
            return;
        }

        private void btnStopStopwatch_Click(object sender, RoutedEventArgs e)
        {
            StopwatchRunning = false;
        }

        private async void btnStartStopwatch_Click(object sender, RoutedEventArgs e)
        {
            StopwatchRunning = true;
            while (StopwatchRunning)
            {
                await Task.Delay(1000);
                StopwatchTime += 1;
            }
        }

        private void btnRestartStopwatch_Click(object sender, RoutedEventArgs e)
        {
            StopwatchTime = 0;
        }
    }
}
