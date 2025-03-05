using CommonProcesador;
using DNU_ProcesadorT112.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorT112.BaseDatos
{
   public class DAOFichero
    {
        public static Int64 GuardarErrorFicheroEnBD(Int64 ID_Archivo, String Mensaje)
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



                    command.ExecuteNonQuery();
                    conn.Close();
                }
                return 0;

            }
            catch (Exception err)
            {
                Logueo.Error("ProcesaArch(): ERROR AL GUARDAR EL ERROR EN BD DEL ARCHIVO " + ID_Archivo + " " + err.Message);
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
                    new SqlCommand("proc_ActualizaEstatusFichero", conn);
                    //command.CommandText = "proc_InsertaFichero";
                    command.CommandType = CommandType.StoredProcedure;
                    // command.Transaction = transaccionSQL;

                    int resp = -1;


                    command.Parameters.Add(new SqlParameter("@ID_Fichero", ID_Archivo));
                    command.Parameters.Add(new SqlParameter("@ClaveEstatus", Convert.ToInt32( ClaveEstatus)));



                    command.ExecuteNonQuery();
                    conn.Close();
                }
                return 0;

            }
            catch (Exception err)
            {
                Logueo.Error("ProcesaArch(): ERROR AL CAMBIAR EL ESTATUS DEL ARCHIVO " + ID_Archivo + " " + err.Message);
                return -1;
            }
        }
    }
}
