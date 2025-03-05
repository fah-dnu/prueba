using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public class RequestLoginWsPB
    {
        private string m_NombreUsuario;
        private string m_Password;

        public string NombreUsuario { set => m_NombreUsuario = value; get => m_NombreUsuario; }
        public string Password { set => m_Password = value; get => m_Password; }
    }
}
