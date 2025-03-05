using CommonProcesador;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.LogicaNegocio
{
    class LNOperacionesArchivos
    {

        internal static void moverArchivo(string pathOrigen, string pathDestino)
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


        private static bool renombrarArchivo(string pathOrigen, string pathDestino)
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

        public static void crearDirectorio(string directorioEntrada,string directorioSalida)
        {//

            if (!Directory.Exists(directorioEntrada))
                Directory.CreateDirectory(directorioEntrada);
            if (!Directory.Exists(directorioSalida))
                Directory.CreateDirectory(directorioSalida);
            if (!Directory.Exists(directorioEntrada + "\\Procesados"))
                Directory.CreateDirectory(directorioEntrada + "\\Procesados");
            if (!Directory.Exists(directorioEntrada + "\\Erroneos"))
                Directory.CreateDirectory(directorioEntrada + "\\Erroneos");
            //if (!Directory.Exists(directorioEntrada + "\\Correctos"))
            //    Directory.CreateDirectory(directorioEntrada + "\\Correctos");
        }



    }
}
