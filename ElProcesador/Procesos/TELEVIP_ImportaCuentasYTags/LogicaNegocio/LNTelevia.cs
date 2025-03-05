using CommonProcesador;
using DNU.Lealtad.Utils.Televia;
using DNU.Lealtad.Utils.Televia.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace TELEVIP_ImportaTagsAfiliados.LogicaNegocio
{
    class LNTelevia
    {

               
        public static List<String> ObtieneTagsDeCuenta(String  laCuenta, String Password)
        {
            List<String> losTags = new List<String>();
            String ElFlujo = "";
            try
            {
                //1- CONSULTAR EL USUARIO ORIGINAL DE TELEVIA.
                ElFlujo += "ObtieneTagsDeCuenta: 1,";
               
                //2- REEMPLAZAR LOS CAMPOS EDITABLES EN EL OBJETO OBTENIDO DE TELEVIA.
                var televiaWSInput = new TeleviaWSInput();
                ElFlujo += "2,";
                var mess = new Message();
                mess.IdAccount = "0000773910";// laCuenta; // "0000773910";
                mess.PassAccount = "0127"; //Password; // laPeticion.PassTelevia; // "0127";

                var televiaProcess = new TeleviaProcess();
                ElFlujo += "3,";
                var auth = new Authorization();
                auth.ServiceName =  "Meda";
                auth.Password = "0TPr0vd3R";

                televiaWSInput.Message = mess;
                televiaWSInput.Authorization = auth;
                televiaWSInput.Message.NewCustomerCompleteMobile = null;

                Logueo.EntradaSalida("ENVIADO TELEVIA: " + new JavaScriptSerializer().Serialize(televiaWSInput), "ObtieneTagsDeCuenta", false);

                var res5 = televiaProcess.InfoBasicCustomer(televiaWSInput);

                ElFlujo += "9,";

                Logueo.EntradaSalida("RECIBIDO TELEVIA: " + res5.ResponseCode + ": " + res5.ResponseMessage + ";" + " JSON:" + new JavaScriptSerializer().Serialize(res5), "ObtieneTagsDeCuenta", true);


                ElFlujo += "10,";
                if (Int32.Parse(res5.ResponseCode) == 0)
                {
                    foreach (Row unTagAsignado in res5.MessageBasicInfo.Row)
                    {
                      //  TagSaldo unTag = new TagSaldo(unTagAsignado.NumberTag);
                        //unTag.Saldo = float.Parse(unTagAsignado.Balance);
                        losTags.Add(unTagAsignado.NumberTag);
                    }
                   
                }

                return losTags;
            }
            catch (Exception err)
            {
                Logueo.Error("LNTeleviaServer.ObtieneTagsDeCuenta()" + err.Message);
                throw new Exception("NO SE OBTUVERON LOS TAGS DE LA CUENTA " + laCuenta + ": " + err.Message);

            }
            finally
            {
                Logueo.Error(ElFlujo);

            }

            return losTags;
        }
    
    

    }
}
