using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public class DireccionEntrega
    {
        private string m_Email;
        private string m_Telefono;
        private string m_Calle;
        private string m_NExterior;
        private string m_NInterior;
        private string m_Colonia;
        private string m_Municipio;
        private string m_CP;
        private string m_Estado;
        private string m_Pais;
        private string m_Referencia;

        public string Email { set => m_Email = value; get => m_Email; }
        public string Telefono { set => m_Telefono = value; get => m_Telefono; }
        public string Calle { set => m_Calle = value; get => m_Calle; }
        public string NExterior { set => m_NExterior = value; get => m_NExterior; }
        public string NInterior { set => m_NInterior = value; get => m_NInterior; }
        public string Colonia { set => m_Colonia = value; get => m_Colonia; }
        public string Municipio { set => m_Municipio = value; get => m_Municipio; }
        public string CP { set => m_CP = value; get => m_CP; }
        public string Estado { set => m_Estado = value; get => m_Estado; }
        public string Pais { set => m_Pais = value; get => m_Pais; }
        public string Referencia { set => m_Referencia = value; get => m_Referencia; }
    }
}
