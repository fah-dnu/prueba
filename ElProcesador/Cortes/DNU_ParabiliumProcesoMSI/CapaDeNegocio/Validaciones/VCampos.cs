using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliumProcesoMSI.CapaDeNegocio.Validaciones
{
    class VCampos
    {
        public static string validarCadenasNulas(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return string.Empty;
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            return valor.ToString();

        }
    }
}
