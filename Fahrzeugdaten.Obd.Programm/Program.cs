using System;

namespace Fahrzeugdaten.Obd.Programm
{
    class Program
    {
        public class Konfiguration
        {
            public System.Collections.Generic.List<string> Werte;

            public Konfiguration()
            {
                Werte = new System.Collections.Generic.List<string>();
            }
        }

        static void Main(string[] args)
        {
            Zugriff.InitPi();

            var konfig = System.IO.File.ReadAllText("konfig.json");
            var konfig_json = Newtonsoft.Json.JsonConvert.DeserializeObject<Konfiguration>(konfig);

            foreach(var w in konfig_json.Werte) Zugriff.Abfragen(w);

            Zugriff.StehendeAusgabeAktiv = true;
            Zugriff.DebugAusgabe = false;
            Zugriff.Laufpause = 0;
            Zugriff.Start();

            while (true)
            {

            }
        }
    }
}
