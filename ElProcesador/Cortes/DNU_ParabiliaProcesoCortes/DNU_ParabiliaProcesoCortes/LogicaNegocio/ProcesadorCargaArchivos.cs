using CommonProcesador;
using DNU_ParabiliaProcesoCortes.CapaDatos;
using DNU_ParabiliaProcesoCortes.Common.Funciones;
using DNU_ParabiliaProcesoCortes.dataService;
using DNU_ParabiliaProcesoCortes.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class ProcesadorCargaArchivos
    {
        ValidacionesArchivos validacionesArchivo;
        public ProcesadorCargaArchivos() {
            validacionesArchivo = new ValidacionesArchivos();
        }

        internal void decodificarArchivo(string pPath, string pNomArchivo, string directorioSalida)
        {
            FileInfo InfoArchivo = new FileInfo(pPath);
            Guid idLog = Guid.NewGuid();

            try
            {
                Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] Inicia Validacion del Archivo: " + pPath + "[" + idLog + "]");
                if (validacionesArchivo.validarContenido(InfoArchivo))
                {
                    //if (insertaDatos(pNomArchivo))
                    //{
                        string rutaFinal =  directorioSalida + "\\Correctos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_");
                        FuncionesArchivos.moverArchivo(InfoArchivo.FullName, rutaFinal);
                    //    Logueo.Evento("[AltaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Procesados");
                    //}
                    //else
                    //{
                    //    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                    //    moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                    //    Logueo.Evento("[AltaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
                    //}
                }
                
                else
                {
                    string pathArchivosInvalidos = directorioSalida + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                    FuncionesArchivos.moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                    Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos" + "[" + idLog + "]");
                }
            }
            catch (Exception ex)
            {
                string pathArchivosInvalidos = directorioSalida + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                FuncionesArchivos.moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [decodificarArchivo] [El proceso de validación del archivo:" + pNomArchivo + "][Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]" + "[" + idLog + "]");
                Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos" + "[" + idLog + "]");
            }
        }

        public static Boolean ProcesaArch(String path, String claveColectiva, string tipoManuFactura, ref List<DatosArchivo> lstArchvio, string directorioSalida, string subproducto = null, bool subproductoValido = false, bool esCredito = false, string log = "", string ip = "")
        {
            try
            {

                Int64 ID_Fichero = 0;
                Logueo.Evento("[" + ip + "] [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Obteniendo registros]");

                ObtieneRegistrosDeArchivo(path, ref lstArchvio, log, ip);
                if (!subproductoValido)
                {
                    Logueo.Error("[" + ip + "] [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [SUBPRODUCTO INVALIDO]");
                    throw new Exception("SUBPRODUCTO INVALIDO");
                }

                //if (!DAOArchivo.ColectivaValida(claveColectiva, DBProcesadorArchivo.BDLecturaAutorizador, log, ip))
                //{
                //    Logueo.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [LA COLECTIVA NO EXISTE O NO CUMPLE LAS VALIDACIONES]");
                //    throw new Exception("LA COLECTIVA NO EXISTE O NO CUMPLE LAS VALIDACIONES");
                //}
                //validando que la tarjeta exista en la base donde se guardara en saldos externos,esto se quitara cuando se unifiquen
                //if (!string.IsNullOrEmpty(subproducto))
                //{
                //    if (!DAOArchivo.ColectivaValida(claveColectiva, DBProcesadorArchivo.BDEscrituraAutorizadorExterno))
                //        throw new Exception("LA COLECTIVA NO EXISTE O NO CUMPLE LAS VALIDACIONES PBC");
                //}
                Logueo.Evento("[" + ip + "] [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Guardando fichero en base de datos]");

                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    DataTable dtDetalle = null;
                    conn.Open();
                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                    {
                        try
                        {

                            //GUARDA EL FICHERO
                            ID_Fichero = DAOArchivo.GuardarFicheroEnBD(path, conn, transaccionSQL, log, ip);

                            if (ID_Fichero == 0)
                            {
                                transaccionSQL.Rollback();
                                Logueo.Error("[" + ip + "] [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO]");
                                throw new Exception("ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO");
                            }


                            //Guarda los detalles
                            Logueo.Evento("[" + ip + "] [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Generando detalle]");

                            dtDetalle = DAOArchivo.GeneraDataTableDetalle(lstArchvio, ID_Fichero, subproducto);

                            Boolean guardoLosDetalles = DAOArchivo.GuardarFicheroDetallesEnBD(dtDetalle);

                            if (!guardoLosDetalles)
                            {
                                transaccionSQL.Rollback();
                                Logueo.Error("[" + ip + "] [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO]");
                                throw new Exception("ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO");
                            }

                            transaccionSQL.Commit();
                        }
                        catch (Exception err)
                        {
                            Logueo.Error("[" + ip + "] [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [ProcesaArch(): ERROR AL ALMACENAR EL FICHERO: " + err.Message + "]");
                            transaccionSQL.Rollback();
                        }
                    }
                }




                if (ID_Fichero == 0)
                {
                    Logueo.Error("[" + ip + "] [GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [NO SE PUDO OBTENER UN ID_FICHERO VALIDO]");
                    throw new Exception("NO SE PUDO OBTENER UN ID_FICHERO VALIDO");
                }
                StringBuilder sb = new StringBuilder();

                bool ejecucionCorrecta = false;//ejecutaActivacion(ref lstArchvio, ref sb, claveColectiva, tipoManuFactura, subproducto, subproductoValido, esCredito, log, ip);
                FuncionesArchivos.renombraArchivo(path, ejecucionCorrecta, sb, log, ip, directorioSalida);

                sb.Clear();

                return true;


            }
            catch (Exception err)
            {
                Logueo.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [" + String.Format("AltaTarjetasAnonimasProcesaArch: {0} --- Stack : {1} ", err.Message, err.StackTrace) + "]");
                FuncionesArchivos.renombraArchivoPorError(path, lstArchvio, err.Message, log, ip, directorioSalida);
                return false;
            }
        }

        public static void ObtieneRegistrosDeArchivo(String elPath, ref List<DatosArchivo> elArchivo, string log, string ip)
        {

            string sLine = "";
            elArchivo.Clear();
            try
            {
                //
                using (StreamReader objReader = new StreamReader(elPath))
                {
                    while (objReader.Peek() > -1)
                    {
                        sLine = objReader.ReadLine();

                        if (string.IsNullOrEmpty(sLine))
                        {
                            break;
                        }

                        var splitted = sLine.Split('|');
                        //LogueoPrAltaTarjParabilia.Debug("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Procesando archivo "+ sLine + "]");

                        elArchivo.Add(new DatosArchivo
                        {
                            Emisor = splitted[0],
                            Cuenta = splitted[1],//String.Format("{0}{1}", splitted[0], splitted[1]),
                            NumeroTarjeta = splitted[2],
                            Tarjeta = splitted[3],
                            Vencimiento = splitted[4],
                            RawData = sLine
                        });
                    }
                }
            }
            catch (Exception err)
            {
                Logueo.Error("[" + ip + "] [ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source + "]");
            }
        }

        internal void decodificarArchivoManual(string pPath, string pNomArchivo, string directorioSalida,string rutaFecha, Archivo archivoLog)
        {
            FileInfo InfoArchivo = new FileInfo(pPath);
            Guid idLog = Guid.NewGuid();

            try
            {
                Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] Inicia Validacion del Archivo: " + pPath + "[" + idLog + "]");
                if (validacionesArchivo.validarContenido(InfoArchivo,archivoLog))
                {
                    //if (insertaDatos(pNomArchivo))
                    //{
                    string rutaFinal = directorioSalida + "\\Correctos\\" + rutaFecha + "\\"+ InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_");
                    FuncionesArchivos.moverArchivo(InfoArchivo.FullName, rutaFinal);
                    archivoLog.Procesado = true ;
                    Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [archivo procesado:"+ InfoArchivo.Name+"]");

                    //   listaArchivosProcesados.Add(nombreArchivo);
                    //    Logueo.Evento("[AltaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Procesados");
                    //}
                    //else
                    //{
                    //    string pathArchivosInvalidos = directorio + "\\Erroneos\\" + InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                    //    moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                    //    Logueo.Evento("[AltaEmpleado] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a carpeta \\Erroneos");
                    //}
                }
                else
                {
                    archivoLog.Procesado = false;
                    string pathArchivosInvalidos = directorioSalida + "\\Erroneos\\" + rutaFecha + "\\"+ InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                    FuncionesArchivos.moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                    Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a Erroneos" + "[" + idLog + "]");
                }
            }
            catch (Exception ex)
            {
                archivoLog.Procesado = false;
                string pathArchivosInvalidos = directorioSalida + "\\Erroneos\\" + rutaFecha + "\\"+ InfoArchivo.Name.Replace("EN_PROCESO_", "PROCESADO_CON_ERRORES_");
                FuncionesArchivos.moverArchivo(InfoArchivo.FullName, pathArchivosInvalidos);
                Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [decodificarArchivo] [El proceso de validación del archivo:" + pNomArchivo + "][Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]" + "[" + idLog + "]");
                Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] Archivo " + InfoArchivo.FullName.Replace("EN_PROCESO_", "") + " finalizo de procesar y se envía a Erroneos" + "[" + idLog + "]");
            }
        }
    }
}
