using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu_ProcesadorSQLite.Entidades
{
    public class Brand
    {
        public int? brandId { get; set; }
        public string brandKey { get; set; }
        public string socialReason { get; set; }
        public string commercialName { get; set; }
        public int? typeId { get; set; }
        public string typeKey { get; set; }
        public string typeDescription { get; set; }
    }
}
