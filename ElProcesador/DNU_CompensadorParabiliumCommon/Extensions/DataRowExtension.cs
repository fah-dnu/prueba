using DNU_CompensadorParabiliumCommon.Utilidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Extensions
{
    public static class DataRowExtension
    {
        public static string GetStringValue(this DataRow row, string key)
        {
            try
            {
                if (row == null)
                    return string.Empty;

                if (row[key] == null)
                    return string.Empty;

                return row[key].ToString();


            }
            catch(Exception ex)
            {
                Log.Error(ex);
                return string.Empty;
            }
        }
    }
}
