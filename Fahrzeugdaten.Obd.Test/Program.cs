using System;

namespace Fahrzeugdaten.Obd.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Zugriff.InitPi();

            Zugriff.Abfragen("Drehzahl");
            Zugriff.Abfragen("Wassertemperatur");
            Zugriff.Abfragen("Gaspedalstellung");
            Zugriff.Abfragen("Kraftstofffuellung");
            
            Zugriff.StehendeAusgabeAktiv = true;
            Zugriff.Laufpause = 0;
            Zugriff.Start();

            System.Threading.Thread.Sleep(30000);

            Obd.Zugriff.Halt();
        }
    }
}
