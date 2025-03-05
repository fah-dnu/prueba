using CommonProcesador;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using TELEVIP_ImportaTagsAfiliados.Entidades;

namespace TELEVIP_ImportaTagsAfiliados.BaseDatos
{
    class DAOTagTelevia
    {
        
        public static List<Tag> ObtenerNuevosTagsAfiliados()
        {
            List<Tag> losNuevosTags = new List<Tag>();


            try
            {
              //  Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;

                SqlDatabase database = new SqlDatabase(BDTelevip.strBDLectura);
                DbCommand command = database.GetStoredProcCommand("TVIP_ConsultaTagsAfiliados");


                laTabla = database.ExecuteDataSet(command).Tables[0];

                foreach (DataRow renglon in laTabla.Rows)
                {
                    Tag unTag = new Tag();
                    
                    unTag.Fecha_Alta = DateTime.Parse(renglon["fecha_alta"].ToString());
                    unTag.ID_Tag = (renglon["id_tag"].ToString());
                    unTag.ID_cuentaTelevia = renglon["id_cuenta"].ToString();
                    unTag.Importado = false;



                    losNuevosTags.Add(unTag);
                }

                return losNuevosTags;
            }
            catch (Exception ex)
            {

                Logueo.Error("ObtenerNuevosTagsAfiliados()" + ex.Message);
                throw ex;
            }
        }
      
    }
}
