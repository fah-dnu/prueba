using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class Tjsn_js
    {
        private string _fileId;
        private string _issuerId;
        private string _clientId;
        private string _idSubemissor;
        private string _brand;
        private string _filenameBase2;
        private string _filename;
        private string _sequence;
        private string _fileNumber;
        private string _totalFiles;
        private string _referenceDate;
        private string _recordsTotal;
        private string _recordsAmnt;
        private string _creditTotal;
        private string _creditAmnt;
        private string _debitTotal;
        private string _debitAmnt;
        private string _unknownTotal;
        private string _unknownAmnt;
        private string _rejectedTotal;
        private string _rejectedAmnt;
        private string _occurrenceTotal;
        private string _occurrenceAmnt;
        private string _expiredTotal;
        private List<Content> _content;

        /// <summary>
        /// Propiedad que obtiene el ID de archivo de Root
        /// </summary>
        [JsonProperty("FILE_ID")]
        public string FileId { get { return _fileId; } set { _fileId = value; } }

        /// <summary>
        /// Propiedad que obtiene el ID de emisor de Root
        /// </summary>
        [JsonProperty("ISSUER_ID")]
        public string IssuerId { get { return _issuerId; } set { _issuerId = value; } }

        /// <summary>
        /// Propiedad que obtiene el ID de cliente de Root
        /// </summary>
        [JsonProperty("CLIENT_ID")]
        public string ClientId { get { return _clientId; } set { _clientId = value; } }

        /// <summary>
        /// Propiedad que obtiene el ID de subemisor de Root
        /// </summary>
        [JsonProperty("ID_SUBEMISSOR")]
        public string IdSubemissor { get { return _idSubemissor; } set { _idSubemissor = value; } }

        /// <summary>
        /// Propiedad que obtiene la marca de Root
        /// </summary>
        [JsonProperty("BRAND")]
        public string Brand { get { return _brand; } set { _brand = value; } }

        /// <summary>
        /// Propiedad que obtiene el nombre base del archivo de Root
        /// </summary>
        [JsonProperty("FILENAME_BASE2")]
        public string FilenameBase2 { get { return _filenameBase2; } set { _filenameBase2 = value; } }

        /// <summary>
        /// Propiedad que obtiene el nombre del archivo de Root
        /// </summary>
        [JsonProperty("FILENAME")]
        public string Filename { get { return _filename; } set { _filename = value; } }

        /// <summary>
        /// Propiedad que obtiene la secuencia de Root
        /// </summary>
        [JsonProperty("SEQUENCE")]
        public string Sequence { get { return _sequence; } set { _sequence = value; } }

        /// <summary>
        /// Propiedad que obtiene el número de archivo de Root
        /// </summary>
        [JsonProperty("FILE_NUMBER")]
        public string FileNumber { get { return _fileNumber; } set { _fileNumber = value; } }

        /// <summary>
        /// Propiedad que obtiene el número total de archivos de Root
        /// </summary>
        [JsonProperty("TOTAL_FILES")]
        public string TotalFiles { get { return _totalFiles; } set { _totalFiles = value; } }

        /// <summary>
        /// Propiedad que obtiene la fecha de referencia de Root
        /// </summary>
        [JsonProperty("REFERENCE_DATE")]
        public string ReferenceDate { get { return _referenceDate; } set { _referenceDate = value; } }

        /// <summary>
        /// Propiedad que obtiene el total de registros de Root
        /// </summary>
        [JsonProperty("RECORDS_TOTAL")]
        public string RecordsTotal { get { return _recordsTotal; } set { _recordsTotal = value; } }

        /// <summary>
        /// Propiedad que obtiene el importe de registros de Root
        /// </summary>
        [JsonProperty("RECORDS_AMNT")]
        public string RecordsAmnt { get { return _recordsAmnt; } set { _recordsAmnt = value; } }

        /// <summary>
        /// Propiedad que obtiene el total de créditos de Root
        /// </summary>
        [JsonProperty("CREDIT_TOTAL")]
        public string CreditTotal { get { return _creditTotal; } set { _creditTotal = value; } }

        /// <summary>
        /// Propiedad que obtiene el importe de créditos de Root
        /// </summary>
        [JsonProperty("CREDIT_AMNT")]
        public string CreditAmnt { get { return _creditAmnt; } set { _creditAmnt = value; } }

        /// <summary>
        /// Propiedad que obtiene el total de débitos de Root
        /// </summary>
        [JsonProperty("DEBIT_TOTAL")]
        public string DebitTotal { get { return _debitTotal; } set { _debitTotal = value; } }

        /// <summary>
        /// Propiedad que obtiene el importe de débitos de Root
        /// </summary>
        [JsonProperty("DEBIT_AMNT")]
        public string DebitAmnt { get { return _debitAmnt; } set { _debitAmnt = value; } }

        /// <summary>
        /// Propiedad que obtiene el total de desconocidos de Root
        /// </summary>
        [JsonProperty("UNKNOWN_TOTAL")]
        public string UnknownTotal { get { return _unknownTotal; } set { _unknownTotal = value; } }

        /// <summary>
        /// Propiedad que obtiene el importe de desconocidos de Root
        /// </summary>
        [JsonProperty("UNKNOWN_AMNT")]
        public string UnknownAmnt { get { return _unknownAmnt; } set { _unknownAmnt = value; } }

        /// <summary>
        /// Propiedad que obtiene el total de rechazados de Root
        /// </summary>
        [JsonProperty("REJECTED_TOTAL")]
        public string RejectedTotal { get { return _rejectedTotal; } set { _rejectedTotal = value; } }

        /// <summary>
        /// Propiedad que obtiene el importe de rechazados de Root
        /// </summary>
        [JsonProperty("REJECTED_AMNT")]
        public string RejectedAmnt { get { return _rejectedAmnt; } set { _rejectedAmnt = value; } }

        /// <summary>
        /// Propiedad que obtiene el total de ocurrencias de Root
        /// </summary>
        [JsonProperty("OCCURRENCE_TOTAL")]
        public string OccurrenceTotal { get { return _occurrenceTotal; } set { _occurrenceTotal = value; } }

        /// <summary>
        /// Propiedad que obtiene el importe de ocurrencias de Root
        /// </summary>
        [JsonProperty("OCCURRENCE_AMNT")]
        public string OccurrenceAmnt { get { return _occurrenceAmnt; } set { _occurrenceAmnt = value; } }

        /// <summary>
        /// Propiedad que obtiene el total de vencidos de Root
        /// </summary>
        [JsonProperty("EXPIRED_TOTAL")]
        public string ExpiredTotal { get { return _expiredTotal; } set { _expiredTotal = value; } }

        /// <summary>
        /// Propiedad que obtiene la lista de contenido de Root
        /// </summary>
        [JsonProperty("CONTENT")]
        public List<Content> Content { get { return _content; } set { _content = value; } }


        public Tjsn_js()
        {
            _content = new List<Content>();
        }


    }

    public class CardholderBillingData
    {
        private string _billingValue;
        private string _billingCurrency;
        private string _billingConversionRate;

        /// <summary>
        /// Propiedad que obtiene el valor de Billing Value
        /// </summary>
        [JsonProperty("BILLING_VALUE")]
        public string BillingValue { get { return _billingValue; } set { _billingValue = value; } }

        /// <summary>
        /// Propiedad que obtiene la moneda de Billing
        /// </summary>
        [JsonProperty("BILLING_CURRENCY")]
        public string BillingCurrency { get { return _billingCurrency; } set { _billingCurrency = value; } }

        /// <summary>
        /// Propiedad que obtiene la tasa de conversión de Billing
        /// </summary>
        [JsonProperty("BILLING_CONVERSION_RATE")]
        public string BillingConversionRate { get { return _billingConversionRate; } set { _billingConversionRate = value; } }

        public CardholderBillingData()
        {
            _billingValue = string.Empty;
            _billingCurrency = string.Empty;
            _billingConversionRate = string.Empty;
        }
    }
    public class Clearing
    {
        private string _version;
        private string _installment;
        private string _currency;
        private string _value;
        private string _boardingFee;
        private string _commission;
        private string _interchangeFeeSign;
        private string _settlementDate;
        private string _isInternational;
        private string _presentation;
        private string _actionCode;
        private string _totalPartialTransaction;
        private string _flagPartialSettlement;
        private string _cancel;
        private string _confirm;
        private string _add;
        private string _credit;
        private string _debit;

        /// <summary>
        /// Propiedad que obtiene la versión de Clearing
        /// </summary>
        [JsonProperty("VERSION")]
        public string Version { get { return _version; } set { _version = value; } }

        /// <summary>
        /// Propiedad que obtiene el número de cuotas de Clearing
        /// </summary>
        [JsonProperty("INSTALLMENT")]
        public string Installment { get { return _installment; } set { _installment = value; } }

        /// <summary>
        /// Propiedad que obtiene la moneda de Clearing
        /// </summary>
        [JsonProperty("CURRENCY")]
        public string Currency { get { return _currency; } set { _currency = value; } }

        /// <summary>
        /// Propiedad que obtiene el valor de Clearing
        /// </summary>
        [JsonProperty("VALUE")]
        public string Value { get { return _value; } set { _value = value; } }

        /// <summary>
        /// Propiedad que obtiene la indicación de boarding fee de Clearing
        /// </summary>
        [JsonProperty("BOARDING_FEE")]
        public string BoardingFee { get { return _boardingFee; } set { _boardingFee = value; } }

        /// <summary>
        /// Propiedad que obtiene la comisión de Clearing
        /// </summary>
        [JsonProperty("COMMISSION")]
        public string Commission { get { return _commission; } set { _commission = value; } }

        /// <summary>
        /// Propiedad que obtiene el signo de la tasa de intercambio de Clearing
        /// </summary>
        [JsonProperty("INTERCHANGE_FEE_SIGN")]
        public string InterchangeFeeSign { get { return _interchangeFeeSign; } set { _interchangeFeeSign = value; } }

        /// <summary>
        /// Propiedad que obtiene la fecha de liquidación de Clearing
        /// </summary>
        [JsonProperty("SETTLEMENT_DATE")]
        public string SettlementDate { get { return _settlementDate; } set { _settlementDate = value; } }

        /// <summary>
        /// Propiedad que obtiene la indicación de si es internacional de Clearing
        /// </summary>
        [JsonProperty("IS_INTERNATIONAL")]
        public string IsInternational { get { return _isInternational; } set { _isInternational = value; } }

        /// <summary>
        /// Propiedad que obtiene la presentación de Clearing
        /// </summary>
        [JsonProperty("PRESENTATION")]
        public string Presentation { get { return _presentation; } set { _presentation = value; } }

        /// <summary>
        /// Propiedad que obtiene el código de acción de Clearing
        /// </summary>
        [JsonProperty("ACTION_CODE")]
        public string ActionCode { get { return _actionCode; } set { _actionCode = value; } }

        /// <summary>
        /// Propiedad que obtiene el número total de transacciones parciales de Clearing
        /// </summary>
        [JsonProperty("TOTAL_PARTIAL_TRANSACTION")]
        public string TotalPartialTransaction { get { return _totalPartialTransaction; } set { _totalPartialTransaction = value; } }

        /// <summary>
        /// Propiedad que obtiene la indicación de transacción parcial de Clearing
        /// </summary>
        [JsonProperty("FLAG_PARTIAL_SETTLEMENT")]
        public string FlagPartialSettlement { get { return _flagPartialSettlement; } set { _flagPartialSettlement = value; } }

        /// <summary>
        /// Propiedad que obtiene la indicación de cancelación de Clearing
        /// </summary>
        [JsonProperty("CANCEL")]
        public string Cancel { get { return _cancel; } set { _cancel = value; } }

        /// <summary>
        /// Propiedad que obtiene la indicación de confirmación de Clearing
        /// </summary>
        [JsonProperty("CONFIRM")]
        public string Confirm { get { return _confirm; } set { _confirm = value; } }

        /// <summary>
        /// Propiedad que obtiene la indicación de adición de Clearing
        /// </summary>
        [JsonProperty("ADD")]
        public string Add { get { return _add; } set { _add = value; } }

        /// <summary>
        /// Propiedad que obtiene la indicación de crédito de Clearing
        /// </summary>
        [JsonProperty("CREDIT")]
        public string Credit { get { return _credit; } set { _credit = value; } }

        /// <summary>
        /// Propiedad que obtiene la indicación de débito de Clearing
        /// </summary>
        [JsonProperty("DEBIT")]
        public string Debit { get { return _debit; } set { _debit = value; } }

        public Clearing()
        {
            _version = string.Empty;
            _installment = string.Empty;
            _currency = string.Empty;
            _value = string.Empty;
            _boardingFee = string.Empty;
            _commission = string.Empty;
            _interchangeFeeSign = string.Empty;
            _settlementDate = string.Empty;
            _isInternational = string.Empty;
            _presentation = string.Empty;
            _actionCode = string.Empty;
            _totalPartialTransaction = string.Empty;
            _flagPartialSettlement = string.Empty;
            _cancel = string.Empty;
            _confirm = string.Empty;
            _add = string.Empty;
            _credit = string.Empty;
            _debit = string.Empty;
        }
    }
    public class Transaction
    {
        private string _arn = string.Empty;
        private string _cardBrandId = string.Empty;
        private string _externalId = string.Empty;
        private string _version = string.Empty;
        private string _pan = string.Empty;
        private string _binCard = string.Empty;
        private string _cardId = string.Empty;
        private string _authorization = string.Empty;
        private string _localDate = string.Empty;
        private string _gmtDate = string.Empty;
        private string _installmentNbr = string.Empty;
        private string _mcc = string.Empty;
        private string _sourceCurrency = string.Empty;
        private string _sourceValue = string.Empty;
        private string _destCurrency = string.Empty;
        private string _destValue = string.Empty;
        private string _purchaseValue = string.Empty;
        private string _installmentValue1 = string.Empty;
        private string _installmentValueN = string.Empty;
        private string _boardingFee = string.Empty;
        private string _merchant = string.Empty;
        private MerchantData _merchantData;
        private string _entryMode = string.Empty;
        private string _authorizationDate = string.Empty;
        private string _status = string.Empty;
        private string _transactionQualifier = string.Empty;
        private List<object> _classification;
        private string _operationType = string.Empty;
        private string _posEntryMode = string.Empty;
        private string _issuerExchangeRate = string.Empty;
        private string _cdtAmount = string.Empty;
        private string _productCode = string.Empty;
        private string _reasonCode = string.Empty;
        private string _uuid = string.Empty;
        private string _operationCode = string.Empty;
        private string _agency = string.Empty;
        private string _accountNumber = string.Empty;
        private string _latePresentation = string.Empty;
        private string _errorCode = string.Empty;
        private CardholderBillingData _cardholderBillingData;
        private string _receivedChange = string.Empty;

        /// <summary>
        /// Propiedad que obtiene el ARN de Transaction
        /// </summary>
        [JsonProperty("ARN")]
        public string Arn { get { return _arn; } set { _arn = value; } }

        /// <summary>
        /// Propiedad que obtiene el ID de la marca de tarjeta de Transaction
        /// </summary>
        [JsonProperty("ID_CARDBRAND")]
        public string CardBrandId { get { return _cardBrandId; } set { _cardBrandId = value; } }

        /// <summary>
        /// Propiedad que obtiene el ID externo de Transaction
        /// </summary>
        [JsonProperty("EXTERNAL_ID")]
        public string ExternalId { get { return _externalId; } set { _externalId = value; } }

        /// <summary>
        /// Propiedad que obtiene la versión de Transaction
        /// </summary>
        [JsonProperty("VERSION")]
        public string Version { get { return _version; } set { _version = value; } }

        /// <summary>
        /// Propiedad que obtiene el PAN de Transaction
        /// </summary>
        [JsonProperty("PAN")]
        public string Pan { get { return _pan; } set { _pan = value; } }

        /// <summary>
        /// Propiedad que obtiene el BIN de la tarjeta de Transaction
        /// </summary>
        [JsonProperty("BIN_CARD")]
        public string BinCard { get { return _binCard; } set { _binCard = value; } }

        /// <summary>
        /// Propiedad que obtiene el ID de la tarjeta de Transaction
        /// </summary>
        [JsonProperty("CARD_ID")]
        public string CardId { get { return _cardId; } set { _cardId = value; } }

        /// <summary>
        /// Propiedad que obtiene la autorización de Transaction
        /// </summary>
        [JsonProperty("AUTHORIZATION")]
        public string Authorization { get { return _authorization; } set { _authorization = value; } }

        /// <summary>
        /// Propiedad que obtiene la fecha local de Transaction
        /// </summary>
        [JsonProperty("LOCAL_DATE")]
        public string LocalDate { get { return _localDate; } set { _localDate = value; } }

        /// <summary>
        /// Propiedad que obtiene la fecha GMT de Transaction
        /// </summary>
        [JsonProperty("GMT_DATE")]
        public string GmtDate { get { return _gmtDate; } set { _gmtDate = value; } }

        /// <summary>
        /// Propiedad que obtiene el número de cuotas de Transaction
        /// </summary>
        [JsonProperty("INSTALLMENT_NBR")]
        public string InstallmentNbr { get { return _installmentNbr; } set { _installmentNbr = value; } }

        /// <summary>
        /// Propiedad que obtiene el MCC de Transaction
        /// </summary>
        [JsonProperty("MCC")]
        public string Mcc { get { return _mcc; } set { _mcc = value; } }

        /// <summary>
        /// Propiedad que obtiene la moneda de origen de Transaction
        /// </summary>
        [JsonProperty("SOURCE_CURRENCY")]
        public string SourceCurrency { get { return _sourceCurrency; } set { _sourceCurrency = value; } }

        /// <summary>
        /// Propiedad que obtiene el valor de origen de Transaction
        /// </summary>
        [JsonProperty("SOURCE_VALUE")]
        public string SourceValue { get { return _sourceValue; } set { _sourceValue = value; } }

        /// <summary>
        /// Propiedad que obtiene la moneda de destino de Transaction
        /// </summary>
        [JsonProperty("DEST_CURRENCY")]
        public string DestCurrency { get { return _destCurrency; } set { _destCurrency = value; } }

        /// <summary>
        /// Propiedad que obtiene el valor de destino de Transaction
        /// </summary>
        [JsonProperty("DEST_VALUE")]
        public string DestValue { get { return _destValue; } set { _destValue = value; } }

        /// <summary>
        /// Propiedad que obtiene el valor de compra de Transaction
        /// </summary>
        [JsonProperty("PURCHASE_VALUE")]
        public string PurchaseValue { get { return _purchaseValue; } set { _purchaseValue = value; } }

        /// <summary>
        /// Propiedad que obtiene el valor de la primera cuota de Transaction
        /// </summary>
        [JsonProperty("INSTALLMENT_VALUE_1")]
        public string InstallmentValue1 { get { return _installmentValue1; } set { _installmentValue1 = value; } }

        /// <summary>
        /// Propiedad que obtiene el valor de la última cuota de Transaction
        /// </summary>
        [JsonProperty("INSTALLMENT_VALUE_N")]
        public string InstallmentValueN { get { return _installmentValueN; } set { _installmentValueN = value; } }

        /// <summary>
        /// Propiedad que obtiene el cargo por embarque de Transaction
        /// </summary>
        [JsonProperty("BOARDING_FEE")]
        public string BoardingFee { get { return _boardingFee; } set { _boardingFee = value; } }

        /// <summary>
        /// Propiedad que obtiene el comerciante de Transaction
        /// </summary>
        [JsonProperty("MERCHANT")]
        public string Merchant { get { return _merchant; } set { _merchant = value; } }

        /// <summary>
        /// Propiedad que obtiene los datos del comerciante de Transaction
        /// </summary>
        [JsonProperty("MERCHANT_DATA")]
        public MerchantData MerchantData { get { return _merchantData; } set { _merchantData = value; } }

        /// <summary>
        /// Propiedad que obtiene el modo de entrada de Transaction
        /// </summary>
        [JsonProperty("ENTRY_MODE")]
        public string EntryMode { get { return _entryMode; } set { _entryMode = value; } }

        /// <summary>
        /// Propiedad que obtiene la fecha de autorización de Transaction
        /// </summary>
        [JsonProperty("AUTHORIZATION_DATE")]
        public string AuthorizationDate { get { return _authorizationDate; } set { _authorizationDate = value; } }

        /// <summary>
        /// Propiedad que obtiene el estado de Transaction
        /// </summary>
        [JsonProperty("STATUS")]
        public string Status { get { return _status; } set { _status = value; } }

        /// <summary>
        /// Propiedad que obtiene el calificador de Transaction
        /// </summary>
        [JsonProperty("TRANSACTION_QUALIFIER")]
        public string TransactionQualifier { get { return _transactionQualifier; } set { _transactionQualifier = value; } }

        /// <summary>
        /// Propiedad que obtiene la clasificación de Transaction
        /// </summary>
        [JsonProperty("CLASSIFICATION")]
        public List<object> Classification { get { return _classification; } set { _classification = value; } }

        /// <summary>
        /// Propiedad que obtiene el tipo de operación de Transaction
        /// </summary>
        [JsonProperty("OPERATION_TYPE")]
        public string OperationType { get { return _operationType; } set { _operationType = value; } }

        /// <summary>
        /// Propiedad que obtiene el modo de entrada POS de Transaction
        /// </summary>
        [JsonProperty("POS_ENTRY_MODE")]
        public string PosEntryMode { get { return _posEntryMode; } set { _posEntryMode = value; } }

        /// <summary>
        /// Propiedad que obtiene la tasa de cambio del emisor de Transaction
        /// </summary>
        [JsonProperty("ISSUER_EXCHANGE_RATE")]
        public string IssuerExchangeRate { get { return _issuerExchangeRate; } set { _issuerExchangeRate = value; } }

        /// <summary>
        /// Propiedad que obtiene el monto CDT de Transaction
        /// </summary>
        [JsonProperty("CDT_AMOUNT")]
        public string CdtAmount { get { return _cdtAmount; } set { _cdtAmount = value; } }

        /// <summary>
        /// Propiedad que obtiene el código de producto de Transaction
        /// </summary>
        [JsonProperty("PRODUCT_CODE")]
        public string ProductCode { get { return _productCode; } set { _productCode = value; } }

        /// <summary>
        /// Propiedad que obtiene el código de razón de Transaction
        /// </summary>
        [JsonProperty("REASON_CODE")]
        public string ReasonCode { get { return _reasonCode; } set { _reasonCode = value; } }

        /// <summary>
        /// Propiedad que obtiene el UUID de Transaction
        /// </summary>
        [JsonProperty("UUID")]
        public string UUID { get { return _uuid; } set { _uuid = value; } }

        /// <summary>
        /// Propiedad que obtiene el código de operación de Transaction
        /// </summary>
        [JsonProperty("OPERATION_CODE")]
        public string OperationCode { get { return _operationCode; } set { _operationCode = value; } }

        /// <summary>
        /// Propiedad que obtiene la agencia de Transaction
        /// </summary>
        [JsonProperty("AGENCY")]
        public string Agency { get { return _agency; } set { _agency = value; } }

        /// <summary>
        /// Propiedad que obtiene el número de cuenta de Transaction
        /// </summary>
        [JsonProperty("ACCOUNT_NUMBER")]
        public string AccountNumber { get { return _accountNumber; } set { _accountNumber = value; } }

        /// <summary>
        /// Propiedad que obtiene la presentación tardía de Transaction
        /// </summary>
        [JsonProperty("LATE_PRESENTATION")]
        public string LatePresentation { get { return _latePresentation; } set { _latePresentation = value; } }

        /// <summary>
        /// Propiedad que obtiene el código de error de Transaction
        /// </summary>
        [JsonProperty("ERROR_CODE")]
        public string ErrorCode { get { return _errorCode; } set { _errorCode = value; } }

        /// <summary>
        /// Propiedad que obtiene los datos de facturación del titular de la tarjeta de Transaction
        /// </summary>
        [JsonProperty("CARDHOLDER_BILLING_DATA")]
        public CardholderBillingData CardholderBillingData { get { return _cardholderBillingData; } set { _cardholderBillingData = value; } }

        /// <summary>
        /// Propiedad que obtiene el cambio recibido de Transaction
        /// </summary>
        [JsonProperty("RECEIVED_CHANGE")]
        public string ReceivedChange { get { return _receivedChange; } set { _receivedChange = value; } }

        public Transaction()
        {
            _arn = string.Empty;
            _cardBrandId = string.Empty;
            _externalId = string.Empty;
            _version = string.Empty;
            _pan = string.Empty;
            _binCard = string.Empty;
            _cardId = string.Empty;
            _authorization = string.Empty;
            _cardholderBillingData = new CardholderBillingData();


        }
    }
    public class Content
    {
        private string _id;
        private string _recordCode;
        private Transaction _transaction;
        private Clearing _clearing;

        /// <summary>
        /// Propiedad que obtiene el ID de Content
        /// </summary>
        [JsonProperty("ID")]
        public string Id { get { return _id; } set { _id = value; } }

        /// <summary>
        /// Propiedad que obtiene el código de registro de Content
        /// </summary>
        [JsonProperty("RECORD_CODE")]
        public string RecordCode { get { return _recordCode; } set { _recordCode = value; } }

        /// <summary>
        /// Propiedad que obtiene la transacción de Content
        /// </summary>
        [JsonProperty("TRANSACTION")]
        public Transaction Transaction { get { return _transaction; } set { _transaction = value; } }

        /// <summary>
        /// Propiedad que obtiene el Clearing de Content
        /// </summary>
        [JsonProperty("CLEARING")]
        public Clearing Clearing { get { return _clearing; } set { _clearing = value; } }

        public Content()
        {
            _id = string.Empty;
            _recordCode = string.Empty;
            _transaction = new Transaction();
            _clearing = new Clearing();
        }
    }
    public class MerchantData
    {
        private string _serviceLocation;

        /// <summary>
        /// Propiedad que obtiene la ubicación de servicio de MerchantData
        /// </summary>
        [JsonProperty("SERVICE_LOCATION")]
        public string ServiceLocation { get { return _serviceLocation; } set { _serviceLocation = value; } }

        public MerchantData()
        {
            _serviceLocation = string.Empty;
        }
    }
}
