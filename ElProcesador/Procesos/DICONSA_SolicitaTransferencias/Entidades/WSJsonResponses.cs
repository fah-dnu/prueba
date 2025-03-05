using System;

namespace DICONSA_SolicitaTransferencias.Entidades
{
    /// <summary>
    /// Clase de control de la entidad WebService Json Responses (para las llamadas a Sr Pago)
    /// </summary>
    public class WSJsonResponses
    {
        public class Login
        {
            public object   Connection  { get; set; }
            public bool     Success     { get; set; }
            public object   Error       { get; set; }
        }

        public class LoginConnection
        {
            public string Token     { get; set; }
            public string Expires   { get; set; }
        }

        public class GetBankAccounts
        {
            public bool     Success     { get; set; }
            public object   Result      { get; set; }
            public object   Error       { get; set; }
        }

        public class GetBankAccountsResult
        {
            public Int64    Id                      { get; set; }
            public string   Bank_Name               { get; set; }
            public string   Alias                   { get; set; }
            public string   Type                    { get; set; }
            public string   Account_Number_Suffix   { get; set; }
            public bool     Active                  { get; set; }
            public string   Authorized              { get; set; }
            public string   Timestamp               { get; set; }
            public bool     Default                 { get; set; }
        }

        public class PostTransfer
        {
            public bool     Success     { get; set; }
            public object   Result      { get; set; }
            public object   Error       { get; set; }
        }
        
        public class Error
        {
            public string   Code                { get; set; }
            public string   Message             { get; set; }
            public string   Description         { get; set; }
            public int      Http_Status_Code    { get; set; }
        }      
    }
}
