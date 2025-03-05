using CommonProcesador;
using DNU_VencimientosParabilia.Entidades;
using DNU_VencimientosParabilia.Utilidades;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;


namespace DNU_VencimientosParabilia.BaseDatos
{
    public class DAODatosBase
    {

        public static IEnumerable<ColectivaContrato> obtenerColectivas(String connectionString)
        {
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            List<ColectivaContrato> ieColectivaContrato = new List<ColectivaContrato>();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP PROCNOC_Parabilia_ObtenerDatosColectivasContrato]");
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = connection;
                        cmd.CommandText = "PROCNOC_Parabilia_ObtenerDatosColectivasContrato";


                        connection.Open();
                        SqlDataReader dr = cmd.ExecuteReader();

                        var tmpColectiva = String.Empty;
                        var tmpIdColectiva = String.Empty;
                        List<ValorContrato> lstValoresContrato = new List<ValorContrato>();

                        while (dr.Read())
                        {
                            if (tmpColectiva != dr["ClaveColectiva"].ToString())
                            {
                                if (String.IsNullOrEmpty(tmpColectiva))
                                {
                                    tmpColectiva = dr["ClaveColectiva"].ToString();
                                    tmpIdColectiva = dr["ID_Colectiva"].ToString();

                                    lstValoresContrato.Add(new ValorContrato
                                    {
                                        Nombre = dr["Nombre"].ToString(),
                                        Valor = dr["Valor"].ToString()
                                    });
                                }
                                else
                                {
                                    ieColectivaContrato.Add(new ColectivaContrato
                                    {
                                        ID_Colevtiva = tmpIdColectiva,
                                        ClaveColectiva = tmpColectiva,
                                        lstValoresContrato = lstValoresContrato
                                    });

                                    tmpColectiva = dr["ClaveColectiva"].ToString();
                                    tmpIdColectiva = dr["ID_Colectiva"].ToString();

                                    lstValoresContrato = new List<ValorContrato>();

                                    lstValoresContrato.Add(new ValorContrato
                                    {
                                        Nombre = dr["Nombre"].ToString(),
                                        Valor = dr["Valor"].ToString()
                                    });
                                }
                            }
                            else
                            {
                                lstValoresContrato.Add(new ValorContrato
                                {
                                    Nombre = dr["Nombre"].ToString(),
                                    Valor = dr["Valor"].ToString()
                                });
                            }
                        }

