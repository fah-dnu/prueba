using DNU_NewRelicNotifications.Models;
using NewRelic.Api.Agent;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_NewRelicNotifications.BussinesLayer
{
    class BLApmTransaction
    {
        private readonly DAOApmTransaction dataAccesObjectClient;
        public BLApmTransaction(string conexion)
        {
            dataAccesObjectClient = new DAOApmTransaction(conexion);
        }
        [Trace]
        public ClientInfo GetClientInfo(ClientInfo requestClientInfo, string usuario, string pUserTemp, Guid? idLog, String ClaveEmpresa)
        {
            DataTable tablaSpeiId = dataAccesObjectClient.GetClientInfo(requestClientInfo, pUserTemp, ClaveEmpresa);
            requestClientInfo.client = "";
            requestClientInfo.clientID = "";
            if (!(tablaSpeiId is null))
            {
                if (tablaSpeiId.Rows.Count > 0)
                {
                    if (tablaSpeiId.Columns.Count > 1 && tablaSpeiId.Rows[0][0].ToString() != "error")
                    {
                        requestClientInfo.client = tablaSpeiId.Rows[0][0].ToString();
                        requestClientInfo.clientID = tablaSpeiId.Rows[0][1].ToString();
                    }
                }
            }
            return requestClientInfo;
        }

    }
}
