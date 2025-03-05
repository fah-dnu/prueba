using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonitoreaCuentas.Utilidades;
using CommonProcesador;
using CommonProcesador.Utilidades;
using System.IO;
using MonitoreaCuentas.Entidades;
using MonitoreaCuentas.BaseDatos;
using System.Configuration;

namespace MonitoreaCuentas.LogicaNegocio
{
    public class LNMonitor
    {
        public static int EnviaEmailCuentasSaldoBajo()
        {
            try
            {
                List<Cuenta> lasCuentas = new List<Cuenta>();
                StringBuilder elHTML = new StringBuilder();
                
                lasCuentas = DAOCuentas.ListarCuentasBajoSaldo();

               
                if (lasCuentas.Count == 0)
                {
                    throw new Exception("No hay cuentas para alertar por saldo bajo.");
                }

                foreach (Cuenta unaCta in lasCuentas)
                {
                    elHTML = new StringBuilder();

                    elHTML.Append("<tr>");
                    elHTML.Append("<td>");
                    elHTML.Append(unaCta.ID_Cuenta);
                    elHTML.Append("</td>");
                    elHTML.Append("<td>");
                    elHTML.Append(unaCta.CuentaHabiente);
                    elHTML.Append("</td>");
                    elHTML.Append("<td><i>");
                    elHTML.Append(unaCta.nombreCuenta);
                    elHTML.Append("</i></td>");
                    elHTML.Append("<td> <b>");
                    elHTML.Append(String.Format("{0:C}",unaCta.Saldo));
                    elHTML.Append("</b></td>");
                    elHTML.Append("</tr>");
//                    ListaDistribucion=unaCta.ListaDistribucion;

                    LNMonitor.EnviarEmail(elHTML.ToString(), unaCta.ListaDistribucion, unaCta.CuentaHabiente);
                    Logueo.Error("se envio Corrreo a "  + unaCta.ListaDistribucion + ": ID_Cuenta" + unaCta.ID_Cuenta);
                }

                return 0;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }


        public static void EnviarEmail(String ListadoCuentas, String ListaDistribucion, String strColectiva)
        {

            try
            {
                StringBuilder emailHtml = new StringBuilder(File.ReadAllText(ConfigurationManager.AppSettings["HtmlSaldos"].ToString()));

                emailHtml.Replace("[LISTADOCUENTAS]", ListadoCuentas);

                
                LNEmail.Send(ListaDistribucion, "info@dnu.mx", emailHtml.ToString(), "¡Saldo por Agotarse " + strColectiva + "!");
                
            }
            catch (Exception err)
            {
                Logueo.Error("Error al Enviar correo:" + err.Message);
                throw err;
            }
        }
    }
}
