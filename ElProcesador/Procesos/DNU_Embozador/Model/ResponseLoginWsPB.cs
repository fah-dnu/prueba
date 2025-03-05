using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public class ResponseLoginWsPB
    {
        private string m_CodRespuesta;
        private string m_DescRespuesta;
        private string m_UserID;
        private string m_PrimerApellido;
        private string m_SegundoApellido;
        private string m_Token;
        private string m_NombreUsuario;

        public string CodRespuesta { set => m_CodRespuesta = value; get => m_CodRespuesta; }
        public string DescRespuesta { set => m_DescRespuesta = value; get => m_DescRespuesta; }
        public string UserID { set => m_UserID = value; get => m_UserID; }
        public string PrimerApellido { set => m_PrimerApellido = value; get => m_PrimerApellido; }
        public string SegundoApellido { set => m_SegundoApellido = value; get => m_SegundoApellido; }
        public string Token { set => m_Token = value; get => m_Token; }
        public string NombreUsuario { set => m_NombreUsuario = value; get => m_NombreUsuario; }
    }
}
