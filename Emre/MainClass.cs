using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emre
{
    public class MainClass
    {
        public static string[] Headers = new[] { "NE_Name", "Id", "Description", "Shutdown", "Dcn", "Trap_Input_Rate",
            "Trap_Input_Resure_Rate", "Trap_Output_Rate", "Trap_Output_Resure_Rate", "Carrier_Time", "Clock_Priority", "Clock_Sync" };

        public List<string> ClassString { get; set; }
        public MainClass(List<string> classString)
        {
            ClassString = classString;
            SubClasses = new List<SubClass>();
        }

        public string Id { get; set; } = "NULL";
        public string Ip { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Shutdown { get; set; } = "Disable";
        public string Dcn { get; set; } = "Disable";
        public int? Trap_Input_Rate { get; set; }
        public int? Trap_Input_Resure_Rate { get; set; }
        public int? Trap_Output_Rate { get; set; }
        public int? Trap_Output_Resure_Rate { get; set; }
        public int? Carrier_Time { get; set; }
        public int? Clock_Priority { get; set; }
        public string Clock_Sync { get; set; } = "Disable";
        public List<string> Ports { get; set; }
        public List<SubClass> SubClasses { get; set; }

        public string GetId() => ClassString.FirstOrDefault(x => x.Contains("Eth")).Split(' ').FirstOrDefault(x => x.Contains("Eth"));
        public string GetDescription() => ClassString.Any(x => x.Contains("description")) ? ClassString.Single(x => x.Contains("description")).Replace(" description ", "") : string.Empty;
        public string GetShutdown() => ClassString.Any(x => x.Contains("undo shutdown")) ? "Disable" : "Enable";
        public string GetDcn() => ClassString.Contains("undo dcn") ? "Disable" : "Enable";
        public int? GetTrapRate(string inOut, int index)
        {
            if (ClassString.Any(x => x.Contains("trap") && x.Contains(inOut)))
            {
                var trapString = ClassString.Single(x => x.Contains("trap") && x.Contains(inOut));
                try
                {
                    return Convert.ToInt32(trapString.Split(' ').ElementAt(index));
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
        public int? GetCarrier()
        {
            if (ClassString.Any(x => x.Contains("carrier")))
            {
                var trapString = ClassString.Single(x => x.Contains("carrier"));
                return Convert.ToInt32(trapString.Split(' ').ElementAt(3));
            }
            return null;
        }
        public int? GetClockPriority()
        {
            if (ClassString.Any(x => x.Contains("clock priority")))
            {
                var trapString = ClassString.Single(x => x.Contains("clock priority"));
                return Convert.ToInt32(trapString.Split(' ').ElementAt(3));
            }
            return null;
        }
        public string GetClockSync()
        {
            if (ClassString.Any(x => x.Contains("clock synchronization")))
            {
                var trapString = ClassString.Single(x => x.Contains("clock synchronization"));
                return trapString.Split(' ').ElementAt(3);
            }
            return "Disable";
        }
        public List<string> GetPorts()
        {
            if (ClassString.Any(x => x.Contains("port")))
            {
                return ClassString.Where(x => x.Contains("port")).ToList();
            }
            return null;
        }

        private string GetIp()
        {
            var ipRow = ClassString.FirstOrDefault(x => x.Contains("ip address"));
            if (ipRow is null) return "NULL";
            return ipRow.Split(' ').ElementAt(3);
        }

        public void SetClassProperties()
        {
            Ip = GetIp();
            Id = GetId();
            Description = GetDescription();
            Shutdown = GetShutdown();
            Dcn = GetDcn();
            Trap_Input_Rate = GetTrapRate("input", 3);
            Trap_Input_Resure_Rate = GetTrapRate("input", 5);
            Trap_Output_Rate = GetTrapRate("out", 3);
            Trap_Output_Resure_Rate = GetTrapRate("out", 5);
            Carrier_Time = GetCarrier();
            Clock_Priority = GetClockPriority();
            Clock_Sync = GetClockSync();
            Ports = GetPorts();
        }
    }
}
