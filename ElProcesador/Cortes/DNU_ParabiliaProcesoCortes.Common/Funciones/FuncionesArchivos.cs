using CommonProcesador;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Common.Funciones
{
    public class FuncionesArchivos
    {

        public static void renombraArchivo(string elpathFile, bool ejecucionCorrecta, StringBuilder sb, string log, string ip,string directorioSalida)
        {
            if (ejecucionCorrecta)
            {
                Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [" + log + "] [Iniciar el renombramiento del Archivo procesado: " + elpathFile + "]");
                string filename = Path.GetFileName(elpathFile);
                //string root = Path.GetDirectoryName(elpathFile);
                string finalFileName = directorioSalida + "\\Correctos\\" + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", "");



                File.Delete(elpathFile);
                if (File.Exists(finalFileName))
                {
                    var tempName = Path.GetFileName(finalFileName);
                    //#if DEBUG
                    //                    var directorio = PNConfig.Get("PRALTATARJPARABILIA", "DirectorioArchivosParabilia");
                    //#else

                    var directorio = PNConfig.Get("PRALTATARJPARABILIA", "DirectorioEntrada");
                    //#endif
                    DirectoryInfo directory = new DirectoryInfo(directorio);
                    var consecutivo = directory.GetFiles().Where(w => w.Name.Contains(tempName.Replace("PROCESADO_", ""))).Count() + 1;
                    finalFileName = finalFileName.Replace("PROCESADO_", string.Format("PROCESADO_{0}_{1}_", consecutivo, DateTime.Now.Hour));
                }

                using (StreamWriter sw = File.CreateText(finalFileName))
                {
                    sw.WriteLine(sb.ToString());
                }
            }
            else
            {
                Logueo.Evento("[ProcesarAltaTarjetas] [PROCESADORNOCTURNO] [" + log + "] [Iniciar el renombramiento del Archivo procesado con error: " + elpathFile + "]");
                string filename = Path.GetFileName(elpathFile);
                //string root = Path.GetDirectoryName(elpathFile);
                string finalFileName = directorioSalida + "\\Erroneos\\" + "\\PROCESADO_CON_ERROR_" + filename.Replace("EN_PROCESO_", "");
                File.Delete(elpathFile);

                if (File.Exists(finalFileName))
                {
                    var tempName = Path.GetFileName(finalFileName);

                    //#if DEBUG
                    //                    var directorio = PNConfig.Get("PRALTATARJPARABILIA", "DirectorioArchivosParabilia");
                    //#else

                    var directorio = PNConfig.Get("PRALTATARJPARABILIA", "DirectorioEntrada");
                    //#endif
                    DirectoryInfo directory = new DirectoryInfo(directorio);
                    var consecutivo = directory.GetFiles().Where(w => w.FullName.Contains(tempName.Replace("PROCESADO_CON_ERROR_", ""))).Count() + 1;
                    finalFileName = finalFileName.Replace("PROCESADO_CON_ERROR_", string.Format("PROCESADO_CON_ERROR_{0}_{1}_", consecutivo, DateTime.Now.Hour));
                }


                using (StreamWriter sw = File.CreateText(finalFileName))
                {
                    sw.WriteLine(sb.ToString());
                }
            }

        }

        public static void renombraArchivoPorError(string path, dynamic lstArchivo,
           string message, string log, string ip,string directorioSalida)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                "Emisor".PadRight(9),
                "Cuenta".PadRight(16),
                "Número Tarjeta".PadRight(29),
                "Tarjeta".PadRight(24),
                "Vencimiento".PadRight(15),
                "Clabe".PadRight(18),
                "Código Resultado".PadRight(20),
                "Mensaje Resultado"));

            sb.AppendLine("------------------------------------------------------------------------------------------------------------------------------------");
            foreach (var item in lstArchivo)
            {
                sb.AppendLine(String.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                    item.Emisor.PadRight(9),
                    item.Cuenta.PadRight(16),
                    item.NumeroTarjeta.PadRight(29),
                    item.Tarjeta.PadRight(24),
                    item.Vencimiento.PadRight(15),
                    "99".PadRight(18),
                    "99".PadRight(20),
                    message));
            }

            renombraArchivo(path, false, sb, log, ip, directorioSalida);
        }

        internal void OcurrioError(object sender, ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        public static void moverArchivo(string pathOrigen, string pathDestino)
        {
            FileInfo archivo = new FileInfo(pathDestino);
            if (!Directory.Exists(archivo.DirectoryName))
                Directory.CreateDirectory(archivo.DirectoryName);

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

        internal static bool renombrarArchivo(string pathOrigen, string pathDestino)
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


    }
}
