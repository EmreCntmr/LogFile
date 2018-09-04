using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emre
{
    public class SubClass
    {
        public static string[] Headers = new[] { "Vlan_Type", "Vlan_Value", "Mtu", "IP", "Subnet_Mask", "IP_Relay_Address",
            "IP_Binding", "Isis_Enable", "Isis_BFD", "Isis_Enable_Value", "Isis_Cost", "Isis_Enable_Level", "Isis_Circuit_Type", "Isis_Circuit_Level",
            "Isis_Small", "Mpls_Enable", "Mpls_Te_Enable", "Mpls_Rsvp_Enable", "Mpls_Mtu", "Trust_Upstream", "Trust_Value", "L2vc_Id", "L2vc_Policy"
            , "QOS_Profile_Name" };

        public string Id { get; set; }
        public string Vlan_Type { get; set; }
        public int? Vlan_Value { get; set; }
        public string Description { get; set; }
        
        public int? Trap_Input_Rate { get; set; }
        public int? Trap_Input_Resure_Rate { get; set; }

        public int? Trap_Output_Rate { get; set; }
        public int? Trap_Output_Resure_Rate { get; set; }

        public int? Mtu { get; set; }

        public string IP { get; set; }
        public string Subnet_Mask { get; set; }
        public string IP_Relay_Address { get; set; }
        public string IP_Binding { get; set; }

        public string Isis_Enable { get; set; } = "Disable";
        public string Isis_Bdf { get; set; } = "Disable";
        public int? Isis_Enable_Value { get; set; }

        public int? Isis_Cost { get; set; }
        public string Isis_Enable_Level { get; set; }

        public string Isis_Circuit_Type { get; set; }
        public string Isis_Circuit_Level { get; set; }
        public string Isis_Small { get; set; }

        public string Mpls_Enable { get; set; } = "Disable";
        public string Mpls_Te_Enable { get; set; } = "Disable";
        public string Mpls_Rsvp_Enable { get; set; } = "Disable";
        public int? Mpls_Mtu { get; set; }

        public string Trust_Upstream { get; set; }
        public string Trust_Value { get; set; }

        public int? L2vc_Id { get; set; }
        public string L2vc_Policy { get; set; }
        public string QOS_Profile_Name { get; set; } = "NULL";
        public string FIX_QOS_Description { get; set; } = "NULL";
        public string Check_QOS { get; set; } = "NULL";

        public List<string> ClassString { get; set; }

        public SubClass(List<string> classString)
        {
            ClassString = classString;
        }
        public void SetProperties()
        {
            Id = GetId();
            Vlan_Type = GetVlan().Item1;
            Vlan_Value = GetVlan().Item2;
            Description = GetDescription();
            Trap_Input_Rate = GetTrapRate("input", 3);
            Trap_Input_Resure_Rate = GetTrapRate("input", 5);
            Trap_Output_Rate = GetTrapRate("out", 3);
            Trap_Output_Resure_Rate = GetTrapRate("out", 5);
            Mtu = GetMtu();
            IP = GetIpAdress().Item1;
            Subnet_Mask = GetIpAdress().Item2;
            IP_Relay_Address = GetIpRelayAdress();
            IP_Binding = GetIpBinding();
            SetIsis();
            SetMpls();
            SetTrust();
            SetFix();
        }
        public string GetId() => ClassString.FirstOrDefault(x => x.Contains("Eth")).Split(' ').FirstOrDefault(x => x.Contains("Eth"));
        public void SetFix()
        {
            if (!ClassString.Any(x => x.Contains("qos-profile")))
                return;
            var splitedQuosProfile = ClassString.FirstOrDefault(x => x.Contains("qos-profile")).Split(' ').ToList();
            var fix = splitedQuosProfile.FirstOrDefault(x => x.Contains("FIX"));
            if (fix is null) return;
            QOS_Profile_Name = fix;
            var QOS_Description = fix.Split('_').SingleOrDefault(x => x.Contains("FIX")).Remove(0, 3);
            var d = Description.Split('_').LastOrDefault().Split('|');
            var count = d.Count();
            try
            {
                FIX_QOS_Description = d.ElementAt(count - 3);

            }
            catch (Exception)
            {

                FIX_QOS_Description = "NULL";

            }
            Check_QOS = QOS_Description == FIX_QOS_Description ? "True" : "False";
        }
        private Tuple<string, int?> GetVlan()
        {
            if (ClassString.FirstOrDefault(x => x.Contains("vlan")) == null) return new Tuple<string, int?>(null, null);
            var splitedVlan = ClassString.FirstOrDefault(x => x.Contains("vlan")).Split(' ').ToList();
            var vlanKey = splitedVlan.Single(x => x.Contains("vlan"));
            var vlanIndex = splitedVlan.IndexOf(vlanKey);
            var vlanType = splitedVlan[vlanIndex + 1];
            var vlanValue = Convert.ToInt32(splitedVlan[vlanIndex + 2]);
            return new Tuple<string, int?>(vlanType, vlanValue);
        }
        public string GetDescription() => ClassString.Any(x => x.Contains("description")) ? ClassString.Single(x => x.Contains("description")).Replace(" description ", "") : string.Empty;
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
        public int? GetMtu()
        {
            if (ClassString.Any(x => x.Contains("mtu")))
            {
                var splitedMtu = ClassString.FirstOrDefault(x => x.Contains("mtu")).Split(' ').ToList();
                var mtuKey = splitedMtu.Single(x => x.Contains("mtu"));
                var mtuIndex = splitedMtu.IndexOf(mtuKey);
                var mtuValue = splitedMtu.ElementAt(mtuIndex + 1);
                return Convert.ToInt32(mtuValue);
            }
            return null;
        }
        public Tuple<string, string> GetIpAdress()
        {
            var ipString = ClassString.SingleOrDefault(x => x.Contains("ip") && x.Contains("address") && !x.Contains("relay"));
            if (ipString == null) return new Tuple<string, string>(null, null);
            var splitedIp = ipString.Split(' ').ToList();
            var ipAdressKey = splitedIp.Single(x => x.Contains("address"));
            var ipIndex = splitedIp.IndexOf(ipAdressKey);
            var ip = splitedIp[ipIndex + 1];
            var subnetMask = splitedIp[ipIndex + 2];
            return new Tuple<string, string>(ip, subnetMask);
        }
        public string GetIpRelayAdress()
        {
            var ipString = ClassString.FirstOrDefault(x => x.Contains("ip") && x.Contains("address") && x.Contains("relay"));
            if (ipString == null) return null;
            var splitedIp = ipString.Split(' ').ToList();
            var ipAdressKey = splitedIp.Single(x => x.Contains("address"));
            var ipIndex = splitedIp.IndexOf(ipAdressKey);
            return splitedIp[ipIndex + 1];
        }
        public string GetIpBinding()
        {
            var ipString = ClassString.FirstOrDefault(x => x.Contains("ip") && x.Contains("binding"));
            if (ipString == null) return null;
            return ipString.Split(' ').LastOrDefault();
        }
        private void SetIsis()
        {
            Isis_Small = "Disable";
            var isisRows = ClassString.Where(x => x.Contains("isis"));
            if (!isisRows.Any()) return;
            var isisSample = isisRows.FirstOrDefault();
            var isisIndex = isisSample.IndexOf("isis");
            Isis_Bdf = isisRows.Any(x => x.Contains("bdf")) ? "Enable" : "Disable";

            var enableRow = isisRows.FirstOrDefault(x => x.Contains("enable"));
            if (enableRow != null)
            {
                var splitedenableRow = enableRow.Split(' ');
                Isis_Enable = splitedenableRow.ElementAt(isisIndex + 1);
                Isis_Enable_Value = Convert.ToInt32(splitedenableRow.ElementAt(isisIndex + 2));
            }

            var circuitRow = isisRows.SingleOrDefault(x => x.Contains("circuit-type"));
            if (circuitRow != null)
                Isis_Circuit_Type = circuitRow.Split(' ').LastOrDefault();

            var circuitLevelRow = isisRows.SingleOrDefault(x => x.Contains("circuit-level"));
            if (circuitLevelRow != null)
                Isis_Circuit_Level = circuitLevelRow.Split(' ').LastOrDefault();

            var costRow = isisRows.FirstOrDefault(x => x.Contains("cost"));
            if (costRow != null)
            {
                var splitedenableRow = costRow.Split(' ');
                Isis_Cost = Convert.ToInt32(splitedenableRow.ElementAt(isisIndex + 2));
                try
                {
                    Isis_Enable_Level = splitedenableRow.ElementAt(isisIndex + 3);
                }
                catch { }
            }
            var smallRow = isisRows.SingleOrDefault(x => x.Contains("small"));
            if (smallRow != null)
                Isis_Small = "Enable";
        }
        private void SetMpls()
        {
            var mplsRows = ClassString.Where(x => x.Contains("mpls"));
            if (mplsRows.Count() < 3)
            {
                Mpls_Enable = "Disable";
                return;
            }
            Mpls_Enable = "Enable";
            var mplsSample = mplsRows.FirstOrDefault();
            var mplsIndex = mplsSample.IndexOf("mpls");
            Mpls_Te_Enable = mplsRows.Any(x => x.Contains("te")) ? "Enable" : "Disable";
            Mpls_Rsvp_Enable = mplsRows.Any(x => x.Contains("rsvp")) ? "Enable" : "Disable";
            var mtuRow = mplsRows.SingleOrDefault(x => x.Contains("mtu"));
            if (mtuRow != null)
                Mpls_Mtu = Convert.ToInt32(mtuRow.Split(' ').LastOrDefault());

            var bfdRow = mplsRows.SingleOrDefault(x => x.Contains("bfd"));
            var l2vcRow = mplsRows.FirstOrDefault(x => x.Contains("l2vc"));
            if (l2vcRow != null)
            {
                var splitedL2vc = l2vcRow.Split(' ').ToList();
                var policy = splitedL2vc.SingleOrDefault(x => x.Contains("policy"));
                var policyIndex = splitedL2vc.IndexOf(policy);
                L2vc_Id = Convert.ToInt32(splitedL2vc.ElementAt(policyIndex - 1));
                L2vc_Policy = splitedL2vc.ElementAt(policyIndex + 1);
            }
        }
        private void SetTrust()
        {
            var trustRows = ClassString.Where(x => x.Contains("trust"));
            if (!trustRows.Any()) return;
            var trustSample = trustRows.FirstOrDefault();
            var trustIndex = trustSample.IndexOf("trust");

            var enableRow = trustRows.SingleOrDefault(x => x.Contains("upstream"));
            if (enableRow != null)
                Trust_Upstream = enableRow.Split(' ').LastOrDefault();

            var circuitRow = trustRows.LastOrDefault();
            if (circuitRow != null)
                Trust_Value = circuitRow.Split(' ').LastOrDefault();
        }
    }
}
