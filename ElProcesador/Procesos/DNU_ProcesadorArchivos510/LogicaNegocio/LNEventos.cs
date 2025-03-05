using DNU_ProcesadorArchivos.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorArchivos510.LogicaNegocio
{
    class LNEventos
    {

        public static bool EjecutarEventos(DataTable losEventos, Archivo elArchivo)
        {
            try
            {

                for (int i = 0; i < losEventos.Rows.Count; i++)
                {

                    for (int j = 0; j < losEventos.Columns.Count; j++)
                    {

                        losEventos.Rows[i][j].ToString();
                    }
                   
                  
                }

            }
            catch (Exception err)
            {

                return false;
            }
            return true;

        }
    }
}
