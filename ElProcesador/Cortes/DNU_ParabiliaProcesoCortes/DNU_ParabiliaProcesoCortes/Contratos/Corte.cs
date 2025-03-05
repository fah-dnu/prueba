using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Xsl;

namespace DNU_ParabiliaProcesoCortes.Contratos
{
    abstract class Corte
    {
        XslCompiledTransform transformador;
        private int numCorte;
        //este de abajo solo es de pruebas
        public Corte(XslCompiledTransform transformador) {
            this.transformador = transformador;
        }
        public Corte()
        {
      
        }
        public int getNumcorte() {
            return numCorte;
        }
        public abstract bool inicio(string id=null,string fecha = null);

        public abstract bool inicioSinConexionSFTP(string fecha = null);

    }
  

}
