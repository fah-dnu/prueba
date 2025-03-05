using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImportarEmpleados.Entidades
{
    public class Empleado
    {
        public Int64 ID_Empleado { get; set; }
        public int ID_Estatus { get; set; }
        public Int64 ID_Detalle { get; set; }
        public String NumeroEmpleado { get; set; }
        public String Nombre { get; set; }
        public String APaterno { get; set; }
        public String AMaterno { get; set; }
        public String TelefonoMovil { get; set; }
        public String EmailEmpresarial { get; set; }
        public String EmailPersonal { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public DateTime FechaActivacion { get; set; }
        public DateTime FechaAlta { get; set; }
        public Int64 ID_CadenaComercial { get; set; }
        public Boolean EsCreadoClubEscala { get; set; }
        public Boolean EsCreadoColectiva { get; set; }
        public Decimal LimiteCompra { get; set; }
        public String CicloNominal { get; set; }
        public String Baja { get; set; }
        public String DiaPago { get; set; }
        public String Reservado1 { get; set; }
        public String Reservado2 { get; set; }
        public int ID_TipoFalla { get; set; }
    }
}
