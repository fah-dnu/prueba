using System;
using System.Collections.Generic;
using System.Text;
using CommonProcesador;
using DNU_Multipacs.Entidades;
using DNU_ParabiliaProcesoCortes.Common.Entidades;
using Newtonsoft.Json;
using RestSharp;

namespace DNU_Multipacs
{
    public class Timbrado
    {
        string xmlTimbrado { get; set; }
        public TaxStamp Facturama(Factura factura,string ruta)
        {
            TaxStamp respuesta = new TaxStamp ();
           
            try
            {
                StringBuilder sbResponse = new StringBuilder();

                //Se genera el POST
                var client = new RestClient(factura.URLProvedorCertificado);//"https://api.facturama.mx/api-lite/2/cfdis");//factura.URLProvedorCertificado);
                var request = new RestRequest(Method.POST);

                string username = factura.UserPAC;//"kapital"; // "pruebas";
                string password = factura.UserPassPAC;//"@Kapital2021"  // "pruebas2011";

                string credenciales = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));

                /*Se forma el Json de la factura*/
                var regimen = factura.RegimenFiscal;//factura.getParametro("@RegimenFiscal");
                var folio = DateTime.Now.Year.ToString().Substring(2, 2);
                var fecha = DateTime.Now.Year.ToString().Substring(2, 2);
                if (DateTime.Now.Month.ToString().Length == 1)
                {
                    folio = folio + "0";
                }
                folio = folio + DateTime.Now.Month.ToString();

                if (DateTime.Now.Day.ToString().Length == 1)
                {
                    folio = folio + "0";
                }
                folio = folio + DateTime.Now.Day.ToString();

                folio = fecha/*folio +*/+ factura.Folio.Substring(factura.Folio.Length-7, 7);
                // folio = folio + factura.Folio.Substring(factura.Folio.Length - 3, 3);
                //000000000
                factura.Folio = folio;//se asigna el nuevo valor
                var facturaRequest = new CFDI_Facturama
                {
                    Issuer = new Issuer
                    {
                        Name = factura.Emisora.NombreORazonSocial,//"CORPORACION NOMI FIN",// factura.Emisora.NombreORazonSocial,
                        FiscalRegime = regimen,// "601",//regimen, // "601",
                        Rfc = factura.Emisora.RFC,//"CNF120614443",// "AAA010101AAA",
                    },
                    Receiver = new Receiver
                    {
                        Name = factura.Receptora.NombreORazonSocial,
                        CfdiUse = factura.UsoCFDI,
                        Rfc = factura.Receptora.RFC,
                    },
                    Folio = folio, //"200",
                    CfdiType = factura.TipoComprobante,
                    NameId = "1",
                    ExpeditionPlace = factura.LugarExpedicion,
                    PaymentForm = factura.FormaPago,
                    PaymentMethod = factura.MetodoPago,
                    Currency = "MXN",
                    Date = factura.FechaEmision.ToString("yyyy-MM-dd HH:mm:ss").Replace(" ","T")//DateTime.Parse(factura.FechaEmision.ToShortDateString())
            };

                facturaRequest.Items = new List<Item>();

                foreach (var item in factura.losDetalles)
                {
                    facturaRequest.Items.Add(new Item()
                    {
                        Quantity = item.Cantidad,
                        ProductCode = item.ClaveProdServ,
                        UnitCode = item.ClaveUnidad,
                        Description = item.NombreProducto,
                        UnitPrice = item.PrecioUnitario,
                        Subtotal = item.PrecioUnitario * item.Cantidad, //50.00m,
                        Taxes = new List<Tax> {
                            new Tax {
                                Name = "IVA",
                                Rate = decimal.Parse(item.impTasaOCuota), // 0.16m,
                                Total = decimal.Parse(item.impImporte), // 960.00m,
                                Base = decimal.Parse(item.impBase), //6000.00m,
                                IsRetention = false
                            }
                        },
                        Total = item.Total + decimal.Parse(item.impImporte), // 6960.00m
                    });
                }

                //Headers
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Basic " + credenciales);

                //Body
                request.AddJsonBody(facturaRequest);
              
