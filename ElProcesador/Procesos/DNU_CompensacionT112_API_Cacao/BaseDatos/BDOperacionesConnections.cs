using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensacionT112_API_Cacao.BaseDatos
{
    public class BDOperacionesConnections
    {
        private SqlConnection DBOpsLectura;
        private SqlConnection DBOpsEscritura;

        public SqlConnection GetOperacionesLecturaConnection(int i)
        {
            if (DBOpsLectura == null)
            {
                DBOpsLectura = new SqlConnection(String.Format(BDOperacionesEvertec.strBDLecturaArchivo,i.ToString()));

            }

            if (DBOpsLectura.State == System.Data.ConnectionState.Closed)
            {
                DBOpsLectura.Open();
            }


            return DBOpsLectura;

        }

        public SqlConnection GetOperacionesEscrituraConnection(int i)
        {
            if (DBOpsEscritura == null)
            {
                DBOpsEscritura = new SqlConnection(String.Format(BDOperacionesEvertec.strBDEscrituraArchivo,i.ToString()));

            }

            if (DBOpsEscritura.State == System.Data.ConnectionState.Closed)
            {
                DBOpsEscritura.Open();
            }


            return DBOpsEscritura;

        }



        public  void Close()
        {
            if (DBOpsEscritura != null)
            {
                if (DBOpsEscritura.State == System.Data.ConnectionState.Open)
                    DBOpsEscritura.Close();

                DBOpsEscritura.Dispose();
            }
            if (DBOpsLectura != null)
            {
                if (DBOpsLectura.State == System.Data.ConnectionState.Open)
                    DBOpsLectura.Close();

                DBOpsLectura.Dispose();
            }

        }

    }
}
