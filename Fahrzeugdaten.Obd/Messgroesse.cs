/*Copyright 2020 D.Breunig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.*/

using System;
using System.Collections.Generic;

namespace Fahrzeugdaten.Obd
{
    public static class Erfassung
    {
        public static Dictionary<String, Messgroesse> Groessen = new Dictionary<string, Messgroesse>
        {
            { "PID1", new Messgroesse(4, VerfuegbarePID, "01 00", "41 00", "Verfuegbare PID1", "") },
            { "PID2", new Messgroesse(4, VerfuegbarePID, "01 20", "41 20", "Verfuegbare PID2", "") },
            { "PID3", new Messgroesse(4, VerfuegbarePID, "01 40", "41 40", "Verfuegbare PID3", "") },
            { "PID4", new Messgroesse(4, VerfuegbarePID, "01 60", "41 60", "Verfuegbare PID4", "") },
            { "PID5", new Messgroesse(4, VerfuegbarePID, "01 80", "41 80", "Verfuegbare PID5", "") },
            { "PID6", new Messgroesse(4, VerfuegbarePID, "01 A0", "41 A0", "Verfuegbare PID6", "") },
            { "PID7", new Messgroesse(4, VerfuegbarePID, "01 C0", "41 C0", "Verfuegbare PID7", "") },
            { "Motorlast", new Messgroesse(1, ErstelleGaspedal, "01 04", "41 04", "Motorlast, berechnet", "%") },
            { "Wassertemperatur", new Messgroesse(1, ErstelleTempWasserUOel, "01 05", "41 05", "Wassertemperatur", "°C") },
            { "Kraftstoffdruck", new Messgroesse(1, ErstelleKraftstoffdruck, "01 0A", "41 0A", "Kraftstoffdruck", "kPa") },
            { "Saugrohrdruck", new Messgroesse(1, ErstelleSaugrohrdruck, "01 0B", "41 0B", "Saugrohrdruck", "kPa") },
            { "Drehzahl", new Messgroesse(2, ErstelleDrehzahl, "01 0C", "41 0C", "Motordrehzahl", "1/min") },
            { "Laengsgeschwindigkeit", new Messgroesse(1, ErstelleSaugrohrdruck, "01 0D", "41 0D", "Laengsgeschwindigkeit", "km/h") },
            { "Saugrohrtemperatur", new Messgroesse(1, ErstelleTempWasserUOel, "01 0F", "41 0F", "Saugrohrtemperatur", "°C") },
            { "Luftmassenstrom", new Messgroesse(1, ErstelleMassenstrom, "01 10", "41 10", "Luftmassenstrom", "g/s") },
            { "Gaspedalstellung", new Messgroesse(1, ErstelleGaspedal, "01 11", "41, 11", "Gaspedalstellung", "%") },            
            { "Oeltemperatur", new Messgroesse(1, ErstelleTempWasserUOel, "01 5C", "41 5C", "Oeltemperatur", "°C") },
            { "Kraftstofffuellung", new Messgroesse(1, ErstelleGaspedal, "01 2F", "41 2F", "Kraftstofffuellung", "%") }
        };

        public static void MessgroessenVerarbeitung(string nachricht)
        {
            if (nachricht.Contains("48 6B"))
            {
                string pid = nachricht.Substring(nachricht.IndexOf("48 6B") + ("48 6B 11 ").Length, 5);
                if (Zugriff.DebugAusgabe) Zugriff.Ausgabe("Messgroessenverarbeitung PID: " + pid);

                foreach (var g in Groessen)
                {
                    if (g.Value.schluessel == pid)
                    {
                        string antwort = nachricht.Substring(nachricht.IndexOf(pid) + pid.Length + 1);
                        if (Zugriff.DebugAusgabe) Zugriff.Ausgabe("Messgroessenverarbeitung rufe Funktion für Antwort " + antwort);
                        if (g.Value.WertErstellen(antwort))
                        {
                            if (Zugriff.DebugAusgabe)
                            {
                                Zugriff.Ausgabe("Messgroessenverarbeitung Wert erstellt");
                                Zugriff.Ausgabe("Publishe " + g.Value.wert_string + " auf " + g.Value.benennung);
                            }

                            Zugriff.client.Publish(g.Value.benennung, g.Value.wert_string, OpenNETCF.MQTT.QoS.FireAndForget, false);
                        }
                        else
                        {
                            if (Zugriff.DebugAusgabe) Zugriff.Ausgabe("Messgroessenverarbeitung kein Wert erstellt");
                        }
                        break;
                    }
                }
            }
        }

