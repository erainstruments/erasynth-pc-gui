using System;
using System.Text;
using System.Windows;
using System.IO.Ports;
using System.Threading;
using System.Management;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Controls;
using System.Collections.Generic;

using MahApps.Metro;
using MahApps.Metro.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Windows.Input;

namespace Era.Synth.Control.Panel
{
    public partial class Window1
    {
        public bool IsConnected = false;
        SerialPort port;
        bool IsDebugOn = true;

        public Window1()
        {
            InitializeComponent();
            
            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent("Blue"), ThemeManager.GetAppTheme("BaseDark"));
            TabControlHelper.SetUnderlined(tabControl, UnderlinedType.SelectedTabItem);
            
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(errorHandler);
            Closing += OnWindowClosing;
            Loaded += Window1_Loaded;

            try
            {
                WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
                ManagementEventWatcher watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += new EventArrivedEventHandler(deviceChangeListener);
                watcher.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            prepareDeviceList();

            uiModSine.Checked -= uiModWaveTypeChanged;
            uiModSine.IsChecked = true;
            uiModSine.Checked += uiModWaveTypeChanged;

            uiSweepStartType.SelectionChanged -= uiSweepUnit_SelectionChanged;
            uiSweepStopType.SelectionChanged  -= uiSweepUnit_SelectionChanged;
            uiSweepStepType.SelectionChanged  -= uiSweepUnit_SelectionChanged;

            uiSweepStartType.SelectedIndex = 2;
            uiSweepStepType.SelectedIndex  = 2;
            uiSweepStopType.SelectedIndex  = 2;

            uiSweepStartType.SelectionChanged += uiSweepUnit_SelectionChanged;
            uiSweepStopType.SelectionChanged  += uiSweepUnit_SelectionChanged;
            uiSweepStepType.SelectionChanged  += uiSweepUnit_SelectionChanged;



            //uiRfFrequency.ValueIncremented += valueIncremented;
            //uiRfAmplitude.ValueIncremented += valueIncremented;

            //uiModFmDev.ValueIncremented += valueIncremented;
            //uiModFreq.ValueIncremented += valueIncremented;
            //uiModPulsePeriod.ValueIncremented += valueIncremented;
            //uiModPulseWidth.ValueIncremented += valueIncremented;

            //uiSweepDwell.ValueIncremented += valueIncremented;
            //uiSweepStart.ValueIncremented += valueIncremented;
            //uiSweepStep.ValueIncremented += UiRfFrequency_ValueIncremented;
            //uiSweepStop.ValueIncremented += UiRfFrequency_ValueIncremented;
            


            //uiRfFrequency.ValueDecremented += valueDecremented;
            //uiRfAmplitude.ValueDecremented += valueDecremented;

            //uiModFmDev.ValueDecremented += valueDecremented;
            //uiModFreq.ValueDecremented += valueDecremented;
            //uiModPulsePeriod.ValueDecremented += valueDecremented;
            //uiModPulseWidth.ValueDecremented += valueDecremented;

            //uiSweepDwell.ValueDecremented += valueDecremented;
            //uiSweepStart.ValueDecremented += valueDecremented;
            //uiSweepStep.ValueDecremented += valueDecremented;
            //uiSweepStop.ValueDecremented += valueDecremented;

        }

        //private void UiRfFrequency_ValueIncremented(object sender, NumericUpDownChangedRoutedEventArgs args)
        //{
        //    throw new NotImplementedException();
        //}

        private void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            //uiBaudRate.SelectionChanged += uiBaudRate_SelectionChanged;
        }

        public void prepareDeviceList()
        {
            uiDeviceList.SelectionChanged -= uiDeviceList_SelectionChanged;
            uiDeviceList.Items.Clear();
            foreach (Device device in DeviceList.getAllDevices()) { uiDeviceList.Items.Add(device); }
            uiDeviceList.DisplayMemberPath = "DisplayName";
            if (uiDeviceList.Items.Count == 1) { uiDeviceList.SelectedIndex = 0; }
            uiDeviceList.SelectionChanged += uiDeviceList_SelectionChanged;
        }

        private void errorHandler(object sender, UnhandledExceptionEventArgs args)
        {
            MessageBox.Show("An error occured : \n" + ((Exception)args.ExceptionObject).ToString());
        }

