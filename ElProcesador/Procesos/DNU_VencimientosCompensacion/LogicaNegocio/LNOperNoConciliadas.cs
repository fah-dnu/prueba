using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DNU_VencimientoCompensacion.BaseDatos;

namespace DNU_VencimientoCompensacion.LogicaNegocio
{
    public class LNOperNoConciliadas
    {
        private string conn;
        public LNOperNoConciliadas(string conn)
        {
            this.conn = conn;
        }

        public bool validarOperaciones()
        {
            bool result = false;
            DataTable dtOperaciones = BDsps.EjecutarSP("Pronoc_Travel_VencimientoOperacionesNoConciliadas", null, conn).Tables["Table"];
            try
            {
                if (dtOperaciones.Rows.Count > 0)
                {
                    Logueo.Evento("[validarOperaciones] ");
                    if (dtOperaciones.Rows[0]["Codigo"].ToString() == "00")
                    {
                        Logueo.Evento("[validarOperaciones] =00");
                        result = true;
                    }
                    else
                        result = false;
                    Logueo.Evento("[validarOperaciones] " + dtOperaciones.Rows[0]["Respuesta"].ToString());
                }
            }catch(Exception ex)
            {
                Logueo.Error("[VENCIMIENTO] [validarOperaciones] [Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }


            return result;
        }

    }
}
