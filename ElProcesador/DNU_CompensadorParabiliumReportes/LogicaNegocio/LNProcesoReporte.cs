using DNU_CompensadorParabiliumCommon.BaseDatos;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumCommon.LogicaNegocio;
using DNU_CompensadorParabiliumCommon.Utilidades;
using DNU_CompensadorParabiliumReportes.BaseDatos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumReportes.LogicaNegocio
{
    public class LNProcesoReporte
    {

        /// <summary>
        /// Metodo que devuleve la configuracion correspondiente el reporte de PD
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="conexion"></param>
        /// <returns></returns>
        public static DataTable ObtenerCuentasInternas(DataTable dt, string conexion)
        {
            DataTable reporteCopy = crearTabReportCopy();
            int count = 0, rowTab = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (count != 0) 
                {
                    reporteCopy.Rows.Add();
                    reporteCopy.Rows[rowTab]["Tarjeta"] = row["Tarjeta"].ToString().TrimEnd(' ');

                    rowTab += 1;
                }
                count += 1;
            }

            return DAOUtilidades.ObtenerCuentaInterna(reporteCopy, conexion);


        }

        private static DataTable crearTabReportCopy() 
        {
            var finalDT = new DataTable();
            finalDT.Columns.Add("Tarjeta", typeof(System.String));

            return finalDT;
        }

        public bool ObtenerYGenerarReports(string connWriteAuto, string etiquetaLogueo, string tipoReporte, string connReadT112
                                    , string connReadAuto)
        {
            Hashtable ht;
            ht = new Hashtable();
            ht.Add("@cveReporte", tipoReporte);
            DataTable reportesAgenerar = ExecuteSp.ejecutarSp("Procnoc_ObtenerReportesAgenerar_v7", ht, connReadAuto);

            foreach (DataRow dr in reportesAgenerar.Rows)
            {
                try
                {
                    ht = new Hashtable();

                    ht.Add("@fecha", dr["fechaPresentacion"]);
                    ht.Add("@ClavePlugIn", dr["ClavePlugin"]);

                    DataTable respReporte = ExecuteSp.ejecutarSp(dr["spExecute"].ToString(), ht, connReadT112);

                    bool respGenerarFile = generarFile(dr["dirSalida"].ToString(), dr["ClavePlugin"].ToString().Replace("|", "")
                               , dr["fechaPresentacion"], dr["fechaCompensacion"]
                               , respReporte, etiquetaLogueo, dr["identificador"].ToString()
                               , tipoReporte, dr["tipoProd"].ToString(), dr["urlSFTP"].ToString(), dr["portSFTP"].ToString(), dr["userSFTP"].ToString(), dr["pwstSFTP"].ToString(), dr["tipoServicio"].ToString());

                    if (respGenerarFile)
                    {
                        ht = new Hashtable();

                        ht.Add("@idReporte", dr["idReporte"]);
                        ht.Add("@cveReporte", tipoReporte);

                        ExecuteSp.ejecutarSp("Procnoc_ActualizarEstatusReporte", ht, connWriteAuto);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(etiquetaLogueo + "[" + dr["spExecute"].ToString() + "] [ObtenerReportes] [" + ex.Message + "] [" + ex.StackTrace + "]");
                }
            }

            return true;
        }

        /// <summary>
        /// /Generar archivo para transacciones no procesadad por Timeout..
        /// </summary>
        /// <param name="dirSalida"></param>
        /// <param name="clavePlugin"></param>
        /// <param name="fecPresentacion"></param>
        /// <param name="fecCompensacion"></param>
        /// <param name="reporte"></param>
        /// <param name="idLog"></param>
        /// <param name="identificador"></param>
        /// <param name="cveReporte"></param>
        /// <param name="tipoProd"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="tipoEnvio"></param>
        public void generarFileTxnNoProcesasadas(string dirSalida, string clavePlugin, object fecPresentacion
                                    , object fecCompensacion, DataTable reporte, string etiquetaLogueo, string identificador, bool reporteinicial
                                    , string cveReporte = null, string tipoProd = null, string host = null, string port = null, string user = null, string password = null, string tipoEnvio = null)
        {
            try
            {
                string nombrecomplemeto = string.Empty;

                if (!reporteinicial)
                    nombrecomplemeto = "_reproceso_"; ///segundo reporte
                else
                    nombrecomplemeto = "_";  //primer reporte


                string nombreArchivoSalida = identificador + nombrecomplemeto
                                                           + Convert.ToDateTime(DateTime.Now).ToString("yyyyMMdd")
                                                           + ".txt";

                Log.Evento(etiquetaLogueo + "[Inicia Generación de Archivo Reporte con nombre: " + nombreArchivoSalida);

                if (tipoEnvio.Equals("SFTP"))
                {
                    if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(port) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                    {


                        string temp = string.Empty;
                        temp = Path.Combine(Environment.CurrentDirectory, nombreArchivoSalida);

                        StreamWriter streamSalidaTemp = File.CreateText(temp);
                        foreach (DataRow dr in reporte.Rows)
                        {
                            try
                            {
                                streamSalidaTemp.WriteLine(dr["Registro"].ToString());
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        streamSalidaTemp.Close();

                        Log.Evento(etiquetaLogueo + "[Inicia Envio de archivo por SFTP nombre: " + nombreArchivoSalida);
                        LNFtp sftpConnected = new LNFtp(host, dirSalida, user, password, port);
                        sftpConnected.UploadFileSFTP(temp, string.Empty);
                        if (File.Exists(temp))
                        {
                            File.Delete(temp); // Eliminar temporal

                        }
                    }
                    else
                    {
                        Log.Error(etiquetaLogueo + "[generarFile " + clavePlugin + ", " + fecPresentacion + "] [Faltan parametros para enviar archivo por SFTP] ");
                    }
                }
                else
                {
                    StreamWriter streamSalida = File.CreateText(dirSalida + "\\" + nombreArchivoSalida);
                    foreach (DataRow dr in reporte.Rows)
                    {
                        try
                        {
                            streamSalida.WriteLine(dr["Registro"].ToString());
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    streamSalida.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(etiquetaLogueo + "[generarFile " + clavePlugin + ", " + fecPresentacion + "] [" + ex.Message + "] [" + ex.StackTrace + "]");
            }
        }

        public bool generarFile(string dirSalida, string clavePlugin, object fecPresentacion
                                    , object fecCompensacion, DataTable reporte, string etiquetaLogueo, string identificador
                                    , string cveReporte = null, string tipoProd = null, string host = null, string port = null, string user = null, string password = null, string tipoEnvio = null)
        {
            bool respuesta = false;
            try
            {
                string fechaComTemp = string.IsNullOrEmpty(fecCompensacion.ToString())
                                        ? DateTime.Now.ToString("yyyyMMdd")
                                        : Convert.ToDateTime(fecCompensacion).ToString("yyyyMMdd");
                string nombreArchivoSalida;
                if (cveReporte != null && cveReporte.Equals("PRED2"))
                {

                    nombreArchivoSalida = identificador + "_" + tipoProd + "_"
                                                          + Convert.ToDateTime(fecPresentacion).ToString("ddMMyy")
                                                          + "_" + clavePlugin + ".txt";

                }
                else
                {

                    nombreArchivoSalida = identificador + "_" + clavePlugin + "_"
                                                          + Convert.ToDateTime(fecPresentacion).ToString("yyyyMMdd")
                                                          + "_" + fechaComTemp + ".txt";
                }

                Log.Evento(etiquetaLogueo + "[Inicia Generación de Archivo Reporte con nombre: " + nombreArchivoSalida);

                if (tipoEnvio.Equals("SFTP"))
                {
                    if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(port) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                    {


                        string temp = string.Empty;
                        temp = Path.Combine(Environment.CurrentDirectory, nombreArchivoSalida);

                        StreamWriter streamSalidaTemp = File.CreateText(temp);
                        foreach (DataRow dr in reporte.Rows)
                        {
                            try
                            {
                                streamSalidaTemp.WriteLine(dr[0].ToString());
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        streamSalidaTemp.Close();

                        Log.Evento(etiquetaLogueo + "[Inicia Envio de archivo por SFTP nombre: " + nombreArchivoSalida);
                        LNFtp sftpConnected = new LNFtp(host, dirSalida, user, password, port);
                        sftpConnected.UploadFileSFTP(temp, string.Empty);
                        if (File.Exists(temp))
                        {
                            File.Delete(temp); // Eliminar temporal

                        }

                        return respuesta = true;

                    }
                    else
                    {
                        Log.Error(etiquetaLogueo + "[generarFile " + clavePlugin + ", " + fecPresentacion + "] [Faltan parametros para enviar archivo por SFTP] ");
                    }
                }
                else
                {
                    StreamWriter streamSalida = File.CreateText(dirSalida + "\\" + nombreArchivoSalida);
                    foreach (DataRow dr in reporte.Rows)
                    {
                        try
                        {
                            streamSalida.WriteLine(dr[0].ToString());
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    streamSalida.Close();

                    return respuesta = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(etiquetaLogueo + "[generarFile " + clavePlugin + ", " + fecPresentacion + "] [" + ex.Message + "] [" + ex.StackTrace + "]");
            }

            return respuesta;
        }

        public static string ConsultarMonedaCliente(string moneda)
        {
            return DNU_CompensadorParabiliumCommon.BaseDatos.DAOComparador.ObtieneMonedaCuenta(moneda);
        }
        public static string ObtenerTarjetaConsultaMoneda(DataTable dt)
        {
            string tarjeta = string.Empty;
            string claveMoneda = string.Empty;
            foreach (DataRow row in dt.Rows)
            {
                if (!string.IsNullOrEmpty(row["Tarjeta"].ToString()))
                {
                    tarjeta = (row["Tarjeta"].ToString());
                    break;
                }
            }
            if (!string.IsNullOrEmpty(tarjeta))
            {
                claveMoneda = ConsultarMonedaCliente(tarjeta);
            }
            return claveMoneda;
        }

        public DataTable CalcularNuevoImporte(DataTable reporte, ValoresContratos val, string monedaCuenta)
        {
            string MCCR;
            string MonedaOrigen;
            string Importe;
            decimal IMPORTE;
            if (monedaCuenta.Equals("484")) //Si es Mexico
            {
                foreach (DataRow row in reporte.Rows)
                {
                    MCCR = string.Empty;
                    MonedaOrigen = string.Empty;
                    if (!string.IsNullOrEmpty(row["Tarjeta"].ToString())) //Valdia que exista tarjeta en la fila nos ayuda a a saltar cabecero y blancos
                    {
                        MCCR = string.IsNullOrEmpty(row["MCCR"].ToString()) ? "0" : row["MCCR"].ToString();
                        MonedaOrigen = string.IsNullOrEmpty(row["MonedaOrigen"].ToString()) ? "0" : row["MonedaOrigen"].ToString();
                        Importe = string.IsNullOrEmpty(row["Importe"].ToString()) ? "0" : row["Importe"].ToString();

                        if (Convert.ToDecimal(MCCR) > 0)
                        {
                            IMPORTE =
                                   Convert.ToDecimal(Importe) +
                                                               ((Convert.ToDecimal(Importe) * Convert.ToDecimal(val.MarkupPorcentaje))
                                                              + Convert.ToDecimal(val.MarkupFijo));
                            row["Importe"] = decimal.Round(IMPORTE, 2);

                        }


                    }

                }
            }
            return reporte;
        }

        public static ValoresContratos ObtenerContratosCliente(string conexion, string gc, string ic)
        {
            DataTable contratos = new DataTable();
            ValoresContratos Vcontrato = new ValoresContratos();


            Vcontrato = LUtilidades.obtenerDatosContrato(DAOUtilidades.ObtieneParametrosCliente(conexion, gc, string.Empty));
            Vcontrato.EnmascararTarjeta = string.IsNullOrEmpty(Vcontrato.EnmascararTarjeta) ? "0" : Vcontrato.EnmascararTarjeta;
            Vcontrato.MostrarCI = string.IsNullOrEmpty(Vcontrato.MostrarCI) ? "0.0" : Vcontrato.MostrarCI;
            Vcontrato.MarkupPorcentaje = string.IsNullOrEmpty(Vcontrato.MarkupPorcentaje) ? "0.0" : Vcontrato.MarkupPorcentaje;
            Vcontrato.MarkupFijo = string.IsNullOrEmpty(Vcontrato.MarkupFijo) ? "0.0" : Vcontrato.MarkupFijo;
            return Vcontrato;

        }
        /// <summary>
        /// metodo que calcula la comision para el preeliminar detallado
        /// </summary>
        /// <param name="dtconfig"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public string obtenerPorcentageComision(DataTable dtconfig, string MostrarCI)
        {
            decimal comisionFinal = 0;
            decimal comision = 0;
            string comisions = string.Empty;
            foreach (DataRow row in dtconfig.Rows)
            {
                if (!string.IsNullOrEmpty(row["Comision"].ToString()))
                {
                    comision = Convert.ToDecimal(row["Comision"].ToString());
                    break;
                }

            }
            comisionFinal = Convert.ToDecimal(MostrarCI) * comision;

            return decimal.Round(comisionFinal, 2).ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirSalida"></param>
        /// <param name="clavePlugin"></param>
        /// <param name="fecPresentacion"></param>
        /// <param name="fecCompensacion"></param>
        /// <param name="reporte"></param>
        /// <param name="idLog"></param>
        /// <param name="identificador"></param>
        /// <param name="cveReporte"></param>
        /// <param name="tipoProd"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="tipoEnvio"></param>
        public void generarFilePresentado(string dirSalida, string clavePlugin, object fecPresentacion
                            , object fecCompensacion, DataTable reporte, string etiquetaLogueo, string identificador
                            , string cveReporte = null, string tipoProd = null, string host = null, string port = null, string user = null, string password = null, string tipoEnvio = null, string conn = null)
        {

            ValoresContratos contrato = new ValoresContratos();
            DataTable dtConfiguracion = new DataTable();
            string comision = string.Empty;
            string monedaCuenta = string.Empty;
            try
            {


                string fechaComTemp = string.IsNullOrEmpty(fecCompensacion.ToString())
                                        ? DateTime.Now.ToString("yyyyMMdd")
                                        : Convert.ToDateTime(fecCompensacion).ToString("yyyyMMdd");
                string nombreArchivoSalida;

                nombreArchivoSalida = identificador + "_" + clavePlugin + "_"
                                                      + Convert.ToDateTime(fecPresentacion).ToString("yyyyMMdd")
                                                      + "_" + fechaComTemp + ".txt";

                Log.Evento(etiquetaLogueo + "[Inicia Generación de Archivo Reporte con nombre: " + nombreArchivoSalida);


                contrato = ObtenerContratosCliente(conn, clavePlugin, string.Empty);
                dtConfiguracion = ObtenerCuentasInternas(reporte, conn);
                comision = obtenerPorcentageComision(reporte, contrato.MostrarCI);
                monedaCuenta = ObtenerTarjetaConsultaMoneda(reporte);
                reporte = CalcularNuevoImporte(reporte, contrato, monedaCuenta);

                if (tipoEnvio.Equals("SFTP"))
                {
                    if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(port) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                    {


                        string temp = string.Empty;
                        temp = Path.Combine(Environment.CurrentDirectory, nombreArchivoSalida);

                        StreamWriter streamSalidaTemp = File.CreateText(temp);
                        foreach (DataRow dr in reporte.Rows)
                        {
                            try
                            {
                                streamSalidaTemp.WriteLine(EvaluarSalidaRegistro(dr[0].ToString(), dtConfiguracion, dr, contrato, comision));
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        streamSalidaTemp.Close();

                        Log.Evento(etiquetaLogueo + "[Inicia Envio de archivo por SFTP nombre: " + nombreArchivoSalida);
                        LNFtp sftpConnected = new LNFtp(host, dirSalida, user, password, port);
                        sftpConnected.UploadFileSFTP(temp, string.Empty);
                        if (File.Exists(temp))
                        {
                            File.Delete(temp); // Eliminar temporal

                        }


                    }
                    else
                    {
                        Log.Error(etiquetaLogueo + "[generarFile " + clavePlugin + ", " + fecPresentacion + "] [Faltan parametros para enviar archivo por SFTP] ");
                    }
                }
                else
                {
                    StreamWriter streamSalida = File.CreateText(dirSalida + "\\" + nombreArchivoSalida);
                    foreach (DataRow dr in reporte.Rows)
                    {
                        try
                        {
                            streamSalida.WriteLine(EvaluarSalidaRegistro(dr["Registro"].ToString(), dtConfiguracion, dr, contrato, comision));
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    streamSalida.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(etiquetaLogueo + "[generarFile " + clavePlugin + ", " + fecPresentacion + "] [" + ex.Message + "] [" + ex.StackTrace + "]");
            }
        }
        /// <summary>
        /// Evalua y aplica configuracion para el reporte, obtiene la cuenta interna...
        /// </summary>
        /// <param name="registro"></param>
        /// <param name="dtConfiguracion"></param>
        /// <param name="dr"></param>
        /// <returns>regresa el nuevo regisdtro para escribirlo en el archivo con la configuracion aplicada</returns>
        private static string EvaluarSalidaRegistro(string registro, DataTable dtConfiguracion, DataRow dr, ValoresContratos v, string comision)
        {

            string tarjeta = string.IsNullOrEmpty(dr["Tarjeta"].ToString()) ? string.Empty : dr["Tarjeta"].ToString().TrimEnd(' ');
            string cuentaInterna = string.Empty;
            string enmascarar = string.Empty;
            if (!string.IsNullOrEmpty(tarjeta)) //Valida que en efecto sea un renglon de una tarjeta y no espacio o cabeceros.
            {
                var a = (from dtc in dtConfiguracion.AsEnumerable() ///Expresion que busca en el datatable la configuracion de la tarjeta
                         where dtc.Field<string>("Tarjeta").Trim() == dr["Tarjeta"].ToString().Trim()
                         select new
                         {
                             cuentaInterna = dtc.Field<string>("CuentaInterna"),
                             Tarjeta = dtc.Field<string>("Tarjeta"),

                         }
                ).FirstOrDefault();

                cuentaInterna = string.IsNullOrEmpty(a.cuentaInterna) ? string.Empty : a.cuentaInterna;

                if (v.EnmascararTarjeta.Equals("1"))
                    registro = registro.Replace("@Tarjeta", "* * ** " + a.Tarjeta.Substring(12, 4));
                else
                    registro = registro.Replace("@Tarjeta", a.Tarjeta);


                registro = registro.Replace("@CuentaInterna", cuentaInterna);
                registro = registro.Replace("@Comision", comision.ToString());
                registro = registro.Replace("@Importe", dr["Importe"].ToString());


            }

            return registro;
        }

        //Para evaluar los medios de acceso con los contratos de encriptar debido a las caracteristicas de AES
        public bool generarFileAES(string dirSalida, string clavePlugin, object fecPresentacion
                                    , object fecCompensacion, DataTable reporte, string etiquetaLogueo, string identificador
                                    , string cveReporte = null, string tipoProd = null, string host = null, string port = null, string user = null, string password = null, string tipoEnvio = null)
        {
            bool respuesta = false;
            try
            {
                string fechaComTemp = string.IsNullOrEmpty(fecCompensacion.ToString())
                                        ? DateTime.Now.ToString("yyyyMMdd")
                                        : Convert.ToDateTime(fecCompensacion).ToString("yyyyMMdd");
                string nombreArchivoSalida;

                nombreArchivoSalida = identificador + "_" + clavePlugin + "_"
                                                      + Convert.ToDateTime(fecPresentacion).ToString("yyyyMMdd")
                                                      + "_" + fechaComTemp + ".txt";


                Log.Evento(etiquetaLogueo + "[Inicia Generación de Archivo Reporte con nombre: " + nombreArchivoSalida);

                if (tipoEnvio.Equals("SFTP"))
                {
                    if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(port) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                    {


                        string temp = string.Empty;
                        temp = Path.Combine(Environment.CurrentDirectory, nombreArchivoSalida);

                        StreamWriter streamSalidaTemp = File.CreateText(temp);
                        foreach (DataRow dr in reporte.Rows)
                        {
                            try
                            {
                                streamSalidaTemp.WriteLine(EvaluarRegistroAES(dr["Registro"].ToString(), dr));
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        streamSalidaTemp.Close();

                        Log.Evento(etiquetaLogueo + "[Inicia Envio de archivo por SFTP nombre: " + nombreArchivoSalida);
                        LNFtp sftpConnected = new LNFtp(host, dirSalida, user, password, port);
                        sftpConnected.UploadFileSFTP(temp, string.Empty);
                        if (File.Exists(temp))
                        {
                            File.Delete(temp); // Eliminar temporal

                        }

                        return respuesta = true;
                    }
                    else
                    {
                        Log.Error(etiquetaLogueo + "[generarFile " + clavePlugin + ", " + fecPresentacion + "] [Faltan parametros para enviar archivo por SFTP] ");
                    }
                }
                else
                {
                    StreamWriter streamSalida = File.CreateText(dirSalida + "\\" + nombreArchivoSalida);
                    foreach (DataRow dr in reporte.Rows)
                    {
                        try
                        {
                            streamSalida.WriteLine(EvaluarRegistroAES(dr["Registro"].ToString(), dr));
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    streamSalida.Close();

                    return respuesta = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(etiquetaLogueo + "[generarFile " + clavePlugin + ", " + fecPresentacion + "] [" + ex.Message + "] [" + ex.StackTrace + "]");
            }

            return respuesta;
        }

        private static string EvaluarRegistroAES(string registro, DataRow dr) 
        {

            if(dr["EnmascararTarjeta"].ToString().Equals("1"))
                registro = registro.Replace("@Tarjeta","* * ** " + dr["Tarjeta"].ToString().Substring(12,4));
            else
                registro = registro.Replace("@Tarjeta", dr["Tarjeta"].ToString());

            registro = registro.Replace("@MAReportar", dr["MedioAccesoAReportar"].ToString());
            registro = registro.Replace("@CuentaInterna", dr["CuentaInterna"].ToString());

            return registro;
        }

        public static void insertarReporte(string clavePlugin, string fechaPresentacion
                            , string connWriteAuto, string fechaComp, string tipoReporte)
        {
            Hashtable ht = new Hashtable();

            ht.Add("@clavePlugin", clavePlugin);
            ht.Add("@fechaPresentacion", fechaPresentacion);
            ht.Add("@fechaCompensacion", fechaComp);
            ht.Add("@cveReporte", tipoReporte);

            ExecuteSp.ejecutarSp("Procnoc_InsertarReporteAgenerar", ht, connWriteAuto);
        }

        public static void insertarReportePRED(string fechaPresentacion
                            , string connAutoWrite, string fechaComp, string tipoReporte
                            , string ma)
        {
            Hashtable ht = new Hashtable();

            ht.Add("@fechaPresentacion", fechaPresentacion);
            ht.Add("@fechaCompensacion", fechaComp);
            ht.Add("@cveReporte", tipoReporte);
            ht.Add("@ma", ma);

            ExecuteSp.ejecutarSp("Procnoc_InsertarReporteAgenerarPRED", ht, connAutoWrite);
        }

        public static void insertarReportePREDV2(string fechaPresentacion
                            , string connAutoWrite, string fechaComp, string tipoReporte
                            , string ma)
        {
            Hashtable ht = new Hashtable();

            ht.Add("@fechaPresentacion", fechaPresentacion);
            ht.Add("@fechaCompensacion", fechaComp);
            ht.Add("@cveReporte", tipoReporte);
            ht.Add("@bin", ma);

            ExecuteSp.ejecutarSp("Procnoc_InsertarReporteAgenerarPREDV2", ht, connAutoWrite);
        }
    }
}