        private void deviceChangeListener(object sender, EventArrivedEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { prepareDeviceList(); }));
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }
        
        #region Home Tab

        private void uiRfOnOffClick(object sender, RoutedEventArgs e)
        {
            checkConnection();

            Button btn = sender as Button;
            if (btn.Content.ToString() == "RF OFF")
            {
                try
                {
                    Command.send(Command.RF_ON);
                    btn.Content = "RF ON";
                    btn.Background = Brushes.Green;
                }
                catch (Exception ex)
                {
                    giveError(ex);
                }
            }
            else
            {
                try
                {
                    Command.send(Command.RF_OFF);
                    btn.Content = "RF OFF";
                    btn.Background = Brushes.Red;
                }
                catch (Exception ex)
                {
                    giveError(ex);
                }                
            }           
            
        }
        
        private void uiRfFrequency_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                checkConnection();

                string freq = uiRfFrequency.Value.ToString();
                string type = uiRfFrequencyType.Text;

                int multiplier = 1;
                if (type == "KHz") { multiplier = 1000; }
                else if (type == "MHz") { multiplier = 1000000; }
                else if (type == "GHz") { multiplier = 1000000000; }

                double freqVal = 250000;

                try
                {
                    freqVal = Convert.ToDouble(freq);
                    freqVal = freqVal * (ulong)multiplier;
                }
                catch
                {
                    MessageBox.Show("Please enter proper numerical values");
                    return;
                }
                
                try { Command.send(Command.FREQUENCY, freqVal.ToString()); }
                catch (Exception ex) { giveError(ex); }
            }
        }

        private void uiRfAmplitude_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                checkConnection();

                string amp = uiRfAmplitude.Value.ToString();
                double ampVal = -60.0;
                try { ampVal = Convert.ToDouble(amp); }
                catch { MessageBox.Show("Please enter proper numerical values"); return; }
                
                try { Command.send(Command.AMPLITUDE, (ampVal > 0 ? "+" : "") +  ampVal.ToString("0.0").Replace(",", ".")); }
                catch (Exception ex) { giveError(ex); }
            }
        }

        #endregion
        
        #region Modulation

        private void uiModOnOff_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            Button btn = sender as Button;
            if (btn.Content.ToString() == "OFF")
            {
                try
                {
                    Command.send(Command.MODULATION_ON);
                    btn.Content = "ON";
                    btn.Background = Brushes.Green;
                }
                catch (Exception ex)
                {
                    giveError(ex);
                }
            }
            else
            {
                try
                {
                    Command.send(Command.MODULATION_OFF);
                    btn.Content = "OFF";
                    btn.Background = Brushes.Red;
                }
                catch (Exception ex)
                {
                    giveError(ex);
                }
            }
        }

        private void uiModInternal_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            Button btn = sender as Button;
            try
            {
                Command.send(Command.MODULATION_INTERNAL);
                btn.Background = Brushes.Green;
                uiModExternal.Background = Brushes.LightGray;
                uiModMicro.Background = Brushes.LightGray;

                if (uiModPulse.Background != Brushes.Green)
                {
                    uiModSine.Visibility = Visibility.Visible;
                    uiModTriangle.Visibility = Visibility.Visible;
                    uiModRamp.Visibility = Visibility.Visible;
                    uiModSquare.Visibility = Visibility.Visible;

                    uiModFreq.Visibility = Visibility.Visible;
                    lblModFreq.Visibility = Visibility.Visible;
                }

                
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiModExternal_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();
            Button btn = sender as Button;
            try
            {
                Command.send(Command.MODULATION_EXTERNAL);
                btn.Background = Brushes.Green;
                uiModInternal.Background = Brushes.LightGray;
                uiModMicro.Background = Brushes.LightGray;

                uiModSine.Visibility        = Visibility.Hidden;
                uiModTriangle.Visibility    = Visibility.Hidden;
                uiModRamp.Visibility        = Visibility.Hidden;
                uiModSquare.Visibility      = Visibility.Hidden;

                uiModFreq.Visibility        = Visibility.Hidden;
                lblModFreq.Visibility       = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiModMicro_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            Button btn = sender as Button;
            try
            {
                Command.send(Command.MODULATION_MICROPHONE);
                btn.Background = Brushes.Green;
                uiModInternal.Background = Brushes.LightGray;
                uiModExternal.Background = Brushes.LightGray;

                uiModSine.Visibility        = Visibility.Hidden;
                uiModTriangle.Visibility    = Visibility.Hidden;
                uiModRamp.Visibility        = Visibility.Hidden;
                uiModSquare.Visibility      = Visibility.Hidden;

                uiModFreq.Visibility        = Visibility.Hidden;
                lblModFreq.Visibility       = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }



        private void uiModAm_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            Button btn = sender as Button;
            try
            {
                Command.send(Command.MODULATION_AM);
                btn.Background = Brushes.Green;

                uiModNbfm.Background = Brushes.LightGray;
                uiModWbfm.Background = Brushes.LightGray;
                uiModPulse.Background = Brushes.LightGray;

                uiModMicro.Visibility = Visibility.Visible;

                if (uiModInternal.Background == Brushes.Green)
                {
                    uiModFreq.Visibility = Visibility.Visible;
                    lblModFreq.Visibility = Visibility.Visible;

                    uiModSine.Visibility = Visibility.Visible;
                    uiModSquare.Visibility = Visibility.Visible;
                    uiModRamp.Visibility = Visibility.Visible;
                    uiModTriangle.Visibility = Visibility.Visible;
                }

                uiModAmDepth.Visibility         = Visibility.Visible;
                lblModAmDepth.Visibility        = Visibility.Visible;

                lblModFmDev.Visibility          = Visibility.Hidden;
                uiModFmDev.Visibility           = Visibility.Hidden;

                lblModPulsePeriod.Visibility    = Visibility.Hidden;
                uiModPulsePeriod.Visibility     = Visibility.Hidden;
                uiModPulsePeriodUnit.Visibility = Visibility.Hidden;

                lblModPulseWidth.Visibility     = Visibility.Hidden;
                uiModPulseWidth.Visibility      = Visibility.Hidden;
                uiModPulseWidthUnit.Visibility  = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiModNbfm_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            Button btn = sender as Button;
            try
            {
                Command.send(Command.MODULATION_NBFM);
                btn.Background = Brushes.Green;

                uiModAm.Background = Brushes.LightGray;
                uiModWbfm.Background = Brushes.LightGray;
                uiModPulse.Background = Brushes.LightGray;

                uiModMicro.Visibility = Visibility.Visible;

                if (uiModInternal.Background == Brushes.Green)
                {
                    uiModFreq.Visibility = Visibility.Visible;
                    lblModFreq.Visibility = Visibility.Visible;
                }

                uiModAmDepth.Visibility = Visibility.Hidden;
                lblModAmDepth.Visibility = Visibility.Hidden;

                lblModFmDev.Visibility = Visibility.Visible;
                uiModFmDev.Visibility = Visibility.Visible;

                lblModPulsePeriod.Visibility = Visibility.Hidden;
                uiModPulsePeriod.Visibility = Visibility.Hidden;
                uiModPulsePeriodUnit.Visibility = Visibility.Hidden;

                lblModPulseWidth.Visibility = Visibility.Hidden;
                uiModPulseWidth.Visibility = Visibility.Hidden;
                uiModPulseWidthUnit.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiModWbfm_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            Button btn = sender as Button;
            try
            {
                Command.send(Command.MODULATION_NBFM);
                btn.Background = Brushes.Green;

                uiModAm.Background = Brushes.LightGray;
                uiModNbfm.Background = Brushes.LightGray;
                uiModPulse.Background = Brushes.LightGray;

                uiModMicro.Visibility = Visibility.Visible;

                if (uiModInternal.Background == Brushes.Green)
                {
                    uiModFreq.Visibility = Visibility.Visible;
                    lblModFreq.Visibility = Visibility.Visible;
                }

                uiModAmDepth.Visibility = Visibility.Hidden;
                lblModAmDepth.Visibility = Visibility.Hidden;

                lblModFmDev.Visibility = Visibility.Visible;
                uiModFmDev.Visibility = Visibility.Visible;

                lblModPulsePeriod.Visibility = Visibility.Hidden;
                uiModPulsePeriod.Visibility = Visibility.Hidden;
                uiModPulsePeriodUnit.Visibility = Visibility.Hidden;

                lblModPulseWidth.Visibility = Visibility.Hidden;
                uiModPulseWidth.Visibility = Visibility.Hidden;
                uiModPulseWidthUnit.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiModPulse_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            Button btn = sender as Button;
            try
            {
                Command.send(Command.MODULATION_PULSE);
                btn.Background = Brushes.Green;

                uiModAm.Background = Brushes.LightGray;
                uiModNbfm.Background = Brushes.LightGray;
                uiModWbfm.Background = Brushes.LightGray;

                uiModMicro.Visibility = Visibility.Hidden;

                uiModFreq.Visibility = Visibility.Hidden;
                lblModFreq.Visibility = Visibility.Hidden;

                uiModAmDepth.Visibility = Visibility.Hidden;
                lblModAmDepth.Visibility = Visibility.Hidden;

                lblModFmDev.Visibility = Visibility.Hidden;
                uiModFmDev.Visibility = Visibility.Hidden;

                lblModPulsePeriod.Visibility = Visibility.Visible;
                uiModPulsePeriod.Visibility = Visibility.Visible;
                uiModPulsePeriodUnit.Visibility = Visibility.Visible;

                lblModPulseWidth.Visibility = Visibility.Visible;
                uiModPulseWidth.Visibility = Visibility.Visible;
                uiModPulseWidthUnit.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiModWaveTypeChanged(object sender, RoutedEventArgs e)
        {
            checkConnection();

            RadioButton btn = sender as RadioButton;

            if (btn.Content.ToString() == "SINE")
            {
                try { Command.send(Command.MODULATION_SINE); }
                catch (Exception ex) { giveError(ex); }
            }
            else if (btn.Content.ToString() == "TRIANGLE")
            {
                try { Command.send(Command.MODULATION_TRIANGLE); }
                catch (Exception ex) { giveError(ex); }
            }
            else if (btn.Content.ToString() == "RAMP")
            {
                try { Command.send(Command.MODULATION_RAMP); }
                catch (Exception ex) { giveError(ex); }
            }
            else if (btn.Content.ToString() == "SQUARE")
            {
                try { Command.send(Command.MODULATION_SQUARE); }
                catch (Exception ex) { giveError(ex); }
            }
        }

        private void uiModFreq_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                checkConnection();

                string val = uiModFreq.Value.ToString();

                int freqVal = 250000;

                try { freqVal = Convert.ToInt32(val); }
                catch { MessageBox.Show("Please enter proper numerical values"); return; }
                
                try { Command.send(Command.MODULATION_FREQ, freqVal.ToString()); }
                catch (Exception ex) { giveError(ex); }
            }
        }

        private void uiModAmDepth_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                checkConnection();

                string val = uiModAmDepth.Value.ToString();
                int ampVal = 250000;

                try { ampVal = Convert.ToInt32(val); }
                catch { MessageBox.Show("Please enter proper numerical values"); return; }
                
                try
                { Command.send(Command.MODULATION_AM_DEPTH, ampVal.ToString());  }
                catch (Exception ex) { giveError(ex); }
            }
        }

        private void uiModFmDev_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                checkConnection();

                string val = uiModFmDev.Value.ToString();

                int fm = 250000;

                try { fm = Convert.ToInt32(val); }
                catch { MessageBox.Show("Please enter proper numerical values"); return; }
                
                try
                { Command.send(Command.MODULATION_FM_DEV, fm.ToString()); }
                catch (Exception ex) { giveError(ex); }
            }
        }

        private void uiModPulseWidth_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                checkConnection();

                string val = uiModPulseWidth.Value.ToString();
                int width = 0;

                try { width = Convert.ToInt32(val); }
                catch { MessageBox.Show("Please enter proper numerical values"); return; }

                string unit = (uiModPulseWidthUnit.SelectedValue as ComboBoxItem).Content.ToString();

                switch (unit)
                {
                    case "s": width *= 1000000; break;
                    case "mS": width *= 1000; break;
                    case "uS": width *= 1; break;
                }
                
                try { Command.send(Command.MODULATION_PULSE_WIDTH, width.ToString());}
                catch (Exception ex) { giveError(ex); }
            }
        }

        private void uiModPulsePeriod_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                checkConnection();

                string val = uiModPulsePeriod.Value.ToString();
                int period = 0;

                try { period = Convert.ToInt32(val); }
                catch { MessageBox.Show("Please enter proper numerical values"); return; }

                string unit = (uiModPulsePeriodUnit.SelectedItem as ComboBoxItem).Content.ToString();

                switch (unit)
                {
                    case "s": period *= 1000000; break;
                    case "mS": period *= 1000; break;
                    case "uS": period *= 1; break;
                }
                
                try {  Command.send(Command.MODULATION_PULSE_PERIOD, period.ToString()); }
                catch (Exception ex) { giveError(ex); }
            }
        }

        #endregion
        
        #region Sweep Tab

        private void uiSweepOnOff_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();
            
            Button btn = sender as Button;
            if (btn.Content.ToString() == "OFF")
            {
                try
                {
                    Command.send(Command.SWEEP_ON);
                    btn.Content = "ON";
                    btn.Background = Brushes.Green;
                }
                catch (Exception ex)
                {
                    giveError(ex);
                }
            }
            else
            {
                try
                {
                    Command.send(Command.SWEEP_OFF);
                    btn.Content = "OFF";
                    btn.Background = Brushes.Red;
                }
                catch (Exception ex)
                {
                    giveError(ex);
                }
            }
        }

        private void uiSweepFreeRun_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();
            
            Button btn = sender as Button;            
            try
            {
                Command.send(Command.SWEEP_FREE_RUN);
                btn.Background = Brushes.Green;
                uiSweepExternal.Background = Brushes.LightGray;
                uiSweepDwell.Visibility = Visibility.Visible;
                lblSweepDweel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                giveError(ex);
            }            
        }

        private void uiSweepExternal_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            Button btn = sender as Button;
            try
            {
                Command.send(Command.SWEEP_EXTERNAL);
                btn.Background = Brushes.Green;
                uiSweepFreeRun.Background = Brushes.LightGray;
                uiSweepDwell.Visibility = Visibility.Hidden;
                lblSweepDweel.Visibility = Visibility.Hidden;

            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiSweepStart_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                checkConnection();

                string start_freq = uiSweepStart.Value.ToString();
                string start_type = uiSweepStartType.Text;

                int start_m = 1;
                if (start_type == "KHz") { start_m = 1000; }
                else if (start_type == "MHz") { start_m = 1000000; }
                else if (start_type == "GHz") { start_m = 1000000000; }

                double startFreqVal = 250000;

                try
                {

                    startFreqVal = Convert.ToDouble(start_freq);
                    startFreqVal = startFreqVal * (ulong)start_m;
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex);
                    MessageBox.Show("Please enter proper numerical values");
                    return;
                }
                
                try {  Command.send(Command.SWEEP_START, startFreqVal.ToString());  }
                catch (Exception ex) { giveError(ex); }
            }
        }

        private void uiSweepStep_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                checkConnection();

                string step_freq = uiSweepStep.Value.ToString();
                string step_type = uiSweepStepType.Text;

                int step_m = 1;
                if (step_type == "KHz") { step_m = 1000; }
                else if (step_type == "MHz") { step_m = 1000000; }
                else if (step_type == "GHz") { step_m = 1000000000; }

                double stepFreqVal = 1;

                try
                {
                    stepFreqVal = Convert.ToDouble(step_freq);
                    stepFreqVal = stepFreqVal * (ulong)step_m;
                }
                catch
                {
                    MessageBox.Show("Please enter proper numerical values");
                    return;
                }
                
                try { Command.send(Command.SWEEP_STEP, stepFreqVal.ToString()); }
                catch (Exception ex) { giveError(ex); }
            }
        }

        private void uiSweepStop_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                checkConnection();

                string stop_freq = uiSweepStop.Value.ToString();
                string stop_type = uiSweepStopType.Text;

                int stop_m = 1;
                if (stop_type == "KHz") { stop_m = 1000; }
                else if (stop_type == "MHz") { stop_m = 1000000; }
                else if (stop_type == "GHz") { stop_m = 1000000000; }

                double stopFreqVal = 250000;

                try
                {
                    stopFreqVal = Convert.ToDouble(stop_freq);
                    stopFreqVal = stopFreqVal * (ulong)stop_m;
                }
                catch
                {
                    MessageBox.Show("Please enter proper numerical values");
                    return;
                }
                
                try  {  Command.send(Command.SWEEP_STOP, stopFreqVal.ToString());  }
                catch (Exception ex) {  giveError(ex); }
            }
        }

        private void uiSweepDwell_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                checkConnection();

                string dwell = uiSweepDwell.Value.ToString();
                uint dwellVal = 0;
                try
                {
                    dwellVal = Convert.ToUInt32(dwell);
                }
                catch
                {
                    MessageBox.Show("Please enter proper numerical values");
                    return;
                }
                
                try{ Command.send(Command.SWEEP_DWELL, dwellVal.ToString()); }
                catch (Exception ex) { giveError(ex); }
            }
        }

        private void uiSweepUnit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            try
            {
                int i = 1;
                string a1 = cmb.SelectionBoxItem.ToString();
                switch (a1)
                {
                    case "KHz": i = 1000; break;
                    case "MHz": i = 1000000; break;
                    case "GHz": i = 1000000000; break;
                }
                
                try
                {
                    double val1 = Convert.ToDouble(uiSweepStart.Value);
                    double val2 = Convert.ToDouble(uiSweepStep.Value);
                    double val3 = Convert.ToDouble(uiSweepStop.Value);

                    val1 = val1 * (ulong)i;
                    val2 = val2 * (ulong)i;
                    val3 = val3 * (ulong)i;
                    
                    string a2 = (cmb.SelectedItem as ComboBoxItem).Content.ToString();
                    switch (a2)
                    {
                        case "KHz": val1 /= 1000; val2 /= 1000; val3 /= 1000; break;
                        case "MHz": val1 /= 1000000; val2 /= 1000000; val3 /= 1000000; break;
                        case "GHz": val1 /= 1000000000; val2 /= 1000000000; val3 /= 1000000000; break;
                    }

                    if (cmb.Name == "uiSweepStartType") { uiSweepStart.Value = val1; }
                    if (cmb.Name == "uiSweepStepType") { uiSweepStep.Value = val2; }
                    if (cmb.Name == "uiSweepStopType") { uiSweepStop.Value = val3; }
                    
                }
                catch (Exception ex) { Debug.WriteLine(ex); }
                
   
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        #endregion
        
        #region Reference

        private void uiRefInternal_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            Button btn = sender as Button;
            try
            {
                Command.send(Command.REFERENCE_INTERNAL);
                btn.Background = Brushes.Green;
                uiRefExternal.Background = Brushes.LightGray;
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiRefExternal_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            Button btn = sender as Button;
            try
            {
                Command.send(Command.REFERENCE_EXTERNAL);
                btn.Background = Brushes.Green;
                uiRefInternal.Background = Brushes.LightGray;
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiRefTcxo_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            Button btn = sender as Button;
            try
            {
                Command.send(Command.REFERENCE_TCXO);
                btn.Background = Brushes.Green;
                uiRefOcxo.Background = Brushes.LightGray;
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiRefOcxo_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            Button btn = sender as Button;
            try
            {
                Command.send(Command.REFERENCE_OCXO);
                btn.Background = Brushes.Green;
                uiRefTcxo.Background = Brushes.LightGray;
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        #endregion

        #region Diagnostics

        private void uiTemperature_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            try
            {
                // remove data receive event to avoid conflict in reading data
                // Serial port removes buffer after read and data arrives to event handler first.            
                // If we want to read induvidually, we need to write response to debug panel seperate than event handler
                port.DataReceived -= Port_DataReceived;
                Command.send(Command.TEMPERATURE);
                Thread.Sleep(100);

                string response = port.ReadExisting();
                writeDebugPanel(response);
                port.DataReceived += Port_DataReceived;

                response = response.Substring(0, response.IndexOf("\r"));

                double celcius = Convert.ToDouble(response.Replace(".", ","));
                double fahrenheit = (celcius * 9 / 5) + 32;

                uiTemperature.Content = "Temperature: " + celcius + "°C / " + fahrenheit + "°F";
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiPLLXtal_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            try
            {
                // remove data receive event to avoid conflict in reading data
                // Serial port removes buffer after read and data arrives to event handler first.            
                // If we want to read induvidually, we need to write response to debug panel seperate than event handler
                port.DataReceived -= Port_DataReceived;
                Command.send(Command.PLL_XTAL);
                Thread.Sleep(100);

                string response = port.ReadExisting();
                writeDebugPanel(response);
                port.DataReceived += Port_DataReceived;

                response = response.Substring(0, response.IndexOf("\r"));

                if (response == "1")
                {
                    uiPLLXtal.Background = Brushes.Green;
                    uiPLLXtal.Content = "PLL Lock (Xtal) : Locked";
                }
                else
                {
                    uiPLLXtal.Background = Brushes.Red;
                    uiPLLXtal.Content = "PLL Lock (Xtal) : Unlocked";
                }
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiPLLLMX1_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            try
            {
                // remove data receive event to avoid conflict in reading data
                // Serial port removes buffer after read and data arrives to event handler first.            
                // If we want to read induvidually, we need to write response to debug panel seperate than event handler
                port.DataReceived -= Port_DataReceived;
                Command.send(Command.PLL_LMX1);
                Thread.Sleep(100);

                string response = port.ReadExisting();
                writeDebugPanel(response);
                port.DataReceived += Port_DataReceived;

                response = response.Substring(0, response.IndexOf("\r"));

                if (response == "1")
                {
                    uiPLLLMX1.Background = Brushes.Green;
                    uiPLLLMX1.Content = "PLL Lock (LMX1) : Locked";
                }
                else
                {
                    uiPLLLMX1.Background = Brushes.Red;
                    uiPLLLMX1.Content = "PLL Lock (LMX1) : Unlocked";
                }
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiPLLLMX2_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            try
            {
                // remove data receive event to avoid conflict in reading data
                // Serial port removes buffer after read and data arrives to event handler first.            
                // If we want to read induvidually, we need to write response to debug panel seperate than event handler
                port.DataReceived -= Port_DataReceived;
                Command.send(Command.PLL_LMX2);
                Thread.Sleep(100);

                string response = port.ReadExisting();
                writeDebugPanel(response);
                port.DataReceived += Port_DataReceived;

                response = response.Substring(0, response.IndexOf("\r"));

                if (response == "1")
                {
                    uiPLLLMX2.Background = Brushes.Green;
                    uiPLLLMX2.Content = "PLL Lock (LMX2) : Locked";
                }
                else
                {
                    uiPLLLMX2.Background = Brushes.Red;
                    uiPLLLMX2.Content = "PLL Lock (LMX2) : Unlocked";
                }
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiPower_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            try
            {
                // remove data receive event to avoid conflict in reading data
                // Serial port removes buffer after read and data arrives to event handler first.            
                // If we want to read induvidually, we need to write response to debug panel seperate than event handler

                string v, a = "";

                port.DataReceived -= Port_DataReceived;
                Command.send(Command.VOLTAGE);
                Thread.Sleep(250);

                string response = port.ReadExisting();
                v = response;
                writeDebugPanel(response);
                Command.send(Command.CURRENT);
                Thread.Sleep(100);

                response = port.ReadExisting();
                a = response;
                writeDebugPanel(response);
                port.DataReceived += Port_DataReceived;

                a = a.Substring(0, response.IndexOf("\r"));
                v = v.Substring(0, response.IndexOf("\r"));

                double current = Convert.ToDouble(a.Replace(".", ","));
                double voltage = Convert.ToDouble(v.Replace(".", ","));
                double power = current * voltage;

                uiPower.Content = current.ToString("0.00") + " A x " + voltage.ToString("0.00") + " V = " + power.ToString("0.00") + " W";

            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiRSSI_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            try
            {
                // remove data receive event to avoid conflict in reading data
                // Serial port removes buffer after read and data arrives to event handler first.            
                // If we want to read induvidually, we need to write response to debug panel seperate than event handler

                port.DataReceived -= Port_DataReceived;
                Command.send(Command.RSSI);
                Thread.Sleep(100);

                string response = port.ReadExisting();
                writeDebugPanel(response);
                port.DataReceived += Port_DataReceived;

                uiRSSI.Content = "WIFI RSSI (dBm) : " + response;
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiDiagReadAll_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            try
            {
                // remove data receive event to avoid conflict in reading data
                // Serial port removes buffer after read and data arrives to event handler first.            
                // If we want to read induvidually, we need to write response to debug panel seperate than event handler
                port.DataReceived -= Port_DataReceived;
                Command.send(Command.READ_DIAGNOSTIC);
                Thread.Sleep(100);
                string response = port.ReadExisting();
                writeDebugPanel(response);
                port.DataReceived += Port_DataReceived;

                int first_char = response.IndexOf("{");
                int last_char = response.LastIndexOf("}");

                response = response.Substring(first_char, (response.Length - first_char - (response.Length - last_char - 1)));
                response = response.Replace("{", "").Replace("}", "").Replace("\"", "");

                string[] values = response.Split(',');

                for (int i = 0; i < values.Length; i++)
                {
                    writeDebugPanel(values[i]);
                    values[i] = values[i].Split(':')[1];
                }

                // Index = 0 Temperature Info
                double celcius = Convert.ToDouble(values[0].Replace(".", ","));
                double fahrenheit = (celcius * 9 / 5) + 32;
                uiTemperature.Content = "Temperature: " + celcius + "°C / " + fahrenheit + "°F";


                // Index = 1 PLL Lock XTAL
                if (values[1] == "1")
                {
                    uiPLLXtal.Background = Brushes.Green;
                    uiPLLXtal.Content = "PLL Lock (Xtal) : Locked";
                }
                else
                {
                    uiPLLXtal.Background = Brushes.Red;
                    uiPLLXtal.Content = "PLL Lock (Xtal) : Unlocked";
                }



                // Index = 2 PLL Lock LMX1
                if (values[2] == "1")
                {
                    uiPLLLMX1.Background = Brushes.Green;
                    uiPLLLMX1.Content = "PLL Lock (LMX1) : Locked";
                }
                else
                {
                    uiPLLLMX1.Background = Brushes.Red;
                    uiPLLLMX1.Content = "PLL Lock (LMX1) : Unlocked";
                }

                // Index = 3 PLL Lock LMX2
                if (values[3] == "1")
                {
                    uiPLLLMX2.Background = Brushes.Green;
                    uiPLLLMX2.Content = "PLL Lock (LMX2) : Locked";
                }
                else
                {
                    uiPLLLMX2.Background = Brushes.Red;
                    uiPLLLMX2.Content = "PLL Lock (LMX2) : Unlocked";
                }

                // Index = 4 Current
                // Index = 5 Voltage
                double current = Convert.ToDouble(values[4].Replace(".", ","));
                double voltage = Convert.ToDouble(values[5].Replace(".", ","));
                double power = current * voltage;

                uiPower.Content = current.ToString("0.00") + " A x " + voltage.ToString("0.00") + " V = " + power.ToString("0.00") + " W";

                // Index = 6 RSSI Value
                uiRSSI.Content = "WIFI RSSI (dBm) : " + values[6];

                // Index = 7 Embedded Version
                lblEM.Text = "Embedded Version : " + values[7];
                lblESP8266.Text = "ESP8266 Embedded Ver. : " + values[8];

                // Trigger RSSI to read again
                uiRSSI.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

                // Index = 8 Serial Number
                lblSerialNumber.Text = "Serial Number : " + values[9];

                // Index = 9 Model 
                lblModel.Text = "Model : " + (values[10] == "0" ? "ERASynth" : (values[10] == "1" ? "ERASynth+" : (values[10] == "2" ? "ERASynth++" : "")));
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }
        #endregion

        #region Setting

        private void uiSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Network configuration change", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No) { return; }

            string sta_ssid = uiStationSSID.Text;
            string sta_pass = uiStationPass.Text;
            string hotspot_ssid = uiHotspotSSID.Text;
            string hotspot_pass = uiHotspotPass.Text;

            string ip1 = uiIPAdress.Text;
            string ip2 = uiSubnetMask.Text;
            string ip3 = uiDefaultGateway.Text;

            Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

            MatchCollection result5 = ip.Matches(ip1);
            MatchCollection result6 = ip.Matches(ip2);
            MatchCollection result7 = ip.Matches(ip3);

            if (sta_ssid == "" || sta_pass == "" || hotspot_ssid == "" || hotspot_pass == "" || result5.Count == 0 || result6.Count == 0 || result7.Count == 0) { MessageBox.Show("Please enter valid IP addresses and values!!"); return; }
            if (uiStation.Background == Brushes.Green && uiHotspot.Background == Brushes.Green) { MessageBox.Show("Please select Wifi mode (Station or Hotspot)"); return; }

            checkConnection();

            try
            {
                // station ssid
                // station pass
                // hotspot ssid
                // hotspot pass

                // ip 1
                // ip2 
                // ip3
                // wifi mode

                Command.send(Command.STATION_SSID, sta_ssid);
                Thread.Sleep(100);

                Command.send(Command.STATION_PASS, sta_pass);
                Thread.Sleep(100);

                Command.send(Command.HOTSPOT_SSID, hotspot_ssid);
                Thread.Sleep(100);

                Command.send(Command.HOTSPOT_PASS, hotspot_pass);
                Thread.Sleep(100);
                
                string[] ip_1 = ip1.Split('.');
                string[] ip_2 = ip2.Split('.');
                string[] ip_3 = ip3.Split('.');

                for (int i = 0; i < ip_1.Length; i++)
                {
                    ip_1[i] = ip_1[i].ToString().PadLeft(3, '0');
                    ip_2[i] = ip_2[i].ToString().PadLeft(3, '0');
                    ip_3[i] = ip_3[i].ToString().PadLeft(3, '0');
                }

                ip1 = ip_1[0] + "." + ip_1[1] + "." + ip_1[2] + "." + ip_1[3];
                ip2 = ip_2[0] + "." + ip_2[1] + "." + ip_2[2] + "." + ip_2[3];
                ip3 = ip_3[0] + "." + ip_3[1] + "." + ip_3[2] + "." + ip_3[3];

                Command.send(Command.IP_ADDRESS, ip1);
                Thread.Sleep(100);

                Command.send(Command.DEFAULT_GATEWAY, ip3);
                
                Thread.Sleep(100);

                Command.send(Command.SUBNET_MASK, ip2);
                Thread.Sleep(100);

                if (uiStation.Background == Brushes.Green)
                {
                    Command.send(Command.WIFI_STATION);
                }
                else
                {
                    Command.send(Command.WIFI_HOTSPOT);
                }
                
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiHelpButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://erainstruments.com");
        }

        private void uiStation_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            btn.Background = Brushes.Green;
            uiHotspot.Background = Brushes.LightGray;
        }

        private void uiHotspot_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            btn.Background = Brushes.Green;
            uiStation.Background = Brushes.LightGray;
        }

        private void uiEspOnOff_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "ESP8266 module power", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No) { return; }

            checkConnection();

            Button btn = sender as Button;
            if (btn.Content.ToString() == "ESP8266 ON")
            {
                try
                {
                    Command.send(Command.ESP_OFF);
                    btn.Content = "ESP8266 OFF";
                    btn.Background = Brushes.Red;
                }
                catch (Exception ex)
                {
                    giveError(ex);
                }
            }
            else
            {
                try
                {
                    Command.send(Command.ESP_ON);
                    btn.Content = "ESP8266 ON";
                    btn.Background = Brushes.Green;
                }
                catch (Exception ex)
                {
                    giveError(ex);
                }
            }
        }

        private void uiEspCodeMode_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Upload mode Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No) { return; }

            checkConnection();

            try
            {
                Command.send(Command.ESP_UPLOAD_MODE);
                MessageBox.Show("Now you can upload ESP ");
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        private void uiFactoryReset_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();

            MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure?", "Factory Reset Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.No) { return; }

            try
            {
                Command.send(Command.FACTORY_RESET);
            }
            catch (Exception ex)
            {
                giveError(ex);
            }

        }

        #endregion

        #region Debug

        private void debugPanelClear_Click(object sender, RoutedEventArgs e)
        {
            debugPanel.Text = "";
        }

        private void debugPanelSwitch_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn.Content.ToString() == "Turn Off")
            {
                IsDebugOn = false;
                btn.Content = "Turn On";
            }
            else
            {
                IsDebugOn = true;
                btn.Content = "Turn Off";
            }
        }

        public void writeDebugPanel(string input)
        {
            if (IsDebugOn) { debugPanel.AppendText(input); debugPanel.ScrollToEnd(); }
        }
        #endregion
        
        private void uiPreset_Click(object sender, RoutedEventArgs e)
        {
            checkConnection();
            
            try
            {
                Command.send(Command.PRESET);

                uiRfOnOff.Content = "RF ON";
                uiRfOnOff.Background = Brushes.Green;
                uiRfFrequency.Value = 0;
                uiRfFrequencyType.SelectedIndex = 0;
                uiRfAmplitude.Value = 0;

                uiModOnOff.Content = "OFF";
                uiModOnOff.Background = Brushes.Red;

                uiModInternal.Background = Brushes.Green;
                uiModExternal.Background = Brushes.LightGray;
                uiModMicro.Visibility = Visibility.Visible;
                uiModMicro.Background = Brushes.LightGray;


                uiModAm.Background = Brushes.Green;
                uiModNbfm.Background = Brushes.LightGray;
                uiModWbfm.Background = Brushes.LightGray;
                uiModPulse.Background = Brushes.LightGray;

                uiModSine.Visibility     = Visibility.Visible;

                uiModSine.Checked -= uiModWaveTypeChanged;
                uiModSine.IsChecked = false;
                uiModSine.Checked += uiModWaveTypeChanged;

                uiModTriangle.Checked -= uiModWaveTypeChanged;
                uiModTriangle.IsChecked = false;
                uiModTriangle.Checked += uiModWaveTypeChanged;

                uiModRamp.Checked -= uiModWaveTypeChanged;
                uiModRamp.IsChecked = false;
                uiModRamp.Checked += uiModWaveTypeChanged;

                uiModSquare.Checked -= uiModWaveTypeChanged;
                uiModSquare.IsChecked = false;
                uiModSquare.Checked += uiModWaveTypeChanged;

                uiModTriangle.Visibility        = Visibility.Visible;
                uiModRamp.Visibility            = Visibility.Visible;
                uiModSquare.Visibility          = Visibility.Visible;

                lblModFreq.Visibility           = Visibility.Visible;
                uiModFreq.Visibility            = Visibility.Visible;
                uiModFreq.Value = 0;

                lblModAmDepth.Visibility        = Visibility.Visible;
                uiModAmDepth.Visibility         = Visibility.Visible;
                uiModAmDepth.Value = 0;
                
                lblModFmDev.Visibility          = Visibility.Hidden;
                uiModFmDev.Visibility           = Visibility.Hidden;

                lblModPulsePeriod.Visibility    = Visibility.Hidden;
                lblModPulseWidth.Visibility     = Visibility.Hidden;

                uiModPulsePeriod.Visibility     = Visibility.Hidden;
                uiModPulseWidth.Visibility      = Visibility.Hidden;

                uiModPulsePeriodUnit.Visibility = Visibility.Hidden;
                uiModPulseWidthUnit.Visibility  = Visibility.Hidden;



                uiSweepOnOff.Content = "OFF";
                uiSweepOnOff.Background = Brushes.Red;

                uiSweepFreeRun.Background   = Brushes.Green;
                uiSweepExternal.Background  = Brushes.LightGray;
                uiSweepDwell.Visibility     = Visibility.Visible;
                lblSweepDweel.Visibility    = Visibility.Visible;



                uiSweepStart.Value = 0;
                uiSweepStep.Value  = 0;
                uiSweepStop.Value  = 0;
                uiSweepDwell.Value = 0;

                uiRefInternal.Background = Brushes.Green;
                uiRefExternal.Background = Brushes.LightGray;

                uiRefTcxo.Background = Brushes.Green;
                uiRefOcxo.Background = Brushes.LightGray;


                uiStationSSID.Text = "";
                uiStationPass.Text = "";
                uiHotspotPass.Text = "";
                uiHotspotSSID.Text = "";
                uiIPAdress.Text = "";
                uiSubnetMask.Text = "";
                uiDefaultGateway.Text = "";

                uiStation.Background = Brushes.LightGray;
                uiHotspot.Background = Brushes.Green;

                uiEspOnOff.Content = "ESP8266 ON";
                uiEspOnOff.Background = Brushes.Green;

                readAll();
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }
        
        private void uiReadAll_Click_1(object sender, RoutedEventArgs e)
        {
            readAll();
        }

        public void readAll()
        {
            checkConnection();

            try
            {
                // remove data receive event to avoid conflict in reading data
                // Serial port removes buffer after read and data arrives to event handler first.            
                // If we want to read induvidually, we need to write response to debug panel seperate than event handler

                port.DataReceived -= Port_DataReceived;
                Command.send(Command.READ_ALL);
                Thread.Sleep(500);
                string response = port.ReadExisting();
                writeDebugPanel(response);
                port.DataReceived += Port_DataReceived;

                int first_char = response.IndexOf("{");
                int last_char = response.LastIndexOf("}");

                response = response.Substring(first_char, (response.Length - first_char - (response.Length - last_char - 1)));
                response = response.Replace("{", "").Replace("}", "").Replace("\"", "");

                string[] values = response.Split(',');

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = values[i].Split(':')[1];
                }

                // Remove text changed handlers
                //uiRfFrequency.ValueChanged -= valuesChanged;
                //uiRfAmplitude.ValueChanged -= valuesChanged;

                //uiModAmDepth.ValueChanged -= valuesChanged;
                //uiModFmDev.ValueChanged -= valuesChanged;
                //uiModFreq.ValueChanged -= valuesChanged;
                //uiModPulsePeriod.ValueChanged -= valuesChanged;
                //uiModPulseWidth.ValueChanged -= valuesChanged;

                //uiSweepDwell.ValueChanged -= valuesChanged;
                //uiSweepStart.ValueChanged -= valuesChanged;
                //uiSweepStep.ValueChanged -= valuesChanged;
                //uiSweepStop.ValueChanged -= valuesChanged;


                // Index = 0 RF output
                if (Convert.ToInt32(values[0]) == 1)
                {
                    uiRfOnOff.Content = "RF ON";
                    uiRfOnOff.Background = Brushes.Green;
                }
                else
                {
                    uiRfOnOff.Content = "RF OFF";
                    uiRfOnOff.Background = Brushes.Red;
                }

                // Index = 1 Frequency Info
                // Frequency returns as Hz and int

                uiRfFrequency.Value = Convert.ToUInt64(values[1]);
                
                uiRfFrequencyType.SelectedIndex = 3;


                // Index = 2 Amplitude info
                uiRfAmplitude.Value = Convert.ToDouble(values[2].Replace(".", ","));

                // Index = 3 Modulation On Off Info
                if (Convert.ToInt32(values[3]) == 1)
                {
                    uiModOnOff.Content = "ON";
                    uiModOnOff.Background = Brushes.Green;
                }
                else
                {
                    uiModOnOff.Content = "OFF";
                    uiModOnOff.Background = Brushes.Red;
                }

                //Index = 4 Modulation Type
                uiModAm.Background = Brushes.LightGray;
                uiModNbfm.Background = Brushes.LightGray;
                uiModWbfm.Background = Brushes.LightGray;
                uiModPulse.Background = Brushes.LightGray;

                lblModAmDepth.Visibility = Visibility.Hidden;
                lblModFmDev.Visibility = Visibility.Hidden;
                lblModPulsePeriod.Visibility = Visibility.Hidden;
                lblModPulseWidth.Visibility = Visibility.Hidden;

                uiModAmDepth.Visibility = Visibility.Hidden;
                uiModFmDev.Visibility = Visibility.Hidden;
                uiModPulsePeriod.Visibility = Visibility.Hidden;
                uiModPulseWidth.Visibility = Visibility.Hidden;
                uiModPulsePeriodUnit.Visibility = Visibility.Hidden;
                uiModPulseWidthUnit.Visibility = Visibility.Hidden;

                switch (Convert.ToInt32(values[4]))
                {
                    case 0:
                        uiModNbfm.Background = Brushes.Green;
                        lblModFmDev.Visibility = Visibility.Visible;
                        uiModFmDev.Visibility = Visibility.Visible;
                        break;
                    case 1:
                        uiModWbfm.Background = Brushes.Green;
                        lblModFmDev.Visibility = Visibility.Visible;
                        uiModFmDev.Visibility = Visibility.Visible;
                        break;
                    case 2:
                        uiModAm.Background = Brushes.Green;
                        lblModAmDepth.Visibility = Visibility.Visible;
                        uiModAmDepth.Visibility = Visibility.Visible;
                        break;
                    case 3:
                        uiModPulse.Background = Brushes.Green;
                        lblModPulsePeriod.Visibility = Visibility.Visible;
                        lblModPulseWidth.Visibility = Visibility.Visible;

                        uiModPulsePeriod.Visibility = Visibility.Visible;
                        uiModPulsePeriodUnit.Visibility = Visibility.Visible;
                        uiModPulseWidth.Visibility = Visibility.Visible;
                        uiModPulseWidthUnit.Visibility = Visibility.Visible;

                        uiModFreq.Visibility = Visibility.Hidden;
                        lblModFreq.Visibility = Visibility.Hidden;
                        break;
                }

                // Index = 5 Modulation Source
                uiModInternal.Background = Brushes.LightGray;
                uiModExternal.Background = Brushes.LightGray;
                uiModMicro.Background = Brushes.LightGray;

                uiModSine.Visibility = Visibility.Hidden;
                uiModTriangle.Visibility = Visibility.Hidden;
                uiModRamp.Visibility = Visibility.Hidden;
                uiModSquare.Visibility = Visibility.Hidden;

                lblModFreq.Visibility = Visibility.Hidden;
                uiModFreq.Visibility = Visibility.Hidden;

                switch (Convert.ToInt32(values[5]))
                {
                    case 0:

                        uiModInternal.Background = Brushes.Green;

                        if (Convert.ToInt32(values[4]) != 3)
                        {
                            uiModFreq.Visibility = Visibility.Visible;
                            lblModFreq.Visibility = Visibility.Visible;

                            uiModSine.Visibility = Visibility.Visible;
                            uiModTriangle.Visibility = Visibility.Visible;
                            uiModRamp.Visibility = Visibility.Visible;
                            uiModSquare.Visibility = Visibility.Visible;
                        }
                        break;
                    case 1:
                        uiModExternal.Background = Brushes.Green;
                        break;
                    case 2:
                        uiModMicro.Background = Brushes.Green;
                        break;
                }

                // Index = 6 Modulation Wave form
                uiModSine.Checked -= uiModWaveTypeChanged;
                uiModRamp.Checked -= uiModWaveTypeChanged;
                uiModTriangle.Checked -= uiModWaveTypeChanged;
                uiModSquare.Checked -= uiModWaveTypeChanged;

                uiModSine.IsChecked = false;
                uiModTriangle.IsChecked = false;
                uiModRamp.IsChecked = false;
                uiModSquare.IsChecked = false;

                switch (Convert.ToInt32(values[6]))
                {
                    case 0: uiModSine.IsChecked = true; break;
                    case 1: uiModTriangle.IsChecked = true; break;
                    case 2: uiModRamp.IsChecked = true; break;
                    case 3: uiModSquare.IsChecked = true; break;
                }

                uiModSine.Checked += uiModWaveTypeChanged;
                uiModRamp.Checked += uiModWaveTypeChanged;
                uiModTriangle.Checked += uiModWaveTypeChanged;
                uiModSquare.Checked += uiModWaveTypeChanged;

                // Index = 7 Internal Modulation Frequency
                uiModFreq.Value = Convert.ToInt32(values[7]);

                // Index = 8 Modulation FM Deviation
                uiModFmDev.Value = Convert.ToInt32(values[8]);

                // Index = 9 Modulation Am Depth
                uiModAmDepth.Value = Convert.ToInt32(values[9]);

                // Index = 10 Modulation Pulse Period
                uiModPulsePeriod.Value = Convert.ToInt32(values[10]);
                uiModPulsePeriodUnit.SelectedIndex = 2;

                // Index = 11 Modulation Pulse Width
                uiModPulseWidth.Value = Convert.ToInt32(values[11]);
                uiModPulseWidthUnit.SelectedIndex = 2;

                // Index = 12 Sweep On Off
                if (values[12] == "1")
                {
                    uiSweepOnOff.Content = "ON";
                    uiSweepOnOff.Background = Brushes.Green;
                }
                else if (values[12] == "0")
                {
                    uiSweepOnOff.Content = "OFF";
                    uiSweepOnOff.Background = Brushes.Red;
                }

                // Index = 13 Sweep Start Freq
                try { uiSweepStart.Value = Convert.ToUInt64(values[13]); } catch { }

                // Index = 14 Sweep Stop Freq
                try { uiSweepStop.Value = Convert.ToUInt64(values[14]); } catch { }

                // Index = 15 Sweep Step Freq
                try { uiSweepStep.Value = Convert.ToUInt64(values[15]); } catch { }

                // Index = 16 Sweep Dwell Time
                try { uiSweepDwell.Value = Convert.ToInt32(values[16]); } catch { }

                uiSweepStartType.SelectedIndex = 0;
                uiSweepStepType.SelectedIndex = 0;
                uiSweepStopType.SelectedIndex = 0;

                // Index = 17 Sweep Trigger
                uiSweepFreeRun.Background = Brushes.LightGray;
                uiSweepExternal.Background = Brushes.LightGray;
                uiSweepDwell.Visibility = Visibility.Hidden;
                lblSweepDweel.Visibility = Visibility.Hidden;

                if (values[17] == "0") { uiSweepFreeRun.Background = Brushes.Green; uiSweepDwell.Visibility = Visibility.Visible; lblSweepDweel.Visibility = Visibility.Visible; }
                else if (values[17] == "1") { uiSweepExternal.Background = Brushes.Green; }


                // Index = 18 Reference Interanl or External
                uiRefExternal.Background = Brushes.LightGray;
                uiRefInternal.Background = Brushes.LightGray;

                Debug.WriteLine(values[18]);
                if (values[18] == "0")
                {
                    uiRefInternal.Background = Brushes.Green;
                }
                else if (values[18] == "1")
                {
                    uiRefExternal.Background = Brushes.Green;
                }

                // Index = 19 Reference TCXO or OCXO
                uiRefTcxo.Background = Brushes.LightGray;
                uiRefOcxo.Background = Brushes.LightGray;

                if (values[19] == "0")
                {
                    uiRefTcxo.Background = Brushes.Green;
                }
                else if (values[19] == "1")
                {
                    uiRefOcxo.Background = Brushes.Green;
                }

                // Index = 20 Wifi Mode
                uiStation.Background = Brushes.LightGray;
                uiHotspot.Background = Brushes.LightGray;

                if (values[20] == "0")
                {
                    uiStation.Background = Brushes.Green;
                }
                else if (values[20] == "1")
                {
                    uiHotspot.Background = Brushes.Green;
                }

                // Index = 21 Wifi Station SSID
                uiStationSSID.Text = values[21].ToString();

                // Index = 22 Wifi Station Password
                uiStationPass.Text = values[22].ToString();

                // Index = 23 Wifi Hotspot SSID 
                uiHotspotSSID.Text = values[23].ToString();

                // Index = 24 Wifi Hotspot Password
                uiHotspotPass.Text = values[24].ToString();

                // Index = 25 IP Address
                uiIPAdress.Text = values[25].ToString();

                // Index = 27 Subnetmask
                uiSubnetMask.Text = values[27].ToString();

                // Index = 26 Default Gateway
                uiDefaultGateway.Text = values[26].ToString();
                Thread.Sleep(50);
                uiRfFrequency.Value = Convert.ToUInt64(values[1]);
                try { uiSweepStart.Value = Convert.ToUInt64(values[13]); } catch { }
                try { uiSweepStop.Value = Convert.ToUInt64(values[14]); } catch { }
                try { uiSweepStep.Value = Convert.ToUInt64(values[15]); } catch { }
                
                // add text changed handlers back to inputs
                //uiRfFrequency.ValueChanged += valuesChanged;
                //uiRfAmplitude.ValueChanged += valuesChanged;

                //uiModAmDepth.ValueChanged += valuesChanged;
                //uiModFmDev.ValueChanged += valuesChanged;
                //uiModFreq.ValueChanged += valuesChanged;
                //uiModPulsePeriod.ValueChanged += valuesChanged;
                //uiModPulseWidth.ValueChanged += valuesChanged;

                //uiSweepDwell.ValueChanged += valuesChanged;
                //uiSweepStart.ValueChanged += valuesChanged;
                //uiSweepStep.ValueChanged += valuesChanged;
                //uiSweepStop.ValueChanged += valuesChanged;
            }
            catch (Exception ex)
            {
                giveError(ex);
            }
        }

        public void checkConnection()
        {
            if (!IsConnected) { MessageBox.Show("Please connect the device! "); return; }
        }

        private void uiConnectClick(object sender, RoutedEventArgs e)
        {
            if (IsConnected)
            {
                try
                {
                    port.Close();
                    uiConnect.Content = "Connect";
                    IsConnected = false;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            else
            {
                string baud = uiBaudRate.Text;
                string portVal = "";

                try
                {
                    portVal = (uiDeviceList.SelectedItem as Device).DisplayName;
                    if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }

                    try
                    {
                        port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);
                        port.DataReceived += Port_DataReceived;
                        //port.DtrEnable = true;
                        port.Open();
                        port.DiscardInBuffer();
                        port.DiscardOutBuffer();
                        IsConnected = true;
                        
                        Command.setPort(port);
                    }
                    catch
                    {
                        IsConnected = false;
                    }

                    if (IsConnected)
                    {
                        uiConnect.Content = "Disconnect";
                    }
                    else
                    {
                        uiConnect.Content = "Connect";
                        MessageBox.Show("Sorry, something went wrong :(\n We could not connect you to device");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Please connect a device");
                }
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                writeDebugPanel((sender as SerialPort).ReadExisting());
            }));
        }

        private void uiDeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).Text != "")
            {
                uiConnectClick(null, null);
            }
        }

        private void uiBaudRate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).Text != "")
            {
                uiConnectClick(null, null);
            }
        }

        public void giveError(Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            MessageBox.Show("Settings could not be sent!");
        }

        private void link_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("http://erainstruments.com");
        }

        private void uiRfFrequencyType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = 1;
            string a1 = (sender as ComboBox).SelectionBoxItem.ToString();
            switch (a1)
            {
                case "KHz": i = 1000; break;
                case "MHz": i = 1000000; break;
                case "GHz": i = 1000000000; break;
            }

            try
            {
                double val = Convert.ToDouble(uiRfFrequency.Value);
                val = val * (ulong)i;
                string a2 = ((sender as ComboBox).SelectedItem as ComboBoxItem).Content.ToString();
                switch (a2)
                {
                    case "KHz": val /= 1000; break;
                    case "MHz": val /= 1000000; break;
                    case "GHz": val /= 1000000000; break;
                }
                uiRfFrequency.Value = val;

            }
            catch (Exception ex) { Debug.WriteLine(ex); }

        }

        private void valueIncremented(object sender, NumericUpDownChangedRoutedEventArgs args)
        {
            NumericUpDown input = sender as NumericUpDown;
            input.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(input), 0, Key.Enter) { RoutedEvent = Keyboard.KeyDownEvent });    
        }

        private void valuesChanged(object sender, NumericUpDownChangedRoutedEventArgs e)
        {
            try
            {
                NumericUpDown input = sender as NumericUpDown;
                input.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(input), 0, Key.Enter) { RoutedEvent = Keyboard.KeyDownEvent });
            }
            catch (Exception ex) { Debug.WriteLine(ex); }
        }
    }
}