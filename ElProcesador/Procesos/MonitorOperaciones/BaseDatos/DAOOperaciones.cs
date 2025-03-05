using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data.Common;
using MonitoreaOperaciones.Utilidades;
using MonitoreaOperaciones.Entidades;
using CommonProcesador;
using System.Data.SqlClient;

namespace MonitoreaOperaciones.BaseDatos
{
    public class DAOOperaciones
    {

        public static List<DetalleOperaciones> ObtieneEstatusOperaciones()
        {


            List<DetalleOperaciones> losDetalles = new List<DetalleOperaciones>();
            try
            {
                SqlDatabase database = new SqlDatabase(BDOperaciones.strBDLectura);

                int PeriodoTiempo= MonitoreaOperaciones.Utilidades.Config.Instance().Cadenas.PeriodoTiempoMin;

                DbCommand command = database.GetStoredProcCommand("ProcNoct_MonitorOperaciones");
                command.Parameters.Add(new SqlParameter("@Minutos", PeriodoTiempo));
                
                //return database.ExecuteDataSet(command);
                DataSet losDatos = (DataSet)database.ExecuteDataSet(command);

                if (losDatos.Tables[0].Rows.Count > 0)
                {
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {

                        DetalleOperaciones unDetalle = new DetalleOperaciones();

                        unDetalle.Activas = (Int32)losDatos.Tables[0].Rows[k]["Activas"];
                        unDetalle.Declinadas = (Int32)losDatos.Tables[0].Rows[k]["Inactivas"];
                        unDetalle.ID_CadenaComercial = (Int32)losDatos.Tables[0].Rows[k]["ID_ColectivaCadComercial"];
                        unDetalle.NombreCadenaComercial = (String)losDatos.Tables[0].Rows[k]["NombreCadena"];
                        unDetalle.Codigos = (String)losDatos.Tables[0].Rows[k]["Codigos"];

                        losDetalles.Add(unDetalle);

                    }
                }

                return losDetalles;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

      
    }
}
