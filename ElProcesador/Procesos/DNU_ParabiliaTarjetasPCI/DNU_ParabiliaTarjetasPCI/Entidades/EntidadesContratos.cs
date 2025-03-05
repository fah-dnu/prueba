// -----------------------------------------------------------------------
// <copyright file="EntidadesContratos.cs" company="DNU">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------


namespace DNU_ParabiliaTarjetasPCI.Entidades
{

    public class EntidadesContratos
    {
        private string m_IDCliente;
        private string m_cvv;
        private string m_clabe;
        private string m_fechaVencimiento;
        private string m_cacao;
        private string m_tarjeta;
        private string m_nip;
        private string m_rutaSaliad;
        private string m_token;

        /// <summary>
        /// propiedad que  obtiene si se muestra el paramertro de IDCliente
        /// </summary>
        public string IDCliente { get => m_IDCliente; set => m_IDCliente = value; }
        /// <summary>
        ///  propiedad que  obtiene si se muestra el paramertro de Cvv
        /// </summary>
        public string Cvv { get => m_cvv; set => m_cvv = value; }
        /// <summary>
        ///  propiedad que  obtiene si se muestra el paramertro de Clabe
        /// </summary>
        public string Clabe { get => m_clabe; set => m_clabe = value; }
        /// <summary>
        ///  propiedad que  obtiene si se muestra el paramertro de FechaVencimiento
        /// </summary>
        public string FechaVencimiento { get => m_fechaVencimiento; set => m_fechaVencimiento = value; }

        /// <summary>
        ///  propiedad que  obtiene si se muestra el paramertro de Cacao
        /// </summary>
        public string Cacao { get => m_cacao; set => m_cacao = value; }
        /// <summary>
        ///  propiedad que  obtiene si se muestra el paramertro de Tarjeta
        /// </summary>
        public string Tarjeta { get => m_tarjeta; set => m_tarjeta = value; }
        /// <summary>
        ///  propiedad que  obtiene si se muestra el paramertro de Nip
        /// </summary>
        public string Nip { get => m_nip; set => m_nip = value; }
        /// <summary>
        ///  propiedad que  obtiene si se muestra el paramertro de RutaSalida
        /// </summary>
        public string RutaSalida { get => m_rutaSaliad; set => m_rutaSaliad = value; }
        /// <summary>
        ///  propiedad que  obtiene si se muestra el paramertro de Token
        /// </summary>
        public string Token { get => m_token; set => m_token = value; }

        public EntidadesContratos() 
        { 
        
        }
    }

}
