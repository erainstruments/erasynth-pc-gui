using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Era.Synth.Control.Panel
{
    public static class Command
    {
        public static string RF_ON                  = ">P01\r";
        public static string RF_OFF                 = ">P00\r";

        public static string MODULATION_ON          = ">MS1\r";
        public static string MODULATION_OFF         = ">MS0\r";

        public static string MODULATION_INTERNAL    = ">M10\r";
        public static string MODULATION_EXTERNAL    = ">M11\r";
        public static string MODULATION_MICROPHONE  = ">M12\r";

        public static string MODULATION_AM          = ">M02\r";
        public static string MODULATION_NBFM        = ">M00\r";
        public static string MODULATION_WBFM        = ">M01\r";
        public static string MODULATION_PULSE       = ">M03\r";

        public static string MODULATION_SINE        = ">M20\r";
        public static string MODULATION_TRIANGLE    = ">M21\r";
        public static string MODULATION_RAMP        = ">M22\r";
        public static string MODULATION_SQUARE      = ">M23\r";
        
        public static string SWEEP_ON               = ">SS1\r";
        public static string SWEEP_OFF              = ">SS0\r";
        public static string SWEEP_FREE_RUN         = ">S00\r";
        public static string SWEEP_EXTERNAL         = ">S01\r";
         
        public static string REFERENCE_INTERNAL     = ">P10\r";
        public static string REFERENCE_EXTERNAL     = ">P11\r";
        public static string REFERENCE_TCXO         = ">P50\r";
        public static string REFERENCE_OCXO         = ">P51\r";

        public static string READ_DIAGNOSTIC        = ">RD\r";
        public static string TEMPERATURE            = ">RT\r";
        public static string PLL_XTAL               = ">R0\r";
        public static string PLL_LMX1               = ">R1\r";
        public static string PLL_LMX2               = ">R2\r";
        public static string VOLTAGE                = ">RV\r";
        public static string CURRENT                = ">RC\r";
        public static string RSSI                   = ">RR\r";
        public static string EM_VERSION             = ">RE\r";

        public static string ESP_ON                 = ">PE01\r";
        public static string ESP_OFF                = ">PE00\r";

        public static string ESP_UPLOAD_MODE        = ">U\r";
        public static string DEBUG_MESSAGES_ON      = ">PD1\r";
        public static string DEBUG_MESSAGES_OFF     = ">PD0\r";
        public static string FACTORY_RESET          = ">PR\r";
        public static string PRESET                 = ">PP\r";

        public static string READ_ALL = ">RA\r";

        public static string FREQUENCY = ">F{0}\r";
        public static string AMPLITUDE = ">A{0}\r";

        public static string MODULATION_FREQ = ">M3{0}\r";
        public static string MODULATION_AM_DEPTH = ">M5{0}\r";
        public static string MODULATION_FM_DEV = ">M4{0}\r";
        public static string MODULATION_PULSE_PERIOD = ">M6{0}\r";
        public static string MODULATION_PULSE_WIDTH = ">M7{0}\r";

        public static string SWEEP_START = ">S1{0}\r";
        public static string SWEEP_STOP = ">S2{0}\r";
        public static string SWEEP_STEP = ">S3{0}\r";
        public static string SWEEP_DWELL = ">S4{0}\r";

        public static string STATION_SSID = ">PES0{0}\r";
        public static string STATION_PASS = ">PEP0{0}\r";
        public static string HOTSPOT_SSID = ">PES1{0}\r";
        public static string HOTSPOT_PASS = ">PEP1{0}\r";

        public static string IP_ADDRESS      = ">PEI{0}\r";
        public static string SUBNET_MASK     = ">PEN{0}\r";
        public static string DEFAULT_GATEWAY = ">PEG{0}\r";

        public static string WIFI_STATION = ">PEW0\r";
        public static string WIFI_HOTSPOT = ">PEW1\r";

        static SerialPort port;


        public static void setPort(SerialPort input) { port = input; }

        public static void send(string cmd)
        {
            port.Write(cmd);
            System.Diagnostics.Debug.WriteLine(cmd.Replace("\r","") + " sent");
        }

        public static void send(string cmd, string value)
        {
            cmd = string.Format(cmd, value);
            port.Write(cmd);
            System.Diagnostics.Debug.WriteLine(cmd + " sent");
        }
    }
}