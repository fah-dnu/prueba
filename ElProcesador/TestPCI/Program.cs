using DNU_CompensadorParabiliumProcesador;
using DNU_CompensadorParabiliumProcesador.Utilidades;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Mime;
using Newtonsoft.Json.Linq;
using CommonProcesador;
using DNU_CompensadorParabiliumMigration;

namespace TestPCI
{
    class Program
    {//
        static void Main(string[] args)
        {
            //string fileTempEncript = "C:\\Users\\fmaya\\Documents\\Fernando\\Test1FarefoBACK.txt.gpg";
            //string keyFileName = "D:\\DocumentosDNU\\Farefo\\ProcesadorNocturno\\Compensacion\\Producción\\KeysTest\\FarefoV2_0x090985B9_SECRET.asc";
            //EncryptionService.DecryptFile(fileTempEncript, keyFileName
            //                                , "Hhhtv123@twtFRF."
            //                                , "C:\\Users\\fmaya\\Documents\\Fernando\\Test1Farefo.txt");

            var Proceso = "Carga";

            //dynamic js = JsonConvert.DeserializeObject<dynamic>("{\"FILE_ID\": \"0012309090356934563280642\", \"ISSUER_ID\": \"43732\", \"CLIENT_ID\": \"054986a2-2bd8-643r-x3s3-b8d6c874609d\", \"ID_SUBEMISSOR\": \"\", \"BRAND\": \"MASTERCARD\", \"FILENAME_BASE2\": \"T11225201.ipm\", \"FILENAME\": \"MASTERCARD_TRANSACTIONAL_43732_232521_1_20230909133118\", \"SEQUENCE\": 1, \"FILE_NUMBER\": 1, \"TOTAL_FILES\": 1, \"REFERENCE_DATE\": \"2023-09-09\", \"RECORDS_TOTAL\": 2, \"RECORDS_AMNT\": 78.81, \"CREDIT_TOTAL\": 0, \"CREDIT_AMNT\": 0.0, \"DEBIT_TOTAL\": 2, \"DEBIT_AMNT\": 78.81, \"UNKNOWN_TOTAL\": 0, \"UNKNOWN_AMNT\": 0.0, \"REJECTED_TOTAL\": 0, \"REJECTED_AMNT\": 0.0, \"OCCURRENCE_TOTAL\": 0, \"OCCURRENCE_AMNT\": 0.0, \"EXPIRED_TOTAL\": 0, \"CONTENT\": [{\"ID\": \"1202309090185790e-bc50-b588-ee86-bcc61da748b756204518\", \"RECORD_CODE\": 18, \"TRANSACTION\": {\"ARN\": \"55432862346543876543968\", \"ID_CARDBRAND\": \"MDSV5GYOZ0907\", \"EXTERNAL_ID\": \"018a7131-9448-f883-d2e2-f6a219920eae\", \"VERSION\": 1, \"PAN\": \"519357XXXXXX1814\", \"BIN_CARD\": \"519357\", \"CARD_ID\": \"0185790e-bc50-b588-ee86-bcc61da748b7\", \"PRODUCT_REFERENCE_ID\": \"MDS;MDS\", \"TRANSACTION_TYPE_INDICATOR\": \"DEBIT\", \"AUTHORIZATION\": \"438765\", \"LOCAL_DATE\": \"2023-09-07\", \"GMT_DATE\": \"2023-09-07 00:00:00\", \"INSTALLMENT_NBR\": 1, \"MCC\": 4111, \"SOURCE_CURRENCY\": 840, \"SOURCE_VALUE\": 2.9, \"DEST_CURRENCY\": 484, \"DEST_VALUE\": 51.21, \"PURCHASE_VALUE\": 51.21, \"INSTALLMENT_DATA\": {\"INSTALLMENT_TYPE\": \"\", \"GRACE_PERIOD\": 0}, \"INSTALLMENT_VALUE_1\": 51.21, \"INSTALLMENT_VALUE_N\": 0.0, \"BOARDING_FEE\": 0.0, \"MERCHANT\": \"MTA NYCT PAYGO 2 BROADWAY NEW YORK 10004 NY USA\", \"MERCHANT_DATA\": {\"SERVICE_LOCATION\": \"\"}, \"BUSINESS_ARRANGEMENT\": {\"CARD_ACCEPTOR_TAX_ID\": \"\"}, \"ENTRY_MODE\": 0, \"AUTHORIZATION_DATE\": \"2023-09-07T07:49:49.236000\", \"STATUS\": 6, \"TRANSACTION_QUALIFIER\": \"\", \"CLASSIFICATION\": [], \"OPERATION_TYPE\": 0, \"POS_ENTRY_MODE\": \"M\", \"ISSUER_EXCHANGE_RATE\": 17.58, \"CDT_AMOUNT\": 900.2718, \"PRODUCT_CODE\": \"D\", \"REASON_CODE\": \"1401\", \"UUID\": \"018a7131-9448-f883-d2e2-f6a219920eae\", \"OPERATION_CODE\": \"01\", \"AGENCY\": \"\", \"ACCOUNT_NUMBER\": \"\", \"LATE_PRESENTATION\": false, \"PRESENTATION_DATA\": {\"DAY_COUNTER\": 0}, \"ERROR_CODE\": \"\", \"CARDHOLDER_BILLING_DATA\": {\"BILLING_VALUE\": 50.98, \"BILLING_CURRENCY\": \"484\", \"BILLING_CONVERSION_RATE\": 17.57931}, \"RECEIVED_CHANGE\": 0.0}, \"CLEARING\": {\"VERSION\": 0, \"INSTALLMENT\": 1, \"CURRENCY\": 484, \"VALUE\": 51.21, \"BOARDING_FEE\": false, \"COMMISSION\": 0.56, \"INTERCHANGE_FEE_SIGN\": \"C\", \"SETTLEMENT_DATE\": \"2023-09-11\", \"IS_INTERNATIONAL\": true, \"PRESENTATION\": 1, \"ACTION_CODE\": 2, \"REASON_LIST\": [], \"TOTAL_PARTIAL_TRANSACTION\": 0, \"FLAG_PARTIAL_SETTLEMENT\": false, \"CANCEL\": true, \"CONFIRM\": true, \"ADD\": true, \"CREDIT\": false, \"DEBIT\": true}}, {\"ID\": \"1202309090185790e-bc50-b588-ee86-bcc61da748b786096419\", \"RECORD_CODE\": 19, \"TRANSACTION\": {\"ARN\": \"52704871357908642368934\", \"ID_CARDBRAND\": \"MDSBDK4ER0907\", \"EXTERNAL_ID\": \"018a7089-8a01-6cd0-1eee-ecb99ab1e6ae\", \"VERSION\": 1, \"PAN\": \"519357XXXXXX1814\", \"BIN_CARD\": \"519357\", \"CARD_ID\": \"0185790e-bc50-b588-ee86-bcc61da748b7\", \"PRODUCT_REFERENCE_ID\": \"MDS;MDS\", \"TRANSACTION_TYPE_INDICATOR\": \"DEBIT\", \"AUTHORIZATION\": \"329854\", \"LOCAL_DATE\": \"2023-09-07\", \"GMT_DATE\": \"2023-09-07 12:21:14\", \"INSTALLMENT_NBR\": 1, \"MCC\": 5812, \"SOURCE_CURRENCY\": 840, \"SOURCE_VALUE\": 75.91, \"DEST_CURRENCY\": 484, \"DEST_VALUE\": 1322.91, \"PURCHASE_VALUE\": 1322.91, \"INSTALLMENT_DATA\": {\"INSTALLMENT_TYPE\": \"\", \"GRACE_PERIOD\": 0}, \"INSTALLMENT_VALUE_1\": 1322.91, \"INSTALLMENT_VALUE_N\": 0.0, \"BOARDING_FEE\": 0.0, \"MERCHANT\": \"EATALY NY SALIDO 200 FIFTH AVENUE NEW YORK 10010 NY USA\", \"MERCHANT_DATA\": {\"SERVICE_LOCATION\": \"\"}, \"BUSINESS_ARRANGEMENT\": {\"CARD_ACCEPTOR_TAX_ID\": \"\"}, \"ENTRY_MODE\": 0, \"AUTHORIZATION_DATE\": \"2023-09-07T04:46:16.574000\", \"STATUS\": 6, \"TRANSACTION_QUALIFIER\": \"\", \"CLASSIFICATION\": [], \"OPERATION_TYPE\": 0, \"POS_ENTRY_MODE\": \"B\", \"ISSUER_EXCHANGE_RATE\": 17.35, \"CDT_AMOUNT\": 22952.4885, \"PRODUCT_CODE\": \"D\", \"REASON_CODE\": \"1402\", \"UUID\": \"018a7089-8a01-6cd0-1eee-ecb99ab1e6ae\", \"OPERATION_CODE\": \"01\", \"AGENCY\": \"\", \"ACCOUNT_NUMBER\": \"\", \"LATE_PRESENTATION\": false, \"PRESENTATION_DATA\": {\"DAY_COUNTER\": 0}, \"ERROR_CODE\": \"\", \"CARDHOLDER_BILLING_DATA\": {\"BILLING_VALUE\": 1316.98, \"BILLING_CURRENCY\": \"484\", \"BILLING_CONVERSION_RATE\": 17.34922}, \"RECEIVED_CHANGE\": 0.0}, \"CLEARING\": {\"VERSION\": 0, \"INSTALLMENT\": 1, \"CURRENCY\": 484, \"VALUE\": 1322.91, \"BOARDING_FEE\": false, \"COMMISSION\": 14.49, \"INTERCHANGE_FEE_SIGN\": \"C\", \"SETTLEMENT_DATE\": \"2023-09-11\", \"IS_INTERNATIONAL\": true, \"PRESENTATION\": 1, \"ACTION_CODE\": 2, \"REASON_LIST\": [], \"TOTAL_PARTIAL_TRANSACTION\": 0, \"FLAG_PARTIAL_SETTLEMENT\": false, \"CANCEL\": true, \"CONFIRM\": true, \"ADD\": true, \"CREDIT\": false, \"DEBIT\": true}}]}");
            //int val = js.CONTENT.Count;
            ////string origenClave = "CONTENT.TRANSACTION.OPERATION_TYPE";
            //string origenClave = "OPERATION_TYPE";
            //dynamic val3 = "CONTENT[0].TRANSACTION.OPERATION_TYPE";
            //string val2 = js.CONTENT[0][val3];
            //string TRANSACTION = null;

            //Tuple.Create(origenClave, val3);

            //dynamic test = js.val3;

            //foreach (JProperty p in js.CONTENT[0].Properties())
            //{
            //    if(p.Contains(origenClave))
            //        Console.WriteLine(p.Name + ":" + p.Value.ToString());
            //    //Console.WriteLine(p.Name + ":" + p.Value.ToString());
            //}

            //if (js.CONTENT[0].ContainsKey(origenClave))
            //{
            //    Console.WriteLine($"Address: {js.address}");
            //}


            //Migrate.Start("");
            //ParabiliumProcesador.Listening();
            //ParabiliumProcesador.Listening();
            //ParabiliumProcesador.Start();

            //ProcesarVencimientos provVen = new ProcesarVencimientos();
            //provVen.ProcesarloTest();

            //Proceso de embozo
            //Manager.Start();

            //Procesar archivos IPM en Debug
            if (Proceso == "Carga")
            {
                string direccionIP = System.Net.Dns.GetHostName();
                Guid idLog = Guid.NewGuid();
                ThreadContext.Properties["log"] = idLog;
                ThreadContext.Properties["ip"] = direccionIP;

                string log = ThreadContext.Properties["log"].ToString();
                string ip = ThreadContext.Properties["ip"].ToString();
                string etiquetaProcesoCompLog = "[" + ip + "] [" + log + "] [COMPENSACION LOCAL] ";
                LNArchivoListener.EscucharDirectorio(etiquetaProcesoCompLog);
                LogueoCompensador.Info(etiquetaProcesoCompLog + "[TERMINA EscucharDirectorios]");
                while (true) { }
                ;
            }
            //if (Proceso == "IPM")
            //{
            //    var obj = new RespuestaInsertFicheroTemp { IdFicheroTemp = 5 };
            //    var definicionFicheroTemp = DAOArchivo.ObtieneDefinicionFicheroTemp(obj, new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo), "TEST");
            //    FileInfo fileInfo = new FileInfo(@"C:\FTP\LocalUser\ftp_volcan_in\Compensacion_files\Errores\ERROR04_MCI.AR.T067.B.CCAR1212.D230911.T111135.A001.ipm");
            //    DataTable dataTable = DAOArchivo.CrearTablaFicheroProvicional();
            //    bool flag = DAOArchivo.LlenarTablaFicheroProvicional(dataTable, definicionFicheroTemp, obj, fileInfo, "TEST");
            //}


            //Azure Blob Storage
            //IProcesoNocturno _reportes = new ProcesaReportes();
            //_reportes.Procesar();
        }
    }
}
