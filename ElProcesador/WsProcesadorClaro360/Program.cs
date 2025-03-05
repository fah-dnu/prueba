using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;


namespace WsProcesadorClaro360
{
    class Program
    {
        static void Main()
        {
            //#if (!DEBUG)


            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
                { 
                    new ServiceElProcesador() 
                };
            ServiceBase.Run(ServicesToRun);


            //#else

            //  DateTime elFecha = DateTime.Parse("2015-05-19T17:33:18.167-05:00");
            /*ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new ServiceElProcesador() 
            };
            ServiceBase.Run(ServicesToRun);*/

            //Procesador service1 = new Procesador();
            //String[] args = new String[] { String.Empty };
            //service1.OnStart(args);
            //System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);



            ////#endif
            /// 
         
        }
    }
}
