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
