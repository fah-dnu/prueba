
namespace MOSHI_PuntosCumpleanyos.Entidades
{
    /// <summary>
    /// Clase para el manejo de la entidad Cuentas
    /// </summary>
    class Cuentas
    {
        public Cuentas(long idcolectiva, string cuentahabiente, string email, string puntos, bool procesada, bool mailok)
        {
            ID_Colectiva = idcolectiva;
            Cuentahabiente = cuentahabiente;
            Email = email;
            Puntos = puntos;
            Procesada = procesada;
            MailOK = mailok;
        }

        public long     ID_Colectiva    { get; set; }
        public string   Cuentahabiente  { get; set; }
        public string   Email           { get; set; }
        public string   Puntos          { get; set; }
        public bool     Procesada       { get; set; }
        public bool     MailOK          { get; set; }
    }
}
