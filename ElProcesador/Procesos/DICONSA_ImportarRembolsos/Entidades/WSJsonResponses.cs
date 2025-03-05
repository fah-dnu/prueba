using System;

namespace DICONSA_ImportarRembolsos.Entidades
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

        public class GetWithdrawal
        {
            public bool     Success     { get; set; }
            public object   Result      { get; set; }
            public object   Error       { get; set; }
        }

        public class GetWithdrawalResult
        {
            public Int32    Total       { get; set; }
            public object   Operations  { get; set; }
        }

        public class WithdrawalOperations
        {
            public object Total         { get; set; }
            public string Status        { get; set; }
            public object Bank_Account  { get; set; }
            public object Commission    { get; set; }
            public string Date_Request  { get; set; }
            public string Date_Apply    { get; set; }
        }

        public class OperationsAmmounts
        {
            public string Amount { get; set; }
            public string Currency { get; set; }
        }

        public class BankAccount
        {
            public string Bank_Name             { get; set; }
            public string Alias                 { get; set; }
            public string Account_Number_Suffix { get; set; }
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
