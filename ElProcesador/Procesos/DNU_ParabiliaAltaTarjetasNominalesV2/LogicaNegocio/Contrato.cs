using Dnu_AutorizadorCacao_NCliente.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dnu_AutorizadorCacao_NCliente.LogicaNegocio
{
    public static class Contrato
    {

        private static Dictionary<String,String> _contrato;
        public static Dictionary<String, String> contrato
        {
            set
            {
                _contrato = value;
            }
        }

        public static String CodigoVendedor
        {
            get
            {
                return _contrato["@CodigoVendedor"];
            }
        }

        public static String Usuario
        {
            get
            {
                return _contrato["@Usuario"];
            }
        }

        public static String CodigoSuperFranquicia
        {
            get
            {
                return _contrato["@CodigoSuperFranquicia"];
            }
        }
        public static String UsuarioEvertec
        {
            get
            {
                return _contrato["@UsuarioEvertec"];
            }
        }
        public static String PassEvertec
        {
            get
            {
                return _contrato["@PassEvertec"];
            }
        }
        public static String UsuarioSiscard
        {
            get
            {
                return _contrato["@UsuarioSiscard"];
            }
        }
        public static String Emisor
        {
            get
            {
                return _contrato["@Emisor"];
            }
        }
        public static String UrlWebHook
        {
            get
            {
                return _contrato["@urlWebhook"];
            }
        }

    }
}
