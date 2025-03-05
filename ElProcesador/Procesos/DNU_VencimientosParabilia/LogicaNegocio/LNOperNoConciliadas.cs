using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DNU_VencimientoCompensacion.BaseDatos;
using DNU_NewRelicNotifications.Services.Wrappers;
using NewRelic.Api.Agent;

namespace DNU_VencimientoCompensacion.LogicaNegocio
{
    public class LNOperNoConciliadas
    {
        private string conn;
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "VENCIMIENTO";
                _NombreNewRelic = PNConfig.Get(ClaveProceso, "NombreNewRelic");
                if (String.IsNullOrEmpty(_NombreNewRelic))
                {
                    _NombreNewRelic = ClaveProceso + "-SINNOMBRE";
                    Logueo.Evento("Se coloco nombre generico para instrumentacion NewRelic al no encontrar el parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                    Logueo.Error("No se encontro parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                }
                else
                {
                    Logueo.Evento("Se encontro parametro NombreNewRelic: " + _NombreNewRelic + " [" + ClaveProceso + "]");
                }
                return _NombreNewRelic;
            }
        }
        
        public LNOperNoConciliadas(string conn)
        {
            this.conn = conn;
        }

        [Transaction]
        public bool validarOperaciones()
        {
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("validarOperaciones");
            bool result = false;
            DataTable dtOperaciones = BDsps.EjecutarSP("Pronoc_Travel_VencimientoOperacionesNoConciliadas", null, conn).Tables["Table"];
            try
            {
                if (dtOperaciones.Rows.Count > 0)
                {
                    if (dtOperaciones.Rows[0]["Codigo"].ToString() == "00")
                        result = true;
                    else
                        result = false;
                    Logueo.Evento("[validarOperaciones] " + dtOperaciones.Rows[0]["Respuesta"].ToString());
                }
            }catch(Exception ex)
            {
                Logueo.Error("[VENCIMIENTO] [validarOperaciones] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
                ApmNoticeWrapper.NoticeException(ex);
            }


            return result;
        }

    }
}
