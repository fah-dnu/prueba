using DNU_Embozador.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.BaseDatos
{
    public class DAOAutorizador
    {

        public static List<EMBOZO_Parametros> ObtenerParametros(string instancia, string tipo)
        {


            List<EMBOZO_Parametros> lstEMBOZO_Parametros = new List<EMBOZO_Parametros>();

            try
            {
                using (SqlConnection con = new SqlConnection(BDAutorizador.strBDLectura))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "EMBOZO_ObtenerParametrosByInstancia";
                        cmd.Parameters.Add(new SqlParameter("@instancia", instancia));
                        cmd.Parameters.Add(new SqlParameter("@parameterTypeKey", tipo));

                        con.Open();

                        var read = cmd.ExecuteReader();

                        while (read.Read())
                        {
                            Enum.TryParse(read["origin"].ToString(), out ORIGIN origin);

                            lstEMBOZO_Parametros.Add(new EMBOZO_Parametros
                            {
                                content = read["content"].ToString(),
                                final_index = Convert.ToInt32(read["final_index"]),
                                form = read["form"].ToString(),
                                formato = read["formato"].ToString(),
                                f_key = read["f_key"].ToString(),
                                ID_EMBOZO_Instancia = Convert.ToInt32(read["ID_EMBOZO_Instancia"]),
                                ID_EMBOZO_ParameterType = Convert.ToInt32(read["ID_EMBOZO_ParameterType"]),
                                ID_EMBOZO_Parametros = Convert.ToInt32(read["ID_EMBOZO_Parametros"]),
                                initial_index = Convert.ToInt32(read["initial_index"]),
                                longitud = Convert.ToInt32(read["long"]),
                                origin = origin
                            });
                        }



                    }
                }

                return lstEMBOZO_Parametros;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw ex;
            }
        }

        internal static void EjecutarOrigenDatosText(EMBOZO_OrigenDatos item, ref Dictionary<string, Parametro> lstParametrosValores)
        {
            throw new NotImplementedException();
        }

        internal static void EjecutarOrigenDatosSP(EMBOZO_OrigenDatos p_EMBOZO_OrigenDatos,
            Dictionary<string, string> sqlParam,
            ref List<Dictionary<string, Parametro>> lstParametrosValores)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(BDAutorizador.ObtenerConeccion(p_EMBOZO_OrigenDatos.q_connStringConfigurationKey)))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = p_EMBOZO_OrigenDatos.q_text;
                        cmd.Parameters.Add(new SqlParameter("@daysback", sqlParam["daysback"]));
                        cmd.Parameters.Add(new SqlParameter("@clavesEstatus", sqlParam["clavesEstatus"]));
                        cmd.Parameters.Add(new SqlParameter("@instanciaEmbozador", sqlParam["instanciaEmbozador"]));
                        cmd.Parameters.Add(new SqlParameter("@ID_Colectiva", sqlParam["ID_Colectiva"]));

                        cmd.CommandTimeout = p_EMBOZO_OrigenDatos.q_timeOut;

                        con.Open();

                        var read = cmd.ExecuteReader();
                        var columns = new List<string>();

                        for (int i = 0; i < read.FieldCount; i++)
                        {
                            columns.Add(read.GetName(i));
                        }

                        while (read.Read())
                        {
                            Dictionary<string, Parametro> dParametrosValores = new Dictionary<string, Parametro>();

                            foreach (var item in columns)
                            {

                                if (dParametrosValores.ContainsKey(item))
                                {
                                    dParametrosValores[item] = new Parametro
                                    {
                                        key = item,
                                        value = read[item].ToString()
                                    };
                                }
                                else
                                {
                                    dParametrosValores.Add(item, new Parametro
                                    {
                                        key = item,
                                        value = read[item].ToString()
                                    });
                                }
                            }

                            lstParametrosValores.Add(dParametrosValores);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw ex;
            }
        }

        internal static void ActualizaGeneracionArchivo(string archivoNombre, string instancia, EstatusGeneracionArchivo estatus)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(BDAutorizador.strBDEscritura))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "EMBOZO_ActualizaGeneracionArchivo";
                        cmd.Parameters.Add(new SqlParameter("@ID_Instancia", instancia));
                        cmd.Parameters.Add(new SqlParameter("@claveEstatus", estatus.ToString()));
                        cmd.Parameters.Add(new SqlParameter("@nombreArchivo", archivoNombre));

                        SqlParameter cod = new SqlParameter("@CodigoRespuesta", SqlDbType.VarChar, 5);
                        cod.Direction = ParameterDirection.Output;


                        SqlParameter descResp = new SqlParameter("@DescRespuesta", SqlDbType.VarChar, 500);
                        descResp.Direction = ParameterDirection.Output;

                        cmd.Parameters.Add(cod);
                        cmd.Parameters.Add(descResp);

                        con.Open();

                        cmd.ExecuteNonQuery();

                        var codExecucion = cmd.Parameters["@CodigoRespuesta"].Value;
                        var descExecucion = cmd.Parameters["@DescRespuesta"].Value;

                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw ex;
            }
        }

        internal static void ActualizaProceoMediosAccesoGenerados(List<Dictionary<string, Parametro>> lstParametrosValores, string fileName, string instancia)
        {
            try
            {
                foreach (var item in lstParametrosValores)
                {
                    using (SqlConnection con = new SqlConnection(BDAutorizador.strBDEscritura))
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = con;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "EMBOZO_ActualizaMediosAccesoProcesados";
                            cmd.Parameters.Add(new SqlParameter("@ID_MA", item["ID_MA"].value));
                            cmd.Parameters.Add(new SqlParameter("@ID_Instancia", instancia));
                            cmd.Parameters.Add(new SqlParameter("@nombreArchivo", fileName));

                            if (Constants.VERSION.Equals("3") || Constants.VERSION.Equals("4"))
                            {
                                DbParameter paramSSN = cmd.CreateParameter();
                                paramSSN.ParameterName = "@pinblock";
                                paramSSN.DbType = DbType.AnsiStringFixedLength;
                                paramSSN.Direction = ParameterDirection.Input;
                                paramSSN.Value = item["PINBLOCKBD"].value;
                                paramSSN.Size = 50;
                                cmd.Parameters.Add(paramSSN);
                            }

                            SqlParameter cod = new SqlParameter("@CodigoRespuesta", SqlDbType.VarChar, 5);
                            cod.Direction = ParameterDirection.Output;


                            SqlParameter descResp = new SqlParameter("@DescRespuesta", SqlDbType.VarChar, 500);
                            descResp.Direction = ParameterDirection.Output;

                            cmd.Parameters.Add(cod);
                            cmd.Parameters.Add(descResp);

                            con.Open();

                            cmd.ExecuteNonQuery();

                            var codExecucion = cmd.Parameters["@CodigoRespuesta"].Value;
                            var descExecucion = cmd.Parameters["@DescRespuesta"].Value;

                            if (!codExecucion.Equals("0000"))
                                throw new Exception("Error en Actualizacion de MA embozado " + descExecucion);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error durante la insercion de MA's procesados en archivo de embozo " + fileName + " - " + ex.Message);
                throw ex;
            }
        }

        internal static IEnumerable<EMBOZO_OrigenDatos> ObtenerOrigenDatos()
        {
            List<EMBOZO_OrigenDatos> lstEMBOZO_OrigenDatos = new List<EMBOZO_OrigenDatos>();

            try
            {
                using (SqlConnection con = new SqlConnection(BDAutorizador.strBDLectura))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "EMBOZO_ObtenerOrigenDatos";

                        con.Open();

                        var read = cmd.ExecuteReader();

                        while (read.Read())
                        {
                            //Enum.TryParse(read["origin"].ToString(), out ORIGIN origin);

                            lstEMBOZO_OrigenDatos.Add(new EMBOZO_OrigenDatos
                            {
                                ID_EMBOZO_OrigenDatos = Convert.ToInt32(read["ID_EMBOZO_OrigenDatos"]),
                                ID_EMBOZO_Instancia = read["ID_EMBOZO_Instancia"].ToString(),
                                q_connStringConfigurationKey = read["q_connStringConfigurationKey"].ToString(),
                                q_text = read["q_text"].ToString(),
                                q_timeOut = Convert.ToInt32(read["q_timeOut"]),
                                q_type = read["q_type"].ToString(),
                                ID_Colectiva = read["ID_Colectiva"].ToString(),
                                GeneraNIP = read["GeneraNIP"].ToString(),
                                ObtieneDireccionEntregaAuto = read["ObtieneDireccionEntregaAutorizador"].ToString(),
                            });
                        }



                    }
                }

                return lstEMBOZO_OrigenDatos;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw ex;
            }
        }

        public static List<EMBOZO_Parametros> ObtenerParame(string instancia, string tipo)
        {


            List<EMBOZO_Parametros> lstEMBOZO_Parametros = new List<EMBOZO_Parametros>();

            try
            {
                using (SqlConnection con = new SqlConnection(BDAutorizador.strBDLectura))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;
                        cmd.CommandText = "EMBOZO_ObtenerParametrosByInstancia";
                        cmd.Parameters.Add(new SqlParameter("@instancia", instancia));
                        cmd.Parameters.Add(new SqlParameter("@parameterTypeKey", tipo));

                        con.Open();

                        var read = cmd.ExecuteReader();

                        while (read.Read())
                        {
                            Enum.TryParse(read["origin"].ToString(), out ORIGIN origin);

                            lstEMBOZO_Parametros.Add(new EMBOZO_Parametros
                            {
                                content = read["content"].ToString(),
                                final_index = Convert.ToInt32(read["final_index"]),
                                form = read["form"].ToString(),
                                formato = read["formato"].ToString(),
                                f_key = read["f_key"].ToString(),
                                ID_EMBOZO_Instancia = Convert.ToInt32(read["ID_EMBOZO_Instancia"]),
                                ID_EMBOZO_ParameterType = Convert.ToInt32(read["ID_EMBOZO_ParameterType"]),
                                ID_EMBOZO_Parametros = Convert.ToInt32(read["ID_EMBOZO_Parametros"]),
                                initial_index = Convert.ToInt32(read["initial_index"]),
                                longitud = Convert.ToInt32(read["longitud"]),
                                origin = origin
                            });
                        }



                    }
                }

                return lstEMBOZO_Parametros;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw ex;
            }
        }


        internal static void EjecutarOrigenDatosDireccionEntrega(string conn, string userID, Dictionary<string, Parametro> item)
        {
            DireccionEntrega dirEntrega = new DireccionEntrega();
            int contador = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(conn))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = con;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "EMBOZO_ObtenerDireccionEntrega";
                        cmd.Parameters.Add(new SqlParameter("@userID", userID));

                        con.Open();

                        var read = cmd.ExecuteReader();
                        var columns = new List<string>();

                        for (int i = 0; i < read.FieldCount; i++)
                        {
                            columns.Add(read.GetName(i));
                        }

                        while (read.Read())
                        {
                            contador = 1;
                            foreach (var col in columns)
                            {
                                item.Add(col, new Parametro
                                {
                                    key = col,
                                    value = read[col].ToString()
                                });

                            }
                        }

                        if (contador == 0)
                        {
                            throw new Exception("El usuario ID " + userID + " no tiene dirección de entrega asociada");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw ex;
            }
        }
    }
}
