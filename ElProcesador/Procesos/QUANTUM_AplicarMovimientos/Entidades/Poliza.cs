using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Executer.Entidades
{
    public class Poliza
    {

        public int ID_Operacion { get; set; }
        public int ID_Poliza { get; set; }
        public int ID_Evento { get; set; }
        public Int64 ID_CadenaComercial { get; set; }
        public String FechaCreacion { get; set; }
        public String Concepto { get; set; }
        public String Observaciones { get; set; }
        public Int64 ReferenciaNumerica { get; set; }
        public float Importe { get; set; }
        public List<DetallePoliza> Detalles { get; set; }
        public List<Saldo> Saldos { get; set; }
        public Boolean isProcesada { get; set; }
        public Boolean Cancelacion { get; set; }
        //private String _idMensajeISO;

        public Poliza()
        {
            Detalles = new List<DetallePoliza>();
            ID_Operacion = 0;
            FechaCreacion = "";
            Importe = 0;
            isProcesada = false;
        }



        public void addDetalles(DetallePoliza detalle)
        {
            this.Detalles.Add(detalle);
        }

    }
}
