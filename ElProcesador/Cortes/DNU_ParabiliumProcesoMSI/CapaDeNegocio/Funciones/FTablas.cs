using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliumProcesoMSI.CapaDeNegocio.Funciones
{
    internal class FTablas
    {

        internal static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                    {
                        if (pro.Name.ToLower().Contains("fecha"))
                        {
                            pro.SetValue(obj, Convert.ToDateTime(dr[column.ColumnName].ToString()), null);
                        }
                        else
                        {
                            pro.SetValue(obj, dr[column.ColumnName].ToString(), null);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return obj;
        }
    }
   
}
