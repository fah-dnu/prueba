using CommonProcesador;
using DNU_ConciliadorCacao.Entidades;
using DNU_ConciliadorCacao.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNU_ConciliadorCacao.LogicaNegocio
{
   public class LNComparacion
    {

       public static String GeneraConsultaWhereSQL ( List<ComparadorConfig> lasComparaciones,
           string minProcessFileDate)
       {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            StringBuilder laConsulta = new StringBuilder();

            try
            {

                //laConsulta.Append("WHERE ");
                int elTamanio = 0;
                foreach (ComparadorConfig elComparador in lasComparaciones)
                {


                    laConsulta.Append(" DatosBD.");
                    laConsulta.Append("C");
                    laConsulta.Append(elComparador.elCampoBD.Posicion.ToString().PadLeft(3, '0'));
                    laConsulta.Append(elComparador.elComparador);
                    laConsulta.Append(" FicheroDetalle.");
                    laConsulta.Append("C");
                    laConsulta.Append(elComparador.elCampoArchivo.Posicion.ToString().PadLeft(3, '0'));

                    elTamanio++;

                    if (elTamanio < lasComparaciones.Count)
                    {
                        laConsulta.Append(" AND ");
                    }

                }


            }
            catch (Exception err)
            {
                LogueoProConciliaCacao.Error("[" + ip + "] [ConciliarCacao] [PROCESADORNOCTURNO] [" + log + "] [GeneraConsultaSQL:" + err.Message + "]");
                 throw new Exception("GeneraConsultaSQL:" + err.Message);
            }

            String consultaResult = String.Format("{0} AND ((FicheroDetalle.C003 = DatosBD.C003) OR (DatosBD.C003 = '{1}') )",
               laConsulta.ToString(),
               minProcessFileDate);


            return consultaResult;

        }
    }
}
