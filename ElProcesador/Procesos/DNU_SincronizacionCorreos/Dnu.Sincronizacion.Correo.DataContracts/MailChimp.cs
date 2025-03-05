namespace Dnu.Sincronizacion.Correo.DataContracts
{
    public class MailChimp
    {
        public string email_address { get; set; }
        public string email_type { get; set; }
        public string status_if_new { get; set; }
        public Merge_fields merge_fields { get; set; }
    }
}
