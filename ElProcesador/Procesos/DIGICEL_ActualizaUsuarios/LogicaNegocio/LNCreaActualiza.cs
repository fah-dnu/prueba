using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
//using DALAutorizador.Entidades;
//using DALAutorizador.LogicaNegocio;
using Framework;
using DIGICEL_ActualizaUsuarios.Entidades;
using System.Configuration;

namespace DIGICEL_ActualizaUsuarios.LogicaNegocio
{
    public class LNCreaActualiza
    {
        public  void ActualizaOper(String elDetalle)
        {
            try
            {
                String[] losDatos = elDetalle.Split('|'); 

                if (losDatos.Length >= 2)
                {
                    Usuario unUsuario = new Usuario();

                    String ClaveUsuario = losDatos[1];

                    unUsuario.Baja = losDatos[0] == "1" ? true : false;
                    unUsuario.Telefono = losDatos[1];
                    unUsuario.TipoDigicel = losDatos[2];

                    try
                    {
                       
                            if (losDatos.Length >= 6)
                            {
                                unUsuario.ApPaterno = losDatos[4].Trim().Length == 0 ? "Digicel" : losDatos[4];
                                unUsuario.ApMaterno = losDatos[5].Trim().Length == 0 ? "Panamá" : losDatos[5];
                                unUsuario.Nombre = losDatos[3].Trim().Length == 0 ? "Usuario" : losDatos[3]; 
                            }
                            else
                            {
                                unUsuario.ApPaterno = "Digicel";
                                unUsuario.ApMaterno = "Panamá";
                                unUsuario.Nombre = "Usuario"; 
                            }



                            DIGICEL_ActualizaUsuarios.mx.com.dnu.sitios.WS_Administracion LaOperacion = new DIGICEL_ActualizaUsuarios.mx.com.dnu.sitios.WS_Administracion();

                           int laRespuesta= LaOperacion.CrearUsuarioShopAndWin(PNConfig.Get("DGUSERS", "wsUsuario"), PNConfig.Get("DGUSERS", "wsPassword"), unUsuario.Telefono, unUsuario.Nombre, unUsuario.ApPaterno, unUsuario.ApMaterno, unUsuario.Telefono, "", unUsuario.TipoDigicel, unUsuario.Baja,unUsuario.Baja);

                            if (laRespuesta==0)
                            {
                                Logueo.Evento("Usuario: " + unUsuario.ClaveOperador + " ha sido creado Exitosamente");
                            }
                            else
                            {
                                Logueo.Evento("Usuario: " + unUsuario.ClaveOperador + "NO  ha podido ser creado");
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
