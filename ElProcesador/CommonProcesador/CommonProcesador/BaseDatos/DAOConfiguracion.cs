using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador.Entidades;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System.Data;

using System.Data.Common;
using System.Configuration;
using Dnu.AutorizadorParabiliaAzure.Services;
using Dnu.AutorizadorParabiliaAzure.Models;

namespace CommonProcesador.BaseDatos
{
    public class DAOConfiguracion
    {
        public static Dictionary<String, Dictionary<String, Propiedad>> GetConfiguraciones(string app, string appPass)
        {
            DataSet losDatos = null;
            // Dictionary<String, List<String>> Respuesta = new Dictionary<String, List<string>>();

            Dictionary<String, Dictionary<String, Propiedad>> laRespuesta = new Dictionary<String, Dictionary<String, Propiedad>>();
            Dictionary<String, Propiedad> lasPropiedades = new Dictionary<string, Propiedad>();

            try
            {
                //  Logueo.Error(DBProceso.strBDLectura);
                SqlDatabase database = new SqlDatabase(DBProceso.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("Proc_ObtieneConfiguracion");

                losDatos = database.ExecuteDataSet(command);

                if (null != losDatos)
                {
                    String AppActual;
                    List<String> paginas = new List<string>();
                    for (int k = 0; k < losDatos.Tables[0].Rows.Count; k++)
                    {
                        AppActual = losDatos.Tables[0].Rows[k]["Clave"] == null ? "" : (String)losDatos.Tables[0].Rows[k]["Clave"]; //(String)losDatos.Tables[0].Rows[k]["RoleName"];
                        bool LeyoDatos = true;

                        while ((AppActual == (losDatos.Tables[0].Rows[k]["Clave"] == null ? "" : (String)losDatos.Tables[0].Rows[k]["Clave"])))
                        {
                            //String pag = (string)losDatos.Tables[0].Rows[k]["Path"];
                            String nombre = losDatos.Tables[0].Rows[k]["Nombre"] == null ? "" : (String)losDatos.Tables[0].Rows[k]["Nombre"];
                            String value = losDatos.Tables[0].Rows[k]["Valor"] == null ? "" : (String)losDatos.Tables[0].Rows[k]["Valor"];
                            int ID_Conf = losDatos.Tables[0].Rows[k]["ID_Configuracion"] == null ? 0 : (int)losDatos.Tables[0].Rows[k]["ID_Configuracion"];
                            bool esSecret = false;
                            try
                            {
                                esSecret = losDatos.Tables[0].Rows[k]["EsSecreto"] == null ? false : (bool)losDatos.Tables[0].Rows[k]["EsSecreto"];
                            }
                            catch (Exception ex)
                            {
                                esSecret = false;
                            }

                            value = ValidateSecret(app, appPass, esSecret, value);

                            lasPropiedades.Add(nombre.ToUpper(), new Propiedad(nombre, value, AppActual, ID_Conf, esSecret));

                            if (k == (losDatos.Tables[0].Rows.Count - 1))
                            {
                                break;
                            }
                            else
                            {
                                k++;// LeyoDatos = losDatos.Read();
                            }

                        }
                        // Console.WriteLine("Almacenar rol :" + rolActual);
                        laRespuesta.Add(AppActual, lasPropiedades);
                        //  Console.WriteLine("rol Almacenado :" + rolActual);

                        lasPropiedades = new Dictionary<string, Propiedad>();

                        if (LeyoDatos)
                        {
                            String nombre = losDatos.Tables[0].Rows[k]["Nombre"] == null ? "" : (String)losDatos.Tables[0].Rows[k]["Nombre"];
                            String value = losDatos.Tables[0].Rows[k]["Valor"] == null ? "" : (String)losDatos.Tables[0].Rows[k]["Valor"];
                            int ID_Conf = losDatos.Tables[0].Rows[k]["ID_Configuracion"] == null ? 0 : (int)losDatos.Tables[0].Rows[k]["ID_Configuracion"];
                            bool esSecret = false;
                            try
                            {
                                esSecret = losDatos.Tables[0].Rows[k]["EsSecreto"] == null ? false : (bool)losDatos.Tables[0].Rows[k]["EsSecreto"];
                            }
                            catch (Exception ex)
                            {
                                esSecret = false;
                            }

                            value = ValidateSecret(app, appPass, esSecret, value);

                            lasPropiedades.Add(nombre.ToUpper(), new Propiedad(nombre, value, AppActual, ID_Conf, esSecret));

                            //paginas.Add((string)(losDatos.Tables[0].Rows[k]["Path"] == null ? "" : String.Format((string)losDatos.Tables[0].Rows[k]["Path"]).ToUpper()));

                            AppActual = losDatos.Tables[0].Rows[k]["Clave"] == null ? "" : (String)losDatos.Tables[0].Rows[k]["Clave"];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception("Ha sucedido un error al obtener las Paginas por roles: " + ex);
            }

            return laRespuesta;
        }

        private static string ValidateSecret(string app, string appPass, bool esSecret, string valor) 
        {
            if (esSecret) 
            {
                responseAzure respuestaObtenerCadena = KeyVaultProvider.ObtenerValoresClaveAzureKeyVault(app, appPass, valor);

                valor = respuestaObtenerCadena.valorAzure;

                if (!respuestaObtenerCadena.codRespuesta.Equals("0000"))
                    Logueo.Error(respuestaObtenerCadena.desRespuesta);
            }

            return valor;
        }



    }
}
