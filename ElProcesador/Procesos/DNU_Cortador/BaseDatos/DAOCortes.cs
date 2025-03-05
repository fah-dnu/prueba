using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
//using Executer.Utilidades;
using System.Data;
using Interfases.Enums;
using Interfases.Exceptions;
using Executer.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using Interfases.Entidades;
using CommonProcesador;
using DNU_Cortador.Entidades;

namespace DNU_Cortador.BaseDatos
{
    public class DAOCortes
    {
        public static void generarCorte(SqlConnection connOperacion, SqlTransaction transaccionSQL)
        {

    
            try
            {
                SqlParameter param;


                SqlCommand comando = new SqlCommand("ProcNoct_ejecutor_GeneraLotesCorteCuentasPorCortar", connOperacion);
                comando.CommandType = CommandType.StoredProcedure;
                comando.Transaction = transaccionSQL;
                comando.CommandTimeout = 0;

              
               comando.ExecuteNonQuery();

            }
            catch (SqlException e)
            {
                Logueo.Error("[EJECUTOR]generarCorte(): " + e.Message);
                throw e;
            }
            catch (Exception e)
            {
                Logueo.Error("[EJECUTOR]generarCorte(): " + e.Message);
                throw e;
            }
          

        }

        public static void Televia_generarCorte()
        {

           
            try
            {
                SqlParameter param;

                using (SqlConnection laCadena = BDCortes.BDLectura)
                {
                    laCadena.Open();

                    SqlCommand comando = new SqlCommand("ProcNoct_ejecutor_Televia_GeneraCortes", laCadena);
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.CommandTimeout = 0;
                    //comando.Transaction = transaccionSQL;


                    comando.ExecuteNonQuery();
                }

            }
            catch (SqlException e)
            {
                Logueo.Error("[EJECUTOR]Televia_generarCorte(): " + e.Message);
                throw e;
            }
            catch (Exception e)
            {
                Logueo.Error("[EJECUTOR]Televia_generarCorte(): " + e.Message);
                throw e;
            }
          

        }

        public static List<Corte> Televia_ObtenerCortesParaGenerarPolizas()
        {

            SqlDataReader SqlReader = null;
            List<Corte> losCortes = new List<Corte>();

            try
            {
                SqlParameter param;
                DataTable laTabla = null;

                using (SqlConnection laCadena = BDCortes.BDLectura)
                {
                    laCadena.Open();

                    SqlCommand comando = new SqlCommand("ProcNoct_ejecutor_Televia_ObtieneCortesSinPoliza", laCadena);
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.CommandTimeout = 0;
                    //  comando.Transaction = transaccionSQL;

                    SqlReader = comando.ExecuteReader();

                    if (null != SqlReader)
                    {
                        while (SqlReader.Read())
                        {
                            Corte unCortePorGenerarPoliza = new Corte();

                            unCortePorGenerarPoliza.Autorizacion = SqlReader["Autorizacion"].ToString();
                            unCortePorGenerarPoliza.ClaveEvento = SqlReader["ClaveEvento"].ToString();
                            unCortePorGenerarPoliza.Consecutivo = SqlReader["Consecutivo"].ToString();
                            unCortePorGenerarPoliza.CuentaTelevia = SqlReader["CuentaTelevia"].ToString();
                            unCortePorGenerarPoliza.DescripcionEvento = SqlReader["DescripcionEvento"].ToString();
                            unCortePorGenerarPoliza.FechaAutorizacion = (DateTime.Parse(SqlReader["FechaAutorizacion"].ToString()));
                            unCortePorGenerarPoliza.ID_CadenaComercial = (Int32.Parse(SqlReader["ID_CadenaComercial"].ToString()));
                            unCortePorGenerarPoliza.ID_Evento = Int32.Parse(SqlReader["ID_Evento"].ToString());
                            unCortePorGenerarPoliza.ID_Contrato = Int32.Parse(SqlReader["ID_Contrato"].ToString());
                            unCortePorGenerarPoliza.ID_Corte = Int32.Parse(SqlReader["ID_Corte"].ToString());
                            unCortePorGenerarPoliza.ID_Colectiva = Int32.Parse(SqlReader["ID_Colectiva"].ToString());
                            unCortePorGenerarPoliza.ID_TipoColectivaCuentaTelevia = Int32.Parse(SqlReader["ID_TipoColectivaCuentaTelevia"].ToString());
                            unCortePorGenerarPoliza.Importe = decimal.Parse(SqlReader["Importe"].ToString());
                            unCortePorGenerarPoliza.Observaciones = SqlReader["Observaciones"].ToString();
                            unCortePorGenerarPoliza.Tag = SqlReader["Tag"].ToString();
                            unCortePorGenerarPoliza.TipoCliente = SqlReader["TipoCliente"].ToString();
                            unCortePorGenerarPoliza.esPremiado = Boolean.Parse(SqlReader["esPremiado"].ToString());
                            losCortes.Add(unCortePorGenerarPoliza);
                        }
                    }


                }

            }
            catch (SqlException e)
            {
                Logueo.Error("[EJECUTOR]Televia_ObtenerCortesParaGenerarPolizas(): " + e.Message);
                throw e;
            }
            catch (Exception e)
            {
                Logueo.Error("[EJECUTOR]Televia_ObtenerCortesParaGenerarPolizas(): " + e.Message);
                throw e;
            }
            finally
            {
                try { SqlReader.Close(); }
                catch (Exception e) { Logueo.Error("DAOCortes.Televia_ObtenerCortesParaGenerarPolizas().Try: " + e.Message); }
            }

            return losCortes;

        }

