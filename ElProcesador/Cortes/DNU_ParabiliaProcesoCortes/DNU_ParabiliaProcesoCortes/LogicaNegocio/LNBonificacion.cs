using DNU_ParabiliaProcesoCortes.Entidades;
using Executer.Entidades;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class LNBonificacion
    {
        public static Bonificacion obtenerDatosParaDiccionario(DataTable tabla)
        {

            Bonificacion nuevaBonificacion = new Bonificacion();
            List<DataRow> listaElementosEliminar = new List<DataRow>();
            foreach (DataRow row in tabla.Rows)
            {

                if (row["nombre"].ToString() == "@idEvento")
                {
                    nuevaBonificacion.IdEvento = Convert.ToInt32(row["valor"].ToString());
                    listaElementosEliminar.Add(row);
                }
                else if (row["nombre"].ToString() == "@claveEvento")
                {
                    nuevaBonificacion.ClaveEvento = row["valor"].ToString();

                    listaElementosEliminar.Add(row);


                }
                else if (row["nombre"].ToString() == "@observaciones")
                {
                    nuevaBonificacion.Observaciones = row["valor"].ToString();
                    listaElementosEliminar.Add(row);


                }
                else if (row["nombre"].ToString() == "@concepto")
                {
                    nuevaBonificacion.Concepto = row["valor"].ToString();
                    listaElementosEliminar.Add(row);


                }
                else if (row["nombre"].ToString() == "@saldo")
                {
                    nuevaBonificacion.SaldoTotal = Convert.ToDecimal(row["valor"].ToString());
                    listaElementosEliminar.Add(row);


                }
                else if (row["nombre"].ToString() == "@email")
                {
                    nuevaBonificacion.email = row["valor"].ToString();
                    listaElementosEliminar.Add(row);


                }
                else if (row["nombre"].ToString() == "@refNumerica")
                {
                    nuevaBonificacion.RefNumerica = Convert.ToInt32(row["valor"].ToString());
                    listaElementosEliminar.Add(row);


                }
                else if (row["nombre"].ToString() == "@tarjeta")
                {
                    nuevaBonificacion.Tarjeta = row["valor"].ToString();
                    listaElementosEliminar.Add(row);


                }
                else if (row["nombre"].ToString() == "@ClaveColectiva")
                {
                    nuevaBonificacion.ClaveColectiva = row["valor"].ToString();
                    listaElementosEliminar.Add(row);


                }
                else if (row["nombre"].ToString() == "@cuentaOrigen")
                {
                    nuevaBonificacion.cuentaOrigen = row["valor"].ToString();
                    listaElementosEliminar.Add(row);


                }
                else if (row["nombre"].ToString() == "@cuentaDestino")
                {
                    nuevaBonificacion.cuentaDestino = row["valor"].ToString();
                    listaElementosEliminar.Add(row);


                }

            }
            for (int i = 0; i < listaElementosEliminar.Count; i++)
            {
                tabla.Rows.Remove(listaElementosEliminar[i]);
            }
            tabla.AcceptChanges();


            return nuevaBonificacion;
        }

        public bool RealizarFondeoORetiroTraspaso(Bonificacion elAbono, Dictionary<String, Parametro> losParametros, String fecha, Response errores,
                                        SqlTransaction transaccionSQLTraspaso, SqlConnection conn)
        {
            bool polizaCreada = false;

            try
            {
                try
                {
                    Poliza laPoliza = null;

                    //Genera y Aplica la Poliza
                    Executer.EventoManual aplicador = new Executer.EventoManual(elAbono.IdEvento,
                    elAbono.Concepto, false, Convert.ToInt64(elAbono.RefNumerica), losParametros, elAbono.Observaciones, conn, transaccionSQLTraspaso);
                    laPoliza = aplicador.AplicaContablilidad();
                    //fechaPoliza = laPoliza.FechaCreacion;
                    //IdPoliza = laPoliza.ID_Poliza;

                    if (laPoliza.CodigoRespuesta != 0)
                    {
                        errores.CodRespuesta = laPoliza.CodigoRespuesta.ToString();
                        errores.DescRespuesta = laPoliza.DescripcionRespuesta.Replace("[EventoManual]", "").Replace("NO", "No").Replace(laPoliza.CodigoRespuesta.ToString(), "").Replace("Error:", "").Trim();
                        throw new Exception("No se generó la Póliza: " + laPoliza.DescripcionRespuesta);
                    }

                    else
                    {
                        polizaCreada = true;
                       // Logueo.Evento("Se Realizó la Bonificación de Puntos a la Cuenta de la Colectiva: " + elAbono.IdColectiva.ToString());
                    }
                }

                catch (Exception err)
                {
                    throw err;
                }
            }

            catch (Exception err)
            {
                throw new Exception("RegistraEvManual_AbonoPuntos() " + err.Message);
            }

            return polizaCreada;
        }

    }
}
