using iLO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iLO_test {
    class Program {
        static void Main(string[] args) {
            using (var plugin = new LCDSmartie()) {
                plugin.Wait();
                plugin.Output();

                Console.WriteLine(plugin.function1("overview.uid_led", ""));
                Console.WriteLine(plugin.function1("overview.ers_state", ""));
                Console.WriteLine(plugin.function1("overview.system_health", ""));
                Console.WriteLine(plugin.function2("system_health", ""));

                Console.WriteLine(plugin.function1("fan.fans[label=Fan 1].speed", "%"));
                Console.WriteLine(plugin.function3("Fan 1.speed", "%"));

                Console.WriteLine(plugin.function1("temp.temperature[label=PCI].currentreading", "C"));
                Console.WriteLine(plugin.function4("PCI.currentreading", "C"));
                Console.WriteLine(plugin.function4("PCI.temp_unit", ""));
            }

            Console.Read();
        }
    }
}
