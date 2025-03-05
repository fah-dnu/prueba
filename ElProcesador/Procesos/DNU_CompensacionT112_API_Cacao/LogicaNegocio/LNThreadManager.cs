using CommonProcesador;
using DNU_CompensacionT112_API_Cacao.BaseDatos;
using DNU_CompensacionT112_API_Cacao.Entidades;
using DNU_CompensacionT112Evertec.BaseDatos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_CompensacionT112_API_Cacao.LogicaNegocio
{
    public class LNThreadManager
    {
        public static List<Thread> threads = new List<Thread>();

        public static void IniciaProcesoHilos()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                List<String> fechasAProcesar = DAOT112.ObtenerFechasAProcesar(
                    Convert.ToInt32(PNConfig.Get("COMPT112APICACAO", "ObtenerFechasTimeOut").ToString()));
                List<ConfiguracionFechas> lstConfig = new List<ConfiguracionFechas>();

                int hilos = Convert.ToInt32(PNConfig.Get("COMPT112APICACAO", "NumeroHilos").ToString());
                hilos = 1;
                if (fechasAProcesar.Count == 0)
                {
                    throw new Exception("[COMPT112APICACAO] No se localizaron fechas a procesar");
                }

                int fechaPorHilo = fechasAProcesar.Count / hilos;

                if (fechaPorHilo == 0)
                {
                    fechaPorHilo = fechasAProcesar.Count;
                }




                lstConfig = ObtieneFechasAgrupadas(fechaPorHilo, fechasAProcesar);

                //GeneraLlamadaEnHilos(lstConfig);
                GeneraLlamadaSincrona(lstConfig);

                //Thread.Sleep(200000);
                //return true;
            }
            catch(Exception ex)
            {
                Logueo.Error(String.Format("Error en la ejecución del proceso en hilos - {0}", ex.Message));
                //return false;
            }
        }

        private static void GeneraLlamadaSincrona(List<ConfiguracionFechas> lstConfig)
        {
            int i = 1;
            BDT112Connections dbT112Connections = new BDT112Connections();
            dbT112Connections.GetT112EscrituraConnection(i);
            dbT112Connections.GetT112LecturaConnection(i);
            BDOperacionesConnections dbOpConnections = new BDOperacionesConnections();
            dbOpConnections.GetOperacionesEscrituraConnection(i);
            dbOpConnections.GetOperacionesLecturaConnection(i);

            Logueo.Evento(String.Format("Inicia fechas {0} - {1}", lstConfig.First().FechaInicial, lstConfig.Last().FechaFinal));
            LNEjecutaCompensacion.EjecutaCicloCompensacion(lstConfig.First().FechaInicial , lstConfig.Last().FechaFinal , dbT112Connections, dbOpConnections, i);
            Logueo.Evento(String.Format("Termina fechas {0} - {1}", lstConfig.First().FechaInicial, lstConfig.Last().FechaFinal));
        }

        private static void  GeneraLlamadaEnHilos(List<ConfiguracionFechas> lstConfig)
        {
            Thread t;
            int i = 0;
            foreach (var item in lstConfig)
            {
                BDT112Connections dbT112Connections = new BDT112Connections();
                dbT112Connections.GetT112EscrituraConnection(i);
                dbT112Connections.GetT112LecturaConnection(i);
                BDOperacionesConnections dbOpConnections = new BDOperacionesConnections();
                dbOpConnections.GetOperacionesEscrituraConnection(i);
                dbOpConnections.GetOperacionesLecturaConnection(i);


                Task.Run(() =>
                {
                        LNEjecutaCompensacion.EjecutaCicloCompensacion(item.FechaInicial, item.FechaFinal, dbT112Connections, dbOpConnections, i);
                        Logueo.Evento(String.Format("Termina hilo  fechas {0} - {1}", item.FechaInicial, item.FechaFinal));
                });
                //t = new Thread(() => {

                //    LNEjecutaCompensacion.EjecutaCicloCompensacion(item.FechaInicial, item.FechaFinal, dbT112Connections, dbOpConnections, i);
                //    Logueo.Evento(String.Format("Termina hilo  fechas {1} - {2}", item.FechaInicial, item.FechaFinal));
                //});

                //t.Name = String.Format("Hilo Con fechas {0} - {1}", item.FechaInicial, item.FechaFinal);
                //t.Start();
                //threads.Add(t);
                i++;
            }
        }

        private static List<ConfiguracionFechas> ObtieneFechasAgrupadas(int fechaPorHilo, List<String> fechasAProcesar)
        {
            int contadorFechas = 1;
            string fechaInicial = String.Empty;
            string fechaFinal = String.Empty;
            List<ConfiguracionFechas> lstConfigTemp = new List<ConfiguracionFechas>();

            for (int i = 0; i < fechasAProcesar.Count; i++)
            {
                if (contadorFechas == 1)
                {
                    fechaInicial = DateTime.Parse(fechasAProcesar[i]).ToString("yyyy-MM-dd");
                }

                if (contadorFechas == fechaPorHilo || i == fechasAProcesar.Count - 1)
                {
                    fechaFinal = DateTime.Parse(fechasAProcesar[i]).ToString("yyyy-MM-dd");

                    lstConfigTemp.Add(new ConfiguracionFechas
                    {
                        FechaInicial = fechaInicial,
                        FechaFinal = fechaFinal
                    });

                    contadorFechas = 0;
                }

                contadorFechas++;
            }

            return lstConfigTemp;
        }


        public static void DetieneHilos()
        {
            try
            {


                if (threads == null)
                {
                    return;
                }

                Logueo.Evento(String.Format("Deteniendo hilos {0}", threads.Count));

                foreach (var item in threads)
                {
                  
                    try
                    {
                        Logueo.Evento(String.Format("Deteniendo hilos {0}", item.Name));
                        //if (item.ThreadState == ThreadState.Running)
                        //{
                        item.Abort();
                        //}
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error(String.Format("Error al detener el hilo {0} - {0}", item.Name, ex.Message));
                    }
                }
                Logueo.Evento(String.Format("Hilos detenidos {0}", threads.Count));

            }
            catch (Exception ex)
            {
                Logueo.Error(String.Format("Error al detener los hilos {0}", ex.Message));
            }
        }
    }
}
