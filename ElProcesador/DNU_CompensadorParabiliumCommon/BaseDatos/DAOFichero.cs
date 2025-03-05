using CommonProcesador;
using DNU_CompensadorParabiliumCommon.Utilidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.BaseDatos
{
   public class DAOFichero
    {
        public static Int64 GuardarErrorFicheroEnBD(Int64 ID_Archivo, String Mensaje, string nomRutaActual = null, string Temp = null)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    conn.Open();
                    SqlCommand command = //DBProcesadorArchivo.BDEscrituraArchivo.CreateCommand();
                    new SqlCommand("proc_InsertaErrorFichero", conn);
                    //command.CommandText = "proc_InsertaFichero";
                    command.CommandType = CommandType.StoredProcedure;
                   // command.Transaction = transaccionSQL;

                    int resp = -1;


                    command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Archivo));
                    command.Parameters.Add(new SqlParameter("@Mensaje", Mensaje));
                    command.Parameters.Add(new SqlParameter("@nomRutaActual", nomRutaActual));
                    command.Parameters.Add(new SqlParameter("@temp", Temp));//Para indetificar si es de ficheroTemp(1) otro caso es de fichero


                    command.ExecuteNonQuery();
                    conn.Close();
                }
                return 0;

            }
            catch (Exception err)
            {
                Log.Error("ProcesaArch(): ERROR AL GUARDAR EL ERROR EN BD DEL ARCHIVO " + ID_Archivo + " " + err.Message);
                return -1;
            }
        }
        


        public static Int64 ActualizaEstatusFichero(Int64 ID_Archivo, Utilidades.EstatusFichero ClaveEstatus)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    conn.Open();
                    SqlCommand command = //DBProcesadorArchivo.BDEscrituraArchivo.CreateCommand();
                    new SqlCommand("ProcNoct_COMP_ActualizaEstatusFichero", conn);
                    //command.CommandText = "proc_InsertaFichero";
                    command.CommandType = CommandType.StoredProcedure;
                    // command.Transaction = transaccionSQL;

                    int resp = -1;


                    command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Archivo));
                    command.Parameters.Add(new SqlParameter("@ClaveEstatus", ClaveEstatus.ToString()));



                    command.ExecuteNonQuery();
                    conn.Close();
                }
                return 0;

            }
            catch (Exception err)
            {
                Log.Error("ProcesaArch(): ERROR AL CAMBIAR EL ESTATUS DEL ARCHIVO " + ID_Archivo + " " + err.Message);
                return -1;
            }
        }

     
        public static DataTable ValidaParseEventosSaaS(string grupoCuenta, string idOperacion, long ID_FicheroDetalle)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDLecturaAutorizador))
                {
                    conn.Open();
                    SqlCommand command = 
                    new SqlCommand("ProcNoct_COMP_ValidaMigracionColectivaOperacion", conn);

                    command.CommandType = CommandType.StoredProcedure;


                    command.Parameters.Add(new SqlParameter("@IdGrupoCuenta", grupoCuenta));
                    command.Parameters.Add(new SqlParameter("@idOperacion", idOperacion));



                    using (SqlDataAdapter sda = new SqlDataAdapter(command))
                    {
                        DataSet opcional = new DataSet();
                        sda.Fill(opcional);

                        conn.Close();

                        return opcional.Tables.Count > 0 ? opcional.Tables[0] : null;
                    }
                    
                }

            }
            catch (Exception err)
            {
                Log.Error("ProcesaArch(): ERROR AL VALIDAR PARSE EVENTOS SAAS ID FICHERO DETALLE " + ID_FicheroDetalle + " " + err.Message);
                return null;
            }
        }

        public static void ObtenerAutorizacionOperacion(Int64 idFicheroDetalle, string autorizacion)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    conn.Open();
                    SqlCommand command = new SqlCommand("PROCNOC_COMP_CopiaFicheroDetalleNuevaAutorizacion", conn);

                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@idFicheroDetalle", idFicheroDetalle));
                    command.Parameters.Add(new SqlParameter("@autorizacion", autorizacion));



                    using (SqlDataAdapter sda = new SqlDataAdapter(command))
                    {
                        DataSet opcional = new DataSet();
                        sda.Fill(opcional);

                        conn.Close();
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception("[ObtenerAutorizacionOperacion] [IdFicheroDetalle: " + idFicheroDetalle
                        + "] [Autorizacion: " + autorizacion + "] [" + ex.Message + "]", ex);
            }
        }
    }
}
