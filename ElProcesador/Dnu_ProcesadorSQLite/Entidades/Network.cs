using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu_ProcesadorSQLite.Entidades
{
    public class Network
    {
        public int? networkBrandId { get; set; }
        public int? networkId { get; set; }
        public string key { get; set; }
        public string description { get; set; }
        public string value { get; set; }
    }
}