                        ieColectivaContrato.Add(new ColectivaContrato
                        {
                            ID_Colevtiva = tmpIdColectiva,
                            ClaveColectiva = tmpColectiva,
                            lstValoresContrato = lstValoresContrato
                        });

                    }
                }
            }
            catch (Exception ex)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [DAODatosBase, obtenerColectivas, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }

            return ieColectivaContrato;
        }

        internal static bool ValidaOperacionSaldosInternos(string strBDLectura, Movimiento mov)
        {
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            Boolean saldosInternos = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(strBDLectura))
                {
                    LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP PROCNOC_Parabilia_ValidaOperacionSaldosInternos" + mov.ClaveMA + " ]");
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = connection;
                        cmd.CommandText = "PROCNOC_Parabilia_ValidaOperacionSaldosInternos";
                        //cmd.Parameters.Add(new SqlParameter("@ClaveMA", mov.ClaveMA));
                        SqlParameter paramSSN = cmd.CreateParameter();
                        paramSSN.ParameterName = "@ClaveMA";
                        paramSSN.DbType = DbType.AnsiStringFixedLength;
                        paramSSN.Direction = ParameterDirection.Input;
                        paramSSN.Value = mov.ClaveMA;
                        paramSSN.Size = mov.ClaveMA.Length;//50;
                        cmd.Parameters.Add(paramSSN);

                        var codResultado = new SqlParameter("@Resultado", SqlDbType.VarChar, 5);
                        codResultado.Value = "";
                        codResultado.Direction = ParameterDirection.Output;

                        cmd.Parameters.Add(codResultado);

                        connection.Open();

                        cmd.ExecuteNonQuery();
                        saldosInternos = cmd.Parameters["@Resultado"].Value.ToString().Equals("1");
                    }
                }
            }
            catch (Exception ex)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [DAODatosBase, obtieneMovimientosAVencer, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + " ]");
                throw ex;
            }

            return saldosInternos;
        }

        internal static DataTable obtieneMovimientosAVencer(string strBDLectura)
        {
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            DataTable dtMovimientosAVencer = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(strBDLectura))
                {
                    LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP PROCNOC_Parabilia_ObtieneMovimientosAVencer]");

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = connection;
                        cmd.CommandText = "PROCNOC_Parabilia_ObtieneMovimientosAVencer";

                        connection.Open();

                        SqlDataReader read = cmd.ExecuteReader();
                        dtMovimientosAVencer.Load(read);
                    }
                }
            }
            catch (Exception ex)
            {
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [DAODatosBase, obtieneMovimientosAVencer, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + " ]");
            }

            return dtMovimientosAVencer;
        }

        internal static Resultado actualizarMovimientosAVencerPorColectiva(string connectionString,
            ColectivaContrato poColectiva)
        {
            string log = "";// ThreadContext.Properties["log"].ToString();
            string ip = "";// ThreadContext.Properties["ip"].ToString();
            DataTable dtMovimientosAVencer = new DataTable();
            Resultado res = new Resultado();
            try
            {

                String slNacionales = "15";
                String slInterNacionales = "15";

                if (poColectiva.lstValoresContrato.Where(w => w.Nombre.Equals("@VC_Nacionales")).Any())
                {
                    slNacionales = poColectiva.lstValoresContrato.Where(w => w.Nombre.Equals("@VC_Nacionales")).FirstOrDefault().Valor;
                }

                if (poColectiva.lstValoresContrato.Where(w => w.Nombre.Equals("@VC_Internacionales")).Any())
                {
                    slInterNacionales = poColectiva.lstValoresContrato.Where(w => w.Nombre.Equals("@VC_Internacionales")).FirstOrDefault().Valor;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    LogueoProcesaVencParabilia.Info("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP PROCNOC_Parabilia_ActualizarMovimientosAVencerPorColectiva" + poColectiva.ClaveColectiva + "," + slNacionales + "," + slInterNacionales + " ]");

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = connection;
                        cmd.CommandText = "PROCNOC_Parabilia_ActualizarMovimientosAVencerPorColectiva";
                        cmd.Parameters.Add(new SqlParameter("@ClaveColectiva", poColectiva.ClaveColectiva));
                        cmd.Parameters.Add(new SqlParameter("@VC_Nacionales", slNacionales));
                        cmd.Parameters.Add(new SqlParameter("@VC_Internacionales", slInterNacionales));

                        var codResultado = new SqlParameter("@Codigo", SqlDbType.VarChar, 5);
                        codResultado.Value = "";
                        codResultado.Direction = ParameterDirection.Output;

                        cmd.Parameters.Add(codResultado);

                        var mensajeResultado = new SqlParameter("@Mensaje", SqlDbType.VarChar, 200);
                        mensajeResultado.Value = "";
                        mensajeResultado.Direction = ParameterDirection.Output;

                        cmd.Parameters.Add(mensajeResultado);



                        connection.Open();
                        cmd.ExecuteNonQuery();
                        res.Codigo = cmd.Parameters["@Codigo"].Value.ToString();
                        res.Mensaje = cmd.Parameters["@Mensaje"].Value.ToString();

                    }
                }
            }
            catch (Exception ex)
            {
                res.Codigo = "99";
                res.Mensaje = ex.Message;
                LogueoProcesaVencParabilia.Error("[" + ip + "] [ProcesarVencimientos] [PROCESADORNOCTURNO] [" + log + "] [DAODatosBase, obtenerMovimientosAVencerPorColectiva, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + " ]");
            }

            return res;
        }
    }
}
