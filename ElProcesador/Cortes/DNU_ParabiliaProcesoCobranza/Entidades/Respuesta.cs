using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCobranza.Entidades
{
    public class Respuesta
    {
        public Decimal Saldo { get; set; }
        public String Tarjeta { get; set; }

        public Int64 ID_Poliza { get; set; }
        public String Autorizacion { get; set; }
        public String XmlExtras { get; set; }
        public int CodigoRespuesta { get; set; }
        public String Descripcion { get; set; }

        override public string ToString()
        {
            try
            {
                StringBuilder laCadena = new StringBuilder();

                Type type = this.GetType();
                PropertyInfo[] properties = type.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    // Console.WriteLine("Name: " + property.Name + ", Value: " + property.GetValue(this, null));}

                    String elValor = String.Format("{0}", property.GetValue(this, null));

                    if (elValor.Trim().Length != 0)
                    {
                        laCadena.Append(property.Name);
                        laCadena.Append(":");
                        laCadena.Append(property.GetValue(this, null));
                        laCadena.Append(", ");
                    }

                }

                return laCadena.ToString();
            }
            catch (Exception err)
            {
                return "";
            }
        }
    }
}
