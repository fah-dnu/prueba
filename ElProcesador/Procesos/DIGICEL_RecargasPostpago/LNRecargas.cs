using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using DIGICEL_RecargasPostpago.Entidades;
using DALAutorizador.BaseDatos;
using System.Data;

namespace DIGICEL_RecargasPostpago
{
    public class LNRecargas
    {
        delegate void EnviarLasOperacion(Operacion laOper, String wsUsuario, String wsPass, String Sucursal, String Afiliacion, String Terminal);
      //  public static Dictionary<String, IAsyncResult> ThreadsOperaciones = new Dictionary<string, IAsyncResult>();

        public static Boolean EnviarRegargas()
        {
            try
            {
                String wsUser = PNConfig.Get("DIGIREC", "wsUsuario");
                String wsPass = PNConfig.Get("DIGIREC", "wsPassword");
                String Suc = PNConfig.Get("DIGIREC", "Sucursal");
                String Afl = PNConfig.Get("DIGIREC", "Afiliacion");
                String Term = PNConfig.Get("DIGIREC", "Terminal");


                //obtiene las operaciones para procesarlas
                List<Operacion> lasOperaciones = obtieneLasOperacionesPostPago();
                //Obtener los medios de acceso con los saldos.
                //  losProcesos = DAOProcesos.ObtieneTodosProcesos();

             

                //for (int k = 0; k < losProcesos.Count; k++)
                foreach (Operacion laOperacion in lasOperaciones)
                {

                    // Proceso = losProcesos[k].Clave;// Config.GetValor("PlugIns", String.Format("{0}", k));

                    try
                    {
                        EnviarOperacion elEnvio = new EnviarOperacion();
                        //Inicia los delegados para Ejecutar los batch de las diferentes acciones.
                        EnviarLasOperacion ProcesoAEjecutar = elEnvio.EnviarLaOperacion;


                        //Ejecuta el metodo asincrono
                        
                                    ProcesoAEjecutar.BeginInvoke(laOperacion,wsUser,wsPass,Suc,Afl,Term,
                                    delegate(IAsyncResult ar1)
                                    {
                                        try
                                        {
                                            ProcesoAEjecutar.EndInvoke(ar1);
                                        }
                                        catch (Exception ex)
                                        {

                                        }
                                    }, null);

                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[EnviarRegargas] " + "Procesador Nocturno.EnviarRegargas, El Telefono: " + laOperacion.Telefono + " no pudo ser recargado ," + ex.ToString());
                    }

                }


                return true;
            }
            catch (Exception err)
            {
                Logueo.Error("Error en EnviarRegargas():" + err.ToString());
                return false;

            }
        }


        private static List<Operacion> obtieneLasOperacionesPostPago()
        {
            List<Operacion> lasOper = new List<Operacion>();
            try
            {
                DataSet OperacionesBD = DAORecargaDigicel.ObtieneRecargas();
                for (int k = 0; k < OperacionesBD.Tables[0].Rows.Count; k++)
                {
                    Operacion unaOp = ObtienelaOperFromDataRow(OperacionesBD.Tables[0].Rows[k]);

                    lasOper.Add(unaOp);
                }

               
            }
            catch (Exception err)
            {
                Logueo.Error("Error en obtieneLasOperacionesPostPago():" + err.ToString());
                throw new Exception("Error en obtieneLasOperacionesPostPago():" + err.ToString());
            }

            return lasOper;
        }

        private static Operacion ObtienelaOperFromDataRow(DataRow laOper)
        {
            Operacion laOperacion = new Operacion();
            try
            {
                

                laOperacion.Telefono = (string)(laOper["Telefono"].ToString().Trim());
                laOperacion.Importe = float.Parse(laOper["Importe"].ToString().Trim());

            }
            catch (Exception err)
            {

                Logueo.Error("Error en ObtienelaOperFromDataRow():" + err.ToString());
                throw new Exception("Error en ObtienelaOperFromDataRow():" + err.ToString());
            }
            return laOperacion;
        }
    }
}
