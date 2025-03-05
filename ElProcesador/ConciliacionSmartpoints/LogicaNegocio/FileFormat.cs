using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConciliacionSmartpoints.LogicaNegocio
{
    public class FileFormat
    {
        public static void PadFileLines(string path, int longLine, char symbol)
        {
            if (!File.Exists(path))
            {
                throw new Exception("El archivo no existe: " + path);
            }
            var lines = File.ReadAllLines(path, Encoding.ASCII);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length > longLine)
                {
                    throw new Exception("La longitud de la linea es mayor: " + lines[i]);
                }
                else
                {
                    lines[i] = lines[i].PadRight(longLine, symbol);
                }
            }
            File.WriteAllLines(path, lines, Encoding.ASCII);
        }
    }
}
