using System;
using System.Collections.Generic;
using MonoSerialPort;

namespace Fahrzeugdaten.Obd
{
    public static class Zugriff
    {
        public static OpenNETCF.MQTT.MQTTClient client;

        private static bool freigabe = false;
        private static byte[] buffer = new byte[64];
        private static int buffer_index = 0;

        public static bool DebugAusgabe = false;
        public static bool StehendeAusgabeAktiv = false;

        private static SerialPortInput port = new MonoSerialPort.SerialPortInput("/dev/serial0");

        public static void Ausgabe(string s)
        {
            Console.WriteLine(System.DateTime.Now.ToString("dd:MM:yyyy hh:mm:ss.fff") + ": " + s);
        }

        private static void StehendeAusgabe()
        {
            System.Console.Clear();
            System.Console.WriteLine("Aktualisierung: " + DateTime.Now.ToString("dd:MM:yyyy hh:mm:ss.fff"));
            foreach (var eintrag in Abzufragen)
            {
                Console.WriteLine(eintrag.benennung + " = " + eintrag.wert_string + " " + eintrag.einheit);
            }
        }

        private static List<Obd.Messgroesse> Abzufragen = new List<Messgroesse>
        {
            //Fahrzeugdaten.Obd.Erfassung.Groessen["Drehzahl"]
        };

        public static void Abfragen(string eintrag)
        {
            try
            {
                Abzufragen.Add(Erfassung.Groessen[eintrag]);
            }
            catch (Exception e)
            {
                if (Zugriff.DebugAusgabe) Zugriff.Ausgabe("Abfragen Exception " + e.ToString());
            }            
        }

        public static void InitPi()
        {
            Unosquare.RaspberryIO.Pi.Init<Unosquare.WiringPi.BootstrapWiringPi>();

            var out1 = Unosquare.RaspberryIO.Pi.Gpio[18];
            var out2 = Unosquare.RaspberryIO.Pi.Gpio[23];
            out1.PinMode = Unosquare.RaspberryIO.Abstractions.GpioPinDriveMode.Output;
            out2.PinMode = Unosquare.RaspberryIO.Abstractions.GpioPinDriveMode.Output;

            out1.Write(false);
            System.Threading.Thread.Sleep(100);
            out2.Write(false);
            System.Threading.Thread.Sleep(100);
            out1.Write(true);
            System.Threading.Thread.Sleep(100);
        }

        private static void pDataReceived(object sender, MessageReceivedEventArgs e)
        {
            e.Data.CopyTo(buffer, buffer_index);
            buffer_index += e.Data.Length;

            if (e.Data[e.Data.Length - 1] == 0x3E)
            {
                string buffer_string = BitConverter.ToString(buffer);//.Substring(0, buffer_index);
                string ascii_string = System.Text.Encoding.ASCII.GetString(buffer);
                if (DebugAusgabe) Ausgabe("Lese " + buffer_string + " : " + ascii_string);

                Erfassung.MessgroessenVerarbeitung(ascii_string);

                if (StehendeAusgabeAktiv) StehendeAusgabe();

                for (int i = 0; i < buffer_index; ++i) buffer[i] = 0;
                buffer_index = 0;

                freigabe = true;
            }
        }

        private static void Senden(String s, bool warte_freigabe = true)
        {
            if (DebugAusgabe) Ausgabe("Schreibe String " + s);
            Senden(System.Text.Encoding.ASCII.GetBytes(s), warte_freigabe);
        }

        private static void Senden(byte[] b, bool warte_freigabe = true)
        {
            while (!freigabe && warte_freigabe) { }

            string s = BitConverter.ToString(b);
            if (DebugAusgabe) Ausgabe("Schreibe " + s);

            byte[] b_tmp = new byte[b.Length + 1];
            b.CopyTo(b_tmp, 0);
            b_tmp[b.Length] = 0x0d;

            port.SendMessage(b_tmp);

            freigabe = false;
        }

        private static bool Lauf = false;
        public static int Laufpause = 100;

        private static void Routine()
        {
            port.MessageReceived += pDataReceived;

            port.Connect();

            while (!port.IsConnected) { }

            if (DebugAusgabe) Ausgabe("Verbunden");

            //Senden("ATP3", false); //Modus erzwingen

            if(Laufpause > 0)
            {
                while (Lauf)
                {
                    foreach (var eintrag in Abzufragen) Senden(eintrag.zugriff);
                    System.Threading.Thread.Sleep(Laufpause);
                }
            }
            else
            {
                while (Lauf)
                {
                    foreach (var eintrag in Abzufragen) Senden(eintrag.zugriff);
                }
            }

            port.Disconnect();
        }

        private static System.Threading.Thread thr = new System.Threading.Thread(Routine);

        public static void Start()
        {
            client = new OpenNETCF.MQTT.MQTTClient("127.0.0.1", 1883);
            Zugriff.client.Connect("Fahrzeugdaten.Obd");
            Lauf = true;
            thr.Start();
        }

        public static void Halt()
        {
            Zugriff.client.Disconnect();
            Lauf = false;
            thr.Join();
        }
    }   
}