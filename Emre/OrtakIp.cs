using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emre
{
    public class OrtakIp
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Ip { get; set; }

        public OrtakIp(string name, string id, string ip)
        {
            Name = name;
            Id = id;
            Ip = ip;
        }
    }
}
