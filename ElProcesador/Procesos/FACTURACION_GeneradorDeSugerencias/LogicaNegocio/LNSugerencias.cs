using CommonProcesador;
using DALAutorizador.BaseDatos;
using DALAutorizador.Entidades;
using DALAutorizador.LogicaNegocio;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace FACTURACION_Generador.LogicaNegocio
{
    class LNSugerencias
    {

        public static void Sugiere()
        {
            try
            {
                //Obtiene las Facturas tipo
                List<FacturaTipo> lasFacturas = DAOFacturaTipo.ListaFacturasTipoParaProcNoc();

                DALCentralAplicaciones.Entidades.Usuario elUser = new DALCentralAplicaciones.Entidades.Usuario();
                elUser.ClaveUsuario = "Procesador Noctuno";
                

                //Procesa una por una
                foreach (FacturaTipo unaFac in lasFacturas)
                {
                    Dictionary<String, ParametroFacturaTipo> losParame = new Dictionary<string, ParametroFacturaTipo>();

                    try
                    {
                        foreach (ParametroFacturaTipo unaProp in DAOFacturaTipo.ListaDeValores(unaFac.ID_FacturaTipo, 0, elUser, new Guid(ConfigurationManager.AppSettings["IdApplication"].ToString())).Values)
                        {

                            if (unaProp.Nombre.Equals("@FechaInicial"))
                            {
                                unaProp.Valor = unaFac.FechaInicial.ToString("yyyy-MM-dd");
                            }
                            else if (unaProp.Nombre.Equals("@FechaFinal"))
                            {
                                unaProp.Valor = unaFac.FechaFinal.ToString("yyyy-MM-dd");
                            }
                            else if (unaProp.Nombre.Equals("@FechaEmision"))
                            {
                                unaProp.Valor = !String.IsNullOrEmpty(unaFac.FechaEmision) ?
                                    unaFac.FechaEmision == unaFac.FechaFinal.ToString("yyyy-MM-dd") ?
                                    unaFac.FechaEmision + " 23:59:59" : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") :
                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else if (unaProp.Nombre.Equals("@Observaciones"))
                            {
                                unaProp.Valor = unaProp.Valor + " Generada por Procesador Nocturno";
                            }

                            losParame.Add(unaProp.Nombre, unaProp);
                        }

                        List<Factura> lasFacturaNueva = LNFactura.GeneraFactura(unaFac.ID_FacturaTipo, losParame, elUser, new Guid(ConfigurationManager.AppSettings["IdApplication"].ToString()));
                    }
                    catch (Exception err)
                    {
                        Logueo.Error(err.ToString());
                    }
                }

            }
            catch (Exception err)
            {
                Logueo.Error(err.ToString());
            }
        }
    }
}
