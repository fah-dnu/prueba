using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using System.IO;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using Dnu_ProcesadorCaCaoLog.Entidades;
using Dnu_ProcesadorCaCaoLog.BaseDatos;
using System.Text.RegularExpressions;

namespace Dnu_ProcesadorCaCaoLog.LogicaNegocio
{
    class LNArchivo
    {
        delegate void EnviarDetalle(String elDetalle);
        public static Dictionary<String, IAsyncResult> ThreadsUsuarios = new Dictionary<string, IAsyncResult>();


        public static Boolean ProcesaArch(Archivo unArchivo, String path)
        {
            try
            {
                Int64 ID_Fichero = 0;

                ObtieneRegistrosDeArchivo(path, ref unArchivo);



                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    conn.Open();

                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                    {
                        try
                        {

                            //GUARDA EL FICHERO
                            ID_Fichero = DAOArchivo.GuardarFicheroEnBD(unArchivo, conn, transaccionSQL);

                            if (ID_Fichero == 0)
                            {
                                transaccionSQL.Rollback();
                                Logueo.Error("ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO");
                                throw new Exception("ProcesaArch(): NO SE PUDO OBTENER UN IDENTIFICADOR DE FICHERO VALIDO");
                            }


                            ////Guarda los detalles
                            //Boolean guardoLosDetalles = DAOArchivo.GuardarFicheroDetallesEnBD(unArchivo, ID_Fichero, conn, transaccionSQL);

                            //if (!guardoLosDetalles)
                            //{
                            //    transaccionSQL.Rollback();
                            //    Logueo.Error("ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO");
                            //    throw new Exception("ProcesaArch(): NO SE PUDO INSERTAR LOS DETALLES DEL FICHERO VALIDO");
                            //}

                            transaccionSQL.Commit();
                        }
                        catch (Exception err)
                        {
                            Logueo.Error("ProcesaArch(): ERROR AL ALMACENAR EL FICHERO: " + err.Message);
                            transaccionSQL.Rollback();
                        }
                    }
                }

                //Ejecuta los Eventos de cada fila del Archivo.
                DatosBaseDatos laConfigForConmparacion = DAODatosBase.ObtenerConsulta(unArchivo.ID_Archivo);


                if (ID_Fichero == 0)
                {
                    throw new Exception("NO SE PUDO OBTENER UN ID_FICHERO VALIDO");
                }

                //Insertar los datos obtenidos en la DB de Archivos
                int OperacionesBD = DAODatosBase.InsertaDatosEnTabla(laConfigForConmparacion, ID_Fichero, unArchivo);

               

                return true;

            }
            catch (Exception err)
            {
                Logueo.Error("getUsuarios(): " + err.Message);
                return false;
            }
        }


        internal static string LimpiarArchivo(String elArchivo, string path)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(elArchivo, Encoding.Default))
            {
                String IP = String.Empty;
                String CLIENT = String.Empty;
                String DATE = String.Empty;
                String REQUEST = String.Empty;
                String REQUESTVERSION = String.Empty;

                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    var firstSeparator = line.IndexOf('-');
                    IP = line.Substring(0, firstSeparator).Trim();

                    var secondSeparator = line.IndexOf('[');
                    CLIENT = line.Substring(firstSeparator+1,(secondSeparator - firstSeparator)-1).Trim();

                    var tirthSeparator = line.IndexOf(']');
                    DATE = line.Substring(secondSeparator + 1, (tirthSeparator - secondSeparator)-1).Trim();

                    var fourthSeparator = line.IndexOf("\"");
                    var fifthSeparator = line.IndexOf("\"-\"");
                    REQUEST = line.Substring(fourthSeparator + 1, (fifthSeparator - fourthSeparator)-1).Trim();

                    var lengh = (line.Length - fifthSeparator) - 3;
                    var sta = fifthSeparator + 4;
                    REQUESTVERSION = line.Substring(sta).Replace('\"',' ').Trim();

                    sb.AppendLine(String.Format("|{0}|{1}|{2}|{3}|{4}", IP, CLIENT, DATE, REQUEST, REQUESTVERSION));
                    
                }
            }
            File.Delete(elArchivo);
            using (StreamWriter sw = File.CreateText(elArchivo))
            {
                sw.WriteLine(sb.ToString());
            }

            return elArchivo;
        }

        public static void ObtieneRegistrosDeArchivo(String elPath, ref Archivo elArchivo)
        {
            StreamReader objReader = new StreamReader(elPath);
            string sLine = "";
            //List<Fila> losDetalles = new List<Fila>();
            int noFilaDeARchivo = 0;
            String UltimaFilaLeida = "";

            try
            {
                while (sLine != null)
                {
                    sLine = objReader.ReadLine();

                    if (sLine == null)
                    {
                        break;
                    }

                    elArchivo.LosDatos.Add(DecodificaFila(sLine, elArchivo.laConfiguracionDetalleLectura));

                    noFilaDeARchivo++; //INCREMENTA LA FILA LEIDA

                    UltimaFilaLeida = sLine;
                }

                elArchivo.Nombre = elPath;

            }
            catch (Exception err)
            {
                Logueo.Error("ObtieneRegistrosDeArchivo()" + err.Message + " " + err.StackTrace + ", " + err.Source);
            }
            finally
            {
                objReader.Close();
            }





            return;
        }

        public static Fila DecodificaFila(String Cadena, FilaConfig laConfiguracionDelaFila)
        {
            Fila laFila = new Fila();
            try
            {
                laFila.laConfigDeFila = laConfiguracionDelaFila;
                laFila.DetalleCrudo = Cadena;

                //decodifica el Header.
                if (laConfiguracionDelaFila.PorSeparador)
                {
                    string[] losDato = Cadena.Split(laConfiguracionDelaFila.CaracterSeparacion.ToCharArray());

                    for (int k = 0; k < losDato.Length; k++)
                    {
                        laFila.losCampos.Add(k, losDato[k]);
                    }

                }
                else if (laConfiguracionDelaFila.PorLongitud)
                {

                    Int32 elNumeroCampo = 1;

                    while (Cadena.Length != 0)
                    {
                        try
                        {
                            CampoConfig unaConfig = laConfiguracionDelaFila.LosCampos[elNumeroCampo];

                            laFila.losCampos.Add(elNumeroCampo, Cadena.Substring(0, unaConfig.Longitud));

                            Cadena = Cadena.Substring(unaConfig.Longitud);

                            elNumeroCampo++;
                        }
                        catch (Exception err)
                        {
                            Logueo.Error("DecodificaFila(): Campo: " + elNumeroCampo + ", " + err.Message + " " + err.StackTrace + ", " + err.Source);
                            throw new Exception("DecodificaFila(): Campo: " + elNumeroCampo + ", " + err.Message + " " + err.StackTrace + ", " + err.Source);
                        }
                    }

                }


            }
            catch (Exception err)
            {
                Logueo.Error("DecodificaFila()" + err.Message + " " + err.StackTrace + ", " + err.Source);
            }

            return laFila;
        }

    }
}
