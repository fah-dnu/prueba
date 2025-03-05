using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using QUANTUM_AplicarMovimientos.Entidades;
using Interfases.Entidades;
using QUANTUM_ModificarLineasCredito.Entidades;
using QUANTUM_ModificarLineasCredito.BaseDatos;
using System.Data;
using CommonProcesador;
using System.Data.SqlClient;

namespace QUANTUM_ModificarLineasCredito.LogicaNegocio
{
  public class LNEvento
    {

      
        private static List<LineaCredito> ObtieneMovimientos()
        {
            List<LineaCredito> lasNuevasLineas = new List<LineaCredito>();
            try
            {
                //obtiene el dataset del ws
                wsQuantumLimCred.Quantum unaOperacion = new wsQuantumLimCred.Quantum();

                DataSet losUsuarios = unaOperacion.ModificacionLinea(DateTime.Now.ToString("yyyy-MM-dd"));


                foreach (DataRow renglon in losUsuarios.Tables[0].Rows)
                {
                    LineaCredito unaLineaCredito = new LineaCredito();

                    unaLineaCredito.Consecutivo = renglon["CONSECUTIVO"].ToString();
                    unaLineaCredito.ID_Cliente = renglon["REFERENCIAPAGO"].ToString();
                    unaLineaCredito.LimiteCredito = (Decimal)renglon["IMPORTE"];
                    unaLineaCredito.Tarjeta = renglon["TARJETA"].ToString();
                    unaLineaCredito.DiasVigencia = (int)renglon["DIASVIGENCIA"];



                    lasNuevasLineas.Add(unaLineaCredito);
                }

            }
            catch (Exception err)
            {
                Logueo.Error("obtieneMovimientos()" + err.Message);
            }

            return lasNuevasLineas;
        }

    }
}
