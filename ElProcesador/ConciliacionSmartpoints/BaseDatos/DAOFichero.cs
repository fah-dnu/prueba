using CommonProcesador;
using ConciliacionSmartpoints.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConciliacionSmartpoints.BaseDatos
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
                    command.Parameters.Add(new SqlParameter("@ClaveEstatus", Convert.ToInt32(ClaveEstatus)));



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


        public static List<Ficheros> ObtieneFicheros()
        {
            int idFichero = 0;
            List<Ficheros> losFicheros = new List<Ficheros>();
            try
            {
                SqlDatabase database = new SqlDatabase(DBProcesadorArchivo.strBDLecturaArchivo);
                DbCommand command = database.GetStoredProcCommand("Proc_ObtieneFicheros");

                //return database.ExecuteDataSet(command);
                
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {
                        Ficheros unFichero = new Ficheros();

                        unFichero.IdFichero = Convert.ToInt32(losDatos.Tables[0].Rows[k]["ID_Fichero"]);
                        unFichero.FechaProceso = Convert.ToDateTime(losDatos.Tables[0].Rows[k]["FechaProceso"]);
                        unFichero.NombreFichero = losDatos.Tables[0].Rows[k]["NombreFichero"].ToString();
                        unFichero.IdConsulta = Convert.ToInt32(losDatos.Tables[0].Rows[k]["ID_Consulta"]);
                        unFichero.IdArchivo = Convert.ToInt32(losDatos.Tables[0].Rows[k]["ID_Archivo"]);
                        if(losDatos.Tables[0].Rows[k]["ID_EstatusFichero"] == null)
                        unFichero.ID_EstatusFichero = Convert.ToInt32(losDatos.Tables[0].Rows[k]["ID_EstatusFichero"]);


                        losFicheros.Add(unFichero);

                    }
                }


            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }

            return losFicheros;
        }

        public static void ActializaIdEstatusFichero(int IdFichero)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    conn.Open();
                    SqlCommand command = //DBProcesadorArchivo.BDEscrituraArchivo.CreateCommand();
                    new SqlCommand("Proc_ActializaIdEstatusFichero", conn);
                    //command.CommandText = "proc_InsertaFichero";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@IdFichero", IdFichero));

                    command.ExecuteNonQuery();
                    conn.Close();
                }
                

            }
            catch (Exception err)
            {
                Logueo.Error("ProcesaArch(): ERROR AL GUARDAR EL ERROR EN BD DEL Fichero " + IdFichero + " " + err.Message);
                
            }
        }

        public static void EliminaFicheroDatosBD(int IdFichero)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
                {
                    conn.Open();
                    SqlCommand command = //DBProcesadorArchivo.BDEscrituraArchivo.CreateCommand();
                    new SqlCommand("Proc_EliminaFicheroDatosBD", conn);
                    //command.CommandText = "proc_InsertaFichero";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@IdFichero", IdFichero));

                    command.ExecuteNonQuery();
                    conn.Close();
                }


            }
            catch (Exception err)
            {
                Logueo.Error("ProcesaArch(): ERROR AL GUARDAR EL ERROR EN BD DEL Fichero " + IdFichero + " " + err.Message);

            }
        }


    }
}
