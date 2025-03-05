using CommonProcesador;
using DNU_ConciliadorCacao.Entidades;
using DNU_ConciliadorCacao.Utilidades;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;


namespace DNU_ConciliadorCacao.BaseDatos
{
    public class DAODatosBase
    {

        public static DatosBaseDatos ObtenerConsulta(Int64 ID_Archivo)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            DatosBaseDatos unaConsulta = new DatosBaseDatos();
            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("Proc_ObtieneConfigConsulta");
                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP Proc_ObtieneConfigConsulta " + ID_Archivo + "]");
                command.Parameters.Add(new SqlParameter("@ID_Archivo", ID_Archivo));



                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {


                        unaConsulta.CadenaConexion = (String)losDatos.Tables[0].Rows[k]["CadenaConexion"];
                        unaConsulta.Clave = (String)losDatos.Tables[0].Rows[k]["Clave"];
                        unaConsulta.ID_Consulta = (Int64)losDatos.Tables[0].Rows[k]["ID_Consulta"];
                        unaConsulta.Descripcion = (String)losDatos.Tables[0].Rows[k]["DescripcionConsulta"];
                        unaConsulta.StoredProcedure = (String)losDatos.Tables[0].Rows[k]["StoredProcedure"];


                        //unaConsulta.losDatosdeBD = ObtenerConfiguracionFila(ID_Consulta);
                        for (int i = 0; unaConsulta.ID_Consulta == (Int64)losDatos.Tables[0].Rows[k]["ID_Consulta"]; i++)
                        {
                            CampoConfig unaConfiguracion = new CampoConfig();

                            unaConfiguracion.Descripcion = (String)losDatos.Tables[0].Rows[k]["DescripcionCampoConsulta"];
                            unaConfiguracion.EsClave = (Boolean)losDatos.Tables[0].Rows[k]["EsClave"];
                            unaConfiguracion.ID_TipoColectiva = (Int32)losDatos.Tables[0].Rows[k]["ID_TipoColectiva"];
                            unaConfiguracion.Nombre = (String)losDatos.Tables[0].Rows[k]["Nombre"];
                            unaConfiguracion.Posicion = (Int32)losDatos.Tables[0].Rows[k]["Posicion"];
                            unaConfiguracion.TrimCaracteres = (String)losDatos.Tables[0].Rows[k]["TrimCaracteres"];


                            unaConsulta.losDatosdeBD.Add(unaConfiguracion.Posicion, unaConfiguracion);

                            k++;

                            if (losDatos.Tables[0].Rows.Count == (k))
                            {
                                break;
                            }
                        }


                    }
                }

                return unaConsulta;
            }
            catch (Exception ex)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[ObtenerConsulta] {0} - Stack {1}", ex.Message, ex.StackTrace) + "]");
               return unaConsulta;
            }
        }

        public static object InsertaConciliacionCacao(DatosBaseDatos config, SyscardConciliation rows,
            String apiKey, String user)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            var total = 0;
            try
            {
                //var client = new RestClient("http://162.209.73.201:3001/api/v1/");
                var client = new RestClient(config.CadenaConexion);

                var request = new RestRequest("/set_syscardConciliation", Method.POST);
                request.AddJsonBody(rows);

                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Api-Key", apiKey);
                request.AddHeader("user", user);

                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Request del ws de inserción de datos CACAO]");
                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + Newtonsoft.Json.JsonConvert.SerializeObject(request) + "]");
               
                IRestResponse resultHttp = client.Execute(request);

                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Response del ws de inserción de datos CACAO]");
                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + Newtonsoft.Json.JsonConvert.SerializeObject(resultHttp) + "]");
             }
            catch (Exception err)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[InsertaConciliacionCacao] :{0} Stack {1}", err.Message, err.StackTrace) + "]");
            }

            return total;
        }

        public static Int32 InsertaDatosEnTabla(DatosBaseDatos laConfig
            , Int64 ID_Fichero, Archivo unArchivo, string minProcessFileDate, string maxProcessFileDate,
            String apiKey, String user)
        {
            var total = 0;
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            try
            {
                string emisor = GetEmisorFromFile(unArchivo);
                string idConsulta = GetIdConsultaFromFile(unArchivo);
                string connectionString = laConfig.CadenaConexion;

                DataColumn cID_DatosBD = new DataColumn("ID_DatosBD");
                DataColumn cID_Fichero = new DataColumn("ID_Fichero");
                DataColumn cID_Consulta = new DataColumn("ID_Consulta");
                DataColumn cemisor = new DataColumn("emisor");
                DataColumn ccodigoMovimiento = new DataColumn("codigoMovimiento");
                DataColumn cfechaMovimiento = new DataColumn("fechaMovimiento");
                DataColumn choraMovimiento = new DataColumn("horaMovimiento");
                DataColumn cmonto = new DataColumn("monto");
                DataColumn cnumero_tarjeta = new DataColumn("numero_tarjeta");
                DataColumn cnumero_documento = new DataColumn("numero_documento");
                DataColumn cfechaCreacion = new DataColumn("fechaCreacion");

                DataTable dtres = new DataTable("trxes");
                dtres.Columns.Add(cID_DatosBD);
                dtres.Columns.Add(cID_Fichero);
                dtres.Columns.Add(cID_Consulta);
                dtres.Columns.Add(cemisor);
                dtres.Columns.Add(ccodigoMovimiento);
                dtres.Columns.Add(cfechaMovimiento);
                dtres.Columns.Add(choraMovimiento);
                dtres.Columns.Add(cmonto);
                dtres.Columns.Add(cnumero_tarjeta);
                dtres.Columns.Add(cnumero_documento);
                dtres.Columns.Add(cfechaCreacion);


                //var client = new RestClient("http://162.209.73.201:3001/api/v1/");
                var client = new RestClient(laConfig.CadenaConexion);

                //var request = new RestRequest("/get_trxes_emisor", Method.GET);
                var request = new RestRequest(laConfig.StoredProcedure, Method.GET);
                request.AddQueryParameter("fecha_ini", minProcessFileDate);
                request.AddQueryParameter("fecha_fin", maxProcessFileDate);
                request.AddQueryParameter("emisor", emisor);

                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Api-Key", apiKey);
                request.AddHeader("user", user);

                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Request del ws de obteneción de datos CACAO]");
                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + Newtonsoft.Json.JsonConvert.SerializeObject(request) + "]");
               
                IRestResponse resultHttp = client.Execute(request);

                var result = Newtonsoft.Json.JsonConvert.DeserializeObject<TrxesEmisorResult>(resultHttp.Content);

                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [Response del ws de obteneción de datos CACAO]");
                LogueoProConciliaCacao.Info("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + Newtonsoft.Json.JsonConvert.SerializeObject(resultHttp) + "]");
               
                if (result.detail == null)
                    throw new Exception(String.Format("No se ha recibido información suficiente de la consutla de datos de CACAO - {0}",
                        Newtonsoft.Json.JsonConvert.SerializeObject(resultHttp)));

                if (result.detail.Count() == 0)
                    throw new Exception(String.Format("No se ha recibido información suficiente de la consutla de datos de CACAO - {0}",
                        Newtonsoft.Json.JsonConvert.SerializeObject(resultHttp)));


                foreach (var row in result.detail.Where(w => w.codigo_movimiento != null &&
                                            w.codigo_movimiento != "TF" &&
                                            Convert.ToDecimal(w.monto) > 0 &&
                                            w.status == true))
                {
                    dtres.Rows.Add(
                                null,
                                ID_Fichero,
                                idConsulta,
                                emisor,
                                row.codigo_movimiento,
                                row.fecha,
                                row.created_at.ToString("HH:mm:ss").Replace(":", string.Empty),
                                row.monto,
                                row.numero_tarjeta,
                                row.numero_documento,
                                row.created_at);
                }

                total = dtres.Rows.Count;
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    bulkCopy.DestinationTableName =
                        "dbo.DatosBD";

                    try
                    {
                        // Write from the source to the destination.
                        bulkCopy.WriteToServer(dtres);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            catch (Exception err)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("[InsertaDatosEnTabla] {0} - Stack : {1}", err.Message, err.StackTrace) + "]");
             }

            return total;
        }

        private static string GetIdConsultaFromFile(Archivo unArchivo)
        {
            return unArchivo.ID_ConsultaBD.ToString();
        }

        private static String GetEmisorFromFile(Archivo unArchivo)
        {
            return unArchivo.LosDatos.FirstOrDefault().losCampos[1].Trim();
        }

        public static string GetEmisor(Archivo unArchivo)
        {
            return unArchivo.LosDatos.FirstOrDefault().losCampos[1].
                ToString().Trim();
        }

        public static string GetMinProcessDateFromFile(Archivo unArchivo)
        {
            var year = Convert.ToInt32(unArchivo.LosDatos.FirstOrDefault().losCampos[3].ToString().Trim().Substring(0, 4));
            var month = Convert.ToInt32(unArchivo.LosDatos.FirstOrDefault().losCampos[3].ToString().Trim().Substring(4, 2));
            var day = Convert.ToInt32(unArchivo.LosDatos.FirstOrDefault().losCampos[3].ToString().Trim().Substring(6, 2));
            DateTime tempDate = new DateTime(year, month, day);

            return tempDate.AddDays(-1).ToString("yyyyMMdd");

        }

        public static string GetMaxProcessDateFromFile(Archivo unArchivo)
        {
            return unArchivo.LosDatos.FirstOrDefault().losCampos[3].ToString().Trim();

        }

    }
}
