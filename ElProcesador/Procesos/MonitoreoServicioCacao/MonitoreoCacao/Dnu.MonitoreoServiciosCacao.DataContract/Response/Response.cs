using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu.MonitoreoServiciosCacao.DataContract.Response
{
    public class Response
    {
        public int StatusCode { get; set; }
        public string DescResponse { get; set; }
        public string IdRequest { get; set; }
    }
}
