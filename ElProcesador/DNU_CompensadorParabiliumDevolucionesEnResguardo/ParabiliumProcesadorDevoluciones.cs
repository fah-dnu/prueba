using DNU_CompensadorParabiliumCommon.BaseDatos;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.Utilidades;
using DNU_CompensadorParabiliumDevolucionesEnResguardo.LogicaNegocio;
using Executer.BaseDatos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumDevolucionesEnResguardo
{
    public static class ParabiliumProcesadorDevoluciones
    {

        public static bool DevolucionRecursos()
        {
            try
            {
                Log.EventoDebug("[DevolucionRecursos] INICIA PROCESO DE DEVOLUCIONES");
                using (var conn = new SqlConnection(BDAutorizador.strBDEscritura)) 
                {
                    conn.Open();

                    Log.EventoDebug("[DevolucionRecursos] ObtieneTiposIntegracion");
                    foreach (var ID_TipoIntegracion in LogicaDevolucion.ObtieneTiposIntegracion(conn))
                    {
                        Log.EventoDebug("[DevolucionRecursos] ObtieneDevoluicionesEnResguardoPorTipoIntegracion");
                        foreach (var devolucion in LogicaDevolucion.ObtieneDevoluicionesEnResguardoPorTipoIntegracion(ID_TipoIntegracion, conn) )
                        {
                            Log.Evento("Procesando Devolucion " + devolucion.ID_Devolucion);
                            using (var tran = conn.BeginTransaction())
                            {

                                try
                                {
                                    var response = LogicaDevolucion.RealizaDevolucionDeRecurso(devolucion,conn,tran);
                                    Log.EventoDebug("[DevolucionRecursos] respuesta de generacion de poliza "+ response);
                                    Log.Evento("[DevolucionRecursos] respuesta de generacion de poliza " + response);
                                    if (response)
                                    {
                                        Log.EventoDebug("[DevolucionRecursos] actualiza opearcion ");
                                        DAOUtilerias.ActualizaEstatusOperacionOriginal(
                                        devolucion.ID_Operacion, devolucion.ID_EstatusOperacion, devolucion.ID_EstatusPostOperacion, "DR",
                                        conn,
                                        tran);


                                        Log.EventoDebug("[DevolucionRecursos] actualiza devolucion ");
                                        LogicaDevolucion.ActualizaEstatusDevolucion(new DevolucionModel
                                        {
                                            ID_Devolucion = devolucion.ID_Devolucion,
                                            Estatus = 1,
                                            ID_Poliza = devolucion.ID_Poliza
                                        }, conn, tran);


                                    }
                                    else
                                    {
                                        LogicaDevolucion.ActualizaEstatusDevolucion(new DevolucionModel
                                        {
                                            ID_Devolucion = devolucion.ID_Devolucion,
                                            Estatus = 2
                                        }, conn, tran);
                                    }
                                    Log.Evento("[DevolucionRecursos] Commit para el caso " + devolucion.ID_Devolucion);
                                    tran.Commit();

                                }
                                catch(Exception ex)
                                {
                                    
                                    Log.Error(String.Format("[DevolucionRecursos] Error en el procesamiento de la devolucion {0}", ex.Message));
                                    tran.Rollback();

                                    try
                                    {
                                        LogicaDevolucion.ActualizaEstatusDevolucion(new DevolucionModel
                                        {
                                            ID_Devolucion = devolucion.ID_Devolucion,
                                            Estatus = 2
                                        }, conn, tran);
                                    }
                                    catch (Exception ex2)
                                    {
                                        Log.Error(ex2);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }

    }
}
