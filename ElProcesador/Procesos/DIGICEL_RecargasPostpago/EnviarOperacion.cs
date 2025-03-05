using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DIGICEL_RecargasPostpago.Entidades;
using CommonProcesador;

namespace DIGICEL_RecargasPostpago
{
    public class EnviarOperacion
    {
        public void EnviarLaOperacion(Operacion laOperacion, String wsUsuario, String wsPass, String Sucursal, String Afiliacion, String Terminal)
        {
            try
            {

                wsDNUDigicel.TransactionProcess laConexion = new wsDNUDigicel.TransactionProcess();

                //Loguear Salida

                Logueo.EntradaSalida("[DIGICEL]\t [ENVIADO]\t >>\t[Sell3]\t" + "Usuario:" + wsUsuario + ", MedioPago:" + laOperacion.Telefono + ", Sucursal:" + Sucursal + ", Afiliacion:" + Afiliacion + ", Terminal:" + Terminal + ",Importe:" + laOperacion.Importe + ", Referencia:" + laOperacion.Telefono, wsUsuario, true);


                wsDNUDigicel.trxRespuesta laRespuesta = laConexion.Sell3(wsUsuario, wsPass, laOperacion.Telefono, "TEL", Sucursal, Afiliacion, Terminal, laOperacion.Importe, laOperacion.Telefono, "PN" + DateTime.Now.ToString("HHmmss"), "DIGICEL", "NODISPONIBLE");


                //Loguerar Entrada.
                Logueo.EntradaSalida("[DIGICEL]\t [RECIBIDO]\t <<\t[Sell3]\t" + "Respuesta:" + laRespuesta.codigoRespuesta + ", Descripcion:" + laRespuesta.descripcion + ", Autorizacion:" + laRespuesta.autorizacion + ", Referencia:" + laRespuesta.referencia + ", Leyenda:" + laRespuesta.leyenda, wsUsuario, false);

                if (laRespuesta.codigoRespuesta == 0)
                {
                    Logueo.Evento("Recarga Exitosa al Telefono: " + laOperacion.Telefono + " un monto de " + String.Format("{0:C}", laOperacion.Importe));
                }
                else
                {
                    Logueo.Evento("Recarga Fallida al Telefono: " + laOperacion.Telefono + " un monto de " + String.Format("{0:C}", laOperacion.Importe) + ", " + laRespuesta.codigoRespuesta.ToString() + " " + laRespuesta.descripcion);
                }
            }
            catch (Exception err)
            {
                Logueo.Error("Error con el Telefono: " + laOperacion.Telefono);
            }
        }
    }

}
