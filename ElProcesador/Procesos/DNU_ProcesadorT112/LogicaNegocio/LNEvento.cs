using CommonProcesador;
using DNU_ProcesadorT112.BaseDatos;
using Executer.Entidades;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorT112.LogicaNegocio
{
    public class LNEvento
    {
        /// </summary>
        public static void ProcesarEvento(String MedioAcceso, float Importe , String ClavedeEvento, String ClaveCadenaComercial, String NombreArchivo)
        {
            SqlConnection conn = null;

            try
            {
                conn = new SqlConnection(BDAutorizador.strBDEscritura);
                conn.Open();

                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        Poliza laPoliza = null;

                        Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                        //Se consultan los parámetros del contrato
                        losParametros = new Executer.BaseDatos.DAOEvento(conn, transaccionSQL).ListaDeParamentrosContrato(ClaveCadenaComercial, "", ClavedeEvento, "","");

                        losParametros["@ID_CuentaHabiente"] = new Parametro()
                        {
                            Nombre = "@ID_CuentaHabiente",
                            Valor = (losParametros["@ID_Cuentahabiente"].ToString()),
                            Descripcion = "ID CuentaHabiente",
                            ID_TipoColectiva = 10
                        };
                        losParametros["@Importe"] = new Parametro()
                        {
                            Nombre = "@Importe",
                            Valor = Importe.ToString(),
                            Descripcion = "Importe"
                        };
                        losParametros["@MedioAcceso"] = new Parametro()
                        {
                            Nombre = "@MedioAcceso",
                              Valor = MedioAcceso,
                        };
                        losParametros["@TipoMedioAcceso"] = new Parametro()
                        {
                            Nombre = "@TipoMedioAcceso",
                              Valor ="TAR",
                        };


                        //Genera y Aplica la Poliza
                        Executer.EventoManual aplicador = new Executer.EventoManual(Int32.Parse(losParametros["@ID_Evento"].ToString()),
                           (losParametros["@Descripcion"].ToString()), false, 0, losParametros, "Proceso archivo: " + NombreArchivo, conn, transaccionSQL);
                        laPoliza = aplicador.AplicaContablilidad();

                        if (laPoliza.CodigoRespuesta != 0)
                        {
                            transaccionSQL.Rollback();
                            throw new Exception("No se generó la Póliza: " + laPoliza.DescripcionRespuesta + " Para la Cuenta:" + losParametros["@MedioAcceso"].ToString());
                        }

                        else
                        {
                            transaccionSQL.Commit();
                            Logueo.Evento("SE EJECUTO EL EVENTO:  " + losParametros["@ID_Evento"].ToString()  + ", PARA LA TARJETA:" +losParametros["@MedioAcceso"].ToString());
                        }
                    }

                    catch (Exception err)
                    {
                        transaccionSQL.Rollback();
                        throw err;
                    }
                }
            }

            catch (Exception err)
            {
                throw new Exception("RegistraEvManual_AbonoPuntos() " + err.Message);
            }

            finally
            {
                if (null != conn && ConnectionState.Open == conn.State)
                {
                    conn.Close();
                }
            }

        }

    }
}
