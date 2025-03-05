using CommonProcesador;
using Dnu_ProcesadorCaCaoLog.BaseDatos;
using Dnu_ProcesadorCaCaoLog.Entidades;
using Dnu_ProcesadorCaCaoLog.LogicaNegocio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dnu_ProcesadorCacaoLog
{
#if !DEBUG
    public class ProcesarCacaoLog : IProcesoNocturno
#endif
#if DEBUG
    public class ProcesarCacaoLog
#endif
    {
        public bool Procesar()
        {

            try
            {
                int resp;

                resp = 0;

                try
                {
                    ConfiguracionContexto.InicializarContexto();
                    var config = PNConfig.Get("PROCCACOLOG", "Directorio");
                    DirectoryInfo directory = new DirectoryInfo(config);



                    List<Archivo> losArchivos = DAOArchivo.ObtenerArchivosConfigurados();

                    losArchivos = losArchivos.Where(m => m.Nombre.Contains("LogCACAO")).ToList();
                    //OBTENER TODOS LOS ARCHIVOS CONFIGURADOS EN LA BASE DE DATOS PARA OBTENER LOS PREFIJOS.

                    foreach (Archivo elArchivo in losArchivos)
                    {
                        FileInfo[] files = directory.GetFiles(elArchivo.Prefijo + "*.*");

                        for (int i = 0; i < files.Length; i++)
                        {

                            String elpathFile = (((FileInfo)files[i]).FullName);
                            //elArchivo.UrlArchivo = elpathFile;

                            //cambia el nombre del archivo para evitar que el proceso automatico y el proceso programadado choquen
                            Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                            string filename1 = Path.GetFileName(elpathFile);
                            string root1 = Path.GetDirectoryName(elpathFile);

                            //Limpiar archivo
                            var tmpNameFile =
                                LNArchivo.LimpiarArchivo(((FileInfo)files[i]).FullName, directory.FullName);

                            File.Move(tmpNameFile, elpathFile);

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

        public void Iniciar()
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

        public void Detener()
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
