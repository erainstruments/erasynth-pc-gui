using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Management;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Text;

namespace Era.Synth.Control.Panel
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            ComboBox[] boxes = new ComboBox[4] { uiFreqInputType, uiStartFreqType, uiStopFreqType, uiStepFreqType };
            for (int i = 0; i < boxes.Length; i++)
            {
                boxes[i].Items.Add(new ComboBoxItem { IsSelected = false, Content = "GHz" });
                boxes[i].Items.Add(new ComboBoxItem { IsSelected = true, Content = "MHz" });
                boxes[i].Items.Add(new ComboBoxItem { IsSelected = false, Content = "KHz" });
                boxes[i].Items.Add(new ComboBoxItem { IsSelected = false, Content = "Hz" });
            }

            //
            // show unhandled error
            //
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(errorHandler);
            Closing += OnWindowClosing;

            try
            {
                WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
                ManagementEventWatcher watcher = new ManagementEventWatcher(query);
                watcher.EventArrived += new EventArrivedEventHandler(deviceChangeListener);
                watcher.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            prepareDeviceList();
        }

        public void prepareDeviceList()
        {
            uiDeviceList.Items.Clear();
            foreach (Device device in DeviceList.getAllDevices()) { uiDeviceList.Items.Add(device); }
            uiDeviceList.DisplayMemberPath = "DisplayName";
            if (uiDeviceList.Items.Count == 1) { uiDeviceList.SelectedIndex = 0; }
        }

        /// <summary>
        /// This method takes the errors which is not handles in codes direclty, and show them into screen. This does not prevent app to crash
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void errorHandler(object sender, UnhandledExceptionEventArgs args)
        {
            MessageBox.Show("An error occured : \n" + ((Exception)args.ExceptionObject).ToString());
        }
        
        /// <summary>
        /// This method checks the device changes in system and updates parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void deviceChangeListener(object sender, EventArrivedEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { prepareDeviceList(); }));
        }

        /// <summary>
        /// This method stops all working threads and kill the app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnCWSendClick(object sender, RoutedEventArgs e)
        {
            string freq = uiFreqInput.Text;
            string baud = uiBaudRate.Text;
            string type = uiFreqInputType.Text;
            string portVal = uiDeviceList.Text;
            string amp = uiAmpInput.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

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



            string ampCmd = ">L";

            if (ampVal < 0) { ampCmd += "-"; }
            else { ampCmd += "+"; }

            ampCmd += string.Format("{0:0.0}", Math.Truncate(ampVal * 10) / 10).Replace(",", "").Replace(".", "");
            ampCmd += "\r";

            port.Open();
            port.Write(freqCmd);
            port.Dispose();

            Thread.Sleep(1000);

            port.Open();
            port.Write(ampCmd);
            port.Dispose();
        }

        private void btnSWSendClick(object sender, RoutedEventArgs e)
        {
            string start_freq = uiStartFreq.Text;
            string stop_freq = uiStopFreq.Text;
            string step_freq = uiStepFreq.Text;

            string start_type = uiStartFreqType.Text;
            string stop_type = uiStopFreqType.Text;
            string step_type = uiStepFreqType.Text;

            string portVal = uiDeviceList.Text;
            string baud = uiBaudRate.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

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

            port.Open();
            port.Write(startFreqCmd);
            port.Dispose();

            Thread.Sleep(100);

            port.Open();
            port.Write(stopFreqCmd);
            port.Dispose();

            Thread.Sleep(100);

            port.Open();
            port.Write(stepFreqCmd);
            port.Dispose();

            Thread.Sleep(100);
        }

        private void btnRfSendClick(object sender, RoutedEventArgs e)
        {
            string reference = uiReference.Text;

            string portVal = uiDeviceList.Text;
            string baud = uiBaudRate.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

            string cmd = ">P";

            if (reference == "Internal") { cmd += "0"; }
            else if (reference == "External") { cmd += "1"; }

            cmd += "<?>\r";

            port.Open();
            port.Write(cmd);
            port.Dispose();

            Thread.Sleep(100);
        }

        private void uiRfInfoChecked(object sender, RoutedEventArgs e)
        {
            CheckBox rf = sender as CheckBox;
            if (rf.IsChecked ?? true) { rf.Content = "RF ON"; }
            else { rf.Content = "RF OFF"; }

            string baud = uiBaudRate.Text;
            string portVal = uiDeviceList.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

            port.Open();
            port.Write(">?\r");
            port.Dispose();
            Thread.Sleep(100);
        }

        private void uiSweepInfoChecked(object sender, RoutedEventArgs e)
        {
            CheckBox sw = sender as CheckBox;
            if (sw.IsChecked ?? true) { sw.Content = "SWEEP ON"; }
            else { sw.Content = "SWEEP OFF"; }

            string baud = uiBaudRate.Text;
            string portVal = uiDeviceList.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

            port.Open();
            port.Write(">S0\r");
            port.Dispose();
            Thread.Sleep(100);
        }

        private void uiModPulseSendClick(object sender, RoutedEventArgs e)
        {
            string source = uiModPulseSource.Text;
            string polarity = uiModPulsePloarity.Text;

            string width = uiModPulseWidth.Text;
            string width_type = uiModPulseWidthType.Text;

            string period = uiModPulsePeriod.Text;
            string period_type = uiModPulsePeriodType.Text;

            string source_cmd = ">PM0{0}<?>\r";
            string polarity_cmd = ">PM3{0}<?>\r";
            string width_cmd = ">PM1{0}\r";
            string period_cmd = ">PM2{0}\r";
            
            // set source field
            if (source == "Internal") { source_cmd.Replace("{0}","0"); }
            else { source_cmd.Replace("{0}", "1"); }
            
            // set polarity field
            if (polarity == "Normal") { polarity_cmd.Replace("{0}","0"); }
            else { polarity_cmd.Replace("{0}", "1"); }
            
            // set width field
            if (width.Contains(",") && width_type == "uS") { width = width.Substring(0, width.IndexOf(",")); }
            int width_mul = 1;
            if (width_type == "mS") { width_mul = 1000; }
            double widthVal = 1000;
            try { widthVal = Convert.ToDouble(width); }
            catch {MessageBox.Show("Please enter proper values to fields!"); return; }
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

            string portVal = uiDeviceList.Text;
            string baud    = uiBaudRate.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

            //
            // send commands
            // source_cmd
            // polarity_cmd
            // width_cmd
            // period_cmd

            port.Open();
            port.Write(source_cmd);
            port.Dispose();
            Thread.Sleep(100);

            port.Open();
            port.Write(polarity_cmd);
            port.Dispose();
            Thread.Sleep(100);

            port.Open();
            port.Write(width_cmd);
            port.Dispose();
            Thread.Sleep(100);

            port.Open();
            port.Write(period_cmd);
            port.Dispose();            
        }
        
        private void uiModPulseOnOffCheck(object sender, RoutedEventArgs e)
        {
            CheckBox rf = sender as CheckBox;
            if (rf.IsChecked ?? true) { rf.Content = "PULSE ON"; }
            else { rf.Content = "PULSE OFF"; }

            string baud = uiBaudRate.Text;
            string portVal = uiDeviceList.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

            port.Open();
            port.Write(">PM4\r");
            port.Dispose();
            Thread.Sleep(100);
        }

        private void uiModAmSendClick(object sender, RoutedEventArgs e)
        {
            string source       = uiModAmSource.Text;
            string mod_freq     = uiModAmFreq.Text;
            string mod_depth    = uiModAmDepth.Text;
            
            string source_cmd = ">AM0{0}<?>\r";
            string shape_cmd  = ">AM3{0}<?>\r";
            string freq_cmd = ">AM1{0}\r";
            string depth_cmd = ">AM2{0}\r";

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

            string portVal = uiDeviceList.Text;
            string baud = uiBaudRate.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

            //
            // send commands
            // source_cmd
            // shape_cmd
            // freq_cmd
            // depth_cmd

            port.Open();
            port.Write(source_cmd);
            port.Dispose();
            Thread.Sleep(100);

            port.Open();
            port.Write(shape_cmd);
            port.Dispose();
            Thread.Sleep(100);

            port.Open();
            port.Write(freq_cmd);
            port.Dispose();
            Thread.Sleep(100);

            port.Open();
            port.Write(depth_cmd);
            port.Dispose();
        }

        private void uiModAmOnOffCheck(object sender, RoutedEventArgs e)
        {
            CheckBox rf = sender as CheckBox;
            if (rf.IsChecked ?? true) { rf.Content = "ON"; }
            else { rf.Content = "OFF"; }

            string baud = uiBaudRate.Text;
            string portVal = uiDeviceList.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

            port.Open();
            port.Write(">AM4\r");
            port.Dispose();
            Thread.Sleep(100);
        }

        private void uiModFmSendClick(object sender, RoutedEventArgs e)
        {
            string source = uiModFmSource.Text;
            string mod_freq = uiModFmFreq.Text;
            string mod_sense = uiModFmSens.Text;
            
            string source_cmd = ">FM0{0}<?>\r";
            string freq_cmd = ">FM1{0}<?>\r";
            string sense_cmd = ">FM2{0}\r";
            
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

            port.Open();
            port.Write(source_cmd);
            port.Dispose();
            Thread.Sleep(100);

            port.Open();
            port.Write(freq_cmd);
            port.Dispose();
            Thread.Sleep(100);

            port.Open();
            port.Write(sense_cmd);
            port.Dispose();            
        }

        private void uiModFmOnOffCheck(object sender, RoutedEventArgs e)
        {
            CheckBox rf = sender as CheckBox;
            if (rf.IsChecked ?? true) { rf.Content = "ON"; }
            else { rf.Content = "OFF"; }

            string baud = uiBaudRate.Text;
            string portVal = uiDeviceList.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

            port.Open();
            port.Write(">FM4\r");
            port.Dispose();
            Thread.Sleep(100);
        }

        private void uiTriggerSendClick(object sender, RoutedEventArgs e)
        {
            string source   = uiModFmSource.Text;
            string mode     = uiTriggerMode.Text;

            string source_cmd   = ">T0{0}<?>\r";
            string mode_cmd     = ">T1{0}<?>\r";
            
            // set source field
            if (source == "Bus") { source_cmd.Replace("{0}", "0"); }
            else { source_cmd.Replace("{0}", "1"); }

            // set mode field
            if (mode == "Single") { mode_cmd.Replace("{0}", "0"); }
            else { mode_cmd.Replace("{0}", "1"); }
            
            string portVal  = uiDeviceList.Text;
            string baud     = uiBaudRate.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

            //
            // send commands
            // source_cmd
            // mode_cmd
            
            port.Open();
            port.Write(source_cmd);
            port.Dispose();
            Thread.Sleep(100);

            port.Open();
            port.Write(mode_cmd);
            port.Dispose();
            Thread.Sleep(100);
        }

        private void uiDiagSendClick(object sender, RoutedEventArgs e)
        {
            string source = uiModFmSource.Text;
            string mode = uiTriggerMode.Text;

            string source_cmd = ">T0{0}<?>\r";
            string mode_cmd = ">T1{0}<?>\r";

            // set source field
            if (source == "Bus") { source_cmd.Replace("{0}", "0"); }
            else { source_cmd.Replace("{0}", "1"); }

            // set mode field
            if (mode == "Single") { mode_cmd.Replace("{0}", "0"); }
            else { mode_cmd.Replace("{0}", "1"); }

            string portVal = uiDeviceList.Text;
            string baud = uiBaudRate.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

            //
            // send commands
            // source_cmd
            // mode_cmd

            port.Open();
            port.Write(source_cmd);
            port.Dispose();
            Thread.Sleep(100);

            port.Open();
            port.Write(mode_cmd);
            port.Dispose();
            Thread.Sleep(100);
        }

        private void uiDiagLockClick(object sender, RoutedEventArgs e)
        {
            string portVal = uiDeviceList.Text;
            string baud = uiBaudRate.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);

            port.Open();
            port.Write(">C?\r");
            uiDiagLock.Text = port.ReadLine();
            port.Dispose();
        }

        private void uiDiagTempClick(object sender, RoutedEventArgs e)
        {
            string portVal = uiDeviceList.Text;
            string baud = uiBaudRate.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);
            port.ReadTimeout = 100;
            byte[] bytes = new byte[port.ReadBufferSize];

            port.Open();
            port.Write(">T?\r");
            port.Read(bytes, 0, port.ReadBufferSize);
            uiDiagTemp.Text = port.ReadExisting().ToString().Replace("Temperature:", "");
            port.Dispose();
        }

        private void uiDiagPowerClick(object sender, RoutedEventArgs e)
        {
            string portVal = uiDeviceList.Text;
            string baud = uiBaudRate.Text;

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);
            byte[] bytes = new byte[port.ReadBufferSize];

            port.Open();
            port.Write(">RA\r");
            Thread.Sleep(20);
            port.Read(bytes, 0, bytes.Length);
            uiDiagPower.Text = Encoding.ASCII.GetString(bytes).ToString().Replace("\0", "");
            port.Dispose();
        }      

        public void readAllData()
        {
            string portVal = uiDeviceList.Text;
            string baud = uiBaudRate.Text;
            List<string> list = new List<string>();

            if (portVal.Contains("-")) { portVal = portVal.Split('-')[0].Trim(); }
            SerialPort port = new SerialPort(portVal, int.Parse(baud), Parity.None, 8, StopBits.One);
            
            // Read Frequency Value
            port.Open();
            port.Write(">R0\r");
            Thread.Sleep(20);
            byte[] freq_a = new byte[port.ReadBufferSize];
            port.Read(freq_a, 0, freq_a.Length);
            string freq  = Encoding.ASCII.GetString(freq_a).ToString().Replace("\0", "");
            int freq_val = Convert.ToInt32(freq);
            uiFreqInput.Text = freq_val.ToString();
            (uiFreqInputType.Items.GetItemAt(3) as ComboBoxItem).IsSelected = true;
            port.Dispose();
            list.Add(freq);


            // Read Amlitude Value
            port.Open();
            port.Write(">R1\r");
            Thread.Sleep(20);
            byte[] amp_a = new byte[port.ReadBufferSize];
            port.Read(amp_a, 0, amp_a.Length);
            string amp  = Encoding.ASCII.GetString(amp_a).ToString().Replace("\0", "");
            int amp_val = Convert.ToInt32(amp);
            uiAmpInput.Text = amp_val.ToString();
            port.Dispose();
            list.Add(amp);

            // Read Rf on of
            port.Open();
            port.Write(">R2\r");
            Thread.Sleep(20);
            byte[] rf_a = new byte[port.ReadBufferSize];
            port.Read(rf_a, 0, rf_a.Length);
            string rfonoff = Encoding.ASCII.GetString(rf_a).ToString().Replace("\0", "");
            if (rfonoff == "1")
            {
                uiRfInfo.Checked -= uiRfInfoChecked;
                uiRfInfo.IsChecked = true;
                uiRfInfo.Content = "RF ON";
                uiRfInfo.Unchecked += uiRfInfoChecked;
            }
            else
            {
                uiRfInfo.Unchecked -= uiRfInfoChecked;
                uiRfInfo.IsChecked = false;
                uiRfInfo.Content = "RF OFF";
                uiRfInfo.Unchecked += uiRfInfoChecked;
            }
            port.Dispose();
            list.Add(rfonoff);

            // Read Reference
            port.Open();
            port.Write(">R3\r");
            Thread.Sleep(20);
            byte[] ref_a = new byte[port.ReadBufferSize];
            port.Read(ref_a, 0, ref_a.Length);
            string reference = Encoding.ASCII.GetString(ref_a).ToString().Replace("\0", "");
            int a = Convert.ToInt32(reference);
            (uiReference.Items[a] as ComboBoxItem).IsSelected = true;
            port.Dispose();
            list.Add(reference);

            // Read Lock
            port.Open();
            port.Write(">R4\r");
            Thread.Sleep(20);
            byte[] lock_a = new byte[port.ReadBufferSize];
            port.Read(lock_a, 0, lock_a.Length);
            string locke = Encoding.ASCII.GetString(lock_a).ToString().Replace("\0", "");
            port.Dispose();
            list.Add(locke);

            //  Read Lock Reference
            port.Open();
            port.Write(">R5\r");
            Thread.Sleep(20);
            byte[] l_ref_a = new byte[port.ReadBufferSize];
            port.Read(l_ref_a, 0, l_ref_a.Length);
            string lock_ref = Encoding.ASCII.GetString(l_ref_a).ToString().Replace("\0", "");
            port.Dispose();
            list.Add(lock_ref);

            // Read Start Freq
            port.Open();
            port.Write(">R6\r");
            Thread.Sleep(20);
            byte[] start_a = new byte[port.ReadBufferSize];
            port.Read(start_a, 0, start_a.Length);
            string start_freq = Encoding.ASCII.GetString(start_a).ToString().Replace("\0", "");

            int start_val = Convert.ToInt32(start_freq);
            uiStartFreq.Text = start_val.ToString();
            (uiStartFreqType.Items[3] as ComboBoxItem).IsSelected = true;

            port.Dispose();
            list.Add(start_freq);

            // Read Stop Freq
            port.Open();
            port.Write(">R7\r");
            Thread.Sleep(20);
            byte[] stop_a = new byte[port.ReadBufferSize];
            port.Read(stop_a, 0, stop_a.Length);
            string stop_freq = Encoding.ASCII.GetString(stop_a).ToString().Replace("\0", "");

            int stop_val = Convert.ToInt32(stop_freq);
            uiStopFreq.Text = stop_val.ToString();
            (uiStopFreqType.Items[3] as ComboBoxItem).IsSelected = true;

            port.Dispose();
            list.Add(stop_freq);

            // Read Step Freq
            port.Open();
            port.Write(">R8\r");
            Thread.Sleep(20);
            byte[] step_a = new byte[port.ReadBufferSize];
            port.Read(step_a, 0, step_a.Length);
            string step_freq = Encoding.ASCII.GetString(step_a).ToString().Replace("\0", "");

            int step_val = Convert.ToInt32(step_freq);
            uiStepFreq.Text = step_val.ToString();
            (uiStepFreqType.Items[3] as ComboBoxItem).IsSelected = true;

            port.Dispose();
            list.Add(step_freq);

            // Read Dwell Time
            port.Open();
            port.Write(">R9\r");
            Thread.Sleep(20);
            byte[] dw = new byte[port.ReadBufferSize];
            port.Read(dw, 0, dw.Length);
            string dwell_time = Encoding.ASCII.GetString(dw).ToString().Replace("\0", "");
            port.Dispose(); list.Add(dwell_time);

            // Read Current
            port.Open();
            port.Write(">RA\r");
            Thread.Sleep(20);
            byte[] r_c = new byte[port.ReadBufferSize];
            port.Read(r_c, 0, r_c.Length);
            string current = Encoding.ASCII.GetString(r_c).ToString().Replace("\0", "");
            uiDiagPower.Text = current;
            port.Dispose(); list.Add(current);

            // Read Volt
            port.Open();
            port.Write(">RB\r");
            Thread.Sleep(20);
            byte[] r_v = new byte[port.ReadBufferSize];
            port.Read(r_v, 0, r_v.Length);
            string voltage = Encoding.ASCII.GetString(r_v).ToString().Replace("\0", "");
            port.Dispose(); list.Add(voltage);

            foreach (string s in list)
            {
                Debug.WriteLine(s.ToString().Trim() + " = ");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            readAllData();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            readAllData();
        }
    }
}
