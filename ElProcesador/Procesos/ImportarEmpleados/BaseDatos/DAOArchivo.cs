using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data.Common;
using ImportarEmpleados.Utilidades;
using ImportarEmpleados.Entidades;
using CommonProcesador;
using System.Data.SqlClient;

namespace ImportarEmpleados.BaseDatos
{
    public class DAOArchivo
    {
        public static int Actualiza(Int64 ID_Archivo, EstatusArchivo elEstatus)
        {
            try
            {


                DbCommand command = BDEmpleados.BDEscritura.CreateCommand();
                command.CommandText = "web_ActualizaEstatusArchivo";
                command.CommandType = CommandType.StoredProcedure;
                //command.Transaction = UnaTransaccion;

                int resp = -1;

                command.Parameters.Add(new SqlParameter("@ID_Archivo", ID_Archivo));
                command.Parameters.Add(new SqlParameter("@ID_Estatus", (int)elEstatus));


                resp = command.ExecuteNonQuery();

                return 0;

            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }


        public static List<Archivo> ListaArchivos()
        {
           

            List<Archivo> losArchivos = new List<Archivo>();
            try
            {
                SqlDatabase database = new SqlDatabase(BDEmpleados.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("web_SeleccionaArchivosPorProcesar");
                
                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {

                        Archivo unArchivo = new Archivo();

                        unArchivo.Nombre = (String)losDatos.Tables[0].Rows[k]["Nombre"];
                        unArchivo.CA_Usuario = (String)losDatos.Tables[0].Rows[k]["CA_Usuario"];
                        unArchivo.Fecha = ((DateTime)losDatos.Tables[0].Rows[k]["Fecha"]).ToString("yyyy-MM-dd");
                        unArchivo.ID_Archivo = (Int64)losDatos.Tables[0].Rows[k]["ID_Archivo"];
                        unArchivo.ID_Estatus = (Int32)losDatos.Tables[0].Rows[k]["ID_Estatus"];
                        unArchivo.ID_CadenaComercial = (Int64)losDatos.Tables[0].Rows[k]["ID_CadenaComercial"];
                        unArchivo.UrlArchivo = (String)losDatos.Tables[0].Rows[k]["NombreArchivo"];


                        losArchivos.Add(unArchivo);

                    }
                }

                return losArchivos;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public static ResultadoProceso ObtieneResultado(long ID_Archivo)
        {


            
            try
            {
                ResultadoProceso unResultado = new ResultadoProceso();
                SqlDatabase database = new SqlDatabase(BDEmpleados.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("web_ObtieneResultadoEjecucion");

                command.Parameters.Add(new SqlParameter("@ID_Archivo", ID_Archivo));

                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {

                      

                       unResultado.totRegistros = (String)losDatos.Tables[0].Rows[k]["totRegistros"];
                        unResultado.NombreArchivo = (String)losDatos.Tables[0].Rows[k]["NombreArchivo"];
                        unResultado.NOMBREUSUARIO = (String)losDatos.Tables[0].Rows[k]["NOMBREUSUARIO"];
                        unResultado.TotsinProcesar = (String)losDatos.Tables[0].Rows[k]["TotsinProcesar"];
                        unResultado.TotEmpleadoCreados = (String)losDatos.Tables[0].Rows[k]["TotEmpleadoCreados"];
                        unResultado.TotEmpleadosCuenta = (String)losDatos.Tables[0].Rows[k]["TotEmpleadosCuenta"];
                        unResultado.TotEmpleadosDeposito = (String)losDatos.Tables[0].Rows[k]["TotEmpleadosDeposito"];
                        unResultado.TotEmpleadosProcesados = (String)losDatos.Tables[0].Rows[k]["TotEmpleadosProcesados"];
                        unResultado.TotEmpleadosErrorCrear = (String)losDatos.Tables[0].Rows[k]["TotEmpleadosErrorCrear"];
                        unResultado.TotEmpleadosErrorCta = (String)losDatos.Tables[0].Rows[k]["TotEmpleadosErrorCta"];
                        unResultado.TotEmpleadosErrorDeposito = (String)losDatos.Tables[0].Rows[k]["TotEmpleadosErrorDeposito"];
                        unResultado.TotEmpleadosErrorClub = (String)losDatos.Tables[0].Rows[k]["TotEmpleadosErrorClub"];
                        unResultado.email = (String)losDatos.Tables[0].Rows[k]["Email"];
                        unResultado.TotEmpleados = (String)losDatos.Tables[0].Rows[k]["TotEmpleados"];



                    }
                }

                return unResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
