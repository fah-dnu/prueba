using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConciliacionSmartpoints.Entidades
{
    public class DatosBaseDatos
    {
        public Dictionary<int, CampoConfig> losDatosdeBD = new Dictionary<int, CampoConfig>();
        public Int64 ID_Consulta;

        public String Clave { get; set; }
        public String Descripcion { get; set; }
        public String StoredProcedure { get; set; }
        public String CadenaConexion { get; set; }
    }
}
