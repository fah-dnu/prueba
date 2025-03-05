using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.Entidades
{
    class ResponseAddLot
    {
        public string lot_key { get; set; }
        public List<DValueLot> values { get; set; }
    }
    class DValueLot
    {
        public string masked_value { get; set; }
        public string encrypted_value { get; set; }
        public string encrypted_account { get; set; }
    }
}
