using CommonProcesador;
using DNU_CompensacionT112_API_Cacao.BaseDatos;
using DNU_CompensacionT112_API_Cacao.Entidades;
using DNU_CompensacionT112Evertec.BaseDatos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DNU_CompensacionT112_API_Cacao.LogicaNegocio
{
    class LNEjecutaCompensacion
    {
        static readonly int FECHAPRESENTACION_LEN = 8;
        static readonly string CVE_ESTATUS_ERROR = "0003";
        static readonly string CVE_ESTATUS_OK = "0001";

        /// <summary>
        /// Realiza el ciclo de compensación de operaciones 
        /// </summary>
        /// <returns>TRUE en caso de éxito</returns>
        public static void EjecutaCicloCompensacion(String fechaInicial, String fechaFinal, BDT112Connections T112SqlConnection, BDOperacionesConnections dbOpConnections, int proceso)
        {
            Int64 IdFicheroActual = 0;
            Boolean estatusFichero = true;
            Boolean procesaSiguienteFecha = true;
            try
            {
                Logueo.Evento("INICIA PROCESO - COMPT112APICACAO");
                Int64 idProceso = 0;
                int totalRegistros = 0;
                string msjCompensacion = String.Empty;
              
                List<RegistroACompensar> hs_Registros = new List<RegistroACompensar>();
                bool regsProcOk = false;




                //String strfechaTopeProcesamiento = ObtenerFechaTopeProcesamiento();
                String strfechaTopeProcesamiento = fechaFinal;

                do
                {

                    String strFechaProcesamiento = DAOT112.ObtenerFechaProcesamientoMedianteTopeConfigurado(fechaInicial,strfechaTopeProcesamiento, T112SqlConnection.GetT112LecturaConnection(proceso));

                    if (String.IsNullOrEmpty(strFechaProcesamiento))
                    {
                        procesaSiguienteFecha = false;
                        continue;
                    }

                    //Validamos la existencia de los archivos T112 y logeamos , pero no detenemos el proceso
                    LNValidacionArchivos.ValidaArchivosT112(strFechaProcesamiento, T112SqlConnection.GetT112LecturaConnection(proceso));
                    if(!LNValidacionArchivos.ValidaArchivosOperaciones(strFechaProcesamiento, dbOpConnections.GetOperacionesLecturaConnection(proceso)))
                    {
                        ActualizaFicheroAProcesadoPorFecha(strFechaProcesamiento, CVE_ESTATUS_ERROR, T112SqlConnection.GetT112EscrituraConnection(proceso));
                        LNValidacionArchivos.ValidaArchivosT112(strFechaProcesamiento, T112SqlConnection.GetT112EscrituraConnection(proceso));
                        continue;
                    }



                    //if (!DAOT112.InsertaEstatusFicheros(strFechaProcesamiento, T112SqlConnection.GetT112EscrituraConnection(proceso)))
                    //{
                    //    Logueo.Error("Ocurrió un error al insertar el estatus de los ficheros");
                    //    //return false;
                    //}

                    try
                    {
                        //Se consultan los registros por compensar
                        hs_Registros = DAOT112.ListaRegistrosPorCompensar(strFechaProcesamiento, T112SqlConnection.GetT112LecturaConnection(proceso));
                        Logueo.Evento(String.Format("REGISTROS A COMPENSAR: {0} tiempo inicial {1} strFechaProcesamiento {2}",
                            hs_Registros.Count.ToString(),
                            DateTime.Now.ToString(),
                            strFechaProcesamiento));

                        totalRegistros = 0;

                        if(hs_Registros.Count == 0)
                        {
                            ActualizaFicheroAProcesadoPorFecha(strFechaProcesamiento, CVE_ESTATUS_ERROR, T112SqlConnection.GetT112EscrituraConnection(proceso));
                            continue;
                        }




                        for (int i = 0; i < hs_Registros.Count; i++)
                        {
                                    
                            Boolean ProcesaCompensaciones = false;
                            regsProcOk = true;
                            RegistroACompensar registro = hs_Registros[i];
                            msjCompensacion = string.Empty;
                            IdFicheroActual = registro.IdFichero;

                            try
                            {
                                registro.T112_FechaPresentacion = ObtenerFechaPresentacion(registro);
                                registro.T112_Ciclo = ObtenerCiclo(registro);
                                //SqlCommand cmd = new SqlCommand();
                                
                                CASOS casoOperacion = ObtenerCasoOperacion(registro, T112SqlConnection, dbOpConnections.GetOperacionesLecturaConnection(proceso), proceso);
                                switch (casoOperacion)
                                {
                                    case CASOS.RREP:
                                        //Actualizar registro en T112 como repetido!!
                                        break;
                                    case CASOS.NENC:
                                        //NADA
                                        break;
                                    case CASOS.DUP:
                                        //NADA
                                        break;
                                    case CASOS.COMP:
                                        //llenar campos en Opearaciones con datos correctos
                                        msjCompensacion = DAOOperacionesEvertec.CompensaOperacion(registro, dbOpConnections.GetOperacionesEscrituraConnection(proceso));
                                        ProcesaCompensaciones = true;
                                        break;
                                    case CASOS.REVPA:
                                        //Nada
                                        break;
                                    case CASOS.REVOK:
                                        //Nada
                                        break;
                                    case CASOS.REV:
                                    case CASOS.DEV:
                                        //LLenar en Operaciones campos correspondientes a la devoluciòn 
                                        msjCompensacion = DAOOperacionesEvertec.LlenaCamposDevolucion(registro, dbOpConnections.GetOperacionesEscrituraConnection(proceso));
                                        ProcesaCompensaciones = true;
                                        break;
                                    case CASOS.NOTX:
                                        //Nada
                                        break;
                                    case CASOS.PA:
                                        //Nada
                                        break;
                                    default:
                                        //NADA
                                        break;
                                }



                                totalRegistros++;

                                if (ProcesaCompensaciones)
                                {
                                    if (msjCompensacion.Equals("00"))
                                    {
                                        if (!DAOT112.ActualizaEstatusCompensacionRegistro(Convert.ToInt64(registro.IdFicheroDetalle), casoOperacion.ToString(), T112SqlConnection.GetT112EscrituraConnection(proceso)))
                                        {
                                            estatusFichero = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!DAOT112.ActualizaEstatusCompensacionRegistro(Convert.ToInt64(registro.IdFicheroDetalle), casoOperacion.ToString(), T112SqlConnection.GetT112EscrituraConnection(proceso)))
                                    {
                                        estatusFichero = false;
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                Logueo.Error(String.Format("ERROR EN TRANSACCION {0} : {1}", registro.IdFicheroDetalle, ex.Message));
                                estatusFichero = false;

                            }

                            estatusFichero = ValidaExistenciaFicheroDetallePorProcesar(IdFicheroActual, strFechaProcesamiento,
                                                    T112SqlConnection.GetT112LecturaConnection(proceso), estatusFichero);


                        }


                        Logueo.Evento(String.Format("REGISTROS COMPENSADOS: {0} tiempo final {1}", totalRegistros.ToString(), DateTime.Now.ToString()));
                        Logueo.Evento("FIN PROCESO - COMPT112APICACAO");
                    }

                    catch (Exception ex)
                    {
                        if (regsProcOk)
                        {
                            DAOT112.ActualizaFicheroProcesado(IdFicheroActual, CVE_ESTATUS_ERROR, T112SqlConnection.GetT112EscrituraConnection(proceso));
                            LNValidacionArchivos.ValidaArchivosT112(strFechaProcesamiento, T112SqlConnection.GetT112LecturaConnection(proceso));
                        }
                        T112SqlConnection.Close();
                        dbOpConnections.Close();
                        Logueo.Evento("ERROR , REGISTROS COMPENSADOS: " + totalRegistros.ToString());
                        Logueo.Error("ERROR EN CICLO DE COMPENSACION: " + ex.Message);

                    }


                    if (DateTime.Parse(strFechaProcesamiento) >= DateTime.Parse(strfechaTopeProcesamiento))
                    {
                        if (!HayTransaccionesRestantesPorFecha(strFechaProcesamiento, T112SqlConnection.GetT112EscrituraConnection(proceso)))
                        {
                            Logueo.Evento("Termina ciclo de procesamiento , ya no hay más transacciones por procesar");
                            procesaSiguienteFecha = false;
                            break;
                        }
                    }

                }
                while (procesaSiguienteFecha);
                T112SqlConnection.Close();
                dbOpConnections.Close();
            }


            catch (Exception err)
            {
                T112SqlConnection.Close();
                dbOpConnections.Close();
                Logueo.Error("EjecutaCicloCompensacion():" + err.Message);
            }
        }

        private static bool ValidaExistenciaFicheroDetallePorProcesar(long idFicheroActual, string strFechaProcesamiento, SqlConnection sqlConnection,  bool estatusFichero)
        {
            if (!HayArchivosFicheroRestantes(idFicheroActual, strFechaProcesamiento, sqlConnection))
            {
                try
                {
                    ActualizaFicheroProcesado(idFicheroActual, estatusFichero, sqlConnection);
                    LNValidacionArchivos.ValidaArchivosT112(strFechaProcesamiento, sqlConnection);
                }
                catch (Exception ex)
                {
                    Logueo.Error(String.Format("Error al actualizar el estatus del Fichero {0} - {1}", idFicheroActual, ex.Message));
                }
                return true;
            }

            return estatusFichero;
        }

        private static Boolean ActualizaFicheroAProcesadoPorFecha(string strFechaProcesamiento,string cveEstatus, SqlConnection T112SqlConnection)
        {
            return DAOT112.ActualizaFicheroAProcesadoPorFecha(strFechaProcesamiento, T112SqlConnection, cveEstatus);
        }

        private static bool HayArchivosFicheroRestantes(long IdFichero, string strFechaProcesamiento, SqlConnection T112SqlConnection)
        {
            return DAOT112.HayArchivosPorProcesar(IdFichero, strFechaProcesamiento, T112SqlConnection);
        }

        private static bool HayTransaccionesRestantesPorFecha(string strFechaProcesamiento, SqlConnection T112SqlConnection)
        {
            return DAOT112.HayTransaccionesRestantesPorFecha(strFechaProcesamiento, T112SqlConnection);
        }

        private static string ObtenerCiclo(RegistroACompensar registro)
        {
            if (registro.NombreFichero.Length > 71)
            {
                try
                {
                    var name = registro.NombreFichero.Split('.')[0];
                    return name.Substring(name.Length - 1, 1);
                }
                catch (Exception ex)
                {
                    Logueo.Error(String.Format("Ocurrio un error al intentar obtener el Ciclo - {0}",
                            ex.Message));
                    return string.Empty;
                }
            }
            else
                return registro.NombreFichero.Substring(registro.NombreFichero.LastIndexOf("_T112_") + 15, 1);
        }

        private static string ObtenerFechaPresentacion(RegistroACompensar registro)
        {
            if (registro.NombreFichero.Length > 71)
            {
                try
                {
                    var name = registro.NombreFichero.Split('.')[0];
                    return name.Substring(name.Length - (FECHAPRESENTACION_LEN + 1), FECHAPRESENTACION_LEN);
                }
                catch(Exception ex)
                {
                    Logueo.Error(String.Format("Ocurrio un error al intentar obtener la fecha de presentación - {0}",
                            ex.Message));
                    return string.Empty;
                }
            }
            else
            {
                var month = registro.NombreFichero.Substring(registro.NombreFichero.LastIndexOf("_T112_") + 11,2);
                var day = registro.NombreFichero.Substring(registro.NombreFichero.LastIndexOf("_T112_") + 13,2);
                return String.Format("{0}{1}{2}", DateTime.Now.Year,month,day);
            }
        }

        private static void ActualizaFicheroProcesado(long idFicheroActual, Boolean estatusCorrectoProcesamientoArchivo, SqlConnection T112SqlConnection)
        {
            String claveEsattusProcesamientoFichero = string.Empty;

            if (estatusCorrectoProcesamientoArchivo)
            {
                claveEsattusProcesamientoFichero = CVE_ESTATUS_OK;
            }
            else
            {
                claveEsattusProcesamientoFichero = CVE_ESTATUS_ERROR;
            }

            if (DAOT112.ActualizaFicheroProcesado(idFicheroActual, claveEsattusProcesamientoFichero, T112SqlConnection))
            {
                DAOT112.ActualizaProcesoT112ApiCacao(idFicheroActual, "APR", T112SqlConnection);
            }
            else
            {
                Logueo.Error("No fue posible actualizar el fichero T112 por algun error , revisar Log Error");
            }

            
        }

        private static CASOS ObtenerCasoOperacion(RegistroACompensar registro, BDT112Connections T112SqlConnection, SqlConnection dbOpConnections, int proceso)
        {

            if (String.IsNullOrEmpty(registro.T112_CodigoTx))
            {
                return CASOS.NA;
            }

            if (DAOT112.VerificaRegistroRepetidoT112(registro.IdFicheroDetalle, T112SqlConnection.GetT112LecturaConnection(proceso)))
            {
                return CASOS.RREP;
            }

            OperacionEvertec operacionEvertec = DAOOperacionesEvertec.VerificaRegistroExistenteEnOperaciones(registro, dbOpConnections);

            if (String.IsNullOrEmpty(operacionEvertec.EstatusAutorizacion))
            {
                return CASOS.NENC;
            }

            switch (registro.T112_CodigoTx)
            {

                case  "00" :
                case  "01" :
                case  "18" :
                case  "28" :
                case  "30" :

                    if (String.IsNullOrEmpty(registro.C040))
                    {
                        return ValidaTransaccionNoReverso(operacionEvertec);
                    }
                    else
                    {
                        if (registro.C040.ToUpper().StartsWith("R"))
                            return ValidaTransaccionReverso(operacionEvertec);
                        else
                        {
                            return ValidaTransaccionNoReverso(operacionEvertec);
                        }
                    }

                case "20" :
                    return ValidaTransaccion20Reverso(operacionEvertec);

                case "19":
                case "29":
                    return CASOS.NOTX;
                default:
                    return CASOS.PA;
            }



            throw new Exception("No se identificó el caso ");
        }

        private static CASOS ValidaTransaccion20Reverso(OperacionEvertec operacionEvertec)
        {
            if (!operacionEvertec.EstatusAutorizacion.Equals("A") &&
                !operacionEvertec.EstatusAutorizacion.Equals("R"))
            { 
               
                return CASOS.DEV;


            }
            else
            {
                return CASOS.REVPA;
            }
        

        }

        private static CASOS ValidaTransaccionReverso(OperacionEvertec operacionEvertec)
        {
            if(operacionEvertec.EstatusAutorizacion.Equals("A") ||
                operacionEvertec.EstatusAutorizacion.Equals("R"))
                return CASOS.REVOK;
            else
                return CASOS.REV;

        }

        private static CASOS ValidaTransaccionNoReverso(OperacionEvertec operacionEvertec)
        {
            if(operacionEvertec.EsCompensada)
                return CASOS.DUP;



            switch (operacionEvertec.EstatusAutorizacion)
            {
                case "C":
                case "T":
                    return CASOS.COMP;

                case "A":
                case "R":
                    return CASOS.REVPA;

            }


            throw new Exception("No se pudon validar el caso de la transaccion como no reverso");
        }

        private static string ObtenerFechaTopeProcesamiento()
        {
            String tempDate = DateTime.Now.ToString("yyyy-MM-dd");

            if (!String.IsNullOrEmpty(BDT112.strFechaProcesamientoLimite))
            {
                tempDate = BDT112.strFechaProcesamientoLimite;
            }

            return tempDate; 
        }
    }



    enum CASOS
    {
        RREP = 3,
        NENC = 2,
        DUP = 4,
        COMP = 1,
        REVPA = 5,
        REVOK = 7,
        DEV = 8,
        NOTX = 10,
        PA = 11,
        REV = 12,
        NA = 13
        
    }
}
