using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public class ResponseLoginToken
    {
        public string access_token { get; set; }
        public string expires_in { get; set; }
        public string refresh_expires_in { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        public string session_state { get;set;}
        public string scope { get; set; }

    }
}
