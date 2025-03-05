using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.Entidades
{
    //public class Colectiva
    //{
    //    Direccion _DUbicacion = new Direccion();
    //    Direccion _DFacturacion = new Direccion();
    //    EstatusColectiva elEstatus = new EstatusColectiva();
    //    TipoColectiva elTipoColectiva = new TipoColectiva();

    //    public Int32 Sexo { get; set; }
    //    public Int64 ID_Colectiva { get; set; }
    //    public Int64 ID_ColectivaPadre { get; set; }
    //    public Int64 ID_ColectivaCCM { get; set; }

    //    public String CLABE { get; set; }
    //    public String ActividadComercial { get; set; }
    //    public String NombreComercial { get; set; }
    //    public String TipoPersona { get; set; }


    //    public TipoColectiva TipoColectiva
    //    {
    //        get
    //        {
    //            return elTipoColectiva;
    //        }
    //        set { elTipoColectiva = value; }
    //    }
    //    public EstatusColectiva EstatusColectiva
    //    {
    //        get
    //        {
    //            return elEstatus;
    //        }
    //        set { elEstatus = value; }
    //    }
    //    public Direccion DUbicacion
    //    {
    //        get
    //        {
    //            return _DUbicacion;
    //        }
    //        set { _DUbicacion = value; }
    //    }
    //    public Direccion DFacturacion
    //    {
    //        get
    //        {
    //            return _DFacturacion;
    //        }
    //        set { _DFacturacion = value; }
    //    }
    //    public String ClaveColectiva { get; set; }
    //    public String NombreORazonSocial { get; set; }
    //    public String APaterno { get; set; }
    //    public String AMaterno { get; set; }
    //    public String RFC { get; set; }
    //    public String CURP { get; set; }
    //    public String Telefono { get; set; }
    //    public String Movil { get; set; }
    //    private String _Password = "";// { get; set; }

    //    public String Password
    //    {
    //        get { return _Password; }
    //        set { _Password = value; }
    //    }


    //    public DateTime FechaNacimiento { get; set; }
    //    public String Email { get; set; }
    //    private Dictionary<String, Parametro> losParametrosExtras = new Dictionary<string, Parametro>();

    //    public Dictionary<String, Parametro> LosParametrosExtras
    //    {
    //        set { losParametrosExtras = value; }
    //        get { return losParametrosExtras; }
    //    }

    //    public String ObtieneParametro(String elParametro)
    //    {

    //        try
    //        {

    //            String laRespuesta = "";

    //            if (losParametrosExtras.ContainsKey(elParametro))
    //            {
    //                laRespuesta = losParametrosExtras[elParametro].Valor;
    //            }
    //            else
    //            {
    //                laRespuesta = "";
    //            }

    //            return laRespuesta;

    //        }
    //        catch (Exception err)
    //        {
    //            return "";
    //        }


    //    }


    //    public String NombreCompleto()
    //    {
    //        StringBuilder resp = new StringBuilder();

    //        resp.Append(this.NombreORazonSocial);
    //        resp.Append(" " + this.APaterno);
    //        resp.Append(" " + this.AMaterno);


    //        return resp.ToString().Replace(".", "").Trim();
    //    }

    //    public String Domicilio()
    //    {
    //        StringBuilder resp = new StringBuilder();

    //        resp.Append(this.DFacturacion.Calle);
    //        resp.Append(" " + this.DFacturacion.NumExterior);
    //        if (this.DFacturacion.NumInterior.Trim().Length != 0)
    //        {
    //            resp.Append(", Interior " + this.DFacturacion.NumInterior);
    //        }


    //        resp.Append(" " + this.DFacturacion.EntreCalles);


    //        if (this.DFacturacion.Referencias.Trim().Length != 0)
    //        {
    //            resp.Append(", " + this.DFacturacion.Referencias);
    //        }


    //        return resp.ToString().Replace(".", "").Trim();
    //    }

    //    public String CiudadEstado()
    //    {
    //        StringBuilder resp = new StringBuilder();

    //        //resp.Append("Codigo Postal: ");
    //        resp.Append(this.DFacturacion.Asentamiento.CodigoPostal);
    //        resp.Append(" " + this.DFacturacion.Asentamiento.DesAsentamiento);
    //        resp.Append(", " + this.DFacturacion.Asentamiento.LaCiudad.DesCiudad);
    //        resp.Append(", " + this.DFacturacion.Asentamiento.ElMunicipio.DesMunicipio);
    //        resp.Append(", " + this.DFacturacion.Asentamiento.ElEstado.Descripcion);


    //        return resp.ToString().Replace(".", "").Trim();
    //    }




    //    public String ToString()
    //    {
    //        StringBuilder resp = new StringBuilder();

    //        resp.Append("ID_Colectiva:" + this.ID_Colectiva + "; ");
    //        resp.Append("ClaveColectiva:" + this.ClaveColectiva + "; ");
    //        resp.Append("Nombre:" + this.NombreORazonSocial + "; ");
    //        resp.Append("Apellido Paterno:" + this.APaterno + "; ");
    //        resp.Append("Apellido Materno:" + this.AMaterno + "; ");
    //        resp.Append("TipoColectiva:" + this.TipoColectiva + "; ");

    //        return resp.ToString();
    //    }

    //}

    //public class TipoColectiva
    //{
    //    public TipoColectiva(String unaClave)
    //    {
    //        Clave = unaClave;
    //    }

    //    public TipoColectiva()
    //    {
    //    }
    //    public Int16 ID_TipoColectiva { get; set; }
    //    public String Clave { get; set; }
    //    public String Descripcion { get; set; }
    //}

    //public class EstatusColectiva
    //{
    //    public Int16 ID_EstatusColectiva { get; set; }
    //    public String Clave { get; set; }
    //    public String Descripcion { get; set; }
    //}
}
