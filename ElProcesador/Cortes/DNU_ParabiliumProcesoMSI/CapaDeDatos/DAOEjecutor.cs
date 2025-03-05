using CommonProcesador;
using DNU_ParabiliumProcesoMSI.Servicios.SQLServer;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliumProcesoMSI.CapaDeDatos
{
    class DAOEjecutor
    {
        public Dictionary<String, Parametro> ObtenerDatosParametros(Dictionary<String, Parametro> losParametrosEntrada, SqlConnection conn = null, SqlTransaction transaccionSQL = null, string conexionBD = null)
        {
            try
            {

                Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                //Se consultan los parámetros del contrato
                //ya sea este o el proceso de abajo ambos deben de funcionar
                //losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato
                //(elAbono.ClaveColectiva, elAbono.Tarjeta, elAbono.ClaveEvento, "");

                //funciona
              
                losParametros = ListaDeParamentrosContrato
                (losParametrosEntrada["@ClaveCadenaComercial"].Valor, losParametrosEntrada["@Tarjeta"].Valor, losParametrosEntrada["@Evento"].Valor, "", conn, transaccionSQL);


                return losParametros;

            }

            catch (Exception err)
            {
                //transaccionSQLTraspaso.Rollback();
                throw err;
            }
        }

        public Dictionary<String, Parametro> ListaDeParamentrosContrato(String ClaveCadenaComercial, String Tarjeta, String ClaveEvento, String elUsuario, SqlConnection conn = null, SqlTransaction transaccionSQL = null, Guid? idLog = null)
        {

            try
            {
                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();

                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                DataTable dtResultado;
                // SqlDatabase database = new SqlDatabase(conn.ConnectionString);
                // DbCommand command = database.GetStoredProcCommand("ws_Ejecutor_ObtieneValoresContratos");
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ClaveCadenaComercial", parametro = ClaveCadenaComercial });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ClaveEvento", parametro = ClaveEvento });
                parametros.Add(new ParametrosProcedimiento
                {
                    Nombre = "@Tarjeta",
                    parametro = string.IsNullOrEmpty(Tarjeta)
                                                                    ? "Null"
                                                                    : Tarjeta,
                    encriptado = true,
                    longitud = Tarjeta.Length
                });
                ConexionBD consultaBDRead = new ConexionBD("");
                dtResultado = consultaBDRead.ConstruirConsulta("[ws_Ejecutor_ObtieneValoresContratos]", parametros, "ws_Ejecutor_ObtieneValoresContratos", "", conn, transaccionSQL);

                //database.AddInParameter(command, "@ClaveCadenaComercial", DbType.String, ClaveCadenaComercial);
                //database.AddInParameter(command, "@ClaveEvento", DbType.String, ClaveEvento);
                //database.AddInParameter(command, "@Tarjeta", DbType.String, Tarjeta);

                //laTabla = database.ExecuteDataSet(command, transaccionSQL).Tables[0];

                for (int k = 0; k < dtResultado.Rows.Count; k++)
                {
                    Parametro unParamentro = new Parametro();
                    unParamentro.Nombre = (dtResultado.Rows[k]["Nombre"]).ToString();
                    unParamentro.ID_TipoColectiva = Int16.Parse((dtResultado.Rows[k]["ID_TipoColectiva"]).ToString());
                    unParamentro.Valor = (dtResultado.Rows[k]["valor"]).ToString();
                    larespuesta.Add(unParamentro.Nombre, unParamentro);

                }

                return larespuesta;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
