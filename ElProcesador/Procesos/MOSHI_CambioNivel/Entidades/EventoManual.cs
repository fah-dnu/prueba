
namespace MOSHI_CambioNivel.Entidades
{
    /// <summary>
    /// Clase de control de la entidad Evento Manual
    /// </summary>
    public class EventoManual
    {
        public int      IdEvento                { get; set; }
        public string   ClaveEvento             { get; set; }
        public long     IdColectiva             { get; set; }
        public int      IdTipoColectiva         { get; set; }
        public string   ClaveCadenaComercial    { get; set; }
        public string   Importe                 { get; set; }
        public string   Concepto                { get; set; }
        public string   Observaciones           { get; set; }
        public long     Referencia              { get; set; }
    }
}
