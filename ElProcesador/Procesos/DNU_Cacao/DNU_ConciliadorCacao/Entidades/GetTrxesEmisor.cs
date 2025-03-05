using System;

namespace DNU_ConciliadorCacao.Entidades
{


    public class TrxesEmisorResult
    {
        public int success { get; set; }
        public string message { get; set; }
        public DetailTrxesEmisorResult[] detail { get; set; }
    }

    public class DetailTrxesEmisorResult
    {
        public int id { get; set; }
        public int? type_trx_id { get; set; }
        public string description { get; set; }
        public string localidad { get; set; }
        public int? moneda { get; set; }
        public bool status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public int? client_id { get; set; }
        public string fecha { get; set; }
        public string hora { get; set; }
        public string numero_referencia { get; set; }
        public string monto { get; set; }
        public string codigo_movimiento { get; set; }
        public string numero_tarjeta { get; set; }
        public string codigo_respuesta { get; set; }
        public string numero_documento { get; set; }
        public string request { get; set; }
        public string action { get; set; }
        public string email { get; set; }
    }


    
}
