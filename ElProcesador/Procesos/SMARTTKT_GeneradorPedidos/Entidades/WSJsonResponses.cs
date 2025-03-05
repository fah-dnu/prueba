using System;

namespace SMARTTKT_GeneradorPedidos.Entidades
{
    /// <summary>
    /// Clase de control de la entidad WebService Json Responses
    /// </summary>
    public class WSJsonResponses
    {
        public class Promociones
        {
            public string   CodigoRespuesta { get; set; }
            public string   Descripcion     { get; set; }
            public object   Codigos         { get; set; }
            public object   Error           { get; set; }
        }

        public class Error
        {
            public string   CodigoRespuesta { get; set; }
            public string   Message         { get; set; }
            public string   Descripcion     { get; set; }
        }  
    }
}
