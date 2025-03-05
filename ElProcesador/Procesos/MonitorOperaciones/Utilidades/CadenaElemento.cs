using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace MonitoreaOperaciones.Utilidades
{
  public class CadenaElemento : ConfigurationElement
{

    [ConfigurationProperty("ID_CadenaComercial", DefaultValue = "0", IsKey = true, IsRequired = true)]
        public Int64 ID_CadenaComercial
    {
        get { return ((Int64)(base["ID_CadenaComercial"])); }
        set { base["ID_CadenaComercial"] = value; }
    }

    [ConfigurationProperty("Activas", DefaultValue = "0", IsKey = false, IsRequired = true)]
    public Int64 Activas
    {
        get { return ((Int64)(base["Activas"])); }
        set { base["Activas"] = value; }
    }

    [ConfigurationProperty("Operaciones", DefaultValue = "0", IsKey = false, IsRequired = true)]
    public Int64 Operaciones
    {
        get { return ((Int64)(base["Operaciones"])); }
        set { base["Operaciones"] = value; }
    }

    [ConfigurationProperty("Declinadas", DefaultValue = "0", IsKey = false, IsRequired = true)]
    public Int64 Declinadas
    {
        get { return ((Int64)(base["Declinadas"])); }
        set { base["Declinadas"] = value; }
    }


    [ConfigurationProperty("ListaDistribucion", DefaultValue = "mt@dnu.mx;lt@dnu.mx", IsKey = false, IsRequired = true)]
    public String ListaDistribucion
    {
        get { return ((String)(base["ListaDistribucion"])); }
        set { base["ListaDistribucion"] = value; }
    }

}

}
