using DNU_Cortador.BaseDatos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNU_Cortador.LogicaNegocio
{
    public class LNCortes
    {
        public static bool Televia_ActualizaEstatusDiarios()
        {
            try{

                DNU_Cortador.BaseDatos.DAOCortes.Televia_CalculaEstatusDiario();

                return true;
            } catch (Exception err)
            {
                return false;
            }
        }

        public static bool Televia_GenerarLotesDeCortes()
        {
            try
            {

                DNU_Cortador.BaseDatos.DAOCortes.Televia_generarCorte();

                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

       
    }
}
