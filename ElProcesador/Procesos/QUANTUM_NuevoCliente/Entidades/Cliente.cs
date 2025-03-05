using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfases.Entidades;

namespace QUANTUM_NuevoCliente.Entidades
{
    public  class Cliente
    {

        public Int64 ID_Colectiva { get; set; }
        public Int64 ID_ColectivaPadre { get; set; }
        public Int64 ID_ColectivaCCM { get; set; }

        public String ID_Cliente { get; set; }
        public String NombreORazonSocial { get; set; }
        public String APaterno { get; set; }
        public String AMaterno { get; set; }
        public String RFC { get; set; }
        public String CURP { get; set; }
        public String Telefono { get; set; }
        public String Movil { get; set; }
        public String Password { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public String Email { get; set; }
        public String Tarjeta { get; set; }
        public String ClaveSucursal { get; set; }
        public Decimal LimiteCredito { get; set; }
        public int DiasVigencia { get; set; }

        public String  Consecutivo { get; set; }
         private Dictionary<String, Parametro> losParametrosExtras = new Dictionary<string, Parametro>();

        public String NombreCompleto()
        {
            StringBuilder resp = new StringBuilder();

            resp.Append(this.NombreORazonSocial);
            resp.Append(" " + this.APaterno);
            resp.Append(" " + this.AMaterno);


            return resp.ToString().Replace(".", "").Trim();
        }


        public Dictionary<String, Parametro> LosParametrosExtras
        {
            set { losParametrosExtras = value; }
            get { return losParametrosExtras; }
        }


    
    }
}
