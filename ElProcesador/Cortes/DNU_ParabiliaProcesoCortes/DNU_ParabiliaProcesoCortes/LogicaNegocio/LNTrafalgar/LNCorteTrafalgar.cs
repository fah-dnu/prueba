using CommonProcesador;
using DNU_NewRelicNotifications.Services.Wrappers;
using DNU_ParabiliaProcesoCortes.CapaDatos;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using DNU_ParabiliaProcesoCortes.Contratos;
using DNU_ParabiliaProcesoCortes.dataService;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.LogicaNegocio.LNTrafalgar;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class LNCorteTrafalgar
    {

        XslCompiledTransform _transformador;
        public LNCorteTrafalgar(XslCompiledTransform _transformador)
        {
            this._transformador = _transformador;
        }

        public LNCorteTrafalgar()
        {
            string ArchXSLT = "";
            try
            {
                ConfiguracionContexto.InicializarContexto();
                ArchXSLT = PNConfig.Get("PROCESAEDOCUENTA", "ArchivoXSLT");

                //  ArchXSLT = Path.GetFullPath(Path.Combine(ArchXSLT, @"DNU_ParabiliaProcesoCortes\Certificados\"));
                //  ArchXSLT = ArchXSLT + "CadenaOriginal_3_3.xslt";
                _transformador = new XslCompiledTransform();
                _transformador.Load(ArchXSLT);

            }
            catch (Exception es)
            {
                Logueo.Error("[GeneraReporte] [Error al generar el corte] [Mensaje: " + es.Message + " TRACE: " + es.StackTrace + "]" + ArchXSLT);
            }
        }

        public bool inicio(string fecha = null)
        {
            try
            {

                //facturas externas
                //LNCorteTrafalgarArchExternos lnNegocioTrafExterno = new LNCorteTrafalgarArchExternos(_transformador);
                //lnNegocioTrafExterno.inicio();

                //facturas internas
                LNCorteTrafalgarCredito lnNegocioTrafCredito = new LNCorteTrafalgarCredito(_transformador);
                lnNegocioTrafCredito.inicio();

            }
            catch (Exception err)
            {
                Logueo.Error("[GeneraEstadoCuentaCredito] error al Inicio:" + err.Message + " " + err.StackTrace);
                ApmNoticeWrapper.NoticeException(err);
            }
            Logueo.Evento("[GeneraEstadoCuentaCredito] Fin proceso corte");
            return true;
        }

        public bool inicioCiclo(string fecha = null)
        {

            while (true)
            {
                Logueo.Evento("[GeneraEstadoCuentaCredito] Inicio Busqueda Archivos en SFTP");
                try
                {

                    //facturas externas
                    LNCorteTrafalgarArchExternos lnNegocioTrafExterno = new LNCorteTrafalgarArchExternos(_transformador);
                    lnNegocioTrafExterno.inicio();

                    ////facturas internas
                    //LNCorteTrafalgarCredito lnNegocioTrafCredito = new LNCorteTrafalgarCredito(_transformador);
                    //lnNegocioTrafCredito.inicio();
                }

                catch (Exception err)
                {
                    Logueo.Error("[GeneraEstadoCuentaCredito] error al Inicio:" + err.Message + " " + err.StackTrace);
                    ApmNoticeWrapper.NoticeException(err);
                }
                Thread.Sleep(600000);
            }
            Logueo.Evento("[GeneraEstadoCuentaCredito] Fin proceso corte");
            return true;
        }

    }
}
