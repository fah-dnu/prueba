
namespace MOSHI_CambioNivel.Entidades
{
    /// <summary>
    /// Clase para el manejo de la entidad Cuentas
    /// </summary>
    class Cuentas
    {
        public Cuentas(int idcuenta, string cuentahabiente, string email, bool procesada, bool mailok)
        {
            ID_Cuenta = idcuenta;
            Cuentahabiente = cuentahabiente;
            Email = email;
            Procesada = procesada;
            MailOK = mailok;
        }

        public Cuentas(int idcuenta, bool reseteada)
        {
            ID_Cuenta = idcuenta;
            Reseteada = reseteada;
        }

        public int      ID_Cuenta       { get; set; }
        public string   Cuentahabiente  { get; set; }
        public string   Email           { get; set; }
        public bool     Procesada       { get; set; }
        public bool     MailOK          { get; set; }
        public bool     Reseteada       { get; set; }
    }
}
