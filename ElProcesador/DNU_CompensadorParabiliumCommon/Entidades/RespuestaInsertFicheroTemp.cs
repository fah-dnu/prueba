using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class RespuestaInsertFicheroTemp : RespuestaGral
    {
        public long IdFicheroTemp { get; set; }
        public string extensionFile { get; set; }
    }
}
