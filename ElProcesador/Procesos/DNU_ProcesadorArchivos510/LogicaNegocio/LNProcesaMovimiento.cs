using CommonProcesador;
using DNU_ProcesadorArchivos510.BaseDatos;
using DNU_ProcesadorArchivos510.Entidades;
using Executer.Entidades;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorArchivos510.LogicaNegocio
{
    class LNProcesaMovimiento
    {
        public static int ProcesarMovimiento(Movimiento elMovimiento, String ClaveEvento, String CadenaComercial, SqlConnection conn, SqlTransaction transaccionSQL)
        {

            //using (SqlTransaction transaccionSQL = conn.BeginTransaction())
            //{
                try
                {
                    #region
                    Poliza laPoliza = null;

                    Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                    //Se consultan los parámetros del contrato
                    losParametros = Executer.BaseDatos.DAOEvento.ListaDeParamentrosContrato
                        (CadenaComercial, "", ClaveEvento, "");

                    losParametros["@ID_CuentaHabiente"] = new Parametro()
                    {
                        Nombre = "@ID_CuentaHabiente",
                        Valor = elMovimiento.IdColectiva.ToString(),
                        Descripcion = "ID CuentaHabiente",
                        ID_TipoColectiva = 10
                    };

                    //divide entre 100 porque en el archivo qu
                    losParametros["@Importe"] = new Parametro()
                    {
                        Nombre = "@Importe",
                        //   Valor = String.Format("{0}",float.Parse(elMovimiento.Importe)/100),
                        Valor = elMovimiento.Importe,
                        Descripcion = "Importe"
                    };

                    losParametros["@MedioAcceso"] = new Parametro()
                    {
                        Nombre = "@MedioAcceso",
                        Valor = elMovimiento.ClaveMA

                    };

                    losParametros["@TipoMedioAcceso"] = new Parametro()
                    {
                        Nombre = "@TipoMedioAcceso",
                        Valor = "TAR"
                    };


                    //Genera y Aplica la Poliza @DescEvento
                    Executer.EventoManual aplicador = new Executer.EventoManual(int.Parse(losParametros["@ID_Evento"].toString()),
                        losParametros["@DescEvento"].toString(), false, 0, losParametros, elMovimiento.Observaciones, conn, transaccionSQL);
                    laPoliza = aplicador.AplicaContablilidad();

                    if (laPoliza.CodigoRespuesta != 0)
                    {
                       // transaccionSQL.Rollback();
                        throw new Exception("No se generó la Póliza: " + laPoliza.DescripcionRespuesta);
                    }

                    else
                    {
                       // transaccionSQL.Commit();
                        Logueo.Evento("Se Realizó la Bonificación de Puntos a la Cuenta de la Tarjeta: " + elMovimiento.ClaveMA);
                        return 0;
                    }
                    #endregion
                }
                catch (Exception elerr)
                {
                    Logueo.Error(String.Format("NO SE GENERO LA POLIZA: {0} CON EL IMPORTE {1}", elMovimiento.ClaveMA, elMovimiento.Importe));
                  //  transaccionSQL.Rollback();
                    return 8;
                }
         //   }


        }


    }
}
