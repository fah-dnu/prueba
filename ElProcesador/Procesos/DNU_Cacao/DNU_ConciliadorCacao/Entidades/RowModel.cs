using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ConciliadorCacao.Entidades
{
    public class RowModel
    {
        public long ClaveEmisor { get; set; }
        public string ClaveMovimiento { get; set; }
        public string FechaMovimiento { get; set; }
        public string horaMovimiento { get; set; }
        public string numeroTarjeta { get; set; }
        public string documento { get; set; }
        public string monto { get; set; }
        public int enSiscard { get; set; }
        public int enCacao { get; set; }
    }
    public class SyscardConciliation
    {
        public string fecha { get; set; }
        public string emisor { get; set; }
        public List<Transaccion> transacciones { get; set; }

        public SyscardConciliation(String _emisor, String _fecha)
        {
            emisor = _emisor;
            fecha = _fecha;
            transacciones = new List<Transaccion>();
        }
    }

    public class Transaccion
    {
        public string codigo_movimiento { get; set; }
        public string hora { get; set; }
        public string numero_tarjeta { get; set; }
        public string documento { get; set; }
        public string monto { get; set; }
        public int en_siscard { get; set; }
        public int en_cacao { get; set; }
    }

}
