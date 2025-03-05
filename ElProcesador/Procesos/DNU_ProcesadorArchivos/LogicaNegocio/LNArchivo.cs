using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using DNU_ProcesadorArchivos.Entidades;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using DNU_ProcesadorArchivos.BaseDatos;
using System.Data;
using System.Threading;

namespace DNU_ProcesadorArchivos.LogicaNegocio
{
    class LNArchivo
    {
        delegate void EnviarDetalle(String elDetalle);
        public static Dictionary<String, IAsyncResult> ThreadsUsuarios = new Dictionary<string, IAsyncResult>();


        public static Boolean ProcesaArch(Archivo unArchivo, String path)
        {
            try
            {
                Int64 ID_Fichero = 0;

                //obtener la configuracion del ARchivo.
               // Archivo unArchivo = DAOArchivo.ObtenerArchivo(NombreArchivo, 0);

                //obtener la configuracion de las Filas.

                //obtiene la configuracion del Proceso a ejecutar con este archivo.

                //obtine los campos del archivo
                ObtieneRegistrosDeArchivo(path, ref unArchivo);



                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    conn.Open();

                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                    {
                        try
                        {

                            //GUARDA EL FICHERO
                            ID_Fichero = DAOArchivo.GuardarFicheroEnBD(unArchivo, conn, transaccionSQL);

                            if (ID_Fichero == 0)
                            {
                                transaccionSQL.Rollback();
                                Logueo.Error("ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO");
                                throw new Exception("ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO");
                            }


                            //Guarda los detalles
                            Boolean guardoLosDetalles = DAOArchivo.GuardarFicheroDetallesEnBD(unArchivo, ID_Fichero, conn, transaccionSQL);

                            if (!guardoLosDetalles)
                            {
                                transaccionSQL.Rollback();
                                Logueo.Error("ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO");
                                throw new Exception("ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO");
                            }

                            transaccionSQL.Commit();
                        }
                        catch (Exception err)
                        {
                            Logueo.Error("ProcesaArch(): ERROR AL ALMACENAR EL FICHERO: " + err.Message);
                            transaccionSQL.Rollback();
                        }
                    }
                }

                //si es de Comparacion, obtiene los datos de la base de datos.

                //Realiza las Comparaciones.

                //Ejecuta los Eventos de cada fila del Archivo.

                DatosBaseDatos laConfigForConmparacion = DAODatosBase.ObtenerConsulta(unArchivo.ID_Archivo);


                //Obtener los DAtos la Consulta Obtenida

                //   Datos losDatosObtenidos= DAODatosBase.ObtieneDatosFromConexion(laConfigForConmparacion);

                if (ID_Fichero == 0)
                {
                    throw new Exception("NO SE PUDO OBTENER UN ID_FICHERO VALIDO");
                }

                //Insertar los datos obtenidos en la DB de Archivos
                int OperacionesBD = DAODatosBase.InsertaDatosEnTabla(laConfigForConmparacion, ID_Fichero, unArchivo);

                //OBTINE LAS CONFIGURACIONES DE CAMPOS-FILAS
                List<ComparadorConfig> lasConfiguraciones = DAOComparador.ObtenerConfiguracionFila(ID_Fichero);
                StringBuilder lasRespuesta = new StringBuilder();
                
                String laConsultaWhere = LNComparacion.GeneraConsultaWhereSQL(lasConfiguraciones);


                

                StringBuilder ElCuerpodelMail = new StringBuilder(File.ReadAllText(PNConfig.Get("PROCARCH", "HTMLMailToSend")));

                DataSet losNOArchivoSIBD = DAOComparador.ObtieneDatosNOArchivoSIBaseDatos(ID_Fichero, laConsultaWhere);
                lasRespuesta.Append("Registros NO en Archivo SI en BD:");
                lasRespuesta.Append(losNOArchivoSIBD.Tables[0].Rows.Count);
                ElCuerpodelMail = ElCuerpodelMail.Replace("[NOARCHIVOSIBD]", losNOArchivoSIBD.Tables[0].Rows.Count.ToString());
                ElCuerpodelMail = ElCuerpodelMail.Replace("[NOARCHIVOSIBDDETA]", ConvertDataTableToHTML(losNOArchivoSIBD.Tables[0]));


                DataSet losSIArchivoSIBD = DAOComparador.ObtieneDatosSIArchivoSIBaseDatos(ID_Fichero, laConsultaWhere);
                lasRespuesta.Append("\nRegistros SI en Archivo SI en BD:");
                lasRespuesta.Append(losSIArchivoSIBD.Tables[0].Rows.Count);
                ElCuerpodelMail = ElCuerpodelMail.Replace("[SIARCHIVOSIBD]", losSIArchivoSIBD.Tables[0].Rows.Count.ToString());
                ElCuerpodelMail = ElCuerpodelMail.Replace("[SIARCHIVOSIBDDETA]", ConvertDataTableToHTML(losSIArchivoSIBD.Tables[0]));

                DataSet losSIArchivoNOBD = DAOComparador.ObtieneDatosSIArchivoNOBaseDatos(ID_Fichero, laConsultaWhere);
                lasRespuesta.Append("\nRegistros SI en Archivo NO en BD:");
                lasRespuesta.Append(losSIArchivoNOBD.Tables[0].Rows.Count);
                ElCuerpodelMail = ElCuerpodelMail.Replace("[SIARCHIVONOBD]", losSIArchivoNOBD.Tables[0].Rows.Count.ToString());
                ElCuerpodelMail = ElCuerpodelMail.Replace("[SIARCHIVONOBDDETA]", ConvertDataTableToHTML(losSIArchivoNOBD.Tables[0]));


                ElCuerpodelMail = ElCuerpodelMail.Replace("[NOMBREARCHIVO]", path.Replace("EN_PROCESO_",""));//QUITAMOS EL PREFIJO QUE INDICA QUE ESTA SIENDO PROCESADO
                ElCuerpodelMail= ElCuerpodelMail.Replace("[OPERARCHIVO]",unArchivo.LosDatos.Count.ToString());
                ElCuerpodelMail= ElCuerpodelMail.Replace("[OPERBD]",OperacionesBD.ToString());
               
                
                

                //Genera la Tabla que en algun lado no esten.



                LNMailing.EnviaResultadoConciliacion(ElCuerpodelMail.ToString(), unArchivo);

                return true;

            }
            catch (Exception err)
            {
                Logueo.Error("getUsuarios(): " + err.Message);
                return false;
            }
        }


