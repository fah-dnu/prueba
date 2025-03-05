using DNU_ParabiliaProcesoCortes.CapaDatos;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Contratos
{
    abstract class EdoCuenta
    {
        public abstract bool GeneraEdoCuentaPDF(List<string> archivos, Int64 ID_Corte, string ruta, DAOCortes _daoCortes, SqlConnection conn, string rutaImagen, string claveCliente,
            string logo, string imagenUNE, string imagenCAT, string nombreArchivo, bool noTimbrar, Factura laFacturaExt);

    }
}
