using DNU_ParabiliaAltaTarjetasNominales.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.LogicaNegocio
{
    class LNFondeosYRetiros
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
    }
}
