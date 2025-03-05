using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using DNU_ProcesadorArchivos.LogicaNegocio;
using System.IO;
using DNU_ProcesadorArchivos.BaseDatos;
using DNU_ProcesadorArchivos.Entidades;

namespace DNU_ProcesadorArchivos
{
    class ProcesarArchivos : IProcesoNocturno
    {


        bool IProcesoNocturno.Procesar()
        {

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
