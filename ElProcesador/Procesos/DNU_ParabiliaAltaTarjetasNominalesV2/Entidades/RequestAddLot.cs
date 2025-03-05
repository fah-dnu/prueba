using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.Entidades
{
    class RequestAddLot
    {
        public string lot_key { get; set; }
        public string sub_bins_group_key { get; set; }
        public string lot_type_key { get; set; }
        public string manufacturing_type_key { get; set; }
        /// <summary>
        /// 
        /// </summary>
        ///<example>0/example>
        public string items { get; set; }
        public string user { get; set; }
    }
}
