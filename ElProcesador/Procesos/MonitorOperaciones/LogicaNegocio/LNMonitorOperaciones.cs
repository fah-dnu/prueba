using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonitoreaOperaciones.Utilidades;
using CommonProcesador;
using CommonProcesador.Utilidades;
using System.IO;
using MonitoreaOperaciones.Entidades;
using MonitoreaOperaciones.BaseDatos;
using System.Configuration;

namespace MonitoreaOperaciones.LogicaNegocio
{
    public class LNMonitorOperaciones
    {
        public static int EnviaEmailOperaciones()
        {
            try
            {
                List<DetalleOperaciones> lasCuentas = new List<DetalleOperaciones>();
                StringBuilder elHTML = new StringBuilder();
                
                lasCuentas = DAOOperaciones.ObtieneEstatusOperaciones();


              
               
                if (lasCuentas.Count == 0)
                {
                    throw new Exception("No hay Cadenas con los valores fuera del rango.");
                }


                foreach (MonitoreaOperaciones.Utilidades.CadenaElemento unElemento in MonitoreaOperaciones.Utilidades.Config.Instance().Cadenas.CadenasItems.Cast<MonitoreaOperaciones.Utilidades.CadenaElemento>())
                {
                   // Console.WriteLine("CadenaComercial:{0} LIActivas: {1} ListaDistr: {2}", unElemento.ID_CadenaComercial, unElemento.LI_Activas, unElemento.ListaDistribucion);

                    foreach (DetalleOperaciones unDetalle in lasCuentas)
                    {

                        if (unDetalle.ID_CadenaComercial == unElemento.ID_CadenaComercial)
                        {
                            elHTML = new StringBuilder();

                            elHTML.Append("<tr>");
                            elHTML.Append("<td>");
                            elHTML.Append(unDetalle.ID_CadenaComercial);
                            elHTML.Append("-");
                            elHTML.Append(unDetalle.NombreCadenaComercial);
                            elHTML.Append("</td>");
                            elHTML.Append("<td>");
                            elHTML.Append(unDetalle.Activas);
                            elHTML.Append("</td>");
                            elHTML.Append("<td>");
                            elHTML.Append(unDetalle.Declinadas);
                            elHTML.Append("</td>");
                            elHTML.Append("<td>");
                            elHTML.Append(unDetalle.Codigos);
                            elHTML.Append("</td>");

                            if ((unDetalle.Activas + unDetalle.Declinadas) == 0)
                            {
                                //no hay suficientes operaciones 
                                LNMonitorOperaciones.EnviarEmail(elHTML.ToString(), "!NO HAY OPERACIONES " + unDetalle.NombreCadenaComercial.ToUpper() + "!", unElemento.ListaDistribucion);
                                continue;
                            }

                            if ((unDetalle.Activas < unElemento.Activas ) & unDetalle.Activas!=0)
                            {
                                //Hay menos operaciones Activas que las normales.
                                LNMonitorOperaciones.EnviarEmail(elHTML.ToString(), "!HAY POCAS OPERACIONES AUTORIZADAS " + unDetalle.NombreCadenaComercial.ToUpper() + "!", unElemento.ListaDistribucion);
                                continue;
                            }

                            if (unDetalle.Declinadas > unElemento.Declinadas)
                            {
                                //Hay muchas decliandas
                                LNMonitorOperaciones.EnviarEmail(elHTML.ToString(), "!HAY MUCHAS OPERACIONES DECLINADAS " + unDetalle.NombreCadenaComercial.ToUpper() + "!", unElemento.ListaDistribucion);
                                continue;
                            }

                            if ((unDetalle.Activas + unDetalle.Declinadas) < unElemento.Operaciones | (unDetalle.Activas + unDetalle.Declinadas) < unElemento.Operaciones)
                            {
                                //no hay suficientes operaciones 
                                LNMonitorOperaciones.EnviarEmail(elHTML.ToString(), "!HAY MUY POCAS OPERACIONES " + unDetalle.NombreCadenaComercial.ToUpper() + "!", unElemento.ListaDistribucion);
                                continue;
                            }

                          
                        }
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);

                
            }
        }


        public static void EnviarEmail(String ListadoCuentas, String Subject, String ListaDistribucion)
        {

            try
            {
                StringBuilder emailHtml = new StringBuilder(File.ReadAllText(ConfigurationManager.AppSettings["HtmlMonitorOperaciones"].ToString()));

                emailHtml.Replace("[LISTADOCUENTAS]", ListadoCuentas);



                LNEmail.Send(ListaDistribucion,"info@dnu.mx", emailHtml.ToString(), Subject);
            }
            catch (Exception err)
            {
                Logueo.Error("Error al Enviar correo:" + err.Message);
               // throw err;
            }
        }
    }
}
