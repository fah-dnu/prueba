using CommonProcesador;
using DNU_ProcesadorGenerico.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNU_ProcesadorGenerico.LogicaNegocio
{
   public class LNComparacion
    {

       public static String GeneraConsultaWhereSQL ( List<ComparadorConfig> lasComparaciones)
       {
           StringBuilder laConsulta= new StringBuilder();

           try
           {

               //laConsulta.Append("WHERE ");
               int elTamanio =0;
            foreach(ComparadorConfig elComparador in lasComparaciones)
            {
                

                laConsulta.Append (" DatosBD.");
                laConsulta.Append ("C");
                laConsulta.Append(elComparador.elCampoBD.Posicion.ToString().PadLeft(3, '0'));
                laConsulta.Append(elComparador.elComparador);
                laConsulta.Append(" FicheroDetalle.");
                laConsulta.Append("C");
                laConsulta.Append(elComparador.elCampoArchivo.Posicion.ToString().PadLeft(3, '0'));

                elTamanio++;

                if (elTamanio<lasComparaciones.Count)
                {
                    laConsulta.Append(" AND ");
                }

            }


           }
           catch (Exception err)
           {
               Logueo.Error("GeneraConsultaSQL:" + err.Message);
               throw new Exception("GeneraConsultaSQL:" + err.Message);
           }
           return laConsulta.ToString();
       }
    }
}
