using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumCommon.Entidades
{
    public class RegistroCompensar
    {
        private string m_cveColectiva;
        private string m_descColectiva;
        private string m_cveProducto;
        private string m_grupoCuenta;
        private string m_bin;
        private string m_idFichero;
        private string m_fichero;
        private string m_fechaPresentacion;
        private string m_ciclo;
        private string m_idFicheroDetalle;
        private string m_codigoProceso;
        private string m_codigoFuncion;
        private string m_reverso;
        private string m_representacion;
        private string m_tarjeta;
        private string m_tarjetaEnmascarada;
        private string m_medioAcceso;
        private string m_tipoMedioAcceso;
        private string m_autorizacion;
        private string m_referencia;
        private string m_referencia2;
        private string m_fechaJuliana;
        private string m_fechaOperacion;
        private string m_horaOperacion;
        private string m_impOrigen;
        private string m_cveDivisaOrigen;
        private string m_impDestino;
        private string m_cveDivisaDestino;
        private string m_impCuotaIntercambio;
        private string m_cveDivisaCuotaIntercambio;
        private string m_impIva;
        private string m_cveDivisaIva;
        private string m_impMCCR;
        private string m_cveDivisaMCCR;
        private string m_impMarkUp;
        private string m_cveDivisaMarkUp;
        private string m_impCompensacion;
        private string m_cveDivisaCompensacion;
        private string m_tipoCambioProcesador;
        private string m_comercio;
        private string m_comercioCiudad;
        private string m_comercioPais;
        private string m_comercioMCC;
        private string m_comercioCP;
        private string m_comercioEstado;
        private string m_transaccionDestino;
        private string m_transaccionOrigen;
        private string m_cveEstatusFicheroDetalle;
        private string m_cveTipoAlias;
        private string m_cveTipoRed;

        private string m_tipoCambio;
        private string m_codigoPostal;
        private string m_estado;
        private string m_importeOriginal;
        private string m_importeComisiones;
        private string m_exponentesMoneda;
        private string m_referenciaEmisor;
        private string m_identificadorMensaje;
        private string m_codigoAdquirente;
        private string m_codigoDocumento;
        private string m_codigoRazonMensaje;
        private string m_actividadNegocio;
        private string m_modoEntradaPos;
        private string m_tipoTerminal;
        private string m_numeroControl;
        private string m_indicadorLiquidacion;
        private string m_indicadorDocumentacion;

        private string m_identificadorTransaccion;

        public string cveColectiva { get => m_cveColectiva; set => m_cveColectiva = value; }
        public string descColectiva { get => m_descColectiva; set => m_descColectiva = value; }
        public string cveProducto { get => m_cveProducto; set => m_cveProducto = value; }
        public string grupoCuenta { get => m_grupoCuenta; set => m_grupoCuenta = value; }
        public string bin { get => m_bin; set => m_bin = value; }
        public string idFichero { get => m_idFichero; set => m_idFichero = value; }
        public string fichero { get => m_fichero; set => m_fichero = value; }
        public string ciclo { get => m_ciclo; set => m_ciclo = value; }
        public string fechaPresentacion { get => m_fechaPresentacion; set => m_fechaPresentacion = value; }
        public string idFicheroDetalle { get => m_idFicheroDetalle; set => m_idFicheroDetalle = value; }
        public string codigoProceso { get => m_codigoProceso; set => m_codigoProceso = value; }
        public string codigoFuncion { get => m_codigoFuncion; set => m_codigoFuncion = value; }
        public string reverso { get => m_reverso; set => m_reverso = value; }
        public string representacion { get => m_representacion; set => m_representacion = value; }
        public string tarjeta { get => m_tarjeta; set => m_tarjeta = value; }
        public string tarjetaEnmascarada { get => m_tarjetaEnmascarada; set => m_tarjetaEnmascarada = value; }
        public string MedioAcceso { get => m_medioAcceso; set => m_medioAcceso = value; }
        public string TipoMedioAcceso { get => m_tipoMedioAcceso; set => m_tipoMedioAcceso = value; }
        public string autorizacion { get => m_autorizacion; set => m_autorizacion = value; }
        public string referencia { get => m_referencia; set => m_referencia = value; }
        public string referencia2 { get => m_referencia2; set => m_referencia2 = value; }
        public string fechaJuliana { get => m_fechaJuliana; set => m_fechaJuliana = value; }
        public string fechaOperacion { get => m_fechaOperacion; set => m_fechaOperacion = value; }
        public string horaOperacion { get => m_horaOperacion; set => m_horaOperacion = value; }
        public string impOrigen { get => m_impOrigen; set => m_impOrigen = value; }
        public string cveDivisaOrigen { get => m_cveDivisaOrigen; set => m_cveDivisaOrigen = value; }
        public string impDestino { get => m_impDestino; set => m_impDestino = value; }
        public string cveDivisaDestino { get => m_cveDivisaDestino; set => m_cveDivisaDestino = value; }
        public string impCuotaIntercambio { get => m_impCuotaIntercambio; set => m_impCuotaIntercambio = value; }
        public string cveDivisaCuotaIntercambio { get => m_cveDivisaCuotaIntercambio; set => m_cveDivisaCuotaIntercambio = value; }
        public string impIva { get => m_impIva; set => m_impIva = value; }
        public string cveDivisaIva { get => m_cveDivisaIva; set => m_cveDivisaIva = value; }
        public string impMCCR { get => m_impMCCR; set => m_impMCCR = value; }
        public string cveDivisaMCCR { get => m_cveDivisaMCCR; set => m_cveDivisaMCCR = value; }
        public string impMarkUp { get => m_impMarkUp; set => m_impMarkUp = value; }
        public string cveDivisaMarkUp { get => m_cveDivisaMarkUp; set => m_cveDivisaMarkUp = value; }
        public string impCompensacion { get => m_impCompensacion; set => m_impCompensacion = value; }
        public string cveDivisaCompensacion { get => m_cveDivisaCompensacion; set => m_cveDivisaCompensacion = value; }
        public string tipoCambioProcesador { get => m_tipoCambioProcesador; set => m_tipoCambioProcesador = value; }
        public string comercio { get => m_comercio; set => m_comercio = value; }
        public string comercioCiudad { get => m_comercioCiudad; set => m_comercioCiudad = value; }
        public string comercioPais { get => m_comercioPais; set => m_comercioPais = value; }
        public string comercioMCC { get => m_comercioMCC; set => m_comercioMCC = value; }
        public string comercioCP { get => m_comercioCP; set => m_comercioCP = value; }
        public string comercioEstado { get => m_comercioEstado; set => m_comercioEstado = value; }
        public string transaccionDestino { get => m_transaccionDestino; set => m_transaccionDestino = value; }
        public string transaccionOrigen { get => m_transaccionOrigen; set => m_transaccionOrigen = value; }
        public string cveEstatusFicheroDetalle { get => m_cveEstatusFicheroDetalle; set => m_cveEstatusFicheroDetalle = value; }
        public string cveTipoAlias { get => m_cveTipoAlias; set => m_cveTipoAlias = value; }
        public string cveTipoRed { get => m_cveTipoRed; set => m_cveTipoRed = value; }

        public string tipoCambio { get => m_tipoCambio; set => m_tipoCambio = value; }
        public string codigoPostal { get => m_codigoPostal; set => m_codigoPostal = value; }
        public string estado { get => m_estado; set => m_estado = value; }
        public string importeOriginal { get => m_importeOriginal; set => m_importeOriginal = value; }
        public string importeComisiones { get => m_importeComisiones; set => m_importeComisiones = value; }
        public string exponentesMoneda { get => m_exponentesMoneda; set => m_exponentesMoneda = value; }
        public string referenciaEmisor { get => m_referenciaEmisor; set => m_referenciaEmisor = value; }
        public string identificadorMensaje { get => m_identificadorMensaje; set => m_identificadorMensaje = value; }
        public string codigoAdquirente { get => m_codigoAdquirente; set => m_codigoAdquirente = value; }
        public string codigoDocumento { get => m_codigoDocumento; set => m_codigoDocumento = value; }
        public string codigoRazonMensaje { get => m_codigoRazonMensaje; set => m_codigoRazonMensaje = value; }
        public string actividadNegocio { get => m_actividadNegocio; set => m_actividadNegocio = value; }
        public string modoEntradaPos { get => m_modoEntradaPos; set => m_modoEntradaPos = value; }
        public string tipoTerminal { get => m_tipoTerminal; set => m_tipoTerminal = value; }
        public string numeroControl { get => m_numeroControl; set => m_numeroControl = value; }
        public string indicadorLiquidacion { get => m_indicadorLiquidacion; set => m_indicadorLiquidacion = value; }
        public string indicadorDocumentacion { get => m_indicadorDocumentacion; set => m_indicadorDocumentacion = value; }
        public string identificadorTransaccion { get => m_identificadorTransaccion; set => m_identificadorTransaccion = value; }
    }
}
