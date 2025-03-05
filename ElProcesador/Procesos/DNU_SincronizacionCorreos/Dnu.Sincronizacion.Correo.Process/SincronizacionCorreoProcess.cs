using CommonProcesador;
using Dnu.Sincronizacion.Correo.DataBase;
using Dnu.Sincronizacion.Correo.DataContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Dnu.Sincronizacion.Correo.Process
{
#if !DEBUG
    public class SincronizacionCorreoProcess : IProcesoNocturno
#endif
#if DEBUG
    public class SincronizacionCorreoProcess : IProcesoNocturno
#endif
    {
        public void Detener()
        {

        }

        public void Iniciar()
        {

        }

        public bool Procesar()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();
                //var section = ConfigurationManager.GetSection("BatchSincronizacionCorreo") as NameValueCollection;
                DataBaseAccess dataBase = new DataBaseAccess();
                Logueo.Evento("[SincronizacionCorreoProcess] Se Inicia Proceso nocturno ");      var clientes = dataBase.GetClientesPendientesAutorizar();
                if (clientes.Count() == 0)
                {
                    Logueo.Evento("[Procesar] Finaliza proceso clientes totales: 0" + clientes);
                    return true;
                }
                Logueo.Evento("[Procesar] Clientes totales:" + clientes.Count());
                var DC = PNConfig.Get("CORREOSINCRONIZACION", "DC"); 
                var IdLista = PNConfig.Get("CORREOSINCRONIZACION", "IdLista"); 
                var apikey = PNConfig.Get("CORREOSINCRONIZACION", "apikey");
                var urlCorreo = PNConfig.Get("CORREOSINCRONIZACION", "urlCorreo");

                var email_type = PNConfig.Get("CORREOSINCRONIZACION", "email_type"); 
                var status_if_new = PNConfig.Get("CORREOSINCRONIZACION", "status_if_new"); 
                var country = PNConfig.Get("CORREOSINCRONIZACION", "country"); 
                //https://[Dc].api.mailchimp.com/3.0/lists/[IdLista]/members/[correo_hash]
                urlCorreo = urlCorreo.Replace("[Dc]", DC);
                urlCorreo = urlCorreo.Replace("[IdLista]", IdLista);

                var clientesExitoso = 0;
                var clientesError = 0;

                foreach (var cliente in clientes)
                {
                    try
                    {
                        Logueo.Evento("[Procesar] Envio de cliente: " + cliente.Correo);
                        var uri = urlCorreo.Replace("[correo_hash]", CalculateMD5Hash(cliente.Correo.ToLower()));
                        var mailchimp = new MailChimp
                        {
                            email_address = cliente.Correo,
                            email_type = email_type,
                            status_if_new = status_if_new,
                            merge_fields = new Merge_fields
                            {
                                FNAME = cliente.Nombre,
                                LNAME = cliente.ApellidoPat,
                                ADDRESS = new Address
                                {
                                    addr1 = (cliente.Calle + " " + cliente.NumExterior + " " + cliente.NumInterior).Trim().Length == 0 ? "N/A" : cliente.Calle + " " + cliente.NumExterior + " " + cliente.NumInterior,
                                    addr2 = string.Empty,
                                    city = cliente.Colonia == string.Empty ? "N/A" : cliente.Colonia,
                                    state = cliente.Estado == string.Empty ? "N/A" : cliente.Estado,
                                    zip = cliente.CodigoPostal == string.Empty ? "N/A" : cliente.CodigoPostal,
                                    country = country == string.Empty ? "N/A" : country
                                },
                                PHONE = cliente.Telefono,
                                BIRTHDAY = cliente.FechaNacimiento,
                                PUNTOSMOSH = cliente.Puntos,
                                VISITAS = cliente.Visitas,
                                NIVEL = cliente.Nivel,
                                PUNTOSVENC = cliente.PuntosVenc,
                                ULTIMACOMP = !String.IsNullOrEmpty(cliente.FechaUltimaCompra) ? DateTime.Parse(cliente.FechaUltimaCompra).ToString("MM/dd/yyyy") : null,
                                ULTIMABON = !String.IsNullOrEmpty(cliente.FechaUltimaBonificacion) ? DateTime.Parse(cliente.FechaUltimaBonificacion).ToString("MM/dd/yyyy") : null,
                                
                            }
                        };
                        var sampleListMember = JsonConvert.SerializeObject(mailchimp);

                        using (HttpClient client = new HttpClient())
                        {
                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("apikey", apikey);
                            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                            client.BaseAddress = new Uri(uri);

                            //NOTE: To avoid the method being 'async' we access to '.Result'
                            HttpResponseMessage response = client.PutAsJsonAsync(uri, mailchimp).Result;
                            var message = response.ReasonPhrase;
                            if ((response.IsSuccessStatusCode))
                            {
                                clientesExitoso++;
                                dataBase.ActualizaBonificacionTicket(cliente.Correo);
                            }
                            else
                            {
                                clientesError++;
                            }
                        }
                        Logueo.Evento("[Procesar] Clientes exitoso: " + clientesExitoso + ", Clientes error: " + clientesError);
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error("[Procesar] Proceso con error:" + ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Logueo.Error("[Procesar] Proceso con error:" + ex.ToString());
            }


            return true;
        }

        private static string CalculateMD5Hash(string input)
        {
            // Step 1, calculate MD5 hash from input.
            var md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // Step 2, convert byte array to hex string.
            var sb = new StringBuilder();
            foreach (var @byte in hash)
            {
                sb.Append(@byte.ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
