using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu_ProcesadorCacaoLogCleaner.Entidades
{
    public class LogModel
    {
        public long id { get; set; }
        public long id_fichero { get; set; }
        public string ip { get; set; }
        public string client { get; set; }
        public string date_ { get; set; }
        public string request { get; set; }
        public string request_version { get; set; }
    }
}
