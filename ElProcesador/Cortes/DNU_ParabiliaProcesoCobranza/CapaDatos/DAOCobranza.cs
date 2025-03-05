using CommonProcesador;
using DNU_ParabiliaProcesoCobranza.Entidades;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCobranza.CapaDatos
{
    public class DAOCobranza
    {
        private BaseDeDatos consultaBDRead;
        private DataTable dtResultado;

        public DAOCobranza()
        {
            consultaBDRead = new BaseDeDatos("");
        }

        public DataTable obtenerDatosParaExecute(DatosOperacionesExecute solicitudDatos, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@tarjeta", parametro = solicitudDatos.tarjeta, encriptado = true, longitud = Constantes.longitudMedioAcceso });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@tipoMedioAcceso", parametro = solicitudDatos.tipoMedioAcceso });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@claveEvento", parametro = solicitudDatos.claveEvento });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@importe", parametro = solicitudDatos.importe });

                // consultaBDRead.verificarParametrosNulosString(parametros, "@folioOrigen", solicitudSpeiIn.FolioOrigen);

                dtResultado = consultaBDRead.ConstruirConsulta("[ws_Ejecutor_ObtenerDatosParaExecute]", parametros, "obtenerDatosTarjetaSpeiParaExecute", "", conn, transaccionSQL);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] Error al obtener los datos para execute: " + ex.Message + " " + ex.StackTrace);

                return dtResultado;
            }
        }

        public Dictionary<String, Parametro> ObtenerDatosParametros(Bonificacion elAbono, DataTable datosDiccionario, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {

                Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                //Se consultan los parámetros del contrato
                //ya sea este o el proceso de abajo ambos deben de funcionar
                //losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato
                //(elAbono.ClaveColectiva, elAbono.Tarjeta, elAbono.ClaveEvento, "");

                //funciona
                DAOEventoExecute _DaoEvento = new DAOEventoExecute(conn, transaccionSQL);
                losParametros = _DaoEvento.ListaDeParamentrosContrato
                (elAbono.ClaveColectiva, elAbono.Tarjeta, elAbono.ClaveEvento, "", conn, transaccionSQL);



                //  losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato
                //(elAbono.ClaveColectiva, elAbono.Tarjeta, "TAR",elAbono.ClaveEvento, conn,transaccionSQL);


                if (string.IsNullOrEmpty(elAbono.Concepto))
                {
                    if (elAbono.cuentaOrigen == null)
                    {
                        elAbono.Concepto = losParametros.Values.ElementAt(0).Valor;
                    }
                    else
                    {
                        try
                        {
                            string cuentas = ": Origen:***" + elAbono.cuentaOrigen.Substring((elAbono.cuentaOrigen.Length - 5), 5) + " Destino:***" + elAbono.cuentaDestino.Substring((elAbono.cuentaDestino.Length - 5), (5));
                            elAbono.Concepto = losParametros.Values.ElementAt(0).Valor + cuentas;
                        }
                        catch (Exception ex)
                        {
                            elAbono.Concepto = losParametros.Values.ElementAt(0).Valor;
                        }
                    }

                }
                if (string.IsNullOrEmpty(elAbono.Observaciones))
                {
                    elAbono.Observaciones = "";
                }
                if (elAbono.RefNumerica == null)
                {
                    elAbono.RefNumerica = 0;
                }

                foreach (DataRow fila in datosDiccionario.Rows)
                {
                    if (!string.IsNullOrEmpty(fila["idTipocolectiva"].ToString()))
                    {
                        losParametros[fila["nombre"].ToString()] = new Parametro()
                        {
                            Nombre = fila["nombre"].ToString(),
                            Valor = fila["valor"].ToString(),
                            Descripcion = fila["descripcion"].ToString(),
                            ID_TipoColectiva = Convert.ToInt32(fila["idTipocolectiva"].ToString())
                        };
                    }
                    else
                    {
                        losParametros[fila["nombre"].ToString()] = new Parametro()
                        {
                            Nombre = fila["nombre"].ToString(),
                            Valor = fila["valor"].ToString(),
                            Descripcion = fila["descripcion"].ToString(),
                        };


                    }

                }

                return losParametros;

            }

            catch (Exception err)
            {
                //transaccionSQLTraspaso.Rollback();
                throw err;
            }
        }

        public DataTable ActualizaBanderaCobranza(Int64 ID_Corte, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = ID_Corte });
                
                dtResultado = consultaBDRead.ConstruirConsulta("[procnoc_ejecutor_ActualizaBanderaCobranzaProcesada]", parametros, "ActualizaBanderaCobranza", "", conn, transaccionSQL);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraGastosCobranza] Error al actualizar la bandera de procesado: " + ex.Message + " " + ex.StackTrace);

                return dtResultado;
            }
        }

        public DataTable ActualizaBanderaAnualidad(Int64 ID_Corte, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Corte", parametro = ID_Corte });

                dtResultado = consultaBDRead.ConstruirConsulta("[procnoc_ejecutor_ActualizaBanderaCobranzaAnualidad]", parametros, "ActualizaBanderaAnualidad", "", conn, transaccionSQL);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraGastosAnualidad] Error al actualizar la bandera de procesado: " + ex.Message + " " + ex.StackTrace);

                return dtResultado;
            }
        }
    }
}
