using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    class Evento
    {
        public Int64 ID_ConfiguracionCorte { get; set; }

        // public Int64 ID_AgrupadorEvento { get; set; }
        public int ID_AgrupadorEvento { get; set; }
        public String ClaveEventoAgrupador { get; set; }
        public String ClaveEvento { get; set; }
        public int ID_Evento { get; set; }
        public String Descripcion { get; set; }

        public String ClaveCadenaComercial { get; set; }

        public Int64 ID_CadenaComercial { get; set; }

        public Int64 ID_Contrato { get; set; }

        public Int64 Consecutivo { get; set; }
        public String Parametros_Input { get; set; }
        public String Stored_Procedure { get; set; }
        public string generaPoliza { get; set; }
        public string incluirEnXML { get; set; }
        public string eventoPrincipal { get; set; }
        public string descripcionEventoEdoCuenta { get; set; }
        public string unidadSAT { get; set; }
        public string ClaveProdServSAT { get; set; }
        public string ClaveUnidadSAT { get; set; }
        public string impImpuestoSAT { get; set; }
        public string impTipoFactorSAT { get; set; }
        public string parametroMonto { get; set; }
        public string id_corteEventoAgrupador { get; set; }
        //
    }

}
