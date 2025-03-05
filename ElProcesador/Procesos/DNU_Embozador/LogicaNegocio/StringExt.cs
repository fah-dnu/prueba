using DNU_Embozador.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.LogicaNegocio
{
    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength, char paddChar = ' ')
        {
            if (Constants.VERSION.Equals("2") || Constants.VERSION.Equals("3") || Constants.VERSION.Equals("4"))
            {
                if (paddChar.Equals(' '))
                {
                    if (string.IsNullOrEmpty(value)) return value;
                    return value.Length <= maxLength ? value.PadRight(maxLength, paddChar) : value.Substring(0, maxLength);
                }
                else
                {
                    if (string.IsNullOrEmpty(value)) return value;
                    return value.Length <= maxLength ? value.PadLeft(maxLength, paddChar) : value.Substring(0, maxLength);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value)) return value;
                return value.Length <= maxLength ? value.PadLeft(maxLength, paddChar) : value.Substring(0, maxLength);
            }
        }
    }
}
