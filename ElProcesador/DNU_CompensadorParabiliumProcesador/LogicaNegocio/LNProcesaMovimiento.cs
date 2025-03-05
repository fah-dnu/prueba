using CommonProcesador;
using DNU_CompensadorParabiliumCommon.BaseDatos;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.LogicaNegocio;
using DNU_CompensadorParabiliumCommon.Utilidades;
using DNU_CompensadorParabiliumProcesador.Utilidades;
using Executer.BaseDatos;
using Executer.Entidades;
using Interfases.Entidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumProcesador.LogicaNegocio
{
    public class LNProcesaMovimiento
    {
        public static Poliza EjecutarContabilidad(string referencia, string observaciones, string descEvento
                                , string idEvento, Dictionary<String, Parametro> losParametros
                                , string idficheroDetalle
                                , SqlConnection connConsulta, SqlTransaction transaccionSQL, string etiquetaLogueo)
        {
            Poliza laPoliza = new Poliza();
            try
            {

                Executer.EventoManual aplicador = new Executer.EventoManual(int.Parse(idEvento),
                        descEvento,
                        false,
                        long.Parse(idficheroDetalle),
                        losParametros,
                       observaciones,
                        connConsulta, transaccionSQL);
                laPoliza = aplicador.AplicaContablilidad();


                return laPoliza;
            }
            catch (Exception ex)
            {
                LogueoCompensador.Error(etiquetaLogueo + "[EjecutarContabilidad()] [" + ex.Message + "]");
                laPoliza.CodigoRespuesta = 99;
                laPoliza.DescripcionRespuesta = ex.Message;
                return laPoliza;
            }

        }
    }
}
