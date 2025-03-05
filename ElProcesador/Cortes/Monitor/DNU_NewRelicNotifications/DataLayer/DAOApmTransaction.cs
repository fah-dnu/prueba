using DNU_NewRelicNotifications.DataLayer;
using DNU_NewRelicNotifications.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_NewRelicNotifications.BussinesLayer
{
    class DAOApmTransaction
    {
        private DataDaseConnection queryBDRead;
        private DataTable dtResult;

        public DAOApmTransaction(string conexion)
        {
            queryBDRead = new DataDaseConnection(conexion);

        }
        public DataTable GetClientInfo(ClientInfo requestClientInfo, string pUserTemp, string ClaveEmpresa)
        {
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();

                queryBDRead.verificarParametrosNulosObjetos(parametros, "@tarjeta", requestClientInfo.card);
                queryBDRead.verificarParametrosNulosObjetos(parametros, "@medioAcceso", requestClientInfo.mediaAccess);
                queryBDRead.verificarParametrosNulosObjetos(parametros, "@tipoMedioAcceso", requestClientInfo.typeMediaAccess);
                queryBDRead.verificarParametrosNulosString(parametros, "@id_colectiva", ClaveEmpresa);
             //   queryBDRead.verificarParametrosNulosString(parametros, "@UserTemp", pUserTemp);
              //  parametros.Add(new ParametrosProcedimiento { Nombre = "@AppId", parametro = ConfigurationManager.AppSettings["appID"] });


                dtResult = queryBDRead.ConstruirConsulta("[ws_Parabilia_ObtenerDatosclienteNewRelicNotification]", parametros, "GetClientInfo");
                return dtResult;
            }
            catch (Exception ex)
            {
                //
                return dtResult;
            }
        }
    }
}
