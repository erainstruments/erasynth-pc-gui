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

namespace Era.Synth.Control.Panel
{
    public partial class Window1
    {
        public bool IsConnected = false;
        bool isopen = false;

        SerialPort port;

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
            
        }

        private void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            //uiBaudRate.SelectionChanged += uiBaudRate_SelectionChanged;
        }

        public void prepareDeviceList()
        {
            uiDeviceList.Items.Clear();
            foreach (Device device in DeviceList.getAllDevices()) { uiDeviceList.Items.Add(device); }
            uiDeviceList.DisplayMemberPath = "DisplayName";
            if (uiDeviceList.Items.Count == 1) { uiDeviceList.SelectedIndex = 0; }
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

        public void readAllData()
        {
            List<string> list = new List<string>();
            
            
            // Read Frequency Value
            port.Write(">R0\r");
            Thread.Sleep(20);

            byte[] freq_a = new byte[port.ReadBufferSize];
            port.Read(freq_a, 0, freq_a.Length);
            string freq = Encoding.ASCII.GetString(freq_a).ToString().Replace("\0", "");
            int freq_val = Convert.ToInt32(freq);
            uiRfFrequency.Value = freq_val;
            

            // Read Amlitude Value
            port.Write(">R1\r");
            Thread.Sleep(20);
            byte[] amp_a = new byte[port.ReadBufferSize];
            port.Read(amp_a, 0, amp_a.Length);
            string amp = Encoding.ASCII.GetString(amp_a).ToString().Replace("\0", "");
            int amp_val = Convert.ToInt32(amp);
            uiRfAmplitude.Value = amp_val;


            // Read Rf on of
            port.Write(">R2\r");
            Thread.Sleep(20);
            byte[] rf_a = new byte[port.ReadBufferSize];
            port.Read(rf_a, 0, rf_a.Length);
            string rfonoff = Encoding.ASCII.GetString(rf_a).ToString().Replace("\0", "");
            if (rfonoff == "1")
            {
                uiRfOnOff.Checked -= uiRfOnOffChecked;
                uiRfOnOff.IsChecked = true;
                uiRfOnOff.Unchecked += uiRfOnOffChecked;
            }
            else
            {
                uiRfOnOff.Unchecked -= uiRfOnOffChecked;
                uiRfOnOff.IsChecked = false;
                uiRfOnOff.Unchecked += uiRfOnOffChecked;
            }
            

            // Read Reference
            port.Write(">R3\r");
            Thread.Sleep(20);
            byte[] ref_a = new byte[port.ReadBufferSize];
            port.Read(ref_a, 0, ref_a.Length);
            string reference = Encoding.ASCII.GetString(ref_a).ToString().Replace("\0", "");
            int a = Convert.ToInt32(reference);
            (uiReference.Items[a] as ComboBoxItem).IsSelected = true;


            // Read Lock
            port.Write(">R4\r");
            Thread.Sleep(20);
            byte[] lock_a = new byte[port.ReadBufferSize];
            port.Read(lock_a, 0, lock_a.Length);
            string locke = Encoding.ASCII.GetString(lock_a).ToString().Replace("\0", "");
            

            //  Read Lock Reference
            port.Write(">R5\r");
            Thread.Sleep(20);
            byte[] l_ref_a = new byte[port.ReadBufferSize];
            port.Read(l_ref_a, 0, l_ref_a.Length);
            string lock_ref = Encoding.ASCII.GetString(l_ref_a).ToString().Replace("\0", "");
            

            // Read Start Freq
            port.Write(">R6\r");
            Thread.Sleep(20);
            byte[] start_a = new byte[port.ReadBufferSize];
            port.Read(start_a, 0, start_a.Length);
            string start_freq = Encoding.ASCII.GetString(start_a).ToString().Replace("\0", "");

            int start_val = Convert.ToInt32(start_freq);
            uiSweepStart.Value = start_val;
            uiSweepStartType.SelectedIndex = 0;


            // Read Stop Freq
            port.Write(">R7\r");
            Thread.Sleep(20);
            byte[] stop_a = new byte[port.ReadBufferSize];
            port.Read(stop_a, 0, stop_a.Length);
            string stop_freq = Encoding.ASCII.GetString(stop_a).ToString().Replace("\0", "");
            int stop_val = Convert.ToInt32(stop_freq);
            uiSweepStop.Value = stop_val;
            uiSweepStopType.SelectedIndex = 0;

            
            // Read Step Freq
            port.Write(">R8\r");
            Thread.Sleep(20);
            byte[] step_a = new byte[port.ReadBufferSize];
            port.Read(step_a, 0, step_a.Length);
            string step_freq = Encoding.ASCII.GetString(step_a).ToString().Replace("\0", "");
            int step_val = Convert.ToInt32(step_freq);
            uiSweepStep.Value = step_val;
            uiSweepStepType.SelectedIndex = 0;


            // Read Dwell Time
            port.Write(">R9\r");
            Thread.Sleep(20);
            byte[] dw = new byte[port.ReadBufferSize];
            port.Read(dw, 0, dw.Length);
            string dwell_time = Encoding.ASCII.GetString(dw).ToString().Replace("\0", "");
            
            // Read Current
            port.Write(">RA\r");
            Thread.Sleep(20);
            byte[] r_c = new byte[port.ReadBufferSize];
            port.Read(r_c, 0, r_c.Length);
            string aaa = Encoding.ASCII.GetString(r_c).ToString().Replace("\0", "").Replace(".", ",");
            double current = double.Parse(aaa);
            
            // Read Volt
            port.Write(">RB\r");
            Thread.Sleep(20);
            byte[] r_v = new byte[port.ReadBufferSize];
            port.Read(r_v, 0, r_v.Length);
            double voltage = double.Parse(Encoding.ASCII.GetString(r_v).ToString().Replace("\0", "").Replace(".",","));

            double watt = current * voltage;
            uiDiagPower.Text = watt.ToString("0.00");

        }

        #region Settings Tab

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
                catch(Exception ex)
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
                        port.Open();
                        IsConnected = true;
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

        #endregion  

        #region CW Tab

        private void uiRfSendClick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected)
            {
                MessageBox.Show("Please Connect the Device First!");
                return;
            }

            string freq = uiRfFrequency.Value.ToString();
            string type = uiRfFrequencyType.Text;
            string amp  = uiRfAmplitude.Value.ToString();
            
            int multiplier = 1;
            if (type == "KHz") { multiplier = 1000; }
            else if (type == "MHz") { multiplier = 1000000; }
            else if (type == "GHz") { multiplier = 1000000000; }

            ulong freqVal = 250000;
            double ampVal = -60.0;

            try
            {
                freqVal = Convert.ToUInt64(freq);
                freqVal = freqVal * (ulong)multiplier;
                ampVal = Convert.ToDouble(amp);
            }
            catch
            {
                MessageBox.Show("Please enter proper numerical values");
                return;
            }

            if (freqVal < 250000) { MessageBox.Show("Frequency value can not be less then 250 KHz"); return; }
            if (freqVal > 15000000000) { MessageBox.Show("Frequency value can not be greater then 15 GHz"); return; }

            string freqCmd = ">F" + freqVal.ToString() + "\r";

            if (ampVal < -60.0 || ampVal > 20.0) { MessageBox.Show("Amplitude must be between -60 dBm and +20 dBm"); return; }

            string ampCmd = ">A";

            if (ampVal > 0) { ampCmd += "+"; }
            
            ampCmd += string.Format("{0:0.0}", Math.Truncate(ampVal * 10) / 10).Replace(",", "").Replace(".", "");
            ampCmd += "\r";

            if (IsConnected)
            {
                try
                {
                    port.Write(freqCmd);
                    Debug.WriteLine(freqCmd + " = sent");
                    Thread.Sleep(100);
                    port.Write(ampCmd);
                    Debug.WriteLine(ampCmd + " = sent");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }
        }

        private void uiRfOnOffChecked(object sender, RoutedEventArgs e)
        {
            if (!IsConnected)
            {
                MessageBox.Show("Please Connect the Device First!");
                return;
            }

            ToggleSwitch rf = sender as ToggleSwitch;
            if (rf.IsChecked ?? true) { rf.Content = "ON"; }
            else { rf.Content = "OFF"; }
            
            if (IsConnected)
            {
                try
                {
                    port.Write(">?\r");
                    Debug.WriteLine(">?\r" + " = sent");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }
            
        }

        #endregion

        #region Sweep Tab
        
        private void uiSweepSend_Click(object sender, RoutedEventArgs e)
        {
            string start_freq = uiSweepStart.Value.ToString();
            string stop_freq  = uiSweepStop.Value.ToString();
            string step_freq  = uiSweepStep.Value.ToString();

            string start_type = uiSweepStartType.Text;
            string stop_type  = uiSweepStopType.Text;
            string step_type  = uiSweepStepType.Text;

            int start_m = 1;
            if (start_type == "KHz") { start_m = 1000; }
            else if (start_type == "MHz") { start_m = 1000000; }
            else if (start_type == "GHz") { start_m = 1000000000; }

            int stop_m = 1;
            if (stop_type == "KHz") { stop_m = 1000; }
            else if (stop_type == "MHz") { stop_m = 1000000; }
            else if (stop_type == "GHz") { stop_m = 1000000000; }

            int step_m = 1;
            if (step_type == "KHz") { step_m = 1000; }
            else if (step_type == "MHz") { step_m = 1000000; }
            else if (step_type == "GHz") { step_m = 1000000000; }

            ulong startFreqVal = 250000;
            ulong stopFreqVal = 250000;
            ulong stepFreqVal = 1;

            try
            {
                startFreqVal = Convert.ToUInt64(start_freq);
                startFreqVal = startFreqVal * (ulong)start_m;

                stopFreqVal = Convert.ToUInt64(stop_freq);
                stopFreqVal = stopFreqVal * (ulong)stop_m;

                stepFreqVal = Convert.ToUInt64(step_freq);
                stepFreqVal = stepFreqVal * (ulong)step_m;
            }
            catch
            {
                MessageBox.Show("Please enter proper numerical values");
                return;
            }

            if (startFreqVal < 250000 || startFreqVal > 15000000000) { MessageBox.Show("Start Frequency must be between 250 KHz and 15 GHz"); return; }
            if (stopFreqVal < startFreqVal) { MessageBox.Show("Stop Frequency must be greater then Start Frequency"); return; }
            if (((stopFreqVal - startFreqVal) / stepFreqVal) > 32001) { MessageBox.Show("Sweeping is too much! Make larger steps"); return; }
            if (stepFreqVal < 1 || stepFreqVal > 15000000000) { MessageBox.Show("Step Frequency must be between 1 Hz and 15 GHz"); return; }

            string startFreqCmd = ">S1" + startFreqVal.ToString() + "\r";
            string stopFreqCmd = ">S2" + stopFreqVal.ToString() + "\r";
            string stepFreqCmd = ">S3" + stepFreqVal.ToString() + "\r";

            if (IsConnected)
            {
                try
                {
                    port.Write(startFreqCmd);
                    Debug.WriteLine(startFreqCmd + " = sent");
                    Thread.Sleep(100);

                    port.Write(stopFreqCmd);
                    Debug.WriteLine(stopFreqCmd + " = sent");
                    Thread.Sleep(100);

                    port.Write(stepFreqCmd);
                    Debug.WriteLine(stepFreqCmd + " = sent");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }
        }

        private void uiSweepOnOff_Checked(object sender, RoutedEventArgs e)
        {            
            if (IsConnected)
            {
                try
                {
                    port.Write(">S0\r");
                    Debug.WriteLine(">S0 = sent");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }
        }

        #endregion

        #region Reference

        private void uiReferenceSend_Click(object sender, RoutedEventArgs e)
        {
            string reference = uiReference.Text;

            string cmd = ">P1";

            if (reference == "Internal") { cmd += "0"; }
            else if (reference == "External") { cmd += "1"; }

            cmd += "<?>\r";
            
            if (IsConnected)
            {
                try
                {
                    port.Write(cmd);
                    Debug.WriteLine(cmd + " = sent");                    
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }
        }

        #endregion

        #region Modulation

        private void uiModPulseSend_Click(object sender, RoutedEventArgs e)
        {
            string source   = uiModPulseSource.Text;
            string polarity = uiModPulsePolarity.Text;

            string width      = uiModPulseWidth.Value.ToString();
            string width_type = uiModPulseWidthType.Text;

            string period      = uiModPulsePeriod.Value.ToString();
            string period_type = uiModPulsePeriodType.Text;

            string source_cmd = ">PM0{0}<?>\r";
            string polarity_cmd = ">PM3{0}<?>\r";
            string width_cmd = ">PM1{0}\r";
            string period_cmd = ">PM2{0}\r";

            // set source field
            if (source == "Internal") { source_cmd.Replace("{0}", "0"); }
            else { source_cmd.Replace("{0}", "1"); }

            // set polarity field
            if (polarity == "Normal") { polarity_cmd.Replace("{0}", "0"); }
            else { polarity_cmd.Replace("{0}", "1"); }

            // set width field
            if (width.Contains(",") && width_type == "uS") { width = width.Substring(0, width.IndexOf(",")); }
            int width_mul = 1;
            if (width_type == "mS") { width_mul = 1000; }
            double widthVal = 1000;
            try { widthVal = Convert.ToDouble(width); }
            catch { MessageBox.Show("Please enter proper values to fields!"); return; }
            widthVal *= width_mul;
            if ((int)widthVal < 1000 || (int)widthVal > 10000000) { MessageBox.Show("Pulse Width must be between 1ms and 10s"); return; }
            width_cmd = width_cmd.Replace("{0}", widthVal.ToString());

            // set period field
            if (period.Contains(",") && period_type == "uS") { period = period.Substring(0, period.IndexOf(",")); }
            int period_mul = 1;
            if (period_type == "mS") { period_mul = 1000; }
            double periodVal = 1000;
            try { periodVal = Convert.ToDouble(period); }
            catch { MessageBox.Show("Please enter proper values to fields!"); return; }
            periodVal *= period_mul;
            if ((int)periodVal < 1000 || (int)periodVal > 10000000) { MessageBox.Show("Pulse Width must be between 1ms and 10s"); return; }
            period_cmd = period_cmd.Replace("{0}", periodVal.ToString());

            //
            // send commands
            // source_cmd
            // polarity_cmd
            // width_cmd
            // period_cmd

            if (IsConnected)
            {
                try
                {
                    port.Write(source_cmd);
                    Debug.WriteLine(source_cmd + " = sent");
                    Thread.Sleep(100);

                    port.Write(polarity_cmd);
                    Debug.WriteLine(polarity_cmd + " = sent");
                    Thread.Sleep(100);

                    port.Write(width_cmd);
                    Debug.WriteLine(width_cmd + " = sent");
                    Thread.Sleep(100);

                    port.Write(period_cmd);
                    Debug.WriteLine(period_cmd + " = sent");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }            
        }

        private void uiModPulseOnOff_Checked(object sender, RoutedEventArgs e)
        {
            if (IsConnected)
            {
                try
                {
                    port.Write(">PM4\r");
                    Debug.WriteLine(">PM4\r = sent");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }
        }



        private void uiModAmOnOff_Checked(object sender, RoutedEventArgs e)
        {
            if (IsConnected)
            {
                try
                {
                    port.Write(">AM4\r");
                    Debug.WriteLine(">AM4\r = sent");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }
        }

        private void uiModAmSend_Click(object sender, RoutedEventArgs e)
        {
            string source       = uiModAmSource.Text;
            string mod_freq     = uiModAmFreq.Value.ToString();
            string mod_depth    = uiModAmDepth.Value.ToString();

            string source_cmd  = ">AM0{0}<?>\r";
            string shape_cmd   = ">AM3{0}<?>\r";
            string freq_cmd    = ">AM1{0}\r";
            string depth_cmd   = ">AM2{0}\r";

            // set source field
            if (source == "Internal") { source_cmd.Replace("{0}", "0"); }
            else { source_cmd.Replace("{0}", "1"); }

            // set shape field
            if (uiModAmSine.IsChecked ?? true) { shape_cmd.Replace("{0}", "0"); }
            if (uiModAmSquare.IsChecked ?? true) { shape_cmd.Replace("{0}", "1"); }
            if (uiModAmTriangle.IsChecked ?? true) { shape_cmd.Replace("{0}", "2"); }

            // set freq field
            int freqVal = 1;

            try { freqVal = Convert.ToInt32(mod_freq); }
            catch { MessageBox.Show("Please enter proper values to fields!"); return; }

            if (freqVal < 1 || freqVal > 10000) { MessageBox.Show("Modulation Frequncy must be between 1Hz and 10KHz"); return; }
            freq_cmd = freq_cmd.Replace("{0}", freqVal.ToString());


            // set depth field
            int depthVal = 1;

            try { depthVal = Convert.ToInt32(mod_depth); }
            catch { MessageBox.Show("Please enter proper values to fields!"); return; }

            if (depthVal < 1 || depthVal > 99) { MessageBox.Show("Modulation Depth must be between 1% and 99%"); return; }
            depth_cmd = depth_cmd.Replace("{0}", depthVal.ToString());

            //
            // send commands
            // source_cmd
            // shape_cmd
            // freq_cmd
            // depth_cmd

            if (IsConnected)
            {
                try
                {
                    port.Write(source_cmd);
                    Debug.WriteLine(source_cmd + " = sent");
                    Thread.Sleep(100);

                    port.Write(shape_cmd);
                    Debug.WriteLine(shape_cmd + " = sent");
                    Thread.Sleep(100);

                    port.Write(freq_cmd);
                    Debug.WriteLine(freq_cmd + " = sent");
                    Thread.Sleep(100);

                    port.Write(depth_cmd);
                    Debug.WriteLine(depth_cmd + " = sent");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }
        }



        private void uiModFmOnOff_Checked(object sender, RoutedEventArgs e)
        {
            if (IsConnected)
            {
                try
                {
                    port.Write(">FM4\r");
                    Debug.WriteLine(">FM4\r = sent");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }
        }

        private void uiModFmSend_Click(object sender, RoutedEventArgs e)
        {
            string source    = uiModFmSource.Text;
            string mod_freq  = uiModFmFreq.Value.ToString();
            string mod_sense = uiModFmSense.Value.ToString();

            string source_cmd   = ">FM0{0}<?>\r";
            string freq_cmd     = ">FM1{0}<?>\r";
            string sense_cmd    = ">FM2{0}\r";

            // set source field
            if (source == "Internal") { source_cmd.Replace("{0}", "0"); }
            else { source_cmd.Replace("{0}", "1"); }

            // set freq field
            int freqVal = 1;

            try { freqVal = Convert.ToInt32(mod_freq); }
            catch { MessageBox.Show("Please enter proper values to fields!"); return; }

            if (freqVal < 1 || freqVal > 10000) { MessageBox.Show("Modulation Frequncy must be between 1Hz and 10KHz"); return; }
            freq_cmd = freq_cmd.Replace("{0}", freqVal.ToString());

            // set sensitivity field
            double senseVal = 1;
            int sense_mul = 1;
            if (uiModFmSenseType.Text == "KHz") { sense_mul = 1000; }
            if (uiModFmSenseType.Text == "MHz") { sense_mul = 1000000; }

            try { senseVal = Convert.ToDouble(mod_sense); }
            catch { MessageBox.Show("Please enter proper values to fields!"); return; }

            senseVal *= sense_mul;

            if ((int)senseVal < 1 || (int)senseVal > 10000000) { MessageBox.Show("Modulation Sensitivity must be between 1Hz and 10MHz"); return; }
            sense_cmd = sense_cmd.Replace("{0}", senseVal.ToString().Replace(",", ""));

            string portVal = uiDeviceList.Text;
            string baud = uiBaudRate.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

            //
            // send commands
            // source_cmd
            // freq_cmd
            // sense_cmd
            if (IsConnected)
            {
                try
                {
                    port.Write(source_cmd);
                    Debug.WriteLine(source_cmd + " = sent");
                    Thread.Sleep(100);

                    port.Write(freq_cmd);
                    Debug.WriteLine(freq_cmd + " = sent");
                    Thread.Sleep(100);

                    port.Write(sense_cmd);
                    Debug.WriteLine(sense_cmd + " = sent");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }
        }

        #endregion

        #region Trigger

        private void uiTrigSend_Click(object sender, RoutedEventArgs e)
        {
            string source = uiTrigSource.Text;
            string mode   = uiTrigMode.Text;

            string source_cmd = ">T0{0}<?>\r";
            string mode_cmd   = ">T1{0}<?>\r";

            // set source field
            if (source == "Bus") { source_cmd.Replace("{0}", "0"); }
            else { source_cmd.Replace("{0}", "1"); }

            // set mode field
            if (mode == "Single") { mode_cmd.Replace("{0}", "0"); }
            else { mode_cmd.Replace("{0}", "1"); }

            //
            // send commands
            // source_cmd
            // mode_cmd

            if (IsConnected)
            {
                try
                {
                    port.Write(source_cmd);
                    Debug.WriteLine(source_cmd + " = sent");
                    Thread.Sleep(100);

                    port.Write(mode_cmd);
                    Debug.WriteLine(mode_cmd + " = sent");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }
        }

        #endregion

        #region Diagnostics

        private void uiDiagLockButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsConnected)
            {
                try
                {
                    byte[] bytes = new byte[port.ReadBufferSize];

                    port.Write(">L?\r");
                    Thread.Sleep(100);
                    port.Read(bytes, 0, port.ReadBufferSize);
                    uiDiagLock.Text = Encoding.ASCII.GetString(bytes).Replace("\0", "");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }
        }

        private void uiDiagTempButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsConnected)
            {
                try
                {
                    byte[] bytes = new byte[port.ReadBufferSize];
                    
                    port.Write(">T?\r");
                    Thread.Sleep(100);
                    port.Read(bytes, 0, port.ReadBufferSize);
                    uiDiagTemp.Text = Encoding.ASCII.GetString(bytes).Replace("\0", "").Replace("Temperature: ", "");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }
        }

        private void uiDiagPowerButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsConnected)
            {
                try
                {
                    byte[] bytes = new byte[port.ReadBufferSize];

                    port.Write(">RA\r");
                    Thread.Sleep(20);
                    port.Read(bytes, 0, bytes.Length);
                    string amps = Encoding.ASCII.GetString(bytes).Replace("\0","").Replace(".",",");

                    port.Write(">RB\r");
                    Thread.Sleep(20);
                    port.Read(bytes, 0, bytes.Length);
                    string volts = Encoding.ASCII.GetString(bytes).Replace("\0","").Replace(".",",");
                    
                    double amp  = double.Parse(amps);
                    double volt = double.Parse(volts);

                    double watt = amp * volt;


                    uiDiagPower.Text = amps + " A x " + volts + " V = " + watt.ToString("0.00") + " W";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    MessageBox.Show("Settings could not be sent!");
                }
            }
            else
            {
                MessageBox.Show("Please Connect the Device First!");
            }            
        }


        #endregion

        #region Debug

        #endregion

        private void uiReadAll_Click(object sender, RoutedEventArgs e)
        {
            readAllData();
            //try
            //{
            //    readAllData();
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.ToString());
            //}
        }
    }
}