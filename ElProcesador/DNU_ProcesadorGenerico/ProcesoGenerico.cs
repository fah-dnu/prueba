using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using DNU_ProcesadorGenerico.LogicaNegocio;
using System.IO;
using DNU_ProcesadorGenerico.BaseDatos;
using DNU_ProcesadorGenerico.Entidades;

namespace DNU_ProcesadorGenerico
{
    public class ProcesoGenerico : IProcesoNocturno
    {
        bool Procesar_OLD()
        {

            try
            {
                int resp;

                resp = 0;

                try
                {
                    DirectoryInfo directory = new DirectoryInfo(PNConfig.Get("MEDAARCH", "Directorio"));

                    List<Archivo> losArchivos = DAOArchivo.ObtenerArchivosConfigurados();

                    //losArchivos.Add(new Archivo
                    //{
                    //    Prefijo = "SOL_MEDA"
                    //});

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


        }

        bool IProcesoNocturno.Procesar()
        {

            try
            {
                int resp;

                resp = 0;

                try
                {
                    DirectoryInfo directory = new DirectoryInfo(System.Configuration.ConfigurationManager.AppSettings["MEDA_Directorio"]);//(PNConfig.Get("MEDA", "Directorio"));

                    //List<Archivo> losArchivos = DAOArchivo.ObtenerArchivosConfigurados();

                    var prefijoEntrada = DTOConfiguracionArchivo.REGEX_ENTRADA;

                    //losArchivos.Add(new Archivo
                    //{
                    //    Prefijo = "SOL_MEDA"
                    //});

                    //OBTENER TODOS LOS ARCHIVOS CONFIGURADOS EN LA BASE DE DATOS PARA OBTENER LOS PREFIJOS.

                    foreach (FileInfo file in directory.GetFiles().Where(m => DTOConfiguracionArchivo.REGEX_ENTRADA.IsMatch(m.Name)))
                    {
                        //for (int i = 0; i < files.Length; i++)
                        //{

                        DTOArchivo elArchivo;

                        String elpathFile = file.FullName;//(((FileInfo)files[i]).FullName);
                        //elArchivo.UrlArchivo = elpathFile;

                        //cambia el nombre del archivo para evitar que el proceso automatico y el proceso programadado choquen
                        Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                        //string extension = Path.GetExtension(e.FullPath);
                        string filename1 = Path.GetFileName(elpathFile);
                        // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                        string root1 = Path.GetDirectoryName(elpathFile);
                        File.Move(elpathFile, root1 + "\\EN_PROCESO_" + filename1);

                        elpathFile = root1 + "\\EN_PROCESO_" + filename1;

                        Logueo.Evento("Procesar archivo desde hora programada [" + file.FullName + "] ");

                        //Abrir el Archivo
                        //Crear los objetos Usuario por cada registro del 
                        //iniicar el ciclo de actualizacion.
                        Boolean resp2 = LNArchivo.LeerArchivo(elpathFile, out elArchivo);

                        //RENOMBRAR el ARCHIVO
                        if (resp2)
                        {
                            Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                            //string extension = Path.GetExtension(e.FullPath);
                            string filename = Path.GetFileName(elpathFile);
                            // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                            string root = Path.GetDirectoryName(elpathFile);
                            File.Move(elpathFile, root + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", ""));

                            elpathFile = root + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", "");

                            resp2 = LNArchivo.ProcesarArchivo(elArchivo);

                            if (resp2)
                            {
                                Logueo.Evento("Generando respuesta del Archivo procesado: " + elpathFile);
                                //string extension = Path.GetExtension(e.FullPath);
                                filename = Path.GetFileName(elpathFile);
                                // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                                root = Path.GetDirectoryName(elpathFile);

                                elpathFile = root + "\\" + filename.Replace("PROCESADO_" + DTOConfiguracionArchivo.TAG_ENTRADA, DTOConfiguracionArchivo.TAG_SALIDA);

                                LNArchivo.EscibirArchivo(elpathFile, elArchivo);
                            }
                        }
                        //}
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


        }

        void IProcesoNocturno.Iniciar()
        {

            try
            {
                //LNUsuario.EscucharDirectorio();
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