                //Ejecuta la petición
                IRestResponse response = client.Execute(request);
                Logueo.EntradaSalida("[GeneraEstadoCuentaCredito] peticion Facturama:" + JsonConvert.SerializeObject(request.Body.Value), "", false);

                string statusCode = response.StatusCode.ToString();
                Logueo.EntradaSalida("[GeneraEstadoCuentaCredito] peticion Facturama:" + JsonConvert.SerializeObject(response.Content), "", true);

                //Petición enviada y recibida OK
                if (statusCode == "Created")
                {
                    var content = JsonConvert.DeserializeObject<CFDI_Facturama>(response.Content);

                    //Obtiene XML por medio del ID
                    content.Complement.TaxStamp.FacturaXML = ObtieneXMLFacturama(content.Id, factura.URLProvedorCertificadoObtieneXML,ruta,username,password);

                    if (content.Complement.TaxStamp.FacturaXML == null) {
                        respuesta.Error = "El XML no pudo ser obtenido";
                    } else
                    {
                        respuesta = content.Complement.TaxStamp;
                        factura.XMLTimbre = xmlTimbrado;
                        factura.SelloCFD = respuesta.CfdiSign;
                        factura.SelloSAT = respuesta.SatSign;
                        factura.NoCertificadoSAT = respuesta.SatCertNumber;
                        factura.FechaTimbrado = respuesta.Date;
                        factura.RFCProvedorCertificado = respuesta.RfcProvCertif;
                        factura.UUID = respuesta.Uuid;
                        factura.Sello = respuesta.CfdiSign;

                       // factura.RFCProvedorCertificado = respuesta.RfcProvCertif;
                        factura.NoCertificadoEmisor=respuesta.AutNumProvCertif;
                        factura.CadenaOriginalTimbre = "||" + "1.1"/*respuesta.*/ + "|" + factura.UUID + "|" + factura.FechaTimbrado.ToString("yyyy-MM-dd HH:mm:ss").Replace(" ", "T") + "|" + factura.RFCProvedorCertificado + "|" + factura.SelloCFD + "|" + factura.NoCertificadoSAT + "||";//ObtieneCadenaOriginal(unTimbrazo);

                    }
                }
                else
                {
                    respuesta.Error = response.Content;
                }
            }

            catch (Exception ex)
            {
                throw new Exception("Facturama(): " + ex.Message);
            }

            return respuesta;
        }

        private string ObtieneXMLFacturama(string idFactura, string urlXML,string ruta,string userNameFac,string passwordFac)
        {
            string respuesta = null;

            try
            {
                StringBuilder sbResponse = new StringBuilder();

               var client = new RestClient(/*"https://apisandbox.facturama.mx/cfdi/xml/issuedLite/"*/urlXML + idFactura);
              //  var client = new RestClient("https://api.facturama.mx/cfdi/xml/issuedLite/" + idFactura);                //var client = new RestClient(urlXML + idFactura);
                var request = new RestRequest(Method.GET);

                string username = userNameFac; //"kapital"; //"pruebas";
                string password = passwordFac; //"@Kapital2021";//"pruebas2011";
                string credenciales = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));

                //Headers
                request.AddHeader("Authorization", "Basic " + credenciales);

                //REST_Log.LogRequest(client, request, elUsuario.ClaveUsuario);

                //Ejecuta la petición
                IRestResponse response = client.Execute(request);
                string statusCode = response.StatusCode.ToString();

                //REST_Log.LogResponse(response, elUsuario.ClaveUsuario);

                //Petición enviada y recibida OK
                if (statusCode == "OK")
                {
                    var content = JsonConvert.DeserializeObject<Response_Facturama>(response.Content);
                    
                    //Decodifica y Guarda XML en archivo
                    SaveXMLFacturama(content.Content,ruta);
                    respuesta = content.Content;
                }
                else
                {
                    respuesta = null;
                }
            }

            catch (Exception ex)
            {
                throw new Exception("ObtieneXMLFacturama(): " + ex.Message);
            }

            return respuesta;
        }

        public void SaveXMLFacturama(string XMLbase64,string ruta)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(XMLbase64);
            var contenido = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            xmlTimbrado = contenido;
            System.IO.File.WriteAllText(ruta+"facturaTimbrada.xml", contenido);
        }
    }
}
