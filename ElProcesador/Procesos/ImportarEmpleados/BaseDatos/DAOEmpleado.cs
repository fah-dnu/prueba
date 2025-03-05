using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImportarEmpleados.Entidades;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using ImportarEmpleados.Utilidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using CommonProcesador;

namespace ImportarEmpleados.BaseDatos
{
    public class DAOEmpleado
    {
        public static int Actualiza(Empleado elEmpleado, EstatusEmpleado elEstatus, Int64 ID_Detalle)
        {
            try
            {


                DbCommand command = BDEmpleados.BDEscritura.CreateCommand();
                command.CommandText = "web_ActualizaEstatusEmpleado";
                command.CommandType = CommandType.StoredProcedure;
                //command.Transaction = UnaTransaccion;

                int resp = -1;

                command.Parameters.Add(new SqlParameter("@ID_Detalle", ID_Detalle));
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

        public static List<Empleado> ObtieneEmpleadosPorProcesar(Int64 ID_Archivo)
        {
            List<Empleado> losEmpleados = new List<Empleado>();
            try
            {
                SqlDatabase database = new SqlDatabase(BDEmpleados.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("web_SeleccionaEmpleadosPorProcesar");
                database.AddInParameter(command, "@ID_Archivo", DbType.Int64, ID_Archivo);
                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {

                        Empleado unEmpleado = new Empleado();

                        unEmpleado.Nombre = (String)losDatos.Tables[0].Rows[k]["Nombre"];
                        unEmpleado.APaterno = (String)losDatos.Tables[0].Rows[k]["APaterno"];
                        unEmpleado.AMaterno = (String)losDatos.Tables[0].Rows[k]["AMaterno"];
                        unEmpleado.Baja = (String)losDatos.Tables[0].Rows[k]["Baja"];
                        unEmpleado.CicloNominal = (String)losDatos.Tables[0].Rows[k]["CicloNominal"];
                        unEmpleado.DiaPago = (String)losDatos.Tables[0].Rows[k]["DiaPago"];
                        unEmpleado.EmailEmpresarial = (String)losDatos.Tables[0].Rows[k]["EmailEmpresarial"];
                        unEmpleado.EmailPersonal = (String)losDatos.Tables[0].Rows[k]["EmailPersonal"];
                        unEmpleado.FechaNacimiento = (DateTime)losDatos.Tables[0].Rows[k]["FechaNacimiento"];
                        unEmpleado.ID_CadenaComercial = (Int64)losDatos.Tables[0].Rows[k]["ID_CadenaComercial"];
                        unEmpleado.ID_Empleado = (Int64)losDatos.Tables[0].Rows[k]["consecutivo"];
                        unEmpleado.LimiteCompra = (Decimal)losDatos.Tables[0].Rows[k]["LimiteCompra"];
                        unEmpleado.NumeroEmpleado = (String)losDatos.Tables[0].Rows[k]["NumeroEmpleado"];
                        unEmpleado.Reservado1 = (String)losDatos.Tables[0].Rows[k]["Reservado1"];
                        unEmpleado.Reservado2 = (String)losDatos.Tables[0].Rows[k]["Reservado2"];
                        unEmpleado.TelefonoMovil = (String)losDatos.Tables[0].Rows[k]["TelefonoMovil"];
                        unEmpleado.ID_Estatus = (int)losDatos.Tables[0].Rows[k]["ID_Estatus"];
                        unEmpleado.ID_Detalle = (Int64)losDatos.Tables[0].Rows[k]["ID_Detalle"];


                        losEmpleados.Add(unEmpleado);

                    }
                }

                return losEmpleados;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
