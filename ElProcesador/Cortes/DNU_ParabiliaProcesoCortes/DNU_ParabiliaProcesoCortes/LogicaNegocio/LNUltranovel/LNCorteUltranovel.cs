using DNU_ParabiliaProcesoCortes.LogicaNegocio;
using DNU_ParabiliaProcesoCortes.LogicaNegocio.LNCortes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio.LNUltranovel
{
    class LNCorteUltranovel : LNCorte
    {
        XslCompiledTransform _transformador;
        public LNCorteUltranovel(XslCompiledTransform _transformador) : base(_transformador)
        {
            this._transformador = _transformador;
        }
        ////public override bool inicio(string id = null, string fecha = null)
        ////{
        ////    return false;
        ////}

    }
}
