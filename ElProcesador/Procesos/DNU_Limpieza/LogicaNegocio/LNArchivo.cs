using CommonProcesador;
using DNU_Limpieza.BaseDatos;
using DNU_Limpieza.Entidades;
using DNU_Limpieza.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Limpieza.LogicaNegocio
{
    public class LNArchivo
    {
        public static void ProcesarArchivosPapelera()
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            try
            {
                LogueoLimpieza.Info("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [Inicia metodo ObtenerArchivosConfigurados]");
                List<Archivo> listaArchivos = DAOArchivo.ObtenerArchivosConfigurados();
                LogueoLimpieza.Info("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [Termina metodo ObtenerArchivosConfigurados]");

                string nombreArchivo = "";
                DateTime fechaCreacion = DateTime.Now;
                int diasArchivo = 0;
                string urlPapelera = "";

                foreach (Archivo archivo in listaArchivos)
                {
                    DirectoryInfo directory = new DirectoryInfo(archivo.URlOrigen);
                    FileInfo[] files = directory.GetFiles("*.*");
                    for (int i = 0; i < files.Length; i++)
                    {
                        nombreArchivo = files[i].ToString();
                        fechaCreacion = files[i].LastWriteTime;
                        diasArchivo = archivo.DiasArchivo;
                        urlPapelera = archivo.URLPapelera;

                        DateTime fechaDisponible = DateTime.Now.AddDays(- diasArchivo);

                        if (fechaDisponible >= fechaCreacion)
                        {
                            File.Move(directory + "\\" + nombreArchivo, urlPapelera + "\\" + nombreArchivo);
                            LogueoLimpieza.Info("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [Se movio el archivo " + nombreArchivo + " a la pepelera " + urlPapelera + "]");
                        }
                    }
                }

                LogueoLimpieza.Info("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [Inicia metodo EliminarArchivosPapelera]");
                EliminarArchivosPapelera();
                LogueoLimpieza.Info("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [Termina metodo EliminarArchivosPapelera]");

            }
            catch (Exception ex)
            {
                LogueoLimpieza.Error("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
            }
        }

        public static void EliminarArchivosPapelera()
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString(); 

            try
            {
                List<Archivo> listaArchivos = DAOArchivo.ObtenerArchivosConfigurados();
                string nombreArchivo = "";
                DateTime fechaCambio = DateTime.Now;
                int diasPapelera = 0;
                string urlPapelera = "";

                foreach (Archivo archivo in listaArchivos)
                {
                    DirectoryInfo directory = new DirectoryInfo(archivo.URLPapelera);
                    FileInfo[] files = directory.GetFiles("*.*");
                    for (int i = 0; i < files.Length; i++)
                    {
                        nombreArchivo = files[i].ToString();
                        fechaCambio = files[i].CreationTime;
                        diasPapelera = archivo.DiasPapelera;
                        urlPapelera = archivo.URLPapelera;

                        DateTime fechaDisponible = DateTime.Now.AddDays(- diasPapelera);

                        if (fechaDisponible >= fechaCambio)
                        {
                            File.Delete(urlPapelera + "\\" + nombreArchivo);
                            LogueoLimpieza.Info("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [Se elimino el archivo " + nombreArchivo + " de la pepelera]");
                        }
                    }
                }               
            }
            catch (Exception ex)
            {
                LogueoLimpieza.Error("[" + ip + "] [Limpieza] [PROCESADORNOCTURNO] [" + log + "] [" + ex.Message + "]");
            }
        }
    }
}
