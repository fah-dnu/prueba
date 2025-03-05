using CommonProcesador;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace DNU_ParabiliaAltaTarjetasAnonimasPIEK.Entidades
{//
    public class Archivo
    {
        public String Emisor { get; set; }
        public String Cuenta { get; set; }
        public String NumeroTarjeta { get; set; }
        public String Tarjeta { get; set; }
        public String Vencimiento { get; set; }
        public String RawData { get; set; }
        public String Clabe { get; set; }
        public String Codigo { get; set; }


    }
}
