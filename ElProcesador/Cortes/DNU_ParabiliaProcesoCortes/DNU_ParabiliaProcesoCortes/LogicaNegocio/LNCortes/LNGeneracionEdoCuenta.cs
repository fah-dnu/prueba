using CommonProcesador;
using CrystalDecisions.Shared;
using DNU_ParabiliaProcesoCortes.CapaDatos;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using DNU_ParabiliaProcesoCortes.Contratos;
using DNU_ParabiliaProcesoCortes.ReporteDebito.trafalgar;
using DNU_ParabiliaProcesoCortes.ReportesCredito;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio.LNCortes
{
    class LNGeneracionEdoCuenta : EdoCuenta
    {
        public  override bool GeneraEdoCuentaPDF(List<string> archivos,  long ID_Corte, string ruta, DAOCortes _daoCortes, SqlConnection conn, string rutaImagen, string claveCliente, string logo, string imagenUNE, string imagenCAT, string nombreArchivo, bool noTimbrar, Factura laFacturaExt)
        {
            try
            {
                rutaImagen = rutaImagen + claveCliente;
                LNOperaciones.crearDirectorio(rutaImagen);
                Logueo.Evento("[GeneraEstadoCuentaCredito] generando carpeta:" + ruta);
                if (!ruta.Contains(".pdf"))
                {
                    LNOperaciones.crearDirectorio(ruta);
                }
                DataSet ds = _daoCortes.ObtenerDatosPagos(ID_Corte.ToString(), null, conn);
                ds = _daoCortes.ObtenerDatosCH(ID_Corte.ToString(), null, conn, ds);
                //  dynamic estadoDeCuenta;
                string tarjeta = "";
                int numeroTablaDatosCH = 1;
                bool visualizaSubReporte = false;
                DataSet dsSubreporte = null;
                //EdoCuentaCreditoDemo estadoDeCuenta = new EdoCuentaCreditoDemo();
                EdoCtaCreditoGenerico estadoDeCuenta = new EdoCtaCreditoGenerico();
                if (ds.Tables[0].Rows.Count == 0)
                {
                    ds = _daoCortes.ObtenerDatosCH(ID_Corte.ToString(), null, conn, null);
                    tarjeta = ds.Tables[0].Rows[0]["tarjeta"].ToString();
                    numeroTablaDatosCH = 0;
                }
                else
                {

                    //if (noTimbrar)
                    //{
                    //    estadoDeCuenta.SetParameterValue("Timbrar", imagenUNE);

                    //}
                    //else
                    //{
                    //    estadoDeCuenta.SetParameterValue("Timbrar", imagenUNE);
                    //}
                    tarjeta = ds.Tables[1].Rows[0]["tarjeta"].ToString();


                }//
                if (!tarjeta.Contains("*"))
                {
                    tarjeta = tarjeta.Substring(0, 6) + "******" + tarjeta.Substring(12, 4);
                }
                ds.Tables[numeroTablaDatosCH].Rows[0]["tarjeta"] = tarjeta;


                estadoDeCuenta.SetDataSource(ds);


                //  estadoDeCuenta.SetParameterValue("CadenaOriginal", string.IsNullOrEmpty(laFacturaExt.CadenaOriginal) ? "" : laFacturaExt.CadenaOriginal);
                //if (string.IsNullOrEmpty(logo)) {
                //    logo = null;
                //}
                //if (string.IsNullOrEmpty(imagenCAT))
                //{
                //    imagenCAT = null;
                //}
                //if (string.IsNullOrEmpty(imagenUNE))
                //{
                //    imagenUNE = null;
                //}

               // estadoDeCuenta.SetParameterValue("CadenaOriginal", string.IsNullOrEmpty(laFacturaExt.CadenaOriginal) ? "" : laFacturaExt.CadenaOriginal);

                estadoDeCuenta.SetParameterValue("imagenLogo", logo);
                estadoDeCuenta.SetParameterValue("CAT", imagenCAT);
                estadoDeCuenta.SetParameterValue("UNE", imagenUNE);
                //#endif
                estadoDeCuenta.ExportToDisk(ExportFormatType.PortableDocFormat, ruta + nombreArchivo);
                estadoDeCuenta.Close();
                estadoDeCuenta.Dispose();
                archivos.Add(ruta + nombreArchivo);//
                return true;//
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al genrerar PDF " + ex.Message + " " + ex.StackTrace);
                return false;
            }

        }
    }
}
