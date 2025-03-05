using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Common.Funciones
{
    public class FuncionesFacturas
    {
        public static string obtenerURLPACFacturacion(string nombrePAC, Factura laFactura)
        {
            string urlPAC = "";
            if (nombrePAC.ToLower() == "mysuite")
            {
                urlPAC = PNConfig.Get("PROCESAEDOCUENTA", "RutaSoapMySuit");
            }
            else if (nombrePAC.ToLower() == "facturama")
            {
                urlPAC = PNConfig.Get("PROCESAEDOCUENTA", "URLPACFacturama");
                laFactura.URLProvedorCertificadoObtieneXML = PNConfig.Get("PROCESAEDOCUENTA", "URLPACFacturamaObtenerXML");
            }
            return urlPAC;
        }
    }
}
