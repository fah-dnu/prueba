using DNU_ParabiliumProcesoMSI.CapaDeNegocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliumProcesoMSI
{
    internal class Class1 : LNMSI
    {
        public Class1() {
            inicioProcesoMSI();
        }

        public override bool inicioProcesoMSI()
        {
            return true;
        }
    }
}
    
