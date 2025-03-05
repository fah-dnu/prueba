using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using DNU_ProcesadorClaro360.Entidades;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using DNU_ProcesadorClaro360.BaseDatos;
using System.Data;


namespace DNU_ProcesadorClaro360.LogicaNegocio
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

                if (lasConfiguraciones.Count == 0)
                    return true;

                StringBuilder lasRespuesta = new StringBuilder();
                
                String laConsultaWhere = LNComparacion.GeneraConsultaWhereSQL(lasConfiguraciones);


                DataSet losNOArchivoSIBD = DAOComparador.ObtieneDatosNOArchivoSIBaseDatos(ID_Fichero, laConsultaWhere);

                lasRespuesta.Append("Registros NO en Archivo SI en BD:");
                lasRespuesta.Append(losNOArchivoSIBD.Tables[0].Rows.Count);



                DataSet losSIArchivoSIBD = DAOComparador.ObtieneDatosSIArchivoSIBaseDatos(ID_Fichero, laConsultaWhere);
                lasRespuesta.Append("\nRegistros SI en Archivo SI en BD:");
                lasRespuesta.Append(losSIArchivoSIBD.Tables[0].Rows.Count);


                DataSet losSIArchivoNOBD = DAOComparador.ObtieneDatosSIArchivoNOBaseDatos(ID_Fichero, laConsultaWhere);
                lasRespuesta.Append("\nRegistros SI en Archivo NO en BD:");
                lasRespuesta.Append(losSIArchivoNOBD.Tables[0].Rows.Count);


                StringBuilder ElCuerpodelMail = new StringBuilder(File.ReadAllText(PNConfig.Get("PROCARCH", "HTMLMailToSend") ));
                ElCuerpodelMail = ElCuerpodelMail.Replace("[NOMBREARCHIVO]", path.Replace("EN_PROCESO_",""));//QUITAMOS EL PREFIJO QUE INDICA QUE ESTA SIENDO PROCESADO
                ElCuerpodelMail= ElCuerpodelMail.Replace("[OPERARCHIVO]",unArchivo.LosDatos.Count.ToString());
                ElCuerpodelMail= ElCuerpodelMail.Replace("[OPERBD]",OperacionesBD.ToString());
                ElCuerpodelMail= ElCuerpodelMail.Replace("[SIARCHIVOSIBD]",losSIArchivoSIBD.Tables[0].Rows.Count.ToString());
                ElCuerpodelMail= ElCuerpodelMail.Replace("[SIARCHIVONOBD]",losSIArchivoNOBD.Tables[0].Rows.Count.ToString());
                ElCuerpodelMail= ElCuerpodelMail.Replace("[NOARCHIVOSIBD]",losNOArchivoSIBD.Tables[0].Rows.Count.ToString());

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
                        laFila.losCampos.Add(k, losDato[0]);
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



    }
}
