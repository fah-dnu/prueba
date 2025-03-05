using DNU_ParabiliaProcesoCortes.Common.Entidades;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.LogicaNegocio;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace DNU_ParabiliaProcesoCortes.Contratos
{
    abstract class Facturacion
    {
        public abstract Factura obtenerDatosFacturaCFDI4(Dictionary<string, Parametro> todosLosParametros, Decimal sumaComisiones, Decimal ivaComision, Decimal iva, Decimal ivaIntereses, decimal impIvaOrd, decimal impIvaMor, bool facturaEnBlanco, Cuentas cuenta, List<DetalleFactura> listaDetallesFactura, List<DetalleFactura> listaDetallesFacturaXML, LNValidacionesCampos _validaciones, string folioFactura, Cuentas cuentaEmisorPAC,
            List<DetalleFactura> listaDetallesFacturaExtra);
        public abstract bool generarFacturaV4(Factura factura, string ruta, bool facturaEnBlanco, string tarjeta, List<String> archivos, Cuentas cuenta, XslCompiledTransform _transformador, string nombreArchivo, bool rutaProcesado, string nombreXML = null);
    }
}
