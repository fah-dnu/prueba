using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Mensajeria.Mensajes;
using Framework;
using Mensajeria;
using ImportarEmpleados.Utilidades;
using CommonProcesador;
using ImportarEmpleados.LogicaNegocio;
using System.IO;
using CommonProcesador.Utilidades;


namespace ImportarEmpleados
{
    public class ImportarEmpleado : IProcesoNocturno
    {


        bool IProcesoNocturno.Procesar()
        {

            try
            {
                int resp;
                
                resp = LNArchivo.ProcesarArchivosPendientes();

                if (resp == 0)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
           
            
        }

        void IProcesoNocturno.Iniciar()
        {
            try
            {
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }


        }

        void IProcesoNocturno.Detener()
        {
            try
            {
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }

        }


       
    }
}
