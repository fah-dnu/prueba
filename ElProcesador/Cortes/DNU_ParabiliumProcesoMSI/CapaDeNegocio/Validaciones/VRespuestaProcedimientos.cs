using CommonProcesador;
using DNU_ParabiliumProcesoMSI.Modelos.Entidades;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliumProcesoMSI.CapaDeNegocio.Validaciones
{
    class VRespuestaProcedimientos
    {

        public static bool  BusquedaSinErrores(RespuestaProceso errores, DataTable tabla)
        {
            bool retornoBooleano = true;
            if (tabla.Columns.Count == 2 || tabla.Columns.Count == 3 || tabla.Columns.Count == 4)
            {
                if (tabla.Columns.Count == 2 && (tabla.Rows[0][0].ToString() == "error" || tabla.Rows[0][0].ToString() == "errorExcepcion"))
                {
                    if (tabla.Rows[0][0].ToString() == "error")
                    {
                        errores.CodRespuesta = "9999";
                        errores.DescRespuesta = VCampos.validarCadenasNulas(tabla.Rows[0][1].ToString());
                        Logueo.Error("[ProcesoMSI]  " + JsonConvert.SerializeObject(errores));
                    }
                    else
                    {
                        errores.CodRespuesta = "9999";
                        errores.DescRespuesta = "Error en base de datos";
                        Logueo.Error("[ProcesoMSI]  " + JsonConvert.SerializeObject(errores) + " Exception:" + tabla.Rows[0][1].ToString());
                    }
                    retornoBooleano = false;
                }
                else
                {
                    if (tabla.Rows[0][0].ToString() == "error" || tabla.Rows[0][0].ToString() == "errorExcepcion")
                    {
                        try
                        {
                            errores.CodRespuesta = VCampos.validarCadenasNulas(tabla.Rows[0]["Codigo"].ToString());
                        }
                        catch (Exception ex)
                        {
                            errores.CodRespuesta = "9999";
                        }
                        errores.DescRespuesta = VCampos.validarCadenasNulas(tabla.Rows[0][1].ToString());
                        Logueo.Error("[ProcesoMSI] " + JsonConvert.SerializeObject(errores));
                        retornoBooleano = false;
                    }

                }
            }
            return retornoBooleano;
        }
    }
}
