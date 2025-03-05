
namespace DICONSA_ImportarTXTiendas.Entidades
{
    public class TiendaDiconsa
    {
        public TiendaDiconsa(int idColectiva, bool procesada)
        {
            ID_Colectiva = idColectiva;
            Procesada = procesada;
        }
   
        public int      ID_Colectiva    { get; set; }
        public bool     Procesada       { get; set; }
    }
}
