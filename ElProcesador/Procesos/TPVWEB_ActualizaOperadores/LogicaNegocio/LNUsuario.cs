using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TPVWEB_ActualizaOperadores.Entidades;
using System.IO;
using CommonProcesador;
using System.Threading;

namespace TPVWEB_ActualizaOperadores.LogicaNegocio
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
                Logueo.Evento("Inicia la escucha de la carpeta: "+ PNConfig.Get("FILEMNTR", "Directorio") + " en espera de archivos.");

                FileSystemWatcher elObservador = new FileSystemWatcher(PNConfig.Get("FILEMNTR", "Directorio"));
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
                WatcherChangeTypes elTipoCambio = el.ChangeType;
                //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
                Logueo.Evento("Hubo un Cambio [" + elTipoCambio.ToString() + "] en el directorio: " + PNConfig.Get("FILEMNTR", "Directorio") + " el se recibio el archivo : " + el.FullPath);
                Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                Thread.Sleep(1000*60*1);


                Logueo.Evento("INICIO DE PROCESO DEL ARCHIVO:" + el.FullPath);
                

                String elpathFile = el.FullPath;
                //cambia el nombre del archivo para evitar que el proceso automatico y el proceso programadado choquen
                Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                //string extension = Path.GetExtension(e.FullPath);
                string filename1 = Path.GetFileName(elpathFile);
                // string filenameNoExtension = Path.GetFileNameWithoutExtension(e.FullPath);
                string root1 = Path.GetDirectoryName(elpathFile);
                File.Move(elpathFile, root1 + "\\EN_PROCESO_" + filename1);

                elpathFile = root1 + "\\EN_PROCESO_" + filename1;


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
