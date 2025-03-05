

namespace DNU_ParabiliaTarjetasPCI.Entidades
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Archivo
    {
        private string m_emisor;
        private string m_cuenta;
        private string m_numeroTarjeta;
        private string m_tarjeta;
        private string m_vencimiento;
        private string m_rawData;

        /// <summary>
        /// propiedad que obtiene  el valor del emisor
        /// </summary>
        public string Emisor { get => m_emisor; set => m_emisor = value; }
        /// <summary>
        /// propiedad que obtiene el valor de la cuenta
        /// </summary>
        public string Cuenta { get => m_cuenta; set => m_cuenta = value; }
        /// <summary>
        /// propiedad que obtiene el numero de tarjeta
        /// </summary>
        public string NumeroTarjeta { get => m_numeroTarjeta; set => m_numeroTarjeta = value; }
        /// <summary>
        /// 
        /// </summary>propiedad que obtiene el valor de la tarjeta
        public string Tarjeta { get => m_tarjeta; set => m_tarjeta = value; }
        /// <summary>
        /// propiedad que obtiene el valor de la fecha de vencimiento
        /// </summary>
        public string Vencimiento { get => m_vencimiento; set => m_vencimiento = value; }
        /// <summary>
        /// propíedad  que obtiene el valor del rawData;
        /// </summary>
        public string RawData { get => m_rawData; set => m_rawData = value; }
    }
}
