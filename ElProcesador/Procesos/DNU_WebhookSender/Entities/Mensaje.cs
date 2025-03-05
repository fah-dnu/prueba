using System;

namespace DNU_WebhookSender.Entities
{
    public class Mensaje
    {
        public Int32    ID_Mensaje      { get; set; }
        public String   MensajeJSON     { get; set; }
        public Int32    Reintentos      { get; set; }
        public String   URL             { get; set; }
        public Int64    ID_Operacion    { get; set; }
        public Int64    ID_Colectiva    { get; set; }
        public Boolean  Procesado       { get; set; }
    }
}
