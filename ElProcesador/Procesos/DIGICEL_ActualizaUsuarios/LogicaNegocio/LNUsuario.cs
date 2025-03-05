using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIGICEL_ActualizaUsuarios.Entidades;
using System.IO;
using CommonProcesador;
using System.Threading;

namespace DIGICEL_ActualizaUsuarios.LogicaNegocio
{
    class LNUsuario
    {

        public static Boolean ModificarelUsuario(Usuario elUsusario)
        {
            try
            {


                return true;

            }
            catch (Exception err)
            {
                return false;
            }
        }

        public static void EscucharDirectorio()
        {
            try
            {
                Logueo.Evento("Inicia la escucha de la carpeta: " + PNConfig.Get("DGUSERS", "Directorio") + " en espera de archivos.");

                FileSystemWatcher elObservador = new FileSystemWatcher(PNConfig.Get("DGUSERS", "Directorio"));
                elObservador.NotifyFilter = (NotifyFilters.LastAccess | NotifyFilters.LastWrite |NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);

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
                //WatcherChangeTypes elTipoCambio = el.ChangeType;

                WatcherChangeTypes elTipoCambio = el.ChangeType;
                //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
                Logueo.Evento("Hubo un Cambio [" + elTipoCambio.ToString() + "] en el directorio: " + PNConfig.Get("DGUSERS", "Directorio") + " el se recibio el archivo : " + el.FullPath);
                Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                Thread.Sleep(1000 * 60 * 10);


                Logueo.Evento("INICIO DE PROCESO DEL ARCHIVO:" + el.FullPath);
                

                Logueo.Evento("Hubo un Cambio [" + el.ChangeType.ToString() + "] en el directorio: " + PNConfig.Get("DGUSERS", "Directorio") + " el se recibio el archivo : " + el.FullPath);

                string filename1="";
                String elpathFile="";
                try
                {
                    Logueo.Evento("Hubo 1");

                     elpathFile = el.FullPath;
                     el = null;
                     Logueo.Evento("Hubo 2");
                    //cambia el nombre del archivo para evitar que el proceso automatico y el proceso programadado choquen
                     Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                    //string extension = Path.GetExtension(e.FullPath);
                     Logueo.Evento("Hubo 3");
                     filename1 = Path.GetFileName(elpathFile);
                    // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                     Logueo.Evento("Hubo 4");
                    string root1 = Path.GetDirectoryName(elpathFile);
                    Logueo.Evento("Hubo 5");
                    File.Move(elpathFile, root1 + "\\EN_PROCESO_" + filename1);
                    Logueo.Evento("Hubo 5");
                    elpathFile = root1 + "\\EN_PROCESO_" + filename1;
                }
                catch (Exception err)
                {
                    Logueo.Error("alCambiar()1: " + err.Message + ",  " + err.ToString());
                }

                //Abrir el Archivo
                //Crear los objetos Usuario por cada registro del 
                //iniicar el ciclo de actualizacion.
                Boolean resp = LNArchivo.ProcesaUsuarios(Path.GetFileName(elpathFile).Substring(0, 3), elpathFile);

               //RENOMBRAR el ARCHIVO
                if (resp)
                {
                    Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                    //string extension = Path.GetExtension(e.FullPath);
                    string filename = Path.GetFileName(elpathFile);
                    // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                    string root = Path.GetDirectoryName(elpathFile);
                    File.Move(elpathFile, root + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", ""));
                    File.Move(elpathFile, root + "\\PROCESADO_" + filename.Replace("DIGICEL_", "")); 
                }

               
            }
            catch (Exception err)
            {
                Logueo.Error("alCambiar()2: " + err.Message + ",  " + err.ToString());
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
