using CommonProcesador;
using DNU.Cifrado.DES;
using DNU_ParabiliaAltaTarjetasNominales.BaseDatos;
using DNU_ParabiliaAltaTarjetasNominales.Entidades;
using DNU_ParabiliaAltaTarjetasNominales.Utilities;
using log4net;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace DNU_ParabiliaAltaTarjetasNominales.LogicaNegocio
{
    class LNVirtual
    {

        internal static bool enviaDatosCacaoCreditoVirtuales(DataTable pDtContenidoFile, string pIdArchivo, string nomFile, LNAltaTarjetaNominal clasePadre, string connArchivosCacao, Guid? idLog)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            XmlDocument xm = new XmlDocument();
            SqlConnection connection = new SqlConnection(connArchivosCacao);
            //  SqlConnection connectionParabilia = new SqlConnection(PNConfig.Get("ALTATARJETACREDCACAO", "BDReadAutorizador"));
            DataTable respValidarTarjetaTitular = null;
            bool respValidarTitular = true;
            string tarjetaTitular = null;
            Cifrador cifrador;
            try
            {


                connection.Open();
                string tarjetaRepetida = "";
                string cuentaRepetida = "";
                string fechaVencimiento = null;
                string tarTemp;
                for (int contador = 0; contador < pDtContenidoFile.Rows.Count; contador++)
                {

                    string response = null;
                    try
                    {
                        Guid guid = new Guid();
                        var row = pDtContenidoFile.Rows[contador];
                        //  var row = pDtContenidoFile.Rows[contador];
                        if (row["ID_EstatusCACAO"].ToString() != "2")
                        {
                            if (row["TipoTarjeta"].Equals("A"))
                            {
                                connection.Close();
                                connection.Open();
                                respValidarTarjetaTitular = BDsps.validarTarjetaTitular(row["TarjetaTitular"].ToString(),
                                                            row["TipoMedioAccesoTitular"].ToString(), row["MedioAccesoTitular"].ToString(),
                                                            string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                                                                ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                                                : row["ClaveEmpleadora"].ToString().TrimEnd(' '), connection)
                                                            .Tables["Table"];
                                if (respValidarTarjetaTitular.Rows[0]["Codigo"].ToString() == "0000")
                                    tarjetaTitular = respValidarTarjetaTitular.Rows[0]["TarjetaTitular"].ToString();
                                else
                                    respValidarTitular = false;
                                connection.Close();
                                connection.Open();
                            }

                            if (respValidarTitular)
                            {

                                responseSolicitudDigital respSolDigital = obtenerTarSolicitudDigitalSinSeg((string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' ')) ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                                                                        : row["ClaveEmpleadora"].ToString().TrimEnd(' ')), idLog);

                                if (respSolDigital.CodRespuesta.Equals("0000"))
                                {
                                    string idSolicitud = new Random().Next(1, 99999999).ToString().PadLeft(8, '0');
                                    string regCliente = string.IsNullOrEmpty(row["RegistroEmpleado"].ToString().TrimEnd(' ')) ? idSolicitud
                                    : row["RegistroEmpleado"].ToString().TrimEnd(' ');
                                    ////solicitudAltaTarjeta.RegistroCliente = string.IsNullOrEmpty(solicitudAltaTarjeta.RegistroCliente)
                                    //                                   ? idSolicitud
                                    //                                   : solicitudAltaTarjeta.RegistroCliente;
                                    cifrador = new Cifrador(PNConfig.Get("ALTAEMPLEADOCACAO", "dato1")
                                                , PNConfig.Get("ALTAEMPLEADOCACAO", "dato2"));
                                    string[] fechaVenTemp = cifrador.Descifrar(respSolDigital.Fecha).Split('/');
                                    fechaVencimiento = "20" + fechaVenTemp[1] + fechaVenTemp[0] + "01";

                                    tarTemp = cifrador.Descifrar(respSolDigital.Identificador);
                                    String tmpNombreEmbozo = row["NombreEmbozado"].ToString().Length > 21 ?
                                           row["NombreEmbozado"].ToString().Substring(0, 21) :
                                           row["NombreEmbozado"].ToString().PadRight(21);

                                    response = "<Resultado><Fecha>" + DateTime.Today.ToString("yyyyMMdd") + "</Fecha><Hora>" + DateTime.Now.ToString("HHmmss") +
                                                        "</Hora><Cuenta>" + tarTemp.Substring(0, 13) + "</Cuenta><Tarjeta>" + tarTemp + "</Tarjeta>" +
                                                        "<Nombre>" + tmpNombreEmbozo + "</Nombre><Identificacion>25863</Identificacion><Respuestas>" +
                                                           "<Respuesta><Codigo>00</Codigo><Descripcion>Datos Correctos</Descripcion>" +
                                                        "</Respuesta></Respuestas></Resultado>";

                                    response = response.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&#xD;", "");
                                    xm = new XmlDocument();
                                    xm.LoadXml(response);
                                    //Logueo.EntradaSalida("[WS_AltaEmpleado.CACAO.Prospectación] [RECIBIDO] [enviaDatosEvertec] [NOMBRE:" + xm.GetElementsByTagName("Nombre")[0].InnerText + ";FECHA:"
                                    //    + xm.GetElementsByTagName("Fecha")[0].InnerText + ";HORA:"
                                    //    + xm.GetElementsByTagName("Hora")[0].InnerText + ";Código:"
                                    //    + xm.GetElementsByTagName("Codigo")[0].InnerText + ";Descripción"
                                    //    + xm.GetElementsByTagName("Descripcion")[0].InnerText + "]", "PROCNOC", true);
                                    BDsps.insertarRespuestaCacaoAltasCredito(xm, connection, row["Id_ArchivoDetalle"].ToString(), row["Nombre"].ToString());

                                }
                                else
                                {
                                    responseAltaTarjeta respuestaAltaTarjeta = new responseAltaTarjeta();
                                    if (respSolDigital.CodRespuesta.Equals("1191")
                                        || respSolDigital.CodRespuesta.Equals("9997"))
                                    {
                                        respuestaAltaTarjeta.CodRespuesta = "9997";
                                        respuestaAltaTarjeta.DescRespuesta = "No hay Tarjtetas disponibles para el Emisor";
                                        // respuestaAltaTarjeta.IDSolicitud = solicitudAltaTarjeta.IDSolicitud;
                                        respuestaAltaTarjeta.MotivoRechazo = respSolDigital.CodRespuesta;
                                    }
                                    else
                                    {
                                        respuestaAltaTarjeta.CodRespuesta = "9999";
                                        respuestaAltaTarjeta.DescRespuesta = "Ocurrio un error al generar la Tarjeta";
                                        //  respuestaAltaTarjeta.IDSolicitud = solicitudAltaTarjeta.IDSolicitud;
                                        respuestaAltaTarjeta.MotivoRechazo = respSolDigital.CodRespuesta;
                                    }

                                    //    return respuestaAltaTarjeta;
                                    LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [WS_AltaEmpleado.CACAO.Prospectación, validarTarjetaTitular, Codigo: " + respuestaAltaTarjeta.CodRespuesta +
                                                           ", Descripcion: " + respuestaAltaTarjeta.DescRespuesta + "]");

                                }

                            }
                            else
                            {
                                BDsps.insertarRespuestaValidarTitular(respValidarTarjetaTitular.Rows[0]["Codigo"].ToString(),
                                                        respValidarTarjetaTitular.Rows[0]["Descripcion"].ToString(),
                                                        connection, row["Id_ArchivoDetalle"].ToString(), row["Nombre"].ToString());
                                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [WS_AltaEmpleado.CACAO.Prospectación, validarTarjetaTitular, Codigo: " + respValidarTarjetaTitular.Rows[0]["Codigo"].ToString() +
                                                                ", Descripcion: " + respValidarTarjetaTitular.Rows[0]["Descripcion"].ToString() + "]");

                            }

                        }
                        else
                        {
                            //BDsps.insertarRespuestaCacaoAltasReproceso(connection, row["Id_ArchivoDetalle"].ToString()
                            //            , string.IsNullOrEmpty(row["ClaveEmpleadora"].ToString().TrimEnd(' '))
                            //                    ? row["ClaveEmisor"].ToString().TrimEnd(' ')
                            //                    : row["ClaveEmpleadora"].ToString().TrimEnd(' '), row["RegistroEmpleado"].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [WS_AltaEmpleado.CACAO.Prospectación, enviaDatosCACAO, Resulto un error al convertir la respuesta " + response + " en XML:" + ex.Message + ", " + ex.StackTrace + "]");
                    }
                }
            }
            catch (Exception ex)
            {
                LogueoAltaEmpleadoCacao.Error("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [WS_AltaEmpleado.CACAO.Prospectación, enviaDatosCACAO, Error al guardar respuesta CACAO, Mensaje: " + ex.Message + " TRACE: " + ex.StackTrace + "]");
            }
            finally
            {
                connection.Close();
                //  connectionParabilia.Close();
            }

            return clasePadre.enviarDatosParabiliaCredito(pIdArchivo, nomFile, idLog);
        }

        private static responseSolicitudDigital obtenerTarSolicitudDigitalSinSeg(string claveEmp, Guid? idLog)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            string URLsolicitudDig = PNConfig.Get("ALTAEMPLEADOCACAO", "wsFechasClientes2");//ConfigurationManager.AppSettings["wsFechasClientes2"].ToString();

            requestSolicitudDigital reqSolDig = new requestSolicitudDigital()
            {
                IDSolicitud = new Random().Next(1000000, 999999999).ToString(),
                Emisor = claveEmp
            };
            LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [obtenerTarSolicitudDigitalSinSeg, " + URLsolicitudDig + ", " + JsonConvert.SerializeObject(reqSolDig), "", false + "]");
            
            var client = new RestClient(URLsolicitudDig);
            var request = new RestRequest(Method.POST);
            //request.AddHeader("Authorization", token);
            //request.AddHeader("Credenciales", cred);
            request.RequestFormat = DataFormat.Json;
            request.AddBody(reqSolDig);
            IRestResponse response = client.Execute(request);

            responseSolicitudDigital resp = JsonConvert.DeserializeObject<responseSolicitudDigital>(response.Content);

            LogueoAltaEmpleadoCacao.Info("[" + ip + "] [TarjetasNominativas] [PROCESADORNOCTURNO] [" + log + "] [obtenerTarSolicitudDigitalSinSeg, " + URLsolicitudDig + ", " + JsonConvert.SerializeObject(resp), "", true + "]");
            return resp;
        }
    }
}