        public static string ConvertDataTableToHTML(DataTable dt)
        {
            string html = "<table>";
            //add header row
            //html += "<tr>";
            //for (int i = 0; i < dt.Columns.Count; i++)
            //    html += "<td>" + dt.Columns[i].ColumnName + "</td>";
            //html += "</tr>";
            //add rows
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                html += "<tr>";
                for (int j = 0; j < dt.Columns.Count; j++)
                    html += "<td>" + dt.Rows[i][j].ToString() + "</td>";
                html += "</tr>";
            }
            html += "</table>";
            return html;
        }
        public static void ObtieneRegistrosDeArchivo(String elPath, ref Archivo elArchivo)
        {
            StreamReader objReader = new StreamReader(elPath);
            string sLine = "";
            //List<Fila> losDetalles = new List<Fila>();
            int noFilaDeARchivo = 0;
            String UltimaFilaLeida = "";

            try
            {
                while (sLine != null)
                {
                    sLine = objReader.ReadLine();

                    if (sLine == null)
                    {
                        break;
                    }



                    //DECODIFICA HEADER

                    if ((noFilaDeARchivo == 0) & (elArchivo.LID_Header != 0))
                    {
                        //losDetalles.Add(DecodificaFila(sLine,elArchivo.laConfiguracionHeaderLectura));
                        if (sLine.Substring(0, 7).Contains(elArchivo.ClaveHeader))
                        {
                            elArchivo.Header = DecodificaFila(sLine, elArchivo.laConfiguracionHeaderLectura);
                        }
                    }

                    //DECODIFICA DETALLES
                    if ((noFilaDeARchivo > 0))
                    {
                        if (sLine.Substring(0, 10).Contains(elArchivo.ClaveRegistro))
                        {

                            elArchivo.LosDatos.Add(DecodificaFila(sLine, elArchivo.laConfiguracionDetalleLectura));
                        }
                    }

                    //if (()) //VALIDAR SI LA FILA ACTUAL ES EMV entonces recorrer la siguiente y tomarla como DETALLE EXTENDIDO
                    //{
                    //    sLine = objReader.ReadLine();

                    //    elArchivo.losDatos.Add(DecodificaFila(sLine, elArchivo.laConfiguracionDetalleExtraLectura));
                    //}

                    noFilaDeARchivo++; //INCREMENTA LA FILA LEIDA

                    UltimaFilaLeida = sLine;
                }

                //DECODIFICA FOOTER
                if ((noFilaDeARchivo > 0) & (elArchivo.LID_Footer != 0))
                {
                    if (UltimaFilaLeida.Substring(0, 7).Contains(elArchivo.ClaveFooter))
                    {
                        elArchivo.Footer = DecodificaFila(UltimaFilaLeida, elArchivo.laConfiguracionFooterLectura);
                    }
                }

                elArchivo.Nombre = elPath;

            }
            catch (Exception err)
            {
                Logueo.Error("ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source);
            }
            finally
            {
                objReader.Close();
            }





            return;
        }

        public static Fila DecodificaFila(String Cadena, FilaConfig laConfiguracionDelaFila)
        {
            Fila laFila = new Fila();
            try
            {
                laFila.laConfigDeFila = laConfiguracionDelaFila;
                laFila.DetalleCrudo = Cadena;

                //decodifica el Header.
                if (laConfiguracionDelaFila.PorSeparador)
                {
                    string[] losDato = Cadena.Split(laConfiguracionDelaFila.CaracterSeparacion.ToCharArray());

                    for (int k = 0; k < losDato.Length; k++)
                    {
                        laFila.losCampos.Add(k+1, losDato[k]);
                    }

                }
                else if (laConfiguracionDelaFila.PorLongitud)
                {

                    Int32 elNumeroCampo = 1;

                    while (Cadena.Length != 0)
                    {
                        try
                        {
                            CampoConfig unaConfig = laConfiguracionDelaFila.LosCampos[elNumeroCampo];

                            laFila.losCampos.Add(elNumeroCampo, Cadena.Substring(0, unaConfig.Longitud));

                            Cadena = Cadena.Substring(unaConfig.Longitud);

                            elNumeroCampo++;
                        }
                        catch (Exception err)
                        {
                            Logueo.Error("DecodificaFila(): Campo: " + elNumeroCampo + ", " + err.Message + " " + err.StackTrace + ", " + err.Source);
                            throw new Exception("DecodificaFila(): Campo: " + elNumeroCampo + ", " + err.Message + " " + err.StackTrace + ", " + err.Source);
                        }
                    }

                }


            }
            catch (Exception err)
            {
                Logueo.Error("DecodificaFila()" + err.Message + " " + err.StackTrace + ", " + err.Source);
            }

            return laFila;
        }



        public static void EscucharDirectorio()
        {
            try
            {
                Logueo.Evento("Inicia la escucha de la carpeta: " + PNConfig.Get("PROCARCH", "Directorio") + " en espera de archivos.");

                FileSystemWatcher elObservador = new FileSystemWatcher(PNConfig.Get("PROCARCH", "Directorio"));
                elObservador.NotifyFilter = (NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);

                elObservador.Created += alCambiar;
                elObservador.Error += alOcurrirUnError;

                elObservador.EnableRaisingEvents = true;

            }
            catch (Exception err)
            {
                Logueo.Error("EscucharDirectorio(): " + err.Message);
            }
        }

        public static void alCambiar(object source, FileSystemEventArgs el)
        {
            try
            {
                WatcherChangeTypes elTipoCambio = el.ChangeType;
                //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
                Logueo.Evento("Hubo un Cambio [" + elTipoCambio.ToString() + "] en el directorio: " + PNConfig.Get("PROCARCH", "Directorio") + " el se recibio el archivo : " + el.FullPath);
                Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                Thread.Sleep(1000 * 60 * 1);


                Logueo.Evento("INICIO DE PROCESO DEL ARCHIVO:" + el.FullPath);


                //String elpathFile = el.FullPath;
                ////cambia el nombre del archivo para evitar que el proceso automatico y el proceso programadado choquen
                //Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                ////string extension = Path.GetExtension(e.FullPath);
                //string filename1 = Path.GetFileName(elpathFile);
                //// string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                //string root1 = Path.GetDirectoryName(elpathFile);
                //File.Move(elpathFile, root1 + "\\EN_PROCESO_" + filename1);

                //elpathFile = root1 + "\\EN_PROCESO_" + filename1;


                ////Abrir el Archivo
                ////Crear los objetos Usuario por cada registro del 
                ////iniicar el ciclo de actualizacion.
                //Boolean resp = LNArchivo.ProcesaArch(Path.GetFileName(elpathFile).Substring(0, 3), elpathFile);

                ////RENOMBRAR el ARCHIVO
                //if (resp)
                //{
                //    Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                //    //string extension = Path.GetExtension(e.FullPath);
                //    string filename = Path.GetFileName(elpathFile);
                //    // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                //    string root = Path.GetDirectoryName(elpathFile);
                //    File.Move(elpathFile, root + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", ""));
                //}


                try
                {
                    int resp;

                    resp = 0;

                    try
                    {
                        DirectoryInfo directory = new DirectoryInfo(PNConfig.Get("PROCARCH", "Directorio"));

                        //  String Prefijo = "C360";
                        //String laFecha = "2016-03-27";

                        List<Archivo> losArchivos = DAOArchivo.ObtenerArchivosConfigurados("Claro360");

                        //OBTENER TODOS LOS ARCHIVOS CONFIGURADOS EN LA BASE DE DATOS PARA OBTENER LOS PREFIJOS.

                        foreach (Archivo elArchivo in losArchivos)
                        {

                            FileInfo[] files = directory.GetFiles(elArchivo.Prefijo + "*.*");

                            for (int i = 0; i < files.Length; i++)
                            {

                                String elpathFile = (((FileInfo)files[i]).FullName);
                                elArchivo.UrlArchivo = elpathFile;

                                //cambia el nombre del archivo para evitar que el proceso automatico y el proceso programadado choquen
                                Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                                //string extension = Path.GetExtension(e.FullPath);
                                string filename1 = Path.GetFileName(elpathFile);
                                // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                                string root1 = Path.GetDirectoryName(elpathFile);
                                File.Move(elpathFile, root1 + "\\EN_PROCESO_" + filename1);

                                elpathFile = root1 + "\\EN_PROCESO_" + filename1;

                                Logueo.Evento("Procesar archivo desde hora programada [" + ((FileInfo)files[i]).FullName + "] ");

                                //Abrir el Archivo
                                //Crear los objetos Usuario por cada registro del 
                                //iniicar el ciclo de actualizacion.
                                Boolean resp2 = LNArchivo.ProcesaArch(elArchivo, elpathFile);

                                //RENOMBRAR el ARCHIVO
                                if (resp2)
                                {
                                    Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                                    //string extension = Path.GetExtension(e.FullPath);
                                    string filename = Path.GetFileName(elpathFile);
                                    // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                                    string root = Path.GetDirectoryName(elpathFile);
                                    File.Move(elpathFile, root + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", ""));
                                }
                                else
                                {
                                    Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                                    //string extension = Path.GetExtension(e.FullPath);
                                    string filename = Path.GetFileName(elpathFile);
                                    // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                                    string root = Path.GetDirectoryName(elpathFile);
                                    File.Move(elpathFile, root + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", "ERROR_" ));

                                }
                            }

                        }

                    }

                    catch (Exception ex)
                    {

                        Console.WriteLine(ex.ToString());

                    }

                }
                catch (Exception err)
                {
                    Logueo.Error(err.Message);
                    return ;
                }


            }
            catch (Exception err)
            {
                Logueo.Error("alCambiar(): " + err.Message + ",  " + err.ToString());
            }
        }


        public static void alOcurrirUnError(object source, ErrorEventArgs e)
        {
            try
            {
                Logueo.Error("alOcurrirUnError(): e: " + e.GetException().Message);
            }
            catch (Exception err)
            {
                Logueo.Error("alOcurrirUnError(): " + err.Message);
            }
        }



    }
}
