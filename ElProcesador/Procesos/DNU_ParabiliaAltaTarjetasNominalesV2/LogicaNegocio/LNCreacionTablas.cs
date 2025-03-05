using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaAltaTarjetasNominales.LogicaNegocio
{
    class LNCreacionTablas
    {
        internal static DataTable crearDataTable()
        {
            DataTable dtDatosnew = new DataTable("DetalleAltaTarjetasNominativas");
            var dc = new DataColumn("FechaEnvio", Type.GetType("System.String"));
            var dc1 = new DataColumn("ClaveCliente", Type.GetType("System.String"));
            var dc2 = new DataColumn("ClaveBIN", Type.GetType("System.String"));
            var dc3 = new DataColumn("ClaveEmpleadora", Type.GetType("System.String"));
            var dc4 = new DataColumn("NumeroEmpleado", Type.GetType("System.String"));
            var dc5 = new DataColumn("Nombre", Type.GetType("System.String"));
            var dc6 = new DataColumn("PrimerApellido", Type.GetType("System.String"));
            var dc7 = new DataColumn("SegundoApellido", Type.GetType("System.String"));
            var dc8 = new DataColumn("NombreEmbozado", Type.GetType("System.String"));
            var dc9 = new DataColumn("Telefono", Type.GetType("System.String"));
            var dc10 = new DataColumn("Correo", Type.GetType("System.String"));
            var dc11 = new DataColumn("TipoMedioAcceso", Type.GetType("System.String"));
            var dc12 = new DataColumn("MedioAcceso", Type.GetType("System.String"));
            var dc13 = new DataColumn("TipoTarjeta", Type.GetType("System.String"));
            var dc14 = new DataColumn("TarjetaTitular", Type.GetType("System.String"));
            var dc15 = new DataColumn("TipoMedioAccesoTitular", Type.GetType("System.String"));
            var dc16 = new DataColumn("MedioAccesoTitular", Type.GetType("System.String"));
            var dc17 = new DataColumn("ID_EstatusCACAO", Type.GetType("System.String"));
            var dc18 = new DataColumn("ReintentosCACAO", Type.GetType("System.String"));
            var dc19 = new DataColumn("ID_Archivo", Type.GetType("System.String"));
            var dc20 = new DataColumn("Subproducto", Type.GetType("System.String"));

            //FIELDS DIRECTION
            var dc21 = new DataColumn("Calle", Type.GetType("System.String"));
            var dc22 = new DataColumn("NoExterior", Type.GetType("System.String"));
            var dc23 = new DataColumn("NoInterior", Type.GetType("System.String"));
            var dc24 = new DataColumn("Colonia", Type.GetType("System.String"));
            var dc25 = new DataColumn("DelegacionMun", Type.GetType("System.String"));
            var dc26 = new DataColumn("Ciudad", Type.GetType("System.String"));
            var dc27 = new DataColumn("Estado", Type.GetType("System.String"));
            var dc28 = new DataColumn("CP", Type.GetType("System.String"));
            var dc29 = new DataColumn("Pais", Type.GetType("System.String"));


            dtDatosnew.Columns.Add(dc);
            dtDatosnew.Columns.Add(dc1);
            dtDatosnew.Columns.Add(dc2);
            dtDatosnew.Columns.Add(dc3);
            dtDatosnew.Columns.Add(dc4);
            dtDatosnew.Columns.Add(dc5);
            dtDatosnew.Columns.Add(dc6);
            dtDatosnew.Columns.Add(dc7);
            dtDatosnew.Columns.Add(dc8);
            dtDatosnew.Columns.Add(dc9);
            dtDatosnew.Columns.Add(dc10);
            dtDatosnew.Columns.Add(dc11);
            dtDatosnew.Columns.Add(dc12);
            dtDatosnew.Columns.Add(dc13);
            dtDatosnew.Columns.Add(dc14);
            dtDatosnew.Columns.Add(dc15);
            dtDatosnew.Columns.Add(dc16);
            dtDatosnew.Columns.Add(dc17);
            dtDatosnew.Columns.Add(dc18);
            dtDatosnew.Columns.Add(dc19);
            dtDatosnew.Columns.Add(dc20);

            dtDatosnew.Columns.Add(dc21);
            dtDatosnew.Columns.Add(dc22);
            dtDatosnew.Columns.Add(dc23);
            dtDatosnew.Columns.Add(dc24);
            dtDatosnew.Columns.Add(dc25);
            dtDatosnew.Columns.Add(dc26);
            dtDatosnew.Columns.Add(dc27);
            dtDatosnew.Columns.Add(dc28);
            dtDatosnew.Columns.Add(dc29);


            return dtDatosnew;
        }

        internal static DataTable crearDataTableProducto()
        {
            DataTable dtDatosnew = new DataTable("DetalleAltaTarjetasNominativas");
            var dc = new DataColumn("FechaEnvio", Type.GetType("System.String"));
            var dc1 = new DataColumn("ClaveCliente", Type.GetType("System.String"));
            var dc2 = new DataColumn("ClaveBIN", Type.GetType("System.String"));
            var dc3 = new DataColumn("ClaveEmpleadora", Type.GetType("System.String"));
            var dc4 = new DataColumn("NumeroEmpleado", Type.GetType("System.String"));
            var dc5 = new DataColumn("Nombre", Type.GetType("System.String"));
            var dc6 = new DataColumn("PrimerApellido", Type.GetType("System.String"));
            var dc7 = new DataColumn("SegundoApellido", Type.GetType("System.String"));
            var dc8 = new DataColumn("NombreEmbozado", Type.GetType("System.String"));
            var dc9 = new DataColumn("Telefono", Type.GetType("System.String"));
            var dc10 = new DataColumn("Correo", Type.GetType("System.String"));
            var dc11 = new DataColumn("TipoMedioAcceso", Type.GetType("System.String"));
            var dc12 = new DataColumn("MedioAcceso", Type.GetType("System.String"));
            var dc13 = new DataColumn("TipoTarjeta", Type.GetType("System.String"));
            var dc14 = new DataColumn("TarjetaTitular", Type.GetType("System.String"));
            var dc15 = new DataColumn("TipoMedioAccesoTitular", Type.GetType("System.String"));
            var dc16 = new DataColumn("MedioAccesoTitular", Type.GetType("System.String"));
            var dc17 = new DataColumn("ID_EstatusCACAO", Type.GetType("System.String"));
            var dc18 = new DataColumn("ReintentosCACAO", Type.GetType("System.String"));
            var dc19 = new DataColumn("ClaveSubproducto", Type.GetType("System.String"));
            var dc20 = new DataColumn("ID_Archivo", Type.GetType("System.String"));

            dtDatosnew.Columns.Add(dc);
            dtDatosnew.Columns.Add(dc1);
            dtDatosnew.Columns.Add(dc2);
            dtDatosnew.Columns.Add(dc3);
            dtDatosnew.Columns.Add(dc4);
            dtDatosnew.Columns.Add(dc5);
            dtDatosnew.Columns.Add(dc6);
            dtDatosnew.Columns.Add(dc7);
            dtDatosnew.Columns.Add(dc8);
            dtDatosnew.Columns.Add(dc9);
            dtDatosnew.Columns.Add(dc10);
            dtDatosnew.Columns.Add(dc11);
            dtDatosnew.Columns.Add(dc12);
            dtDatosnew.Columns.Add(dc13);
            dtDatosnew.Columns.Add(dc14);
            dtDatosnew.Columns.Add(dc15);
            dtDatosnew.Columns.Add(dc16);
            dtDatosnew.Columns.Add(dc17);
            dtDatosnew.Columns.Add(dc18);
            dtDatosnew.Columns.Add(dc19);
            dtDatosnew.Columns.Add(dc20);

            return dtDatosnew;
        }

        internal static DataTable crearDataTableCredito()
        {
            DataTable dtDatosnew = new DataTable("DetalleAltaTarjetasNominativas");
            var dc = new DataColumn("FechaEnvio", Type.GetType("System.String"));
            var dc1 = new DataColumn("ClaveCliente", Type.GetType("System.String"));
            var dc2 = new DataColumn("ClaveBIN", Type.GetType("System.String"));
            var dc3 = new DataColumn("ClaveEmpleadora", Type.GetType("System.String"));
            var dc4 = new DataColumn("NumeroEmpleado", Type.GetType("System.String"));
            var dc5 = new DataColumn("Nombre", Type.GetType("System.String"));
            var dc6 = new DataColumn("PrimerApellido", Type.GetType("System.String"));
            var dc7 = new DataColumn("SegundoApellido", Type.GetType("System.String"));
            var dc8 = new DataColumn("NombreEmbozado", Type.GetType("System.String"));
            var dc9 = new DataColumn("Telefono", Type.GetType("System.String"));
            var dc10 = new DataColumn("Correo", Type.GetType("System.String"));
            var dc11 = new DataColumn("TipoMedioAcceso", Type.GetType("System.String"));
            var dc12 = new DataColumn("MedioAcceso", Type.GetType("System.String"));
            var dc13 = new DataColumn("TipoTarjeta", Type.GetType("System.String"));
            var dc14 = new DataColumn("TarjetaTitular", Type.GetType("System.String"));
            var dc15 = new DataColumn("TipoMedioAccesoTitular", Type.GetType("System.String"));
            var dc16 = new DataColumn("MedioAccesoTitular", Type.GetType("System.String"));
            var dc17 = new DataColumn("ClaveSubproducto", Type.GetType("System.String"));
            var dc18 = new DataColumn("LimiteCredito", Type.GetType("System.String"));
            var dc19 = new DataColumn("DiaCorte", Type.GetType("System.String"));
            var dc20 = new DataColumn("ID_EstatusCACAO", Type.GetType("System.String"));
            var dc21 = new DataColumn("ReintentosCACAO", Type.GetType("System.String"));

            dtDatosnew.Columns.Add(dc);
            dtDatosnew.Columns.Add(dc1);
            dtDatosnew.Columns.Add(dc2);
            dtDatosnew.Columns.Add(dc3);
            dtDatosnew.Columns.Add(dc4);
            dtDatosnew.Columns.Add(dc5);
            dtDatosnew.Columns.Add(dc6);
            dtDatosnew.Columns.Add(dc7);
            dtDatosnew.Columns.Add(dc8);
            dtDatosnew.Columns.Add(dc9);
            dtDatosnew.Columns.Add(dc10);
            dtDatosnew.Columns.Add(dc11);
            dtDatosnew.Columns.Add(dc12);
            dtDatosnew.Columns.Add(dc13);
            dtDatosnew.Columns.Add(dc14);
            dtDatosnew.Columns.Add(dc15);
            dtDatosnew.Columns.Add(dc16);
            dtDatosnew.Columns.Add(dc17);
            dtDatosnew.Columns.Add(dc18);
            dtDatosnew.Columns.Add(dc19);
            dtDatosnew.Columns.Add(dc20);
            dtDatosnew.Columns.Add(dc21);

            return dtDatosnew;
        }
    }
}
