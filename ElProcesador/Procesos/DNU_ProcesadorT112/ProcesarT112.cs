using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using DNU_ProcesadorT112.LogicaNegocio;
using System.IO;
using DNU_ProcesadorT112.BaseDatos;
using DNU_ProcesadorT112.Entidades;

namespace DNU_ProcesadorT112
{
    public class ProcesarT112 : IProcesoNocturno
    {
        public ProcesarT112()
        {

        }

        public void ProcesarloTest()
        {
            try
            {
                LNArchivo.EscucharDirectorio();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }

        public void ProcesarTesto()
        {
            //ConfiguracionContexto.InicializarContexto();
            //this.Procesar();
        }

        bool Procesar_Anterior()
        {

            try
            {
                int resp;

                resp = 0;

                try
                {
                    DirectoryInfo directory = new DirectoryInfo(PNConfig.Get("PROCESAT112", "DirectorioEntrada"));

                    //  String Prefijo = "C360";
                    //String laFecha = "2016-03-27";

                    List<Archivo> losArchivos = DAOArchivo.ObtenerArchivosConfigurados();

                    //OBTENER TODOS LOS ARCHIVOS CONFIGURADOS EN LA BASE DE DATOS PARA OBTENER LOS PREFIJOS.

                    foreach (Archivo elArchivo in losArchivos)
                    {
                        Logueo.Evento("Iniciar el procezamiento de archivos : " + elArchivo.Nombre);
                        FileInfo[] files = directory.GetFiles(elArchivo.Nombre + "*.*");

                        for (int i = 0; i < files.Length; i++)
                        {

                            String elpathFile = (((FileInfo)files[i]).FullName);
                            elArchivo.UrlArchivo = elpathFile;

                            //cambia el nombre del archivo para evitar que el proceso automatico y el proceso programadado choquen
                            Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                            //string extension = Path.GetExtension(e.FullPath);
                            string filename1 = Path.GetFileName(elpathFile);

                            elArchivo.NombreArchivoDetectado = filename1;
                            // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                            string root1 = Path.GetDirectoryName(elpathFile);

                            //rellena los espacios para complir con los espacios del layout

                            FileFormat.PadFileLines(elpathFile, int.Parse(LNArchivo.GetLongFila(elArchivo)), ' ');


                            File.Move(elpathFile, root1 + "\\EN_PROCESO_" + filename1);

                            elpathFile = root1 + "\\EN_PROCESO_" + filename1;

                            elArchivo.UrlArchivo = elpathFile;

                            Logueo.Evento("Procesar archivo desde hora programada [" + ((FileInfo)files[i]).FullName + "] ");

                            //Abrir el Archivo
                            //Crear los objetos Usuario por cada registro del 
                            //iniicar el ciclo de actualizacion.
                            RespuestaProceso respuestaProceso = LNArchivo.ProcesaArch(elArchivo, elpathFile);

                            //RENOMBRAR el ARCHIVO
                            if (respuestaProceso.CodigoRespuesta == 0)
                            {
                                Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                                //string extension = Path.GetExtension(e.FullPath);
                                string filename = Path.GetFileName(elpathFile);
                                // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                                string root = Path.GetDirectoryName(elpathFile);
                                // File.Move(elpathFile, PNConfig.Get("PROCESAT112", "DirectorioSalida") + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", ""));

                                moverArchivo(elpathFile, PNConfig.Get("PROCESAT112", "DirectorioSalida") + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", ""));
                                DAOFichero.GuardarErrorFicheroEnBD(respuestaProceso.ID_Fichero, "PROCESADO EXITOSAMENTE, VALDIAR RESULTADOS UNITARIOS");
                                DAOFichero.ActualizaEstatusFichero(respuestaProceso.ID_Fichero, Utilidades.EstatusFichero.Procesado);
                            }
                        }

                    }

                }

                catch (Exception ex)
                {

                    Console.WriteLine(ex.ToString());

                }


                if (resp == 0)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
            return true;

        }

        bool IProcesoNocturno.Procesar()
        {
            Logueo.Evento("[PROCESAT112 ] INICIA PROGRAMADO ");
            try
            {
                int resp;

                resp = 0;

                try
                {
                    DirectoryInfo directory = new DirectoryInfo(PNConfig.Get("PROCESAT112", "DirectorioEntrada"));
                    //DirectoryInfo directory = new DirectoryInfo(@"C:\FTP_dev\cacao\T112");

                    //  String Prefijo = "C360";
                    //String laFecha = "2016-03-27";

                    // List<Archivo> losArchivos = DAOArchivo.ObtenerArchivosConfigurados();

                    //OBTENER TODOS LOS ARCHIVOS CONFIGURADOS EN LA BASE DE DATOS PARA OBTENER LOS PREFIJOS.

                    foreach (Archivo elArchivo in DAOComparador.ObtienArchivosPendientes())
                    {


                        Logueo.Evento("INICIA PROCESO ARCHIVO DE BASE DE DATOS[" + elArchivo.NombreArchivoDetectado + "] ");

                        //Abrir el Archivo
                        //Crear los objetos Usuario por cada registro del 
                        //iniicar el ciclo de actualizacion.
                        RespuestaProceso respuestaProceso = LNArchivo.ProcesaArchivosDesdeBD(elArchivo);

                        //RENOMBRAR el ARCHIVO
                        if (respuestaProceso.CodigoRespuesta == 0)
                        {
                            Logueo.Evento("TERMINA EL PROCSO DE ARCHIVO: " + elArchivo.NombreArchivoDetectado);
                            //string extension = Path.GetExtension(e.FullPath);
                            DAOFichero.GuardarErrorFicheroEnBD(respuestaProceso.ID_Fichero, "PROCESADO EXITOSAMENTE, VALIDAR RESULTADOS UNITARIOS");
                            DAOFichero.ActualizaEstatusFichero(respuestaProceso.ID_Fichero, Utilidades.EstatusFichero.Procesado);
                        }
                        else
                        {
                            Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elArchivo.NombreArchivoDetectado);
                            //string extension = Path.GetExtension(e.FullPath);
                            DAOFichero.GuardarErrorFicheroEnBD(respuestaProceso.ID_Fichero, "PROCESADO CON ALGUN ERROR, VALIDAR RESULTADOS UNITARIOS");
                            DAOFichero.ActualizaEstatusFichero(respuestaProceso.ID_Fichero, Utilidades.EstatusFichero.Procesado);
                        }

                    }



                }

                catch (Exception ex)
                {

                    Logueo.Error("[PROCESAT112] Error en la ejecucion del proceso: " + ex.Message);
                    return false;

                }


                if (resp == 0)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
            return true;

        }

        private void moverArchivo(string pathOrigen, string pathDestino)
        {

            if (!File.Exists(pathDestino))
            {
                File.Move(pathOrigen, pathDestino);
            }
            else
            {
                int numero = 0;
                bool validador = true;
                while (validador)
                {
                    if (numero == 0)
                    {
                        string[] list = pathDestino.Split('\\');
                        string nombreEliminar = list.Last();
                        string[] nom = nombreEliminar.Split('.');
                        string nombre = nom.First() + " - copia." + nom.Last();

                        string nvoPath = pathDestino.Replace(nombreEliminar, nombre);
                        validador = !renombrarArchivo(pathOrigen, nvoPath);
                        numero = numero + 1;
                    }
                    else
                    {
                        numero = numero + 1;
                        string[] list = pathDestino.Split('\\');
                        string nombreEliminar = list.Last();
                        string[] nom = nombreEliminar.Split('.');
                        string nombre = nom.First() + " - copia (" + numero + ")." + nom.Last();

                        string nvoPath = pathDestino.Replace(nombreEliminar, nombre);
                        validador = !renombrarArchivo(pathOrigen, nvoPath);
                    }
                }
            }
        }

        private bool renombrarArchivo(string pathOrigen, string pathDestino)
        {
            if (!File.Exists(pathDestino))
            {
                File.Move(pathOrigen, pathDestino);
                return true;
            }
            else
            {
                return false;
            }
        }

        void IProcesoNocturno.Iniciar()
        {

            try
            {

                LNArchivo.EscucharDirectorio();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }

        void IProcesoNocturno.Detener()
        {

            try
            {
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }


    }
}
