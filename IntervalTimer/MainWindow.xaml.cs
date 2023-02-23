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
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace IntervalTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        const int MYACTION_HOTKEY_ID = 1;
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

        private double CountDownTime { get; set; } = 0;

        private double _CurrCountDownTime = 0;
        public double CurrCountDownTime
        {

            get
            {
                return _CurrCountDownTime;
            }
            set
            {
                _CurrCountDownTime = value;
                OnPropertyChanged();
            }
        }

        HotKey _hotKey; 

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            Dispatcher mainThreadDispatcher = Dispatcher.CurrentDispatcher;
            _hotKey = new HotKey(Key.R, KeyModifier.Shift | KeyModifier.Alt, OnHotKeyHandler);
        }

        private void OnHotKeyHandler(HotKey hotKey)
        {
            RestartTimer();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void btnStartTimer_Click(object sender, RoutedEventArgs e)
        {
            string inputString = this.txtIntervalTime.Text;
            bool isValid = await ValidateStringConversion(this.txtIntervalTime.Text);
            if (isValid){
                CountDownTime = Convert.ToDouble(inputString);

                StartTimer(false);
            }
            else
            {
                CountDownTime = -1;
            }
        }

        private async Task<bool> ValidateStringConversion(string convertingString) 
        {
            if (!ValidateNumString(convertingString))
            {
                await InvalidInput();
                return false;
            }
            else
            {
                return true;
            }
        }

        private async void StartTimer(bool softStart) {
            TimerRunning = true;

            if (!softStart)
            {
                btnStartTimer.Visibility = Visibility.Collapsed;
                btnStopTimer.Visibility = Visibility.Visible;
                txtIntervalTime.Visibility = Visibility.Collapsed;
                txtCountDownTime.Visibility = Visibility.Visible;
            }
            await Timer(CountDownTime);
        }


        private void btnStopTimer_Click(object sender, RoutedEventArgs e)
        {
            StopTimer(false);
        }

        private void StopTimer(bool softStop)
        {
            TimerRunning = false;
            CurrCountDownTime = CountDownTime;

            if (!softStop)
            {
                btnStartTimer.Visibility = Visibility.Visible;
                btnStopTimer.Visibility = Visibility.Collapsed;
                txtIntervalTime.Visibility = Visibility.Visible;
                txtCountDownTime.Visibility = Visibility.Collapsed;
            }
        }

        private void btnRestartTimer_Click(object sender, RoutedEventArgs e)
        {
            RestartTimer();
        }

        private async void RestartTimer()
        {
            StopTimer(true);
            await Task.Delay(1);
            StartTimer(true);
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
            
            new Thread(async () =>
            {
                while (TimerRunning)
                {
                    bool success = await RunNewTimer(intervalTimeSec);
                    if (success) PlaySound();

                }
                //Application.Current.Dispatcher.Invoke((Action)delegate ()
                //{
                //    txtIntervalTime.Visibility = Visibility.Visible;
                //    txtCountDownTime.Visibility = Visibility.Collapsed;
                //}, null);

            }).Start();
        }


        private async Task<bool> RunNewTimer(double intervalTimeSec)
        {
            stopwatch.Restart();
            while (stopwatch.Elapsed.TotalSeconds < intervalTimeSec && TimerRunning)
            {
                Dispatcher.Invoke(() =>
                {
                    CurrCountDownTime = Math.Round(intervalTimeSec - stopwatch.Elapsed.TotalSeconds, 2);
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
