using System;

namespace DNU_WebhookSender.Entities
{
    /// <summary>
    /// Clase de control de la entidad WebService Json Responses (para las llamadas a Sr Pago)
    /// </summary>
    public class HTTPResponses
    {
        public class NotificationResponse
        {
            public int      CodigoRespuesta { get; set; }
            public string   Descripcion     { get; set; }
            public string   Mensaje         { get; set; }
        }
    }
}