        public static bool ErstelleKraftstoffdruck(Messgroesse m, byte[] b)
        {
            try
            {
                int druck = 3 * b[0];

                m.wert = druck;
                m.wert_string = druck.ToString();

            }
            catch (Exception e)
            {
                if (Zugriff.DebugAusgabe) Zugriff.Ausgabe("Kraftstoffdruck erstellen Exception " + e.ToString());
            }

            return false;
        }

        public static bool ErstelleSaugrohrdruck(Messgroesse m, byte[] b)
        {
            try
            {
                int druck = b[0];

                m.wert = druck;
                m.wert_string = druck.ToString();

            }
            catch (Exception e)
            {
                if (Zugriff.DebugAusgabe) Zugriff.Ausgabe("Saugrohrdruck erstellen Exception " + e.ToString());
            }

            return false;
        }

        public static bool ErstelleMassenstrom(Messgroesse m, byte[] b)
        {
            try
            {
                double strom = (256*b[0] + b[1]) / 100;

                m.wert = strom;
                m.wert_string = strom.ToString();

            }
            catch (Exception e)
            {
                if (Zugriff.DebugAusgabe) Zugriff.Ausgabe("Massenstrom erstellen Exception " + e.ToString());
            }

            return false;
        }

        public static bool ErstelleGaspedal(Messgroesse m, byte[] b)
        {
            try
            {
                float stellung = 100 / 255 * b[0];

                m.wert = stellung;
                m.wert_string = stellung.ToString();

            }
            catch (Exception e)
            {
                if (Zugriff.DebugAusgabe) Zugriff.Ausgabe("ErstelleGaspedal Exception " + e.ToString());
            }

            return false;
        }

        public static bool VerfuegbarePID(Messgroesse m, byte[] b)
        {
            try
            {
                string[] pidZerlegt = m.zugriff.Split(" ");
                byte index = byte.Parse(pidZerlegt[1], System.Globalization.NumberStyles.AllowHexSpecifier);

                System.Collections.BitArray bits = new System.Collections.BitArray(b); //umdrehen?

                var l = new List<String>();
                m.wert_string = "";

                for (int i = 0; i < 32; ++i)
                {
                    l.Add("PID " + pidZerlegt[0] + " " + (index + 1).ToString() + ": " + (bits[i] ? "Ja" : "Noi"));
                    m.wert_string += l[i] + "\n";
                }

                m.wert = l;

                return true;

            }
            catch (Exception e)
            {
                if (Zugriff.DebugAusgabe) Zugriff.Ausgabe("VerfuegbarePID erstellen Exception " + e.ToString());
            }

            return false;
        }

        public static bool ErstelleDrehzahl(Messgroesse m, byte[] b)
        {
            try
            {
                int drehzahl = (256 * b[0] + b[1]) / 4;

                m.wert = drehzahl;
                m.wert_string = drehzahl.ToString();

                return true;
            }
            catch (Exception e)
            {
                if (Zugriff.DebugAusgabe) Zugriff.Ausgabe("Drehzahl erstellen Exception " + e.ToString());
            }

            return false;
        }

        public static bool ErstelleTempWasserUOel(Messgroesse m, byte[] b)
        {
            try
            {
                int temperatur = b[0] - 40;

                m.wert = temperatur;
                m.wert_string = temperatur.ToString();

                return true;
            }
            catch (Exception e)
            {
                if (Zugriff.DebugAusgabe) Zugriff.Ausgabe("Temperatur erstellen Exception " + e.ToString());
            }

            return false;
        }
    }

    public class Messgroesse
    {
        private uint anzahl_zeichen;
        public string benennung;
        public string einheit;

        public string zugriff;
        public string schluessel;

        public object wert;
        public string wert_string;

        private Func<Messgroesse, byte[], bool> WertErmitteln;

        public Messgroesse(uint anzahl_zeichen, Func<Messgroesse, byte[], bool> WertErmitteln, string zugriff, string schluessel, string benennung = "", string einheit = "")
        {
            this.anzahl_zeichen = anzahl_zeichen;
            this.zugriff = zugriff;
            this.schluessel = schluessel;
            this.benennung = benennung;
            this.einheit = einheit;
            this.WertErmitteln = WertErmitteln;
        }

        public bool WertErstellen(string antwort)
        {
            string[] getrennt = antwort.Split(" ");

            byte[] b_a = new byte[anzahl_zeichen];

            for (int i = 0; i < anzahl_zeichen; ++i) b_a[i] = byte.Parse(getrennt[i], System.Globalization.NumberStyles.AllowHexSpecifier);

            return WertErmitteln(this, b_a);
        }
    }
}
