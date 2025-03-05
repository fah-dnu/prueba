using CommonProcesador;
using DNU_CompensacionT112_API_Cacao.Entidades;
using DNU_CompensacionT112Evertec.BaseDatos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensacionT112_API_Cacao.LogicaNegocio
{
    public static class LNValidacionArchivos
    {
        static List<string> prefixes = new List<string> { "MB112", "MI112", "MA112" };

        public static void ValidaArchivosT112(string fecha, SqlConnection conn)
        {


            List<ArchivoT112> ficheros = DAOT112.ObtieneArchivosT112PorCiclo(fecha, conn);

            if (ficheros.Count == 0)
            {
                Logueo.Error(String.Format("[FECHA : {0}] NO se encontraron archivos TT12 ", fecha));
            }

            for (int i = 0; i < 7; i++)
            {
                var ciclo = i + 1;


                foreach (var prefix in prefixes)
                {
                    var prefixComplete = String.Format("{0}{1}{2}", prefix, fecha.Replace("-", string.Empty), ciclo);

                    var ficheroT112 = ficheros.Where(w => w.NombreFichero.Contains(prefixComplete))
                        .FirstOrDefault();
                    

                    if (ficheroT112 == null)
                    {
                        DAOT112.InsertaEstatusFicheros(prefixComplete, fecha, null, 1, null, conn);
                        Logueo.Error(String.Format("[FECHA : {0}] Archivo T112 NO localizado {1}", fecha, prefixComplete));
                    }
                    else
                    {
                        var id_estatusprocesoApiCacao = 1;

                        switch (ficheroT112.ID_EstatusFichero)
                        {
                            case 2:
                            case 0:
                                id_estatusprocesoApiCacao = 5;
                                break;
                            case 1:
                                id_estatusprocesoApiCacao = 3;
                                break;

                            case 3:
                                id_estatusprocesoApiCacao = 2;
                                break;

                            default:
                                break;
                        }

                        DAOT112.InsertaEstatusFicheros(prefixComplete, fecha, ficheroT112.ID_Fichero, id_estatusprocesoApiCacao, ficheroT112.ID_EstatusFichero, conn);
                    }
                }
            }
        }


        internal static bool ValidaArchivosOperaciones(string strFechaProcesamiento, SqlConnection sqlConnection)
        {
            bool validacion = true;
            var fechaMes = strFechaProcesamiento.Replace("-",string.Empty).Substring(0, (strFechaProcesamiento.Length - 3)-1);

            List<ArchivoOperaciones> ficheros = 
                DAOOperacionesEvertec.ObtieneArchivosOperacionesPorMes(fechaMes, sqlConnection);

            if(ficheros.Count == 0)
            {
                Logueo.Error(String.Format("[FECHA : {0}] NO se encontraron archivos de Operaciones", fechaMes));
                validacion = false;
            }

            foreach (var item in ficheros)
            {
                //if(item.ID_Estatus == null)
                //{
                //    Logueo.Error(String.Format("[ARCHIVO : {0}] Archivo de Operaciones faltante de procesamiento", item.Nombre));
                //    validacion = false;
                //}

                switch (item.ID_Estatus)
                {
                    case "1":
                        Logueo.Error(String.Format("[ARCHIVO : {0}] Archivo de Operaciones correcto", item.Nombre));
                        break;
                    case "2":
                        Logueo.Error(String.Format("[ARCHIVO : {0}] Archivo de Operaciones en procesamiento", item.Nombre));
                        validacion = false;
                        break;
                    case "3":
                        Logueo.Error(String.Format("[ARCHIVO : {0}] Archivo de Operaciones procesado con error", item.Nombre));
                        validacion = false;
                        break;
                    default:
                        Logueo
                            .Error(String.Format("[ARCHIVO : {0}] Archivo de Operaciones con estatus de procesamiento desconocido {1}",
                                item.Nombre, item.ID_Estatus));
                        validacion = false;
                        break;
                }
            }

            return validacion;
        }
    }
}
