using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CommonProcesador;
using DNU_ParabiliaReportesAzureBlobStorage.BaseDatos;
using DNU_ParabiliaReportesAzureBlobStorage.Entidades;
using DocumentFormat.OpenXml.Spreadsheet;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Renci.SshNet;
using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Dynamic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_ParabiliaReportesAzureBlobStorage.LogicaNegocio
{
    public class LNReportes
    {
        DAOReporteFondeo _daoFondeo;
        DAOTransfronterizo _daoTransfronterizo;
        DAOReporteABU _dAOReporteABU;
        DAOReporteEstatusTarjeta _dAOReporteReporteEstatusTarjetas;
        DAOReporteAsientosContables _dAOReporteReporteAsientosContablesDetallado;
        string DirectorioSalida, connectionStringBlobStorage, containerNameBlobStorage, DirectorioProcesados;
        string Colectivas = PNConfig.Get("REPORTES_AZUREBS", "ColectivasIVAT");
        public LNReportes()
        {
            _daoFondeo = new DAOReporteFondeo();
            _daoTransfronterizo = new DAOTransfronterizo();
            _dAOReporteABU = new DAOReporteABU();
            _dAOReporteReporteEstatusTarjetas = new DAOReporteEstatusTarjeta();
            _dAOReporteReporteAsientosContablesDetallado = new DAOReporteAsientosContables();
        }

        public void CreaDirectorios()
        {
            Logueo.EventoInfo("[generacionReporteAzure] [Inicia crear directorios de salida] ");
#if DEBUG
            DirectorioSalida = "C:\\Reportes";
            DirectorioProcesados = DirectorioSalida + "\\Procesados";
#else
            DirectorioSalida = PNConfig.Get("REPORTES_AZUREBS", "DirectorioSalida");
            DirectorioProcesados = PNConfig.Get("REPORTES_AZUREBS", "DirectorioProcesados");
#endif

            if (!Directory.Exists(DirectorioSalida))
                Directory.CreateDirectory(DirectorioSalida);

            connectionStringBlobStorage = PNConfig.Get("REPORTES_AZUREBS", "ConexionAzure");
            containerNameBlobStorage = PNConfig.Get("REPORTES_AZUREBS", "ContenedorAzure");

            Logueo.EventoInfo("[generacionReporteAzure] [Termina crear directorios de salida] ");
        }

        #region REPORTESMONGE
        public bool GeneraReporteMovimientosDiario(string conn, string plugin, string nombreArchivoMovtosDiario)
        {
            DateTime fechaActual = DateTime.Now;
            DateTime fechaAnterior = fechaActual.AddDays(-1);
            string fecha = fechaAnterior.ToString("yyyyMMdd");
            string[] Colectivas = plugin.Split(',');

            foreach (string colectiva in Colectivas)
            {
                DataTable dtReporte = _daoFondeo.DBGetObtieneReporteMovimientosDiario(colectiva, fecha, conn);
                string nombreArchivo = nombreArchivoMovtosDiario + DateTime.Now.ToString("yyyyMMdd");
                string rutaArchivoOrigen = Path.Combine(DirectorioSalida, nombreArchivo + ".txt");

                bool carpetaExist = validacionCarpeta(DirectorioSalida);

                using (StreamWriter writer = new StreamWriter(rutaArchivoOrigen))
                {
                    // Establece encabezados de Reporte de Movimientos Diario 
                    string[] losEncabezados =  { "IdColectiva",
                                                "IdCuenta",
                                                "IDPoliza",
                                                "Tipo de identificación",
                                                "Número de identificación",
                                                "Nombre Completo del TH",
                                                "Número de Tarjeta",
                                                "Consecutivo de Factura",
                                                "Consecutivo de la Recarga",
                                                "Canal",
                                                "Fecha y hora de la Autorización",
                                                "Fecha y hora del movimiento",
                                                "Tipo de movimiento",
                                                "Descripción",
                                                "Moneda",
                                                "Monto de la transacción" };

                    writer.WriteLine(string.Join("|", losEncabezados));

                    foreach (DataRow row in dtReporte.Rows)
                    {
                        string datosFila = string.Join("|", row.ItemArray);
                        writer.WriteLine(datosFila);
                    }
                }

                if (!File.Exists(rutaArchivoOrigen))
                {
                    Logueo.Error("[generacionReporteAzure] [Error al Generar Reporte de Movimientos Diario ] [Mensaje: No se pudo crear el archivo: " + rutaArchivoOrigen + "]");
                    continue;
                }

                bool envioAzure = EnviarArchivoToAzureBlobStorage(rutaArchivoOrigen, nombreArchivo);

                if (envioAzure == true)
                    return EnvioCarpetaProcesados(rutaArchivoOrigen, nombreArchivo);
            }
            return false;
        }

        public bool GeneraReporteFondeo(string conn, string plugin, string nombreArchivoFondeo)
        {
            string[] Colectivas = plugin.Split(',');
            DateTime fechaActual = DateTime.Now;
            DateTime fechaAnterior = fechaActual.AddDays(-1);
            string fecha = fechaAnterior.ToString("yyyyMMdd");

            foreach (string colectiva in Colectivas)
            {
                DataTable dtReporte = _daoFondeo.DBGetObtieneReporteFondeo(conn, fecha, colectiva);
                string nombreArchivo = nombreArchivoFondeo + DateTime.Now.ToString("yyyyMMdd");
                string rutaArchivo = Path.Combine(DirectorioSalida, nombreArchivo + ".txt");

                using (StreamWriter writer = new StreamWriter(rutaArchivo))
                {
                    // Establece encabezados de Reporte de Fondeo 
                    string[] losEncabezados = { "IdColectivaPadre",
                                                "Fecha y hora",
                                                "Concepto",
                                                "IdPoliza",
                                                "Monto Ingreso",
                                                "Monto Egreso",
                                                "Tipo de identificación",
                                                "Número de identificación",
                                                "Nombre completo TH",
                                                "Id cuenta",
                                                "Número de tarjeta",
                                                "Consecutivo de factura",
                                                "Consecutivo de la recarga",
                                                "Saldo" };

                    writer.WriteLine(string.Join("|", losEncabezados));

                    foreach (DataRow row in dtReporte.Rows)
                    {
                        string datosFila = string.Join("|", row.ItemArray);
                        writer.WriteLine(datosFila);
                    }
                }

                if (!File.Exists(rutaArchivo))
                {
                    Logueo.Error("[generacionReporteAzure] [Error al Generar Reporte de Movimientos Diario ] [Mensaje: No se pudo crear el archivo: " + rutaArchivo + "]");
                    continue;
                }

                bool envioAzure = EnviarArchivoToAzureBlobStorage(rutaArchivo, nombreArchivo);

                if (envioAzure == true)
                {
                    return EnvioCarpetaProcesados(rutaArchivo, nombreArchivo);
                }
            }

            return false;
        }

        public bool GeneraReporteMovsCtaEje(string conn, string plugin, string nombreArchivoMovsCtaEje, string rutaArchivoMovsCtaEje)
        {
            string[] archivos = Directory.GetFiles(rutaArchivoMovsCtaEje + "\\", nombreArchivoMovsCtaEje + "*");

            foreach (string xItem in archivos)
            {
                if (!File.Exists(xItem))
                {
                    Logueo.Error("[EnvioArchivoAzure] [No se encontró el archivo en la ruta: ] [Mensaje: " + rutaArchivoMovsCtaEje + "]");
                    continue;
                }
                Logueo.EventoInfo("[generacionReporteAzure] [Se encuentra archivo: " + nombreArchivoMovsCtaEje + "]");
                bool envioAzure = EnviarArchivoToAzureBlobStorage(xItem, nombreArchivoMovsCtaEje);

                if (envioAzure == true)
                    return CopiarCarpetaProcesados(xItem, nombreArchivoMovsCtaEje);
            }

            return false;
        }

        public bool GeneraReporteActividadDiaria(string conn, string plugin, string nombreArchivoActividadDiaria, string rutaArchivoActividadDiaria)
        {
            string[] archivos = Directory.GetFiles(rutaArchivoActividadDiaria + "\\", nombreArchivoActividadDiaria + "*");

            foreach (string xItem in archivos)
            {
                if (!File.Exists(xItem))
                {
                    Logueo.Error("[EnvioArchivoAzure] [No se encontró el archivo en la ruta: ] [Mensaje: " + rutaArchivoActividadDiaria + "]");
                    continue;
                }
                Logueo.EventoInfo("[generacionReporteAzure] Se encuentra archivo: " + nombreArchivoActividadDiaria);
                bool envioAzure = EnviarArchivoToAzureBlobStorage(xItem, nombreArchivoActividadDiaria);

                if (envioAzure == true)
                    return CopiarCarpetaProcesados(xItem, nombreArchivoActividadDiaria);
            }

            return false;
        }

        public bool GeneraReporteNuevoLQ(string conn, string plugin, string nombreArchivoNuevoLQ, string rutaArchivoNuevoLQ)
        {
            string[] archivos = Directory.GetFiles(rutaArchivoNuevoLQ + "\\", nombreArchivoNuevoLQ + "*");
            foreach (string xItem in archivos)
            {
                if (!File.Exists(xItem))
                {
                    Logueo.Error("[EnvioArchivoAzure] [No se encontró el archivo en la ruta: ] [Mensaje: " + rutaArchivoNuevoLQ + "]");
                    continue;
                }
                Logueo.EventoInfo("[generacionReporteAzure] Se encuentra archivo: " + nombreArchivoNuevoLQ);
                bool envioAzure = EnviarArchivoToAzureBlobStorage(xItem, nombreArchivoNuevoLQ);

                if (envioAzure == true)
                    return CopiarCarpetaProcesados(xItem, nombreArchivoNuevoLQ);
            }

            return true;
        }
        
        public bool GeneraReporteActividadDiariaMonge(string conn, string plugin, string nombreArchivoDA_Monge)
        {
            DateTime fechaActual = DateTime.Now;
            DateTime fechaAnterior = fechaActual.AddDays(-1);
            string fecha = fechaAnterior.ToString("yyyyMMdd");
            string[] Colectivas = plugin.Split(',');

            foreach (string colectiva in Colectivas)
            {
                DataTable dtReporte = _daoFondeo.DBGetObtieneReporteActividadDiaria(colectiva, fecha, conn);
                string rutaArchivoOrigen = Path.Combine(DirectorioSalida, nombreArchivoDA_Monge + ".txt");

                bool carpetaExist = validacionCarpeta(DirectorioSalida);

                using (StreamWriter writer = new StreamWriter(rutaArchivoOrigen))
                {

                    string encabezados = "IdOperacion|Tipo de Registro|Número de Tarjeta|Fecha Transacción|Hora Transacción|Importe|Importe USD|Número de Comercio|Secuencia|Clave Transacción|Clave Respuesta|Número Autorización|Descripción del Comercio|Descripción del País Origen|Concepto de Movimiento|Número de Empleado";
                    writer.WriteLine(encabezados);

                    foreach (DataRow row in dtReporte.Rows)
                    {
                        string datosFila = string.Join("|", row.ItemArray);
                        writer.WriteLine(datosFila);
                    }
                }

                if (!File.Exists(rutaArchivoOrigen))
                {
                    Logueo.Error("[generacionReporteAzure] [Error al Generar Reporte de Actividad Diaria ] [Mensaje: No se pudo crear el archivo: " + rutaArchivoOrigen + "]");
                    continue;
                }

                bool envioAzure = EnviarArchivoToAzureBlobStorage(rutaArchivoOrigen, nombreArchivoDA_Monge);

                if (envioAzure == true)
                    return EnvioCarpetaProcesados(rutaArchivoOrigen, nombreArchivoDA_Monge);
            }
            return false;
        }
        #endregion

        #region REPORTES IVA TRANSFRONTERIZO

        public bool GeneraReporteTransaccionesTransfronterizo(string conn)
        {
            try
            {
                List<CamposReporte> olsCampos = ArmaCamposReporteTransaccionesIvaT();
                string Colectivas = PNConfig.Get("REPORTES_AZUREBS", "ColectivasIVAT");

                string[] colectivasArray = Colectivas.Split(',');

                foreach (string colectivaArray in colectivasArray)
                {
                    int indiceGuionBajo = colectivaArray.IndexOf('_');
                    string ID_colectiva = string.Empty, claveColectiva = string.Empty;

                    if (indiceGuionBajo > -1 && indiceGuionBajo < colectivaArray.Length - 1)
                    {
                        ID_colectiva = colectivaArray.Substring(0, indiceGuionBajo);
                        claveColectiva = colectivaArray.Substring(indiceGuionBajo + 1);
                    }

                    string fecha = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                    string SPReporte = PNConfig.Get("REPORTES_AZUREBS", "SPTransaccionesIVAT");
                    DataTable dtClaveReporte = _daoTransfronterizo.ObtenerClaveReporte(conn, ID_colectiva, SPReporte);

                    string claveReporte = string.Empty, tipoSalida = string.Empty, rutaRemoto = string.Empty;
                    string UrlSFTP = string.Empty, UsuarioSFTP = string.Empty, PasswordSFTP = string.Empty;
                    string PortSFTP = string.Empty, ejecucionActiva = string.Empty;

                    foreach (DataRow filas in dtClaveReporte.Rows)
                    {
                        string Valor = filas["Nombre"].ToString();
                        switch (Valor)
                        {
                            case "@NombreArchivo":
                                claveReporte = filas["valor"].ToString();
                                break;
                            case "@ReportesTipoSalida":
                                tipoSalida = filas["valor"].ToString();
                                break;
                            case "@ReportesRutadeSalida":
                                rutaRemoto = filas["valor"].ToString();
                                break;
                            case "@ReportesUrlSFTP":
                                UrlSFTP = filas["valor"].ToString();
                                break;
                            case "@ReportesUserSFTP":
                                UsuarioSFTP = filas["valor"].ToString();
                                break;
                            case "@ReportesPasswordSFTP":
                                PasswordSFTP = filas["valor"].ToString();
                                break;
                            case "@ReportesPortSFTP":
                                PortSFTP = filas["valor"].ToString();
                                break;
                            case "@EjecucionActiva":
                                ejecucionActiva = filas["valor"].ToString();
                                break;
                        }
                    }

                    if (ejecucionActiva == "0" || string.IsNullOrEmpty(UrlSFTP)
                         || string.IsNullOrEmpty(UsuarioSFTP) || string.IsNullOrEmpty(PasswordSFTP)
                         || string.IsNullOrEmpty(PortSFTP) || string.IsNullOrEmpty(ejecucionActiva))
                    {
                        Logueo.Evento("[GeneraReporteTransfronterizo] No se genera por configuración apagada, colectiva: " + claveColectiva);
                        continue;
                    }

                    string nombreArchivo = claveReporte.TrimEnd('_') + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

                    string rutaArchivo = Path.Combine(DirectorioSalida, nombreArchivo);

                    DataTable dtReporte = _daoTransfronterizo.ObtieneReporteTransaccionesIvaTransfronterizo(conn, SPReporte, fecha, ID_colectiva);

                    if (dtReporte != null)
                    {
                        using (StreamWriter writer = new StreamWriter(rutaArchivo))
                        {
                            for (int i = 0; i < dtReporte.Rows.Count; i++)
                            {
                                string Fila = string.Empty;
                                for (int k = 0; k < dtReporte.Columns.Count; k++)
                                {
                                    CamposReporte campo = olsCampos.Where(x => x.nombre == dtReporte.Columns[k].ColumnName).FirstOrDefault();
                                    if (campo != null)
                                    {
                                        if (campo.nombre == "MontoTransaccion" || campo.nombre == "MontoIVA")
                                        {
                                            string Valor = Truncar(decimal.Parse(dtReporte.Rows[i][k].ToString()), 2);

                                            if (campo.Alineado == "Izq")
                                                Fila += Valor.PadRight(campo.longitud, ' ');
                                            else
                                                Fila += Valor.PadLeft(campo.longitud, ' ');
                                        }
                                        else if (dtReporte.Rows[i][k].ToString().Length <= campo.longitud)
                                        {
                                            if (campo.Alineado == "Izq")
                                                Fila += dtReporte.Rows[i][k].ToString().PadRight(campo.longitud, ' ');
                                            else
                                                Fila += dtReporte.Rows[i][k].ToString().PadLeft(campo.longitud, ' ');
                                        }
                                        else
                                        {
                                            Fila += dtReporte.Rows[i][k].ToString().Substring(0, campo.longitud);
                                        }
                                    }
                                }

                                writer.WriteLine(Fila); //Escribe resultado de SP
                            }
                        }
                    }

                    //SFTP
                    if (tipoSalida == "SFTP")
                    {
                        try
                        {
                            if (File.Exists(rutaArchivo))
                            {
                                CreateFileAndDirectoryWithConnection(UrlSFTP,
                                                                    PortSFTP,
                                                                    UsuarioSFTP,
                                                                    PasswordSFTP,
                                                                    rutaArchivo, //Ubicacion fisica del archivo,
                                                                    rutaRemoto); //La ubicacion en el sftp

                                Logueo.Evento("[GeneraReporteTransfronterizo] Reporte enviado con éxito a SFTP: " + nombreArchivo);

                                // MOVER ARCHIVO A CARPETA DE PROCESADOS
                                string rutaArchivoNueva = rutaArchivo;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logueo.Error("[GeneraReporteTransfronterizo] error al enviar archivo SFTP:" + ex.Message + " " + ex.StackTrace + " Archivo: " + nombreArchivo);

                        }
                    }
                    else if (tipoSalida == "FTP")
                    {
                        try
                        {
                            if (File.Exists(rutaArchivo))
                            {
                                File.Copy(rutaArchivo, rutaRemoto + "/" + nombreArchivo);
                                Logueo.Evento("[GeneraReporteTransfronterizo] Reporte enviado con éxito a FTP: " + rutaRemoto + "/" + nombreArchivo);

                                string rutaArchivoNueva = rutaArchivo;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logueo.Error("[GeneraReporteTransfronterizo] error al enviar archivo FTP:" + ex.Message + " " + ex.StackTrace + " Archivo: " + nombreArchivo);
                        }
                    }

                    if (!File.Exists(rutaArchivo))
                    {
                        Logueo.Error("[GeneraReporteTransfronterizo] [Error al Generar Reporte Transfronterizo] [Mensaje: No se pudo crear el archivo: " + rutaArchivo + "]");
                        continue;
                    }

                    if (File.Exists(rutaArchivo))
                        File.Delete(rutaArchivo);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool GeneraReporteAcumuladoTransfronterizo(string conn)
        {
            try
            {
                string Colectivas = PNConfig.Get("REPORTES_AZUREBS", "ColectivasIVAT");
                string[] colectivasArray = Colectivas.Split(',');

                foreach (string colectivaArray in colectivasArray)
                {
                    int indiceGuionBajo = colectivaArray.IndexOf('_');
                    string ID_colectiva = string.Empty, claveColectiva = string.Empty;

                    if (indiceGuionBajo > -1 && indiceGuionBajo < colectivaArray.Length - 1)
                    {
                        ID_colectiva = colectivaArray.Substring(0, indiceGuionBajo);
                        claveColectiva = colectivaArray.Substring(indiceGuionBajo + 1);
                    }

                    string fecha = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                    string SPReporte = PNConfig.Get("REPORTES_AZUREBS", "SPAcumuladoIVAT");
                    DataTable dtClaveReporte = _daoTransfronterizo.ObtenerClaveReporte(conn, ID_colectiva, SPReporte);

                    string claveReporte = string.Empty, tipoSalida = string.Empty, rutaRemoto = string.Empty;
                    string UrlSFTP = string.Empty, UsuarioSFTP = string.Empty, PasswordSFTP = string.Empty;
                    string PortSFTP = string.Empty, ejecucionActiva = string.Empty;

                    foreach (DataRow filas in dtClaveReporte.Rows)
                    {
                        string Valor = filas["Nombre"].ToString();
                        switch (Valor)
                        {
                            case "@NombreArchivo":
                                claveReporte = filas["valor"].ToString();
                                break;
                            case "@ReportesTipoSalida":
                                tipoSalida = filas["valor"].ToString();
                                break;
                            case "@ReportesRutadeSalida":
                                rutaRemoto = filas["valor"].ToString();
                                break;
                            case "@ReportesUrlSFTP":
                                UrlSFTP = filas["valor"].ToString();
                                break;
                            case "@ReportesUserSFTP":
                                UsuarioSFTP = filas["valor"].ToString();
                                break;
                            case "@ReportesPasswordSFTP":
                                PasswordSFTP = filas["valor"].ToString();
                                break;
                            case "@ReportesPortSFTP":
                                PortSFTP = filas["valor"].ToString();
                                break;
                            case "@EjecucionActiva":
                                ejecucionActiva = filas["valor"].ToString();
                                break;
                        }
                    }

                    if (ejecucionActiva == "0" || string.IsNullOrEmpty(UrlSFTP)
                         || string.IsNullOrEmpty(UsuarioSFTP) || string.IsNullOrEmpty(PasswordSFTP)
                         || string.IsNullOrEmpty(PortSFTP) || string.IsNullOrEmpty(ejecucionActiva))
                    {
                        Logueo.Evento("[GeneraReporteTransfronterizo] No se genera por configuración apagada, colectiva: " + claveColectiva);
                        continue;
                    }

                    string nombreArchivo = claveReporte.TrimEnd('_') + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";

                    string rutaArchivo = Path.Combine(DirectorioSalida, nombreArchivo);

                    DataTable dtReporte = _daoTransfronterizo.ObtieneReporteTransaccionesIvaTransfronterizo(conn, SPReporte, fecha, ID_colectiva);

                    if (dtReporte != null)
                    {
                        using (StreamWriter writer = new StreamWriter(rutaArchivo))
                        {
                            foreach (DataRow row in dtReporte.Rows)
                            {
                                string datosFila = string.Join("", row.ItemArray);
                                writer.WriteLine(datosFila); //Escribe resultado de SP
                            }
                        }
                    }

                    //SFTP
                    if (tipoSalida == "SFTP")
                    {
                        try
                        {
                            if (File.Exists(rutaArchivo))
                            {
                                CreateFileAndDirectoryWithConnection(UrlSFTP,
                                                                    PortSFTP,
                                                                    UsuarioSFTP,
                                                                    PasswordSFTP,
                                                                    rutaArchivo, //Ubicacion fisica del archivo,
                                                                    rutaRemoto); //La ubicacion en el sftp

                                Logueo.Evento("[GeneraReporteTransfronterizo] Reporte enviado con éxito a SFTP: " + nombreArchivo);

                                // MOVER ARCHIVO A CARPETA DE PROCESADOS
                                string rutaArchivoNueva = rutaArchivo;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logueo.Error("[GeneraReporteTransfronterizo] error al enviar archivo SFTP:" + ex.Message + " " + ex.StackTrace + " Archivo: " + nombreArchivo);

                        }
                    }
                    else if (tipoSalida == "FTP")
                    {
                        try
                        {
                            if (File.Exists(rutaArchivo))
                            {
                                File.Copy(rutaArchivo, rutaRemoto + "/" + nombreArchivo);
                                Logueo.Evento("[GeneraReporteTransfronterizo] Reporte enviado con éxito a FTP: " + rutaRemoto + "/" + nombreArchivo);

                                string rutaArchivoNueva = rutaArchivo;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logueo.Error("[GeneraReporteTransfronterizo] error al enviar archivo FTP:" + ex.Message + " " + ex.StackTrace + " Archivo: " + nombreArchivo);
                        }
                    }

                    if (!File.Exists(rutaArchivo))
                    {
                        Logueo.Error("[GeneraReporteTransfronterizo] [Error al Generar Reporte Transfronterizo] [Mensaje: No se pudo crear el archivo: " + rutaArchivo + "]");
                        continue;
                    }

                    if (File.Exists(rutaArchivo))
                        File.Delete(rutaArchivo);

                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool GeneraReporteTransfronterizo(string conn)
        {
            #region Calcular Fechar reporte y ejecución

            List<CamposReporte> olsCampos = ArmaCamposReporteEcomTrx();
            DateTime fechaActual = DateTime.Now;
            DateTime FechaCT1Inicio = new DateTime(fechaActual.Year, 05, 01, 00, 00, 00);//Fecha Cuatrimestre 1
            DateTime FechaCT2Inicio = new DateTime(fechaActual.Year, 09, 01, 00, 00, 00);//Fecha Cuatrimestre 2
            DateTime FechaCT3Inicio = new DateTime(fechaActual.Year, 01, 01, 00, 00, 00);//Fecha Cuatrimestre 3

            string Cuatrimestre = string.Empty;
            string fechaInicio = string.Empty;
            string fechaFinal = string.Empty;
            DateTime fechaTemp = DateTime.Now;
            if (fechaActual.Month == FechaCT1Inicio.Month && fechaActual.Day == FechaCT1Inicio.Day)
            {
                fechaInicio = $"{fechaActual.Year}0101";
                fechaTemp = new DateTime(fechaActual.Year, 05, 01).AddDays(-1);//Calcular ultimo dia del mes para meses que tienen 30 o 31 días
                fechaFinal = $"{fechaTemp.Year}{(fechaTemp.Month).ToString().PadLeft(2, '0')}{fechaTemp.Day}";//Verificar formato de 8 digitos
                Cuatrimestre = "1";
            }
            if (fechaActual.Month == FechaCT2Inicio.Month && fechaActual.Day == FechaCT2Inicio.Day)
            {
                fechaInicio = $"{fechaActual.Year}0501";
                fechaTemp = new DateTime(fechaActual.Year, 09, 01).AddDays(-1);
                fechaFinal = $"{fechaTemp.Year}{(fechaTemp.Month).ToString().PadLeft(2, '0')}{fechaTemp.Day}";
                Cuatrimestre = "2";
            }
#if DEBUG
            else if (true)
#else
            if (fechaActual.Year == FechaCT3Inicio.Year && fechaActual.Month == FechaCT3Inicio.Month
                && fechaActual.Day == FechaCT3Inicio.Day)
#endif
            {
#if DEBUG
                fechaInicio = $"{fechaActual.Year}0901"; // Restar el año nuevo
                fechaTemp = new DateTime(fechaActual.Year, 12, 31);
                fechaFinal = $"{fechaTemp.Year}{fechaTemp.Month}{fechaTemp.Day}";
                Cuatrimestre = "3";
#else
                fechaInicio = $"{fechaActual.Year - 1}0901"; // Restar el año nuevo
                fechaTemp = new DateTime(fechaActual.Year, 01, 01).AddDays(-1);
                fechaFinal = $"{fechaTemp.Year}{fechaTemp.Month}{fechaTemp.Day}";
                Cuatrimestre = "3";
#endif
            }

            #endregion Calcular Fechar reporte y ejecución

            #region Generar y escribir reporte 
            string Colectivas = PNConfig.Get("REPORTES_AZUREBS", "ColectivasIVAT");

            string[] colectivasArray = Colectivas.Split(',');

            foreach (string colectivaArray in colectivasArray)
            {
                int indiceGuionBajo = colectivaArray.IndexOf('_');
                string ID_colectiva = string.Empty, claveColectiva = string.Empty;

                if (indiceGuionBajo > -1 && indiceGuionBajo < colectivaArray.Length - 1)
                {
                    ID_colectiva = colectivaArray.Substring(0, indiceGuionBajo);
                    claveColectiva = colectivaArray.Substring(indiceGuionBajo + 1);
                }

                DataTable dtClaveReporte = _daoTransfronterizo.ObtenerClaveReporte(conn, ID_colectiva, PNConfig.Get("REPORTES_AZUREBS", "SPReporteIVAT"));

                string claveReporte = string.Empty, tipoSalida = string.Empty, rutaRemoto = string.Empty;
                string UrlSFTP = string.Empty, UsuarioSFTP = string.Empty, PasswordSFTP = string.Empty;
                string PortSFTP = string.Empty, ejecucionActiva = string.Empty;

                foreach (DataRow filas in dtClaveReporte.Rows)
                {
                    string Valor = filas["Nombre"].ToString();
                    switch (Valor)
                    {
                        case "@NombreArchivo":
                            claveReporte = filas["valor"].ToString();
                            break;
                        case "@ReportesTipoSalida":
                            tipoSalida = filas["valor"].ToString();
                            break;
                        case "@ReportesRutadeSalida":
                            rutaRemoto = filas["valor"].ToString();
                            break;
                        case "@ReportesUrlSFTP":
                            UrlSFTP = filas["valor"].ToString();
                            break;
                        case "@ReportesUserSFTP":
                            UsuarioSFTP = filas["valor"].ToString();
                            break;
                        case "@ReportesPasswordSFTP":
                            PasswordSFTP = filas["valor"].ToString();
                            break;
                        case "@ReportesPortSFTP":
                            PortSFTP = filas["valor"].ToString();
                            break;
                        case "@EjecucionActiva":
                            ejecucionActiva = filas["valor"].ToString();
                            break;
                    }
                }

                if (ejecucionActiva == "0" || string.IsNullOrEmpty(UrlSFTP)
                     || string.IsNullOrEmpty(UsuarioSFTP) || string.IsNullOrEmpty(PasswordSFTP)
                     || string.IsNullOrEmpty(PortSFTP) || string.IsNullOrEmpty(ejecucionActiva))
                {
                    Logueo.Evento("[GeneraReporteTransfronterizo] No se genera por configuración apagada, colectiva: " + claveColectiva);
                    continue;
                }

                string nombreArchivo = claveReporte.TrimEnd('_') + "_" + claveColectiva + "_" + Cuatrimestre + "_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";

                string rutaArchivo = Path.Combine(DirectorioSalida, nombreArchivo);
                var periodo = Cuatrimestre.PadLeft(2, '0') + (Cuatrimestre == "3" ? DateTime.Now.Year - 1 : DateTime.Now.Year);

                DataTable dtReporte = _daoTransfronterizo.ObtienerReporteTransfronterizoTotalidadTransacciones(conn, fechaInicio, fechaFinal, ID_colectiva);

                if (dtReporte != null)
                {
                    foreach (DataRow row in dtReporte.Rows)
                    {
                        row["Periodo reportado"] = periodo.ToString();
                    }

                    SLDocument documento = new SLDocument();
                    documento.ImportDataTable(1, 1, dtReporte, true);
                    documento.SaveAs(rutaArchivo);

                    #region COMENTADO
                    //using (StreamWriter writer = new StreamWriter(rutaArchivo))
                    //{
                    //    for (int i = 0; i < dtReporte.Rows.Count; i++)
                    //    {
                    //        string Fila = string.Empty;
                    //        for (int k = 0; k < dtReporte.Columns.Count; k++)
                    //        {
                    //            CamposReporte campo = olsCampos.Where(x => x.nombre == dtReporte.Columns[k].ColumnName).FirstOrDefault();
                    //            if (campo != null)
                    //            {
                    //                if (campo.nombre == "MontoTransaccion" || campo.nombre == "MontoIVA")
                    //                {
                    //                    string Valor = string.Empty;
                    //                    if (IsNumeric(dtReporte.Rows[i][k].ToString()))
                    //                        Valor = Truncar(decimal.Parse(dtReporte.Rows[i][k].ToString()), 2);
                    //                    else
                    //                        Valor = dtReporte.Rows[i][k].ToString();

                    //                    Fila += Valor;
                    //                }
                    //                else if (campo.nombre == "Periodo")
                    //                {
                    //                    if (i == 0)
                    //                    {
                    //                        Fila += dtReporte.Rows[i][k].ToString();
                    //                    }
                    //                    else
                    //                    {
                    //                        var periodo = Cuatrimestre.PadLeft(2, '0') + (Cuatrimestre == "3" ? DateTime.Now.Year - 1 : DateTime.Now.Year);
                    //                        Fila += periodo.ToString();
                    //                    }
                    //                }
                    //                else if (campo.nombre == "MesTransaccion")
                    //                {
                    //                    Fila += dtReporte.Rows[i][k].ToString();
                    //                }
                    //                else
                    //                {
                    //                    Fila += dtReporte.Rows[i][k].ToString();
                    //                }
                    //            }
                    //            else
                    //                Fila += dtReporte.Rows[i][k].ToString();
                    //        }

                    //        writer.WriteLine(Fila); //Escribe resultado de SP
                    //    }
                    //}
                    #endregion
                }

                //SFTP
                if (tipoSalida == "SFTP")
                {
                    try
                    {
                        if (File.Exists(rutaArchivo))
                        {
                            CreateFileAndDirectoryWithConnection(UrlSFTP,
                                                                PortSFTP,
                                                                UsuarioSFTP,
                                                                PasswordSFTP,
                                                                rutaArchivo, //Ubicacion fisica del archivo,
                                                                rutaRemoto); //La ubicacion en el sftp

                            Logueo.Evento("[GeneraReporteTransfronterizo] Reporte enviado con éxito a SFTP: " + nombreArchivo);

                            // MOVER ARCHIVO A CARPETA DE PROCESADOS
                            string rutaArchivoNueva = rutaArchivo;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[GeneraReporteTransfronterizo] error al enviar archivo SFTP:" + ex.Message + " " + ex.StackTrace + " Archivo: " + nombreArchivo);

                    }
                }
                else if (tipoSalida == "FTP")
                {
                    try
                    {
                        if (File.Exists(rutaArchivo))
                        {
                            File.Copy(rutaArchivo, rutaRemoto + "/" + nombreArchivo);

                            Logueo.Evento("[GeneraReporteTransfronterizo] Reporte enviado con éxito a FTP: " + rutaRemoto + "/" + nombreArchivo);

                            string rutaArchivoNueva = rutaArchivo;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[GeneraReporteTransfronterizo] error al enviar archivo FTP:" + ex.Message + " " + ex.StackTrace + " Archivo: " + nombreArchivo);
                    }
                }

                if (!File.Exists(rutaArchivo))
                {
                    Logueo.Error("[GeneraReporteTransfronterizo] [Error al Generar Reporte Transfronterizo] [Mensaje: No se pudo crear el archivo: " + rutaArchivo + "]");
                    continue;
                }

                if (File.Exists(rutaArchivo))
                    File.Delete(rutaArchivo);

            }

            #endregion Generar y escribir reporte 

            // Una ejecución Cuatrimestral solo en el horario indicado
            // Dormir para no regenerar en el mismo horario
            //Rango definico en las variables de fecha y hora (Actual 5 min)
            Thread.Sleep(60 * 6 * 1000);
            return true;
        }

        #endregion


        public static bool CreateFileAndDirectoryWithConnection(string host, string port, string username, string password, string absoluteFilePath, string WorkingDirectory)
        {
            bool created = false;
            // FileInfo fi = new FileInfo(absoluteFilePath);
            try
            {
                using (SftpClient sftp = new SftpClient(host, int.Parse(port), username, password))
                {

                    sftp.Connect();
                    Logueo.Evento("Se conectó al FTP");
                    sftp.ChangeDirectory(WorkingDirectory);
                    Logueo.Evento("Se especificó el directorio del SFTP: " + WorkingDirectory);

                    //created = CreateDirectoriesRecursively(Path.GetFullPath(absoluteFilePath), sftp, separatorDirectory);
                    using (var fileStream = new FileStream(absoluteFilePath, FileMode.Open))
                    {
                        Logueo.Evento("Se subirá a SFTP: " + WorkingDirectory + "/" + Path.GetFileName(absoluteFilePath));

                        sftp.BufferSize = 4 * 1024; // bypass Payload error large files
                        sftp.UploadFile(fileStream, WorkingDirectory + "/" + Path.GetFileName(absoluteFilePath));//Path.GetFileName(absoluteFilePath));
                    }

                    sftp.Disconnect();
                    Logueo.Evento("Se subio al SFTP con éxito");

                }
            }
            catch (Exception e)
            {
                created = false;
                Logueo.Error("Error SFTP: " + e.StackTrace);
                throw new Exception(e.Message, e);

            }
            return created;

        }

        public bool GeneraReporteABU(string conn)
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
            {
                DataTable IcasTabla = _dAOReporteABU.ObtieneIcasReporteABU(conn);
            
                foreach (DataRow row in IcasTabla.Rows)
                {
                    string NombreArchivoSFTP = PNConfig.Get("REPORTES_AZUREBS", "NombreABU");

                    string UrlSFTP = PNConfig.Get("REPORTES_AZUREBS", "ftp_host");
                    string UsuarioSFTP = PNConfig.Get("REPORTES_AZUREBS", "ftp_user");
                    string PasswordSFTP = PNConfig.Get("REPORTES_AZUREBS", "ftp_pwd");
                    string PortSFTP = PNConfig.Get("REPORTES_AZUREBS", "ftp_port");
                    string rutaRemoto = PNConfig.Get("REPORTES_AZUREBS", "ftp_ruta");

                    string nombreArchivo = NombreArchivoSFTP + "_" + row["ICA"] + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                    string rutaArchivo = Path.Combine(DirectorioSalida, nombreArchivo);

                    using (StreamWriter writer = new StreamWriter(rutaArchivo))
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            DataTable dtReporte = _dAOReporteABU.ObtieneDatosReporteABU(conn, row["ICA"].ToString(), i);

                            if (dtReporte != null)
                            {
                                foreach (DataRow data in dtReporte.Rows)
                                {
                                    string datosFila = string.Join("", data.ItemArray);
                                    writer.WriteLine(datosFila);//Escribe resultado de SP
                                }
                            }
                        }
                    }

                    if (!File.Exists(rutaArchivo))
                    {
                        Logueo.Error("[GeneraReporteABU] [Error al Generar Reporte ABU ] [Mensaje: No se pudo crear el archivo: " + rutaArchivo + "]");
                        continue;
                    }

                    try
                    {
                        if (File.Exists(rutaArchivo))
                        {
                            CreateFileAndDirectoryWithConnection(UrlSFTP,
                                                                PortSFTP,
                                                                UsuarioSFTP,
                                                                PasswordSFTP,
                                                                rutaArchivo, //Ubicacion fisica del archivo,
                                                                rutaRemoto); //La ubicacion en el sftp

                            Logueo.Evento("[GeneraReporteABU] Reporte enviado con éxito a SFTP: " + nombreArchivo);

                            // MOVER ARCHIVO A CARPETA DE PROCESADOS
                            string Ubicacion_ArchivoNueva = DirectorioProcesados;

                            if (!Directory.Exists(Ubicacion_ArchivoNueva))
                                Directory.CreateDirectory(Ubicacion_ArchivoNueva);

                            File.Move(rutaArchivo, DirectorioProcesados + "\\" + nombreArchivo);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[GeneraReporteABU] error al enviar archivo SFTP:" + ex.Message + " " + ex.StackTrace + " Archivo: " + nombreArchivo);
                    }
                }
            }
            return true;
        }

        public bool GeneraReporteEstatusTarjetas(string conn)
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Thursday)
            {
                DataTable dtColectivas = _dAOReporteReporteEstatusTarjetas.ObtieneColectivasReporteEstatusTarjetas(conn);

                foreach (DataRow item in dtColectivas.Rows)
                {
                    string nombreArchivo = string.Empty;
                    try
                    {
                        string ID_Colectiva = item["ID_Colectiva"].ToString();
                        string ID_TipoCuenta = item["ID_TipoCuenta"].ToString();
                        string ClaveColectiva = item["ClaveColectiva"].ToString();

                        #region ConsultaSFTP

                        string SPReporte = PNConfig.Get("REPORTES_AZUREBS", "SPReporteEstadoTarjetas");
                        DataTable dtClaveReporte = _dAOReporteReporteEstatusTarjetas.ObtenerClaveReporte(conn, ID_Colectiva, SPReporte);

                        string claveReporte = string.Empty, tipoSalida = string.Empty, rutaRemoto = string.Empty;
                        string UrlSFTP = string.Empty, UsuarioSFTP = string.Empty, PasswordSFTP = string.Empty;
                        string PortSFTP = string.Empty, ejecucionActiva = string.Empty;

                        foreach (DataRow filas in dtClaveReporte.Rows)
                        {
                            string Valor = filas["Nombre"].ToString();
                            switch (Valor)
                            {
                                case "@NombreArchivo":
                                    claveReporte = filas["valor"].ToString();
                                    break;
                                case "@ReportesTipoSalida":
                                    tipoSalida = filas["valor"].ToString();
                                    break;
                                case "@ReportesRutadeSalida":
                                    rutaRemoto = filas["valor"].ToString();
                                    break;
                                case "@ReportesUrlSFTP":
                                    UrlSFTP = filas["valor"].ToString();
                                    break;
                                case "@ReportesUserSFTP":
                                    UsuarioSFTP = filas["valor"].ToString();
                                    break;
                                case "@ReportesPasswordSFTP":
                                    PasswordSFTP = filas["valor"].ToString();
                                    break;
                                case "@ReportesPortSFTP":
                                    PortSFTP = filas["valor"].ToString();
                                    break;
                                case "@EjecucionActiva":
                                    ejecucionActiva = filas["valor"].ToString();
                                    break;
                            }
                        }

                        if (ejecucionActiva == "0" || string.IsNullOrEmpty(UrlSFTP)
                             || string.IsNullOrEmpty(UsuarioSFTP) || string.IsNullOrEmpty(PasswordSFTP)
                             || string.IsNullOrEmpty(PortSFTP) || string.IsNullOrEmpty(ejecucionActiva))
                        {
                            Logueo.Evento("[GeneraReporteEstatusTarjeta] No se genera por configuración apagada, colectiva: " + ID_Colectiva);
                            continue;
                        }

                        #endregion ConsultaSFTP

                        nombreArchivo = claveReporte + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                        string rutaArchivo = Path.Combine(DirectorioSalida, nombreArchivo);

                        using (StreamWriter writer = new StreamWriter(rutaArchivo))
                        {
                            DataTable dtReporte = _dAOReporteReporteEstatusTarjetas.ObtieneDatosReporteEstatusTarjetas(conn, ID_Colectiva, ID_TipoCuenta);

                            if (dtReporte != null)
                            {
                                int header = 1;
                                foreach (DataRow row in dtReporte.Rows)
                                {
                                    if (header <= 2)
                                    {
                                        writer.WriteLine(row["Manufactura"]);
                                        header++;
                                        continue;
                                    }
                                    string datosFila = string.Join("|", row.ItemArray);
                                    string[] arData = datosFila.Split('|');

                                    writer.WriteLine(arData[0].PadRight(51) + arData[1].PadRight(51) + arData[2].PadRight(51) + arData[3].PadRight(51) + arData[4]);//Escribe resultado de SP
                                }
                            }
                        }

                        if (!File.Exists(rutaArchivo))
                        {
                            Logueo.Error("[GeneraReporteEstatusTarjetas] [Error al Generar Reporte EstatusTarjetas] [Mensaje: No se pudo crear el archivo: " + rutaArchivo + "]");
                            continue;
                        }

                        string archivoencriptado = rutaArchivo + ".pgp";
                        encryptFile(rutaArchivo, archivoencriptado);
                        File.Delete(rutaArchivo);

                        if (File.Exists(archivoencriptado))
                        {
                            CreateFileAndDirectoryWithConnection(UrlSFTP,
                                                               PortSFTP,
                                                               UsuarioSFTP,
                                                               PasswordSFTP,
                                                               archivoencriptado, //Ubicacion fisica del archivo,
                                                               rutaRemoto); //La ubicacion en el sftp

                            Logueo.Evento("[GeneraReporteEstatusTarjetas] Reporte enviado con éxito a SFTP: " + nombreArchivo);

                            // MOVER ARCHIVO A CARPETA DE PROCESADOS
                            if (!Directory.Exists(DirectorioProcesados))
                                Directory.CreateDirectory(DirectorioProcesados);

                            File.Move(archivoencriptado, DirectorioProcesados + "\\" + nombreArchivo + ".pgp");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error($"[GeneraReporteEstatusTarjetas] error al enviar archivo: {nombreArchivo}" + ex.Message + " " + ex.StackTrace);
                    }
                }
            }

            return true;
        }

        public bool GeneraReporteAsientosContablesDetallado(string conn)
        {
            DataTable dtColectivas = _dAOReporteReporteAsientosContablesDetallado.ObtieneColectivasReporteAsientosContables(conn);

            foreach (DataRow item in dtColectivas.Rows)
            {
                string nombreArchivo = string.Empty;
                string rutaArchivo = string.Empty;
                try
                {
                    string ID_Colectiva = item["ID_Colectiva"].ToString();

                    #region ConsultaSFTP

                    string SPReporte = PNConfig.Get("REPORTES_AZUREBS", "SPReporteAsientosContables_Detallado");
                    DataTable dtClaveReporte = _dAOReporteReporteAsientosContablesDetallado.ObtenerClaveReporte(conn, ID_Colectiva, SPReporte);

                    string prefijoReporte = string.Empty, tipoSalida = string.Empty, rutaRemoto = string.Empty;
                    string UrlSFTP = string.Empty, UsuarioSFTP = string.Empty, PasswordSFTP = string.Empty;
                    string PortSFTP = string.Empty, ejecucionActiva = string.Empty;

                    foreach (DataRow filas in dtClaveReporte.Rows)
                    {
                        string Valor = filas["Nombre"].ToString();
                        switch (Valor)
                        {
                            case "@NombreArchivo":
                                prefijoReporte = filas["valor"].ToString();
                                break;
                            case "@ReportesTipoSalida":
                                tipoSalida = filas["valor"].ToString();
                                break;
                            case "@ReportesRutadeSalida":
                                rutaRemoto = filas["valor"].ToString();
                                break;
                            case "@ReportesUrlSFTP":
                                UrlSFTP = filas["valor"].ToString();
                                break;
                            case "@ReportesUserSFTP":
                                UsuarioSFTP = filas["valor"].ToString();
                                break;
                            case "@ReportesPasswordSFTP":
                                PasswordSFTP = filas["valor"].ToString();
                                break;
                            case "@ReportesPortSFTP":
                                PortSFTP = filas["valor"].ToString();
                                break;
                            case "@EjecucionActiva":
                                ejecucionActiva = filas["valor"].ToString();
                                break;
                        }
                    }

                    if (ejecucionActiva == "0" || string.IsNullOrEmpty(UrlSFTP)
                         || string.IsNullOrEmpty(UsuarioSFTP) || string.IsNullOrEmpty(PasswordSFTP)
                         || string.IsNullOrEmpty(PortSFTP) || string.IsNullOrEmpty(ejecucionActiva))
                    {
                        Logueo.Evento("[GeneraReporteAsientosContables] No se genera por configuración apagada, colectiva: " + ID_Colectiva);
                        continue;
                    }

                    #endregion ConsultaSFTP

                    nombreArchivo = prefijoReporte + DateTime.Now.ToString("ddMMyyyy") + ".txt";
                    rutaArchivo = Path.Combine(DirectorioSalida, nombreArchivo);

                    using (StreamWriter writer = new StreamWriter(rutaArchivo))
                    {
                        DataTable dtReporte = _dAOReporteReporteAsientosContablesDetallado.ObtieneDatosReporteAsientosContablesDetalle(conn, ID_Colectiva);

                        if (dtReporte != null)
                        {
                            string datosheader = String.Empty;
                            foreach (DataColumn columna in dtReporte.Columns)
                            {
                                datosheader += columna.ColumnName + "|";
                            }
                            writer.WriteLine(datosheader.TrimEnd('|'));
                            foreach (DataRow row in dtReporte.Rows)
                            {
                                string datosFila = string.Join("|", row.ItemArray);
                                writer.WriteLine(datosFila);//Escribe resultado de SP
                            }
                        }
                    }

                    if (!File.Exists(rutaArchivo))
                    {
                        Logueo.Error("[GeneraReporteAsientosContables] [Error al Generar Reporte AsientosContables] [Mensaje: No se pudo crear el archivo: " + rutaArchivo + "]");
                        continue;
                    }

                    string archivoencriptado = rutaArchivo + ".pgp";
                    string pathPublicKey = PNConfig.Get("REPORTES_AZUREBS", "SPReporteAsientosContables_Key");
                    encryptFile(rutaArchivo, archivoencriptado, pathPublicKey);
                    File.Delete(rutaArchivo);

                    if (File.Exists(archivoencriptado))
                    {
                        CreateFileAndDirectoryWithConnection(UrlSFTP,
                                                           PortSFTP,
                                                           UsuarioSFTP,
                                                           PasswordSFTP,
                                                           archivoencriptado, //Ubicacion fisica del archivo,
                                                           rutaRemoto); //La ubicacion en el sftp

                        Logueo.Evento("[GeneraReporteAsientosContables] Reporte enviado con éxito a SFTP: " + nombreArchivo);

                        // MOVER ARCHIVO A CARPETA DE PROCESADOS
                        if (!Directory.Exists(DirectorioProcesados))
                            Directory.CreateDirectory(DirectorioProcesados);

                        File.Move(archivoencriptado, DirectorioProcesados + "\\" + nombreArchivo + ".pgp");
                    }
                }
                catch (Exception ex)
                {
                    if (File.Exists(rutaArchivo))
                        File.Delete(rutaArchivo);
                    Logueo.Error($"[GeneraReporteAsientosContables] error al enviar archivo: {nombreArchivo}" + ex.Message + " " + ex.StackTrace);
                }
            }


            return true;
        }

        private static void encryptFile(string inputFile, string outPutFile, string pathPublicKey = null)
        {
            try
            {
                EncryptionService encrypService = new EncryptionService();

                pathPublicKey = String.IsNullOrEmpty(pathPublicKey) ? PNConfig.Get("REPORTES_AZUREBS", "pathPublicKey") : pathPublicKey;

                PgpPublicKeyRing pubKeyRing = encrypService.asciiPublicKeyToRing(pathPublicKey);

                PgpPublicKey publicKey = encrypService.getFirstPublicEncryptionKeyFromRing(pubKeyRing);

                EncryptionService.EncryptFile(inputFile, outPutFile, publicKey, false, false);
            }
            catch (Exception ex)
            {
                throw new Exception("[Ocurrio error al encriptar File] [" + ex.Message + "] [" + ex.StackTrace + "]");
            }
        }

        public bool validacionCarpeta(string Directorio)
        {
            try
            {
                if (!Directory.Exists(Directorio))
                {
                    Logueo.EventoInfo("[generacionReporteAzure] [Se crea carpeta " + Directorio + "]");
                    Directory.CreateDirectory(Directorio);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logueo.Error("[generacionReporteAzure] [Error al crear carpeta de procesados: " + ex.Message + "]");
                throw ex;
            }
        }

        public bool EnviarArchivoToAzureBlobStorage(string rutaArchivo, string nombreArchivo)
        {
            try
            {
                Logueo.EventoInfo("[generacionReporteAzure] [Inicia envío de archivo a Azure Blob Storage.]");
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionStringBlobStorage);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerNameBlobStorage);

                string extencion = $"txt";
                string blobName = $"{nombreArchivo}.{extencion}";

                var listBlob = containerClient.GetBlobs();
                int intentos = 0;
                int limite = 6;

                foreach (var itemBlob in listBlob)
                {
                    if (intentos == 0 && itemBlob.Name.Equals(blobName))
                    {
                        intentos++;
                    }
                    else if (intentos >= 6 && itemBlob.Name.Equals(nombreArchivo + $"_{limite}.txt"))
                    {
                        Logueo.EventoInfo($"[generacionReporteAzure] [Se ha subido este archivo 5 veces, consulte al administrador '{itemBlob.Name}'.]");
                        throw new Exception($"[Se ha subido este archivo 5 veces, consulte al administrador '{itemBlob.Name}'.]");
                    }
                    else if (intentos > 0 && itemBlob.Name.Equals(nombreArchivo + $"_{intentos}.{extencion}"))
                        intentos++;
                }

                blobName = intentos > 0 ? nombreArchivo + $"_{intentos}.{extencion}" : blobName;

                using (FileStream fileStream = File.OpenRead(rutaArchivo))
                {
                    containerClient.UploadBlob(blobName, fileStream);
                }

                Logueo.EventoInfo($"[generacionReporteAzure] [Termina envío de archivo a Azure Blob Storage con el nombre '{blobName}'.]");
                containerClient = null;
                blobServiceClient = null;
                return true;
            }
            catch (Exception ex)
            {
                Logueo.Error($"[generacionReporteAzure] [Error al enviar archivo a Azure: '{ex.Message}'Version=4.0.6.0]");
                throw ex;
            }
        }

        public bool EnvioCarpetaProcesados(string rutaArchivoOrigen, string nombreArchivo)
        {
            try
            {
                bool carpetaExist = validacionCarpeta(DirectorioProcesados);
                string rutaArchivoProcesados = Path.Combine(DirectorioProcesados, nombreArchivo + ".txt");

                Logueo.EventoInfo("[generacionReporteAzure] [Se mueve archivo a carpeta de Procesados.]");
                File.Move(rutaArchivoOrigen, rutaArchivoProcesados);

                return true;
            }
            catch (Exception ex)
            {
                Logueo.Error("[generacionReporteAzure] [Error al enviar archivo a carpeta procesados: " + ex.Message + "]");
                throw ex;
            }
        }

        public bool CopiarCarpetaProcesados(string archivoItem, string nombreArchivo)
        {
            try
            {
                bool carpetaExist = validacionCarpeta(DirectorioProcesados);
                string rutaArchivoProcesados = Path.Combine(DirectorioProcesados, nombreArchivo + ".txt");

                Logueo.EventoInfo("[generacionReporteAzure] [Se copia archivo " + nombreArchivo + " a carpeta de Procesados]");
                File.Copy(archivoItem, rutaArchivoProcesados, true);

                return true;
            }
            catch (Exception ex)
            {
                Logueo.Error("[generacionReporteAzure] [Error al copiar archivo a carpeta procesados: " + ex.Message + "]");
                throw ex;
            }
        }

        private List<CamposReporte> ArmaCamposReporteTransaccionesIvaT()
        {
            try
            {
                List<CamposReporte> olsCampos = new List<CamposReporte>();
                olsCampos.Add(new CamposReporte() { nombre = "NombreTH", longitud = 40, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "IdentificacionTH", longitud = 19, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "IBAN", longitud = 34, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "FechaTransaccion", longitud = 8, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "FechaRegistro", longitud = 8, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "Comercio", longitud = 25, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "ConceptoTranx", longitud = 30, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "Pais", longitud = 30, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "TipoMoneda", longitud = 3, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "CodigoProceso", longitud = 2, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "MontoTransaccion", longitud = 15, Alineado = "Der" });
                olsCampos.Add(new CamposReporte() { nombre = "MontoIVA", longitud = 15, Alineado = "Der" });

                return olsCampos;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private List<CamposReporte> ArmaCamposReporteEcomTrx()
        {
            try
            {
                List<CamposReporte> olsCampos = new List<CamposReporte>();
                olsCampos.Add(new CamposReporte() { nombre = "Codigo", longitud = 40, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "EntidadFinanciera", longitud = 19, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "Periodo", longitud = 34, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "Comercio", longitud = 8, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "MesTransaccion", longitud = 8, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "Moneda", longitud = 25, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "CantidadTransaccion", longitud = 30, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "Monto", longitud = 30, Alineado = "Izq" });
                olsCampos.Add(new CamposReporte() { nombre = "MontoDecimales", longitud = 3, Alineado = "Izq" });

                return olsCampos;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string Truncar(decimal value, int length)
        {
            string[] param = value.ToString().Split('.');

            if (param[1].Length >= length)
                return param[0] + "." + param[1].Substring(0, length);
            else
                return param[0] + "." + param[1].Substring(0, param[1].Length);
        }

        public bool IsNumeric(string value)
        {
            return value.All(char.IsNumber);
        }
    }

    public class CamposReporte
    {
        public string nombre { set; get; }
        public int longitud { set; get; }
        public string Alineado { set; get; }
    }
}
