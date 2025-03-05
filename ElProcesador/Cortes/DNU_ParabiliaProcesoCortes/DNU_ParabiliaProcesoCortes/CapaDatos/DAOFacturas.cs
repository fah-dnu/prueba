//using DALAutorizador.Entidades;
using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using DNU_ParabiliaProcesoCortes.dataService;
using DNU_ParabiliaProcesoCortes.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.CapaDatos
{
    class DAOFacturas
    {
        private BaseDeDatos consultaBDRead;
        private BaseDeDatos consultaBDWrite;
        private DataTable dtResultado;
        public string nuevaPoliza { get; set; }
        public DAOFacturas()
        {

            consultaBDRead = new BaseDeDatos("");
            consultaBDWrite = new BaseDeDatos("");

        }
        public DataTable InsertarFactura(Factura solicitudDatos, Int64 idCorte, SqlConnection conn = null, SqlTransaction transaccionSQL = null, bool sinDeuda = false)
        {
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@fechaEmision", parametro = sinDeuda ? DateTime.Now : solicitudDatos.FechaEmision });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@fechainicial", parametro = sinDeuda ? DateTime.Now : solicitudDatos.FechaEmision });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@fechafinal", parametro = sinDeuda ? DateTime.Now : solicitudDatos.FechaEmision });
                //if (!sinDeuda)
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@fechatimbrado", parametro =null solicitudDatos.FechaTimbrado });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@folio", parametro = solicitudDatos.Folio });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@metodoPago", parametro = solicitudDatos.MetodoPago });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@lugarExpedicion", parametro = solicitudDatos.LugarExpedicion });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@iva", parametro = solicitudDatos.IVA });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@subtotal", parametro = solicitudDatos.SubTotal });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@importeTotal", parametro = (Decimal.Round(solicitudDatos.SubTotal, 2) + Decimal.Round(solicitudDatos.IVA, 2)) });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@cadenaOriginal", parametro = solicitudDatos.CadenaOriginal });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@noCertificadoSAT", parametro = solicitudDatos.NoCertificadoSAT });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@noCertificadoEmisor", parametro = solicitudDatos.NoCertificadoEmisor });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@uuid", parametro = solicitudDatos.UUID });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@sello", parametro = solicitudDatos.Sello });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@selloCFD", parametro = solicitudDatos.SelloCFD });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@selloSAT", parametro = solicitudDatos.SelloSAT });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@xmlTimbre", parametro = solicitudDatos.XMLTimbre });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@xmlCDFI", parametro = solicitudDatos.XMLCFDI });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@formaDePago", parametro = solicitudDatos.FormaPago });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@rfcEmisor", parametro = solicitudDatos.Emisora.RFC });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@rfcReceptor", parametro = solicitudDatos.Receptora.RFC });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@urlQrCode", parametro = solicitudDatos.UrlQrCode });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@cadenaOriginalTimbre", parametro = solicitudDatos.CadenaOriginalTimbre });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idTipoComprobante", parametro = solicitudDatos.TipoComprobante });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idCorte", parametro = idCorte });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idColectivaEmisora", parametro = solicitudDatos.Emisora.ID_Colectiva });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idColectivaReceptora", parametro = solicitudDatos.Receptora.ID_Colectiva });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@nombreEmisor", parametro = solicitudDatos.Emisora.NombreORazonSocial });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@regimenFiscal", parametro = solicitudDatos.RegimenFiscal });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@usoCFDI", parametro = solicitudDatos.UsoCFDI });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@rfcProvedorCertificado", parametro = solicitudDatos.RFCProvedorCertificado });
                int detalles = 1;
                foreach (DetalleFactura detalle in solicitudDatos.losDetalles) {
                    consultaBDRead.verificarParametrosNulosString(parametros, "@nombreProducto" + detalles.ToString(), detalle.NombreProducto);
                    consultaBDRead.verificarParametrosNulosString(parametros, "@impImporte" + detalles.ToString(), detalle.impImporte);
                    consultaBDRead.verificarParametrosNulosString(parametros, "@cantidadProducto" + detalles.ToString(), detalle.Cantidad.ToString());
                    consultaBDRead.verificarParametrosNulosString(parametros, "@totalProducto" + detalles.ToString(), detalle.Total.ToString());
                    consultaBDRead.verificarParametrosNulosString(parametros, "@precioUnitarioProducto" + detalles.ToString(), detalle.PrecioUnitario.ToString());
                    consultaBDRead.verificarParametrosNulosString(parametros, "@ivaProducto" + detalles.ToString(), detalle.impTasaOCuota.ToString());//este se agrega proque a pesar de que
                                                                                                                                                      //en el catalogo producto viene, puede ser que se cambie el iva en los parametrso multiasignacion y ya no cuadraria

                    detalles++;
                }

                //parametros.Add(new ParametrosProcedimiento { Nombre = "@tipoMedioAcceso", parametro = solicitudDatos.tipoMedioAcceso });
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@claveEvento", parametro = solicitudDatos.claveEvento });
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@importe", parametro = solicitudDatos.importe });

                // consultaBDRead.verificarParametrosNulosString(parametros, "@folioOrigen", solicitudSpeiIn.FolioOrigen);

                dtResultado = consultaBDRead.ConstruirConsulta("[EJECUTOR_GenerarNuevaFactura]", parametros, "obtenerDatosTarjetaSpeiParaExecute", "", conn, transaccionSQL);
                return dtResultado;
            }
            catch (Exception ex)
            {

                return dtResultado;
            }
        }

        public DataTable InsertarFacturaExterna(Factura solicitudDatos, Int64 idCorte, SqlConnection conn = null, SqlTransaction transaccionSQL = null, bool sinDeuda = false)
        {
            try
            {
                if (conn is null) {
                    string conexion = DBProcesadorArchivo.strBDEscrituraAutorizador;
                    consultaBDRead = new BaseDeDatos(conexion);//DBProcesadorArchivo.strBDEscrituraAutorizador);
                }
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@fechaEmision", parametro = sinDeuda ? DateTime.Now : solicitudDatos.FechaEmision });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@fechainicial", parametro = sinDeuda ? DateTime.Now : solicitudDatos.FechaEmision });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@fechafinal", parametro = sinDeuda ? DateTime.Now : solicitudDatos.FechaEmision });
                //if (!sinDeuda)
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@fechatimbrado", parametro =null solicitudDatos.FechaTimbrado });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@folio", parametro = solicitudDatos.Folio });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@metodoPago", parametro = solicitudDatos.MetodoPago });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@lugarExpedicion", parametro = solicitudDatos.LugarExpedicion });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@iva", parametro = solicitudDatos.IVA });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@subtotal", parametro = solicitudDatos.SubTotal });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@importeTotal", parametro = (Decimal.Round(solicitudDatos.SubTotal, 2) + Decimal.Round(solicitudDatos.IVA, 2)) });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@cadenaOriginal", parametro = solicitudDatos.CadenaOriginal });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@noCertificadoSAT", parametro = solicitudDatos.NoCertificadoSAT });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@noCertificadoEmisor", parametro = solicitudDatos.NoCertificadoEmisor });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@uuid", parametro = solicitudDatos.UUID });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@sello", parametro = solicitudDatos.Sello });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@selloCFD", parametro = solicitudDatos.SelloCFD });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@selloSAT", parametro = solicitudDatos.SelloSAT });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@xmlTimbre", parametro = solicitudDatos.XMLTimbre });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@xmlCDFI", parametro = solicitudDatos.XMLCFDI });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@formaDePago", parametro = solicitudDatos.FormaPago });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@rfcEmisor", parametro = solicitudDatos.Emisora.RFC });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@rfcReceptor", parametro = solicitudDatos.Receptora.RFC });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@urlQrCode", parametro = solicitudDatos.UrlQrCode });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@cadenaOriginalTimbre", parametro = solicitudDatos.CadenaOriginalTimbre });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idTipoComprobante", parametro = solicitudDatos.TipoComprobante });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idCorte", parametro = idCorte });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idColectivaEmisora", parametro = solicitudDatos.Emisora.ID_Colectiva });
            //    parametros.Add(new ParametrosProcedimiento { Nombre = "@idColectivaReceptora", parametro = solicitudDatos.Receptora.ID_Colectiva });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@nombreEmisor", parametro = solicitudDatos.Emisora.NombreORazonSocial });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@regimenFiscal", parametro = solicitudDatos.RegimenFiscal });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@usoCFDI", parametro = solicitudDatos.UsoCFDI });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@rfcProvedorCertificado", parametro = solicitudDatos.RFCProvedorCertificado });
                int detalles = 1;
                foreach (DetalleFactura detalle in solicitudDatos.losDetalles)
                {
                    consultaBDRead.verificarParametrosNulosString(parametros, "@nombreProducto" + detalles.ToString(), detalle.NombreProducto);
                    consultaBDRead.verificarParametrosNulosString(parametros, "@impImporte" + detalles.ToString(), detalle.impImporte);
                    consultaBDRead.verificarParametrosNulosString(parametros, "@cantidadProducto" + detalles.ToString(), detalle.Cantidad.ToString());
                    consultaBDRead.verificarParametrosNulosString(parametros, "@totalProducto" + detalles.ToString(), detalle.Total.ToString());
                    consultaBDRead.verificarParametrosNulosString(parametros, "@precioUnitarioProducto" + detalles.ToString(), detalle.PrecioUnitario.ToString());
                    consultaBDRead.verificarParametrosNulosString(parametros, "@ivaProducto" + detalles.ToString(), detalle.impTasaOCuota.ToString());//este se agrega proque a pesar de que
                                                                                                                                                      //en el catalogo producto viene, puede ser que se cambie el iva en los parametrso multiasignacion y ya no cuadraria

                    detalles++;
                }

                //parametros.Add(new ParametrosProcedimiento { Nombre = "@tipoMedioAcceso", parametro = solicitudDatos.tipoMedioAcceso });
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@claveEvento", parametro = solicitudDatos.claveEvento });
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@importe", parametro = solicitudDatos.importe });

                // consultaBDRead.verificarParametrosNulosString(parametros, "@folioOrigen", solicitudSpeiIn.FolioOrigen);

                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_ProcesaEstadoCuenta_GenerarNuevaFacturaExterna]", parametros, "Procnoc_ProcesaEstadoCuenta_GenerarNuevaFacturaExterna", "", conn, transaccionSQL);
                return dtResultado;
            }
            catch (Exception ex)
            {

                return dtResultado;
            }
        }

        public DataTable ActualizaFactura(Int64 idFactura ,Factura solicitudDatos, Int64 idCorte, SqlConnection conn = null, SqlTransaction transaccionSQL = null, bool sinDeuda = false)
        {
            try
            {
                if (conn is null)
                {
                    consultaBDRead = new BaseDeDatos(DBProcesadorArchivo.strBDEscrituraAutorizador);
                }
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idFactura", parametro = idFactura });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@fechatimbrado", parametro =solicitudDatos.FechaTimbrado });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@cadenaOriginal", parametro = solicitudDatos.CadenaOriginal });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@noCertificadoSAT", parametro = solicitudDatos.NoCertificadoSAT });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@noCertificadoEmisor", parametro = solicitudDatos.NoCertificadoEmisor });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@uuid", parametro = solicitudDatos.UUID });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@sello", parametro = solicitudDatos.Sello });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@selloCFD", parametro = solicitudDatos.SelloCFD });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@selloSAT", parametro = solicitudDatos.SelloSAT });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@xmlTimbre", parametro = solicitudDatos.XMLTimbre });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@xmlCDFI", parametro = solicitudDatos.XMLCFDI });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@urlQrCode", parametro = solicitudDatos.UrlQrCode });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@cadenaOriginalTimbre", parametro = solicitudDatos.CadenaOriginalTimbre });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@rfcProvedorCertificado", parametro = solicitudDatos.RFCProvedorCertificado });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@folio", parametro = solicitudDatos.Folio });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@EmisoraRFC", parametro = solicitudDatos.Emisora.RFC });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@EmisoraNombreORazonSocial", parametro = solicitudDatos.Emisora.NombreORazonSocial });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@LugarExpedicion", parametro = solicitudDatos.LugarExpedicion });


                //factura.Emisora.RFC = "PIC9508303ZA";
                //factura.Emisora.NombreORazonSocial = "PRODUCTOS INTEGRALES EN COMUNICACION";
                //factura.LugarExpedicion = "52105";// "01030";


                // parametros.Add(new ParametrosProcedimiento { Nombre = "@idTipoComprobante", parametro = solicitudDatos.TipoComprobante });
                // parametros.Add(new ParametrosProcedimiento { Nombre = "@usoCFDI", parametro = solicitudDatos.UsoCFDI });
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@tipoMedioAcceso", parametro = solicitudDatos.tipoMedioAcceso });
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@claveEvento", parametro = solicitudDatos.claveEvento });
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@importe", parametro = solicitudDatos.importe });

                // consultaBDRead.verificarParametrosNulosString(parametros, "@folioOrigen", solicitudSpeiIn.FolioOrigen);

                dtResultado = consultaBDRead.ConstruirConsulta("[EJECUTOR_ActualizarFactura]", parametros, "obtenerDatosTarjetaSpeiParaExecute", "", conn, transaccionSQL);
                return dtResultado;
            }
            catch (Exception ex)
            {

                return dtResultado;
            }
        }


        public DataTable ObtenerDatosMovimientosEvento(string evento, string ivaEvento, string idCuenta, string idCorte, SqlConnection conn = null, SqlTransaction transaccionSQL = null, bool sinDeuda = false)
        {
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@IdCuenta", parametro = idCuenta });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idCorte", parametro = idCorte });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ClaveEvento", parametro = evento });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@IvaClaveEvento", parametro = ivaEvento });

                //parametros.Add(new ParametrosProcedimiento { Nombre = "@tipoMedioAcceso", parametro = solicitudDatos.tipoMedioAcceso });
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@claveEvento", parametro = solicitudDatos.claveEvento });
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@importe", parametro = solicitudDatos.importe });

                // consultaBDRead.verificarParametrosNulosString(parametros, "@folioOrigen", solicitudSpeiIn.FolioOrigen);

                dtResultado = consultaBDRead.ConstruirConsulta("[Ejecutor_Calcula_Montos_Estado_Cuenta]", parametros, "Ejecutor_Calcula_Montos_Estado_Cuenta", "", conn, transaccionSQL);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] Error al ejecutar el SP de obtencion polizas:" + ex.Message+ex.StackTrace);

                return dtResultado;
            }
        }

        public DataTable CalculaInteresDiferimiento(string evento, string ivaEvento, string idCuenta, string idCorte, SqlConnection conn = null, SqlTransaction transaccionSQL = null, bool sinDeuda = false)
        {
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@IdCuenta", parametro = idCuenta });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idCorte", parametro = idCorte });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ClaveEvento", parametro = evento });
                parametros.Add(new ParametrosProcedimiento { Nombre = "@IvaClaveEvento", parametro = ivaEvento });

                //parametros.Add(new ParametrosProcedimiento { Nombre = "@tipoMedioAcceso", parametro = solicitudDatos.tipoMedioAcceso });
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@claveEvento", parametro = solicitudDatos.claveEvento });
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@importe", parametro = solicitudDatos.importe });

                // consultaBDRead.verificarParametrosNulosString(parametros, "@folioOrigen", solicitudSpeiIn.FolioOrigen);

                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_Cortes_CalculaInteresDiferimiento]", parametros, "Ejecutor_Calcula_Montos_Estado_Cuenta", "", conn, transaccionSQL);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] Error al ejecutar el SP de obtencion polizas:" + ex.Message + ex.StackTrace);

                return dtResultado;
            }
        }

        public DataTable ObtieneTarjetasAdicionales(Int64 IDCUENTA, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {//
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@idCuenta", parametro = IDCUENTA });
     
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@tipoMedioAcceso", parametro = solicitudDatos.tipoMedioAcceso });
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@claveEvento", parametro = solicitudDatos.claveEvento });
                //parametros.Add(new ParametrosProcedimiento { Nombre = "@importe", parametro = solicitudDatos.importe });

                // consultaBDRead.verificarParametrosNulosString(parametros, "@folioOrigen", solicitudSpeiIn.FolioOrigen);

                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_Cortes_ObtieneTarjetasAdicionales]", parametros, "Procnoc_Cortes_ObtieneTarjetasAdicionales", "", conn, transaccionSQL);
                return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] Error al ejecutar el SP de obtencion polizas:" + ex.Message + ex.StackTrace);

                return dtResultado;
            }

        }
    }
}
