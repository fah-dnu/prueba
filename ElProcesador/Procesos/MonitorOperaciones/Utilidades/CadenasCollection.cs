using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace MonitoreaOperaciones.Utilidades
{
    [ConfigurationCollection(typeof(CadenaElemento), AddItemName = "Cadena")]
    public class CadenasCollection : ConfigurationElementCollection
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new CadenaElemento();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CadenaElemento)(element)).ID_CadenaComercial;
        }


        public CadenaElemento this[int index]
        {
            get { return (CadenaElemento)BaseGet(index); }
        }

        public CadenaElemento this[string ID_CadenaComercial]
        {
            get { return (CadenaElemento)BaseGet(ID_CadenaComercial); }
        }

    }
}
