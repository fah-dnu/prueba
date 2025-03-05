using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.LogicaNegocio
{
    public class ProcessFile
    {

        public bool ValidaNombreArchivo(string validationStringRegex, string fileName)
        {
            fileName = fileName.Substring(0, fileName.Length - (fileName.Length - fileName.IndexOf('.')));

            if (!Regex.Match(fileName, validationStringRegex).Success)
            {
                return false;
            }

            return true;
        }
    }
}
