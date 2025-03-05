using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using DNU_ProcesadorGenerico.Entidades;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using DNU_ProcesadorGenerico.BaseDatos;
using System.Data;
using Executer.Entidades;
using Interfases.Entidades;
using Executer.BaseDatos;
using DNU_ProcesadorGenerico.WebReference;

namespace DNU_ProcesadorGenerico.LogicaNegocio
{
    class LNArchivo
    {
        //delegate void EnviarDetalle(String elDetalle);
        //public static Dictionary<String, IAsyncResult> ThreadsUsuarios = new Dictionary<string, IAsyncResult>();


        public static Boolean ProcesaArch(Archivo unArchivo, String path)
        {
            try
            {
                Int64 ID_Fichero = 0;

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


                DataSet losNOArchivoSIBD = DAOComparador.ObtieneDatosNOArchivoSIBaseDatos(ID_Fichero, laConsultaWhere);

                lasRespuesta.Append("Registros NO en Archivo SI en BD:");
                lasRespuesta.Append(losNOArchivoSIBD.Tables[0].Rows.Count);



                //DataSet losSIArchivoSIBD = DAOComparador.ObtieneDatosSIArchivoSIBaseDatos(ID_Fichero, laConsultaWhere);
                //lasRespuesta.Append("\nRegistros SI en Archivo SI en BD:");
                //lasRespuesta.Append(losSIArchivoSIBD.Tables[0].Rows.Count);


                //DataSet losSIArchivoNOBD = DAOComparador.ObtieneDatosSIArchivoNOBaseDatos(ID_Fichero, laConsultaWhere);
                //lasRespuesta.Append("\nRegistros SI en Archivo NO en BD:");
                //lasRespuesta.Append(losSIArchivoNOBD.Tables[0].Rows.Count);


                //StringBuilder ElCuerpodelMail = new StringBuilder(File.ReadAllText(PNConfig.Get("PROCARCH", "HTMLMailToSend") ));
                //ElCuerpodelMail = ElCuerpodelMail.Replace("[NOMBREARCHIVO]", path.Replace("EN_PROCESO_",""));//QUITAMOS EL PREFIJO QUE INDICA QUE ESTA SIENDO PROCESADO
                //ElCuerpodelMail= ElCuerpodelMail.Replace("[OPERARCHIVO]",unArchivo.LosDatos.Count.ToString());
                //ElCuerpodelMail= ElCuerpodelMail.Replace("[OPERBD]",OperacionesBD.ToString());
                //ElCuerpodelMail= ElCuerpodelMail.Replace("[SIARCHIVOSIBD]",losSIArchivoSIBD.Tables[0].Rows.Count.ToString());
                //ElCuerpodelMail= ElCuerpodelMail.Replace("[SIARCHIVONOBD]",losSIArchivoNOBD.Tables[0].Rows.Count.ToString());
                //ElCuerpodelMail= ElCuerpodelMail.Replace("[NOARCHIVOSIBD]",losNOArchivoSIBD.Tables[0].Rows.Count.ToString());

                //Genera la Tabla que en algun lado no esten.



                //LNMailing.EnviaResultadoConciliacion(ElCuerpodelMail.ToString(), unArchivo);

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
            string sLine;
            int noFilaDeArchivo = 0;
            String UltimaFilaLeida = "";

            try
            {
                while ((sLine = objReader.ReadLine()) != null)
                {
                    //DECODIFICA HEADER

                    if ((noFilaDeArchivo == 0) & (elArchivo.LID_Header != 0))
                    {
                        if (sLine.Substring(0, 7).Contains(elArchivo.ClaveHeader))
                        {
                            elArchivo.Header = DecodificaFila(sLine, elArchivo.laConfiguracionHeaderLectura);
                        }
                    }

                    //DECODIFICA DETALLES
                    if (noFilaDeArchivo > 0)
                    {
                        if (sLine.Substring(0, 10).Contains(elArchivo.ClaveRegistro))
                        {

                            elArchivo.LosDatos.Add(DecodificaFila(sLine, elArchivo.laConfiguracionDetalleLectura));
                        }
                    }

                    noFilaDeArchivo++; //INCREMENTA LA FILA LEIDA

                    UltimaFilaLeida = sLine;
                }

                //DECODIFICA FOOTER
                if ((noFilaDeArchivo > 0) & (elArchivo.LID_Footer != 0))
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

        public static Boolean LeerArchivo(String path, out DTOArchivo unArchivo)
        {
            try
            {
                //Int64 ID_Fichero = 0;

                unArchivo = ObtenerRegistrosDeArchivo(path);

                /*using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
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


                DataSet losNOArchivoSIBD = DAOComparador.ObtieneDatosNOArchivoSIBaseDatos(ID_Fichero, laConsultaWhere);

                lasRespuesta.Append("Registros NO en Archivo SI en BD:");
                lasRespuesta.Append(losNOArchivoSIBD.Tables[0].Rows.Count);*/

                return true;

            }
            catch (Exception err)
            {
                Logueo.Error("getUsuarios(): " + err.Message);
                unArchivo = null;
                return false;
            }
        }

        public static DTOArchivo ObtenerRegistrosDeArchivo(String elPath)
        {
            StreamReader objReader = new StreamReader(elPath);
            string sLine;
            //int noFilaDeArchivo = 0;
            //String UltimaFilaLeida = "";
            DTOArchivo retorno = new DTOArchivo();

            try
            {
                int contadorSeccion;
                while ((sLine = objReader.ReadLine()) != null)
                {
                    //DECODIFICA HEADER

                    contadorSeccion = 0;

                    var secciones = sLine.Split(new string[] { DTOConfiguracionArchivo.SEPARADOR }, StringSplitOptions.RemoveEmptyEntries);

                    if (secciones[contadorSeccion] == DTOConfiguracionArchivo.TAG_ENCABEZADO)
                    {
                        contadorSeccion++;

                        DateTime fecha;

                        if (!DateTime.TryParseExact(secciones[contadorSeccion++], DTOConfiguracionArchivo.ENC_Fecha.Formato, null, System.Globalization.DateTimeStyles.None, out fecha))
                            throw new Exception("Formato inválido");

                        retorno.ENC_Fecha = fecha;
                    }
                    else if (secciones[contadorSeccion] == DTOConfiguracionArchivo.TAG_REGISTRO)
                    {
                        contadorSeccion++;

                        int contador;
                        string medioPago;
                        string tipoMedioPago;
                        string claveConcepto;
                        decimal importe;
                        string ticket;

                        if (!int.TryParse(secciones[contadorSeccion++], out contador))
                            throw new Exception("Formato inválido");

                        medioPago = secciones[contadorSeccion++];
                        tipoMedioPago = secciones[contadorSeccion++];
                        claveConcepto = secciones[contadorSeccion++];

                        if (!decimal.TryParse(secciones[contadorSeccion++], out importe))
                            throw new Exception("Formato inválido");

                        ticket = secciones[contadorSeccion++];

                        retorno.Filas.Add(new DTOArchivo.DTOFila
                        {
                            ClaveConcepto = claveConcepto,
                            Contador = contador,
                            Importe = importe,
                            MedioPago = medioPago,
                            TipoMedioPago = tipoMedioPago,
                            Ticket = ticket
                        });

                    }
                    else if (secciones[contadorSeccion] == DTOConfiguracionArchivo.TAG_PIE)
                    {
                        contadorSeccion++;

                        int registros;
                        decimal total;

                        if (!int.TryParse(secciones[contadorSeccion++], out registros))
                            throw new Exception("Formato inválido");

                        if (!decimal.TryParse(secciones[contadorSeccion++], out total))
                            throw new Exception("Formato inválido");

                        retorno.PIE_NumeroRegistros = registros;
                        retorno.PIE_ImporteTotal = total;
                    }

                    //noFilaDeArchivo++; //INCREMENTA LA FILA LEIDA

                    //UltimaFilaLeida = sLine;
                }

                retorno.Url = elPath;

                return retorno;
            }
            catch (Exception err)
            {
                Logueo.Error("ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source);
            }
            finally
            {
                objReader.Close();
            }

            return null;
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

        internal static bool ProcesarArchivo(DTOArchivo elArchivo)
        {

            try
            {



                foreach (var fila in elArchivo.Filas)
                {

                    String CadenaComercial = System.Configuration.ConfigurationManager.AppSettings["CadenaComercial"];
                    String Sucursal = System.Configuration.ConfigurationManager.AppSettings["Sucursal"];
                    String Afiliacion = System.Configuration.ConfigurationManager.AppSettings["Afiliacion"];
                    String Terminal = System.Configuration.ConfigurationManager.AppSettings["Terminal"];
                    String Beneficiario = System.Configuration.ConfigurationManager.AppSettings["Beneficiario"];
                    String WSUsuario = System.Configuration.ConfigurationManager.AppSettings["WSUsuario"];
                    String WSPassword = System.Configuration.ConfigurationManager.AppSettings["WSPassword"];



                    //"";
                    //"CADDNU";
                    String ClaveMA = fila.MedioPago;//"5063800000000055";
                    String ClaveEvento = fila.ClaveConcepto;
                    String Concepto = fila.ClaveConcepto;//"Prueba de Ejecucion de Evenot Manual";
                    String Observaciones = "";//"Esta es una observacion";
                    long ReferencuaNumerica = 12;

                    //using (SqlConnection conn = new SqlConnection(DTOConfiguracionArchivo.BD_ESCRITURA))//"Server=64.34.163.109;Database=Autorizador;User Id=sa;Password=dnu6996"))
                    //{
                    //    conn.Open();

                    //    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                    //    {

                    try
                    {
                        //            Poliza laPoliza = null;

                        //   Dictionary<String, Parametro> TodosLosParametros = DAOEvento.ListaDeParamentrosContrato(CadenaComercial, ClaveMA, ClaveEvento, "Procesador");


                        //  TodosLosParametros["@Importe"] = new Parametro() { Nombre = "@Importe", Valor = fila.Importe.ToString(), Descripcion = "Importe" };


                        ////Genera y Aplica la Poliza
                        //Executer.EventoManual aplicador = new Executer.EventoManual(Int32.Parse(TodosLosParametros["@ID_Evento"].Valor), Concepto, false, ReferencuaNumerica, TodosLosParametros, Observaciones, conn, transaccionSQL);
                        //laPoliza = aplicador.AplicaContablilidad();

                        PaymentProcess elProceso = new PaymentProcess();

                        myPayResponse laRespuesta = elProceso.Pay(WSUsuario, WSPassword, fila.TipoMedioPago, fila.MedioPago, null, null, "1299", "000", Beneficiario
                              , float.Parse(fila.Importe.ToString()), 0, null, null, fila.Ticket, Sucursal, Afiliacion, Terminal, "BATCH", fila.ClaveConcepto);

                        //if (int.Parse(laRespuesta.codigoRespuesta)!= 0)
                        //{
                        //    transaccionSQL.Rollback();
                        //    throw new Exception("No se Genero la Póliza del Evento Seleccionado");
                        //}


                        fila.CodigoRespuesta = laRespuesta.codigoRespuesta;
                        fila.NumeroAutorizacion = laRespuesta.autorizacion;

                        return true;

                    }

                    catch (Exception err)
                    {
                        // transaccionSQL.Rollback();
                        Logueo.Error("AplicarMovimiento.LNEventos.EjecuarEvento()" + err.Message);

                        fila.CodigoRespuesta = "9996";
                        fila.NumeroAutorizacion = "000000";
                        //  return false;
                    }
                }
         

                    
                

                return true;
            }
            catch (Exception ex)
            {
                Logueo.Error("ProcesarArchivo(DTOArchivo elArchivo)" + ex.Message + " " + ex.StackTrace + ", " + ex.Source);
                return false;
            }

        }
       
        
        //{
        //    //Aqui irán las reglas de negocio, de momento todas las respuestas serán satisfactorias
        //    try
        //    {
        //        foreach (var fila in elArchivo.Filas)
        //        {
        //            fila.CodigoRespuesta = "0000";
        //            fila.NumeroAutorizacion = 111111;
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Logueo.Error("ProcesarArchivo(DTOArchivo elArchivo)" + ex.Message + " " + ex.StackTrace + ", " + ex.Source);
        //        return false;
        //    }
        //}


       internal static void EscibirArchivo(string elpathFile, DTOArchivo elArchivo)
        {
            List<string> lineas = new List<string>();

            try
            {
                List<string> lineaInterna = new List<string>();
                lineaInterna.Add(DTOConfiguracionArchivo.TAG_ENCABEZADO);
                lineaInterna.Add(elArchivo.ENC_Fecha.ToString(DTOConfiguracionArchivo.ENC_Fecha.Formato));

                lineas.Add(string.Join(DTOConfiguracionArchivo.SEPARADOR, lineaInterna) + DTOConfiguracionArchivo.SEPARADOR);

                foreach (var fila in elArchivo.Filas) 
                {
                    lineaInterna = new List<string>();

                    lineaInterna.Add(DTOConfiguracionArchivo.TAG_REGISTRO);
                    lineaInterna.Add(fila.Contador.ToString());
                    lineaInterna.Add(fila.MedioPago);
                    lineaInterna.Add(fila.TipoMedioPago);
                    lineaInterna.Add(fila.ClaveConcepto);
                    lineaInterna.Add(fila.Importe.ToString());
                    lineaInterna.Add(fila.Ticket);
                    lineaInterna.Add(fila.CodigoRespuesta);
                    lineaInterna.Add(fila.NumeroAutorizacion.ToString());

                    lineas.Add(string.Join(DTOConfiguracionArchivo.SEPARADOR, lineaInterna) + DTOConfiguracionArchivo.SEPARADOR);
                }

                lineaInterna = new List<string>();

                lineaInterna.Add(DTOConfiguracionArchivo.TAG_PIE);
                lineaInterna.Add(elArchivo.PIE_NumeroRegistros.ToString());
                lineaInterna.Add(elArchivo.PIE_ImporteTotal.ToString());

                lineas.Add(string.Join(DTOConfiguracionArchivo.SEPARADOR, lineaInterna) + DTOConfiguracionArchivo.SEPARADOR);

                File.WriteAllLines(elpathFile, lineas);
            }
            catch (Exception err)
            {
                Logueo.Error("ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source);
            }

        }
    }
}
