//
//      ===  Demo LCDSmartie Plugin for c#  ===
//
// dot net plugins are supported in LCD Smartie 5.3 beta 3 and above.
//

// You may provide/use upto 20 functions (function1 to function20).


using iLOViewer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace iLO {
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    public class LCDSmartie : IDisposable {
        const string ConfigFilePath = "iLO.json";
        const int TimerInteval = 3000;
        static readonly Dictionary<string, string> FormatMap = new Dictionary<string, string>() {
            { "Overview.self_test", "status" },
            { "Overview.system_health", "status" },
            { "Overview.uid_led", "status" },
            { "Overview.ers_state", "status" },
            { "Fan.fans.status", "status" },
            { "Temp.temperature.status", "status" },
            { "Temp.temperature.temp_unit", "tempunit" },
        };

        private JObject FConfig = null;
        private iLOConnection FConnection = null;
        private Timer FTimer = null;

        public LCDSmartie() {
            //
            // TODO: Add constructor logic here
            // 
            SmartieInit();
        }

        ~LCDSmartie() {
            Dispose();
        }

        public void Dispose() {
            SmartieFini();
        }

        private void LoadAppConfig() {
            var file = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigFilePath);
            var configContent = File.ReadAllText(file);
            FConfig = JObject.Parse(configContent);
        }

        private void Open() {
            var server = FConfig["Server"];
            FConnection = new iLOConnection() {
                Name = (string)server["Name"],
                Https = (bool)server["Https"],
                Host = (string)server["Host"],
                Port = (int)server["Port"],
                User = (string)server["User"],
                UserPassword = (string)server["Password"],
            };

            FConnection.Login();
            FTimer = new Timer(state => this.Update(), this, TimerInteval, TimerInteval);
        }

        private void Close() {
            if (FConnection != null && FConnection.IsConnected)
                FConnection.Logout();
            if (FTimer != null)
                FTimer.Dispose();

            FConnection = null;
            FTimer = null;
        }

        private void Update() {
            if (FConnection.IsConnected) {
                if (!FConnection.IsRunning) {
                    FConnection.UpdateSystemInfo();
                }
            } else if (DateTime.Now >= FConnection.NextAttempt) {
                if (!FConnection.IsRunning) {
                    FConnection.Login();
                }
            }
        }

        static bool IsNumeric(string s) {
            return Regex.IsMatch(s, @"^\d+$");
        }

        static string FormatJson(JValue node, string postfix) {
            var path = Regex.Replace(node?.Path, @"\[.*\]", "");
            var value = node?.ToString();
            if (FormatMap.TryGetValue(path, out var fmt)) {
                switch (fmt.ToLower()) {
                    case "status":
                        value = value.Substring(value.LastIndexOf("_") + 1);
                        break;
                    case "tempunit":
                        value = "Fahrenheit".Equals(value, StringComparison.OrdinalIgnoreCase) ? "℉" : "℃";
                        break;
                }
            }

            return string.IsNullOrEmpty(value) ? string.Empty : value + postfix;
        }

        static Tuple<string, string> ExtractIndex(string s) {
            if (s.EndsWith("]")) {
                var index = s.LastIndexOf("[");
                return new Tuple<string, string>(s.Substring(0, index).Trim(), s.Substring(index + 1, s.Length - index - 2).Trim());
            }

            return new Tuple<string, string>(s, null);
        }

        static Tuple<string, string> ExtractAttr(string s, char separator) {
            var index = s.IndexOf(separator);
            if (index >= 0)
                return new Tuple<string, string>(s.Substring(0, index).Trim(), s.Substring(index + 1, s.Length - index - 1).Trim());

            return new Tuple<string, string>(s, null);
        }

        static string ReadJson(JToken root, string path, string postfix) {
            try {
                var node = root;
                var names = path.Split('.');
                foreach (var name in names) {
                    if (node == null)
                        break;

                    var index = ExtractIndex(name.Trim());
                    node = ((JObject)node).GetValue(index.Item1, StringComparison.OrdinalIgnoreCase);
                    if (node is JArray) { // "temperature[5]" or "temperature[label=Chipset]"
                        if (IsNumeric(index.Item2))
                            node = ((JArray)node)[int.Parse(index.Item2) - 1];
                        else {
                            var attr = ExtractAttr(index.Item2, '=');
                            node = ((JArray)node).FirstOrDefault(x => (x is JObject)
                                && ((JObject)x).GetValue(attr.Item1)?.ToString().IndexOf(attr.Item2, StringComparison.OrdinalIgnoreCase) >= 0);
                        }

                    }
                }

                if (node is JValue)
                    return FormatJson((JValue)node, postfix);

                return string.Empty;
            } catch (Exception) {
                return string.Empty;
            }
        }
        public void Wait() {
            while (FConnection.IsRunning)
                Thread.Sleep(1000);
        }

        public void Output() {
            Console.WriteLine(FConnection.SystemInfo?.ToString());

        }

        //===============================================================================================================================

        // Smartie will call this when the plugin is 1st loaded
        // This function is optional
        public void SmartieInit() {
            LoadAppConfig();
            Open();
        }


        // Smartie will call this just before the plugin is unloaded
        // This function is optional
        public void SmartieFini() {
            Close();
        }


        //
        // Define the minimum interval that a screen should get fresh data from our plugin.
        // The actual value used by Smartie will be the higher of this value and of the "dll check interval" setting
        // on the Misc tab.  [This function is optional, Smartie will assume 300ms if it is not provided.]
        // 
        public int GetMinRefreshInterval() {
            return TimerInteval;
        }

        // This function is used in LCDSmartie by using the dll command as follows:
        //    $dll(iLO, 1, param1, param2)
        // Smartie will then display on the LCD: function called with (hello, there)
        // iLO properties
        // param1: property path, eg: fan/fans[1]/label, temp.temperature[label=CPU].currentreading
        // param2: postfix if value exists
        public string function1(string param1, string param2) {
            return ReadJson(FConnection.SystemInfo, param1, param2);
        }

        // This function is used in LCDSmartie by using the dll command as follows:
        //    $dll(iLO, 2, param1, param2)
        // Smartie will then display on the LCD: function called with (hello, there)
        // Overview shortcut
        // param1: property path, eg: server_name means Overview.server_name
        // param2: postfix if value exists
        public string function2(string param1, string param2) {
            return function1($"Overview.{param1}", param2);
        }

        // This function is used in LCDSmartie by using the dll command as follows:
        //    $dll(iLO, 3, param1, param2)
        // Smartie will then display on the LCD: function called with (hello, there)
        // fan shortcut
        // param1: property path, eg: Fan 1.speed means Fan/fans[label=Fan 1].speed
        // param2: postfix if value exists
        public string function3(string param1, string param2) {
            var key = ExtractAttr(param1, '.');
            return function1($"Fan.fans[label={key.Item1}].{key.Item2}", param2);
        }

        // This function is used in LCDSmartie by using the dll command as follows:
        //    $dll(iLO, 4, param1, param2)
        // Smartie will then display on the LCD: function called with (hello, there)
        // temperature shortcut
        // param1: property path, eg: CPU.caution means Temp/temperature[label=CPU].caution
        // param2: postfix if value exists
        public string function4(string param1, string param2) {
            var key = ExtractAttr(param1, '.');
            return function1($"Temp.temperature[label={key.Item1}].{key.Item2}", param2);
        }

        // This function is used in LCDSmartie by using the dll command as follows:
        //    $dll(iLO, 20, param1, param2)
        // Smartie will then display on the LCD: function called with (hello, there)
        // loading status
        // param1: not used
        // param2: not used
        public string function20(string param1, string param2) {
            return FConnection.Status;
        }
    }
}
