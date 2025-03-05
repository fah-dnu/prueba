using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using DALAutorizador.Entidades;
using DALAutorizador.LogicaNegocio;
using Framework;
using TPVWEB_ActualizaOperadores.Entidades;
using System.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DALAutorizador.BaseDatos;
using DALAutorizador.Entidades;
using Framework;
using System.Diagnostics;
using DALCentralAplicaciones.Utilidades;
using System.Configuration;
//using DALCajero.BaseDatos;
using System.Data.SqlClient;
using Interfases.Exceptiones;
using DALCentralAplicaciones.Entidades;
using System.Text.RegularExpressions;
using System.Data;
using System.Threading;

namespace TPVWEB_ActualizaOperadores.LogicaNegocio
{
    public class LNCreaActualiza
    {
        public  void ActualizaOper(String elDetalle)
        {
            try
            {
                String[] losDatos = elDetalle.Split('\t');

                if (losDatos.Length >= 2)
                {
                    TPVWEB_ActualizaOperadores.Entidades.Usuario unUsuario = new TPVWEB_ActualizaOperadores.Entidades.Usuario();

                    String ClaveUsuario = losDatos[0];

                    unUsuario.ClaveOperador = ClaveUsuario + "@" + PNConfig.Get("FILEMNTR", "UserSufijo");
                    unUsuario.Password = losDatos[1];



                    try
                    {
                        if (Cliente.EstaEmailEnBaseDatos(unUsuario.ClaveOperador))
                        {
                            //intenta agregar el usuario si es que no existe en BD.
                            try
                            {
                                DALAutorizador.Entidades.Colectiva NuevoUsuario = new DALAutorizador.Entidades.Colectiva();
                                
                                //intenta guardar el Usuario en el Autorizador.
                                LNColectiva.AgregarOperadorToAutoRizadorBB(NuevoUsuario, new DALCentralAplicaciones.Entidades.Usuario() { ClaveUsuario = "Procesador Nocturno" }, new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString()));
                                

                                Cliente objetoCliente = Cliente.Get("");

                                if (losDatos.Length >= 5)
                                {
                                    NuevoUsuario.APaterno = losDatos[3]; //"Operador";
                                    NuevoUsuario.AMaterno = losDatos[4]; //PNConfig.Get("FILEMNTR", "UserSufijo");
                                    NuevoUsuario.NombreORazonSocial = losDatos[2]; //ClaveUsuario.Replace(".", "");
                                }
                                else
                                {
                                    NuevoUsuario.APaterno = "Operador";
                                    NuevoUsuario.AMaterno = PNConfig.Get("FILEMNTR", "UserSufijo");
                                    NuevoUsuario.NombreORazonSocial = ClaveUsuario.Replace(".", "");
                                }

                                NuevoUsuario.Password = unUsuario.Password;
                                NuevoUsuario.ClaveColectiva = unUsuario.ClaveOperador;
                                NuevoUsuario.FechaNacimiento = DateTime.Now;
                                NuevoUsuario.TipoColectiva = new TipoColectiva() { Clave = "OPE", Descripcion = "Operador" };
                                NuevoUsuario.RFC = "";
                                NuevoUsuario.Sexo = 0;
                                NuevoUsuario.Telefono = "5555555555";
                                NuevoUsuario.Movil = "5555555555";
                                NuevoUsuario.CURP = "";
                                NuevoUsuario.Email = unUsuario.ClaveOperador;
                                NuevoUsuario.ID_ColectivaCCM = Int64.Parse(PNConfig.Get("FILEMNTR", "ID_ColectivaCadenaComercial"));

                                if (LNColectiva.AgregarOperadoraCubos(NuevoUsuario, new DALCentralAplicaciones.Entidades.Usuario() { ClaveUsuario = "Procesador Nocturno" }, new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString())) == 0)
                                {
                                    Logueo.Evento("Usuario: " + unUsuario.ClaveOperador + " ha sido modificado agregado a Cubos");
                                }
                            }
                            catch (Exception err)
                            {
                                Logueo.Evento("Usuario: " + unUsuario.ClaveOperador + " solo requiere Actualizacion de Password");
                            }

                            Thread.Sleep(500);
                            //actualiza el Password
                            Cliente.editarPasswordDesdeBaseDatos(unUsuario.Password, unUsuario.ClaveOperador);

                            //activa al operador
                            Cliente.ActualizaEstatus(unUsuario.ClaveOperador, 1);
                            Logueo.Evento("Usuario: " + unUsuario.ClaveOperador + " ha sido modificado");
                        }
                        else
                        {

                            DALAutorizador.Entidades.Colectiva NuevoUsuario = new DALAutorizador.Entidades.Colectiva();

                            if (losDatos.Length >= 5)
                            {
                                NuevoUsuario.APaterno = losDatos[3]; //"Operador";
                                NuevoUsuario.AMaterno = losDatos[4]; //PNConfig.Get("FILEMNTR", "UserSufijo");
                                NuevoUsuario.NombreORazonSocial = losDatos[2]; //ClaveUsuario.Replace(".", "");
                            }
                            else
                            {
                                NuevoUsuario.APaterno = "Operador";
                                NuevoUsuario.AMaterno = PNConfig.Get("FILEMNTR", "UserSufijo");
                                NuevoUsuario.NombreORazonSocial = ClaveUsuario.Replace(".", "");
                            }

                            NuevoUsuario.Password = unUsuario.Password;
                            NuevoUsuario.ClaveColectiva = unUsuario.ClaveOperador;
                            NuevoUsuario.FechaNacimiento = DateTime.Now;
                            NuevoUsuario.TipoColectiva = new TipoColectiva() { Clave = "OPE", Descripcion = "Operador" };
                            NuevoUsuario.RFC = "";
                            NuevoUsuario.Sexo = 0;
                            NuevoUsuario.Telefono = "5555555555";
                            NuevoUsuario.Movil = "5555555555";
                            NuevoUsuario.CURP = "";
                            NuevoUsuario.Email = unUsuario.ClaveOperador;
                            NuevoUsuario.ID_ColectivaCCM = Int64.Parse(PNConfig.Get("FILEMNTR", "ID_ColectivaCadenaComercial"));

                            if (LNColectiva.AgregarOperadorTpvWebAutoBB(NuevoUsuario, new DALCentralAplicaciones.Entidades.Usuario() { ClaveUsuario = "Procesador Nocturno" }, new Guid(ConfigurationManager.AppSettings["IDApplication"].ToString())) == 0)
                            {
                                Logueo.Evento("Usuario: " + unUsuario.ClaveOperador + " ha sido creado Exitosamente");
                            }
                            else
                            {
                                Logueo.Evento("Usuario: " + unUsuario.ClaveOperador + " NO  ha podido ser creado");
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        Logueo.Error("EL Usuario  [" + unUsuario.ClaveOperador + "]  Generó error al definir el nuevo password. " + err.ToString());
                        throw new Exception("EL Usuario  [" + unUsuario.ClaveOperador + "]  Generó error al definir el nuevo password");
                    }
                }
                else
                {
                    Logueo.Error("EL registro  [" + elDetalle + "] no contiene los campos necesarios para procesarlo");
                    throw new Exception("EL registro  [" + elDetalle + "] no contiene los campos necesarios para procesarlo");
                }
            }
            catch (Exception err)
            {
                Logueo.Error("getUsuarios(): " + err.ToString() + ". " + err.Message);
            }
        }
   
    }
}
