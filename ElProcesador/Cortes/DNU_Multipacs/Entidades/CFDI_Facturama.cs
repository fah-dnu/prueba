using System;
using System.Collections.Generic;

namespace DNU_Multipacs.Entidades
{
    public class CFDI_Facturama
    {
        public string Id { get; set; }

        public Issuer Issuer { get; set; }
        public string OrderNumber { get; set; }
        public string Observations { get; set; }
        public List<Item> Items { get; set; }
        public Receiver Receiver { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentForm { get; set; }
        public string CfdiType { get; set; }
        //public CfdiRelations Relations { get; set; }
        public string PaymentConditions { get; set; }
        public string ExpeditionPlace { get; set; }
        public string Currency { get; set; }
        public decimal? CurrencyExchangeRate { get; set; }
        public string PaymentAccountNumber { get; set; }
        public string Serie { get; set; }
        public string Date { get; set; }
        public string NameId { get; set; }
        public string Folio { get; set; }
        public string PaymentBankName { get; set; }
        public Complement Complement { get; set; }
    }

    public class Issuer
    {
        public string FiscalRegime { get; set; }
        public string Rfc { get; set; }
        public string Name { get; set; }
    }

    public class Item
    {
        public string ProductId { get; set; }
        public string ProductCode { get; set; }
        public string IdentificationNumber { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string UnitCode { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public List<Tax> Taxes { get; set; }
        public string CuentaPredial { get; set; }
        public decimal Total { get; set; }
    }

    public class Receiver
    {
        public string Id { get; set; }
        public string Rfc { get; set; }
        public string Name { get; set; }
        public string CfdiUse { get; set; }
        public string TaxResidence { get; set; }
        public string TaxRegistrationNumber { get; set; }
    }

    //public class CfdiRelations
    //{
    //    public string Type { get; set; }
    //    public List<CfdiRelation> Cfdis { get; set; }
    //}

    public class Tax
    {
        public decimal Total { get; set; }
        public string Name { get; set; }
        public decimal Base { get; set; }
        public decimal Rate { get; set; }
        public bool IsRetention { get; set; }
        public bool IsQuota { get; set; }
    }

    public class Complement
    {
        public TaxStamp TaxStamp { get; set; }
        //public List<Payment> Payments { get; set; }
        //public Payroll Payroll { get; set; }        
    }

    public class TaxStamp
    {
        public string Uuid { get; set; }
        public DateTime Date { get; set; }
        public string CfdiSign { get; set; }
        public string SatCertNumber { get; set; }
        public string SatSign { get; set; }
        public string RfcProvCertif { get; set; }
        public string AutNumProvCertif { get; set; }
        public string FacturaXML { get; set; } //agregado
        public string Error { get; set; } //agregado
    }

}
