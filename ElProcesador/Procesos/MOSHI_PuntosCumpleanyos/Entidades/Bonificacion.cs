
namespace MOSHI_PuntosCumpleanyos.Entidades
{
    /// <summary>
    /// Clase de control de la entidad (evento) Bonificación
    /// </summary>
    public class Bonificacion
    {
        public int      IdEvento                { get; set; }
        public string   ClaveEvento             { get; set; }
        public long     IdColectiva             { get; set; }
        public int      IdTipoColectiva         { get; set; }
        public string   ClaveColectiva          { get; set; }
        public string   Importe                 { get; set; }
        public string   Concepto                { get; set; }
        public string   Observaciones           { get; set; }
    }   
}
