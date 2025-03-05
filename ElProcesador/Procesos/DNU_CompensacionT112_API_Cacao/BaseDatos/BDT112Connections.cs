using CommonProcesador;
using DNU_CompensacionT112Evertec.BaseDatos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_CompensacionT112_API_Cacao.BaseDatos
{
    public class BDT112Connections
    {
        SqlConnection DBT112Lectura;
        SqlConnection DBT112Escritura;

        public SqlConnection GetT112LecturaConnection(int i)
        {
            if(DBT112Lectura == null)
            {
                DBT112Lectura = new SqlConnection(String.Format(BDT112.strBDLecturaArchivo,i.ToString()));
                
            }

            if(DBT112Lectura.State == System.Data.ConnectionState.Closed)
            {
                try
                {
                    DBT112Lectura.Open();
                    Logueo.Evento("CONEXION LECTURA T112 - CONEXION REALIZADA CORRECTAMENTE ");
                }
                catch (SqlException exsql)
                {
                    Logueo.Evento(String.Format("CONEXION LECTURA T112 - ERROR AL REALIZA LA CONEXION {0}", exsql.Message));
                    Logueo.Evento(String.Format("CONEXION LECTURA T112 - SE INTENTA NUEVAMENTE REALIZAR LA CONEXION"));
                    Thread.Sleep(1000);
                    try
                    {
                        DBT112Lectura.Open();
                    }
                    catch (SqlException ex)
                    {
                        Logueo.Evento(String.Format("CONEXION LECTURA T112 - ERROR AL REALIZA LA SEGUNDA CONEXION {0}", ex.Message));
                        throw ex;
                    }
                }
            }


            return DBT112Lectura;

        }

        public SqlConnection GetT112EscrituraConnection(int i)
        {
            if (DBT112Escritura == null)
            {
                DBT112Escritura = new SqlConnection(String.Format(BDT112.strBDEscrituraArchivo,i.ToString()));

            }

            if (DBT112Escritura.State == System.Data.ConnectionState.Closed)
            {
                try
                {
                    DBT112Escritura.Open();
                    Logueo.Evento("CONEXION ESCRITURA - CONEXION REALIZADA CORRECTAMENTE ");
                }
                catch(SqlException exsql)
                {
                    Logueo.Evento(String.Format("CONEXION ESCRITURA T112 - ERROR AL REALIZA LA CONEXION {0}",exsql.Message));
                    Logueo.Evento(String.Format("CONEXION ESCRITURA T112 - SE INTENTA NUEVAMENTE REALIZAR LA CONEXION"));
                    Thread.Sleep(1000);
                    try
                    {
                        DBT112Escritura.Open();
                    }
                    catch(SqlException ex)
                    {
                        Logueo.Evento(String.Format("CONEXION ESCRITURA T112 - ERROR AL REALIZA LA SEGUNDA CONEXION {0}", ex.Message));
                        throw ex;
                    }
                }
                
            }


            return DBT112Escritura;

        }



        public  void Close()
        {
            if(DBT112Escritura != null)
            {
                if(DBT112Escritura.State == System.Data.ConnectionState.Open)
                    DBT112Escritura.Close();

                DBT112Escritura.Dispose();
            }
            if(DBT112Lectura != null)
            {
                if(DBT112Lectura.State == System.Data.ConnectionState.Open)
                    DBT112Lectura.Close();

                DBT112Lectura.Dispose();
            }
            
        }





    }
}
