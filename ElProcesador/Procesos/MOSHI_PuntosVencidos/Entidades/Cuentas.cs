using System;

namespace MOSHI_PuntosVencidos.Entidades
{
    /// <summary>
    /// Clase para el manejo de la entidad Cuentas
    /// </summary>
    class Cuentas
    {
        public Cuentas(Int64 idcuenta, Int64 idcolectiva, int idtipocolectiva, string email, float puntosavencer, bool procesadaok)
        {
            ID_Cuenta = idcuenta;
            ID_Colectiva = idcolectiva;
            ID_TipoColectiva = idtipocolectiva;
            Email = email;
            PuntosAVencer = puntosavencer;
            ProcesadaOK = procesadaok;
        }

        public Int64    ID_Cuenta           { get; set; }
        public Int64    ID_Colectiva        { get; set; }
        public int      ID_TipoColectiva    { get; set; }
        public String   Email               { get; set; }
        public float    PuntosAVencer       { get; set; }
        public bool     ProcesadaOK         { get; set; }
    }
}
