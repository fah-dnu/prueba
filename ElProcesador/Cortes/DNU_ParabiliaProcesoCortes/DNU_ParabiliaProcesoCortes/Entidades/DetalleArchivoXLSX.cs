using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    class DetalleArchivoXLSX
    {
        //public string NombreCliente { get; set; }
        //public string NumeroCliente { get; set; }
        //public string FechaCorte { get; set; }
        //public bool Generado { get; set; } = false;
        //public int NumeroOperaciones { get; set; }
        //public bool Timbrado { get; set; } = false;
        //public Decimal MontoTimbrado { get; set; }
        //public Decimal MontoIVATimbrado { get; set; }
        //public string Folio { get; set; }
        //public string NombreEstadoCuenta { get; set; }

        public string AnioMes { get; set; }
        public string ClienteID { get; set; }
        public string NombreCompleto { get; set; }
        public string RFCReceptor { get; set; }
        public string DireccionCompleta { get; set; }
        public DateTime FechaTimbrado { get; set; }
        public string UUID { get; set; }
        public string Folio { get; set; }
        public Decimal IVA { get; set; }
        public Decimal SubTotal { get; set; }
        public Decimal ImporteTotal { get; set; }
        public string NombreEmisor { get; set; }
        public string RFCEmisor { get; set; }
        public bool Timbrado { get; set; } = false;
        public bool Generado { get; set; } = false;
        public DateTime FechaRegistro { get; set; } 
        //public string NombreEstadoCuenta { get; set; }



    }
}
