using CommonProcesador;
using DALAutorizador.BaseDatos;
using DALAutorizador.Entidades;
using DALAutorizador.LogicaNegocio;
using DALAutorizador.Utilidades;
using DALCentralAplicaciones.Entidades;
using DALCentralAplicaciones.Utilidades;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace FACTURACION_Timbrador.LogicaNegocio
{
    public class LNPagos
    {
        public static Boolean AsignarPagoAFactura(Int64 ID_Factura, Int64 ID_Poliza, decimal importePago, Usuario elUser, Guid laAPP)
        {

            try
            {
                Decimal elImportePago = 0;
               
                using (SqlConnection conn = BDAutorizador.BDEscritura)
                {
                    conn.Open();

                    elImportePago = DAOFactura.ObtieneSaldoPagoPendienteAsignar(ID_Poliza);

                    if (!AsignaFacturaPagoExistente(ID_Factura, ID_Poliza, conn, elUser))
                    {
                        Factura laFactura = DAOFactura.ObtieneFactura(ID_Factura, elUser, laAPP);

                        Pago elPago = new Pago();

                        elPago.FechaExpedicion = DateTime.Now;
                        elPago.LugarExpedicion = laFactura.getParametro("@LugarExpedicion") == null ? "NO DEFINIDO" : laFactura.getParametro("@LugarExpedicion");
                        elPago.NumeroCertificado = laFactura.NoCertificadoEmisor;

                        //obtiene folio y Serie-
                        //DAOFactura.ObtieneSerieFolioPago(ref elPago, laFactura.Emisora.ID_Colectiva, conn, transaccionSQL);

                        elPago.TipoComprobante = "P";
                        elPago.Sello = "";

                        Factura LaFacturaPago = laFactura;

                        LaFacturaPago.FechaEmision = DateTime.Now;
                        LaFacturaPago.LugarExpedicion = laFactura.getParametro("@LugarExpedicion") == null ? "NO DEFINIDO" : laFactura.getParametro("@LugarExpedicion");
                        LaFacturaPago.NoCertificadoEmisor = laFactura.NoCertificadoEmisor;

                        LaFacturaPago.ID_FacturaTipo = 0;

                        LaFacturaPago.Folio = elPago.Folio;
                        LaFacturaPago.Serie = elPago.Serie;
                        LaFacturaPago.SubTotal = elImportePago;
                        LaFacturaPago.IVA = 0;
                        LaFacturaPago.ImporteTotal = elImportePago;
                        LaFacturaPago.DescripcionFactura = "PAGO DE FACTURA: " + laFactura.Serie + laFactura.Folio;
                        LaFacturaPago.FechaInicial = LaFacturaPago.FechaEmision;
                        LaFacturaPago.FechaFinal = LaFacturaPago.FechaEmision;
                        LaFacturaPago.TotalConLetra = Utilerias.enLetras(LaFacturaPago.ImporteTotal.ToString());
                        LaFacturaPago.TipoComprobante = "P";
                        LaFacturaPago.MetodoPago = "PUE";
                        LaFacturaPago.ParametrosCalculados = laFactura.ParametrosCalculados;

                        LaFacturaPago.losDetalles = new List<DetalleFactura>();
                        DetalleFactura unDetallePago = new DetalleFactura();
                        unDetallePago.Cantidad = 1;
                        unDetallePago.ClaveProdServ = "84111506";
                        unDetallePago.ClaveUnidad = "ACT";
                        unDetallePago.PrecioUnitario = 0;
                        unDetallePago.ID_Producto = Int32.Parse(Configuracion.Get(Guid.Parse(ConfigurationManager.AppSettings["IDApplication"].ToString()), "ID_ProductoPago").Valor);
                        unDetallePago.NombreProducto = "PAGO";

                        LaFacturaPago.losDetalles.Add(unDetallePago);

                        using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                        {

                            try
                            {

                                DAOFactura.guardarFactura(ref LaFacturaPago, conn, transaccionSQL);

                                foreach (ParametroFacturaTipo elParametroAGuardar in laFactura.ParametrosCalculados.Values)
                                {

                                    DAOFacturaTipo.GuardarParametrodeCreacionFactura(LaFacturaPago.ID_Factura, elParametroAGuardar, conn, transaccionSQL);
                                }

                                LNFactura.AsignaFolioFacturaPago(LaFacturaPago.ID_Factura, conn, transaccionSQL);

                                DAOFactura.AsignarPagoaFactura(ID_Factura, ID_Poliza, LaFacturaPago.ID_Factura, conn, transaccionSQL);



                                transaccionSQL.Commit();
                            }
                            catch (Exception err)
                            {
                                Logueo.Error(err.Message);
                                transaccionSQL.Rollback();
                                return false;
                            }

                        }

                    }

                    return TimbrarPago(ID_Poliza, elImportePago, elUser, laAPP);
                    
                }

            }
            catch (Exception ex)
            {

                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID_Factura"></param>
        /// <param name="ID_Poliza"></param>
        /// <param name="elUser"></param>
        protected static Boolean AsignaFacturaPagoExistente(Int64 ID_Factura, Int64 ID_Poliza, SqlConnection conn, Usuario elUser)
        {
            Int64 idFacturaPago = 0;

            try
            {
                idFacturaPago = DAOFactura.ObtieneIdFacturaPago(ID_Poliza, elUser);

                if (idFacturaPago > 0)
                {
                    using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                    {
                        try
                        {
                            DAOFactura.AsignarPagoaFactura(ID_Factura, ID_Poliza, idFacturaPago, conn, transaccionSQL);

                            transaccionSQL.Commit();
                        }
                        catch (Exception err)
                        {
                            Logueo.Error(err.Message);
                            transaccionSQL.Rollback();
                            return false;
                        }
                    }

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }
            

        public static Boolean TimbrarPago(Int64 ID_Pago, Decimal elImportePago, Usuario elUser, Guid laAPP)
        {
            try
            {
                using (SqlConnection conn = BDAutorizador.BDEscritura)
                {
                    conn.Open();

                    try
                    {

                        List<Factura> lasFacturas = DAOFactura.ObtieneFacturasDePago(ID_Pago);

                        if (lasFacturas.Count == 0)
                        {

                            throw new Exception("ERROR NO HAY FACTURAS ASGINADAS AL PAGO");
                        }

                        Factura laFactura = DAOFactura.ObtieneFacturaPago(lasFacturas[0].ID_FacturaPago, elUser, laAPP);

                        CFDIPago elCFDIPago = new CFDIPago(lasFacturas, laFactura, elImportePago);

                        using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                        {
                            try
                            {
                                //RELACIONA EN BD EL IMPORTE ASGINADO A CADA FACTURA
                                foreach (FacturaRelacionada laRelacion in elCFDIPago.lasFacturasRelacionadas)
                                {
                                    DAOFactura.RelacionarFacturasPagadas(laRelacion, conn, transaccionSQL);
                                }

                                transaccionSQL.Commit();

                            }
                            catch (Exception err)
                            {
                                transaccionSQL.Rollback();
                                throw new Exception("NO SE PUEDE RELACIONAR LOS DOCUMENTOS EN LA BD");
                            }
                        }


                        LNTimbrarPagos.TimbrarFactura(laFactura, elCFDIPago.ToString(), elUser);

                    }
                    catch (Exception err)
                    {
                        Logueo.Error(err.Message);
                        return false;

                    }


                    return true;
                }

            }
            catch (Exception ex)
            {

                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }

        }

    }
}