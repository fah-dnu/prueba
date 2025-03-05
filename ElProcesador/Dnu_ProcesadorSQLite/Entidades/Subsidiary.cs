using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu_ProcesadorSQLite.Entidades
{
    public class Subsidiary
    {
        public int? subsidiaryBrandId { get; set; }
        public int? subsidiaryId { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string street { get; set; }
        public string neighborhood { get; set; }
        public string city { get; set; }
        public string stateKey { get; set; }
        public string state { get; set; }
        public string cp { get; set; }
        public string phone { get; set; }
    }
}