        public static  void Televia_CalculaEstatusDiario()
        {

           

            try
            {


                using (SqlConnection laCadena = BDCortes.BDLectura)
                {
                    laCadena.Open();


                    SqlCommand comando = new SqlCommand("ProcNoct_ejecutor_Televia_CalculaEstatusDiario", laCadena);
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.CommandTimeout = 0;
                    //comando.Transaction = transaccionSQL;


                    comando.ExecuteNonQuery();
                }

            }
            catch (SqlException e)
            {
                Logueo.Error("[EJECUTOR]Televia_CalculaEstatusDiario(): " + e.Message);
                throw e;
            }
            catch (Exception e)
            {
                Logueo.Error("[EJECUTOR]Televia_CalculaEstatusDiario(): " + e.Message);
                throw e;
            }
           

        }

        public  void Televia_ActualizaPolizaEnCorte(Int32 ID_Corte, Int64 ID_Poliza, SqlConnection conn, SqlTransaction transaccionSQL)
        {

          
            try
            {
                SqlParameter param;

               

                    SqlCommand comando = new SqlCommand("ProcNoct_ejecutor_Televia_ActualizaPolizaEnCorte", conn);
                    comando.CommandType = CommandType.StoredProcedure;
                    comando.CommandTimeout = 0;
                    comando.Transaction = transaccionSQL;

                    param = new SqlParameter("@ID_Corte", SqlDbType.BigInt);
                    param.Value = ID_Corte;
                    comando.Parameters.Add(param);

                    param = new SqlParameter("@ID_Poliza", SqlDbType.BigInt);
                    param.Value = ID_Poliza;
                    comando.Parameters.Add(param);


                    comando.ExecuteNonQuery();
               


            }
            catch (SqlException e)
            {
                Logueo.Error("[EJECUTOR]Televia_ActualizaPolizaEnCorte(): " + e.Message);
                throw e;
            }
            catch (Exception e)
            {
                Logueo.Error("[EJECUTOR]Televia_ActualizaPolizaEnCorte(): " + e.Message);
                throw e;
            }
         

      
        }

      
        public  void Televia_ActualizaPeriodoAnteriorEnComplemento(Int32 ID_Corte, String ClaveMA, SqlConnection conn, SqlTransaction transaccionSQL)
        {

            try
            {
                SqlParameter param;

                SqlCommand comando = new SqlCommand("ProcNoct_ejecutor_Televia_ActualizaPeriodoAnteriorEnComplementoEstatus", conn);
                comando.CommandType = CommandType.StoredProcedure;
                comando.CommandTimeout = 0;
                comando.Transaction = transaccionSQL;

                param = new SqlParameter("@ID_Corte", SqlDbType.BigInt);
                param.Value = ID_Corte;
                comando.Parameters.Add(param);

                param = new SqlParameter("@ClaveMA", SqlDbType.VarChar);
                param.Value = ClaveMA;
                comando.Parameters.Add(param);


                comando.ExecuteNonQuery();



            }
            catch (SqlException e)
            {
                Logueo.Error("[EJECUTOR]Televia_ActualizaPolizaEnCorte(): " + e.Message);
                throw e;
            }
            catch (Exception e)
            {
                Logueo.Error("[EJECUTOR]Televia_ActualizaPolizaEnCorte(): " + e.Message);
                throw e;
            }



        }

    }
}
