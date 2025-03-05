using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ProbarConexionAzure
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new Program().pruebaBulkKLAR2();
                return;

                //prueba conexion azure
              //  request.ProtocolVersion = HttpVersion.Version10; // THIS DOES THE TRICK
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                string appAKV = ConfigurationManager.AppSettings["applicationId"].ToString();
                string clave = ConfigurationManager.AppSettings["clientKey"].ToString();
                bool activarAzure = Convert.ToBoolean(ConfigurationManager.AppSettings["enableAzure"]);
                responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(appAKV, clave);
                Console.WriteLine(JsonConvert.SerializeObject(respuesta));
                string cadena = ConfigurationManager.AppSettings["cadena"].ToString();
                responseAzure respuestaObtenerCadena = KeyVaultProvider.ObtenerCadenasDeConexionAzure(appAKV, clave, cadena);// "CACAO-DESA-PN-PN-W");
                Console.WriteLine(JsonConvert.SerializeObject(respuestaObtenerCadena));

                //string cadenaConexion = ConfigurationManager.ConnectionStrings["BDAutorizadorRead"].ToString();
                //DataTable tablaResult = new ConexionBase(cadenaConexion).ConstruirConsulta("pruebaAzure", null, "");
                //DataTable tabla2 = ConexionBase.ejecutarSP("");



            } catch (Exception ex){
                Console.WriteLine(ex.Message);
            }

            Console.ReadKey();
        }

        public void pruebaBulkKLAR() {
            try
            {

                string appAKV = ConfigurationManager.AppSettings["applicationId"].ToString();
                string clave = ConfigurationManager.AppSettings["clientKey"].ToString();
                bool activarAzure = Convert.ToBoolean(ConfigurationManager.AppSettings["enableAzure"]);
                responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(appAKV, clave);

                DataTable dt; //GenerarTablaFicheroST7(file, ID_Fichero);

                dt = new DataTable("Detalle");
                dt.Columns.Add(new DataColumn("ID_FicheroPROSA"));
                dt.Columns.Add(new DataColumn("ID_FicheroTemp"));
                dt.Columns.Add(new DataColumn("P001"));
                dt.Columns.Add(new DataColumn("P002"));
                dt.Columns.Add(new DataColumn("P003"));
                dt.Columns.Add(new DataColumn("P004"));
                dt.Columns.Add(new DataColumn("P005"));
                dt.Columns.Add(new DataColumn("P006"));

                DataRow row = dt.NewRow();
                row["ID_FicheroPROSA"] = null;
                row["ID_FicheroTemp"] = 1;
                row["P001"] = "9";
                row["P002"] = "";
                row["P003"] = "Prueba";
                row["P004"] = "";
                row["P005"] = "";//line.NumeroTarjeta;
                row["P006"] = "1234";
                dt.Rows.Add(row);

                string cadena = ConfigurationManager.AppSettings["cadena"].ToString();
                responseAzure respuestaObtenerCadena = KeyVaultProvider.ObtenerCadenasDeConexionAzure(appAKV, clave, cadena);// "CACAO-DESA-PN-PN-W");

                using (SqlConnection conn = new SqlConnection(respuestaObtenerCadena.valorAzure))
                {
                    conn.Open();
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, null))
                    {
                        bulkCopy.BulkCopyTimeout = 30000;
                        bulkCopy.DestinationTableName = "dbo.FicheroStat07";
                        try
                        {
                            bulkCopy.WriteToServer(dt);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Erro bulk:" + ex.Message);
                        }
                    }
                }
                Console.WriteLine("insercion correcta");

            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                //  Log.Error("GuardarUnDetalleFicheroST7(): " + file.UrlArchivo + ", ERROR:" + err.Message);

            }
            Console.ReadKey();
        }


        public void pruebaBulkKLAR2()
        {
            try
            {

                string appAKV = ConfigurationManager.AppSettings["applicationId"].ToString();
                string clave = ConfigurationManager.AppSettings["clientKey"].ToString();
                bool activarAzure = Convert.ToBoolean(ConfigurationManager.AppSettings["enableAzure"]);
                responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(appAKV, clave);
                string archivo = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase+ConfigurationManager.AppSettings["nombreArchivo"];
                DataTable dt; //GenerarTablaFicheroST7(file, ID_Fichero);

                dt = new DataTable("Detalle");
                dt.Columns.Add(new DataColumn("ID_FicheroPROSA"));
                dt.Columns.Add(new DataColumn("ID_FicheroTemp"));
                dt.Columns.Add(new DataColumn("P001"));
                dt.Columns.Add(new DataColumn("P002"));
                dt.Columns.Add(new DataColumn("P003"));
                dt.Columns.Add(new DataColumn("P004"));
                dt.Columns.Add(new DataColumn("P005"));
                dt.Columns.Add(new DataColumn("P006"));

                using (StreamReader sr = new StreamReader(archivo, Encoding.UTF8))
                {
                   
                    while (!sr.EndOfStream)
                    {
                        String line = sr.ReadLine();

                        DataRow row = dt.NewRow();
                        row["ID_FicheroPROSA"] = null;
                        row["ID_FicheroTemp"] = 1;
                        row["P001"] = line.Substring(1,2);
                        row["P002"] = "";
                        row["P003"] = line.Substring(10, 6);
                        row["P004"] = "";
                        row["P005"] = "";//line.NumeroTarjeta;
                        row["P006"] = line.Substring(17, 3); 
                        dt.Rows.Add(row);

                        string cadena = ConfigurationManager.AppSettings["cadena"].ToString();
                        responseAzure respuestaObtenerCadena = KeyVaultProvider.ObtenerCadenasDeConexionAzure(appAKV, clave, cadena);// "CACAO-DESA-PN-PN-W");

                        using (SqlConnection conn = new SqlConnection(respuestaObtenerCadena.valorAzure))
                        {
                            conn.Open();
                            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, null))
                            {
                                bulkCopy.BulkCopyTimeout = 30000;
                                bulkCopy.DestinationTableName = "dbo.FicheroStat07";
                                try
                                {
                                    bulkCopy.WriteToServer(dt);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Erro bulk:" + ex.Message);
                                }
                            }
                        }
                        Console.WriteLine("insercion correcta");
                    }
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                //  Log.Error("GuardarUnDetalleFicheroST7(): " + file.UrlArchivo + ", ERROR:" + err.Message);

            }
            Console.ReadKey();
        }

    }
}
