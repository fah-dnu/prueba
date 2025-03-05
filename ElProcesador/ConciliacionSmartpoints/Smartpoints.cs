using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonProcesador;
using ConciliacionSmartpoints.BaseDatos;
using ConciliacionSmartpoints.Entidades;
using ConciliacionSmartpoints.LogicaNegocio;

namespace ConciliacionSmartpoints
{
    public class Smartpoints : IProcesoNocturno
    {
         void IProcesoNocturno.Detener()
        {
            try
            {
                //LNUsuario.EscucharDirectorio();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }

         void IProcesoNocturno.Iniciar()
        {
            try
            {
                LNArchivo.EscucharDirectorio();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }

         bool IProcesoNocturno.Procesar()
        {
            try
            {
                int resp;
                resp = 0;

                try
                {                    
                    int IdFichero = 0;                   

                    List<Ficheros> losFicheros = DAOFichero.ObtieneFicheros();
               
                    foreach (Ficheros elFichero in losFicheros)
                    {
                        IdFichero = elFichero.IdFichero;
                       
                      Boolean resp2 = LNArchivo.ProcesaArch(elFichero);

                       if (resp2)
                       {
                        DAOFichero.ActializaIdEstatusFichero(IdFichero);
                       }
                    }

                }

                catch (Exception ex)
                {

                    Console.WriteLine(ex.ToString());

                }


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

        public void ProcesarTesteo()
        {
            ConfiguracionContexto.InicializarContexto();
            //LNArchivo.alCambiar(null,null);
          //IProcesoNocturno.Procesar();
            //Iniciar();
            
        }


    }
}
