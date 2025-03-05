using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dnu.Batch.DataContracts
{
    public class Ticket
    {
        public int IdTicketSucursal { get; set; }
        public int IdTipoAcumulacion { get; set; }
        public string TipoEntrega { get; set; }
        public string ClaveSucursal { get; set; }
        public string NumeroTicket { get; set; }
        public DateTime FechaTicket { get; set; }
        public decimal Importe { get; set; }
        public decimal Descuentos { get; set; }
        public decimal Propina { get; set; }
        public string CodigoUnicoFactura { get; set; }
        public int cliente_id { get; set; }
        public string Email { get; set; }
        public int IdEstatusTicket { get; set; }
        public DateTime FechaInsert { get; set; }
        public string UsuarioInsert { get; set; }
        public DateTime FechaModifico { get; set; }
        public string UsuarioModifico { get; set; }
        public int NumeroReintento { get; set; }
        public string NumeroAutorizacion { get; set; }
        public DateTime FechaBonificacion { get; set; }
        public string Afiliacion { get; set; }
    }
}
