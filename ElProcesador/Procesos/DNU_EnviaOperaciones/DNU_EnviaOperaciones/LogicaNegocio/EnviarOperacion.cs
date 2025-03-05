using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_EnviaOperaciones.LogicaNegocio
{
    class EnviarOperacion
    {

        public static Boolean SendOperation()
        {
            try
            {

                var client = new RestClient("http://64.237.63.18");
                // client.Authenticator = new HttpBasicAuthenticator(username, password);

                var request = new RestRequest("/api/Clientes/saveCliente?wsusuario=Olga&wspassword=123", Method.POST);
                request.AddParameter("id_clienteAutorizador", "123"); // adds to POST or URL querystring based on Method
                request.AddParameter("nombre", "14526"); // adds to POST or URL querystring based on Method


                // easily add HTTP Headers
                request.AddHeader("wsusuario", "Olga");
                request.AddHeader("wspassword", "Olga");

                                
                // add files to upload (works with compatible verbs)
                // request.AddFile(path);

                // execute the request
                IRestResponse response = client.Execute(request);
                var content = response.Content; // raw content as string

                return true;

            } catch (Exception err)
            {

                return false;
            }
        }
    }
}
