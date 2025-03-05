using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CommonProcesador;
using DNU_CompensadorParabiliumCommon.Entidades;
using DNU_CompensadorParabiliumProcesador.LogicaNegocio;
using DNU_VencimientosParabilia.LogicaNegocio;
using Interfases.Entidades;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProcesador
{
    [TestClass]
    public class CuentasCanceladas
    {
        
        [TestMethod]
        public void Caso1()
        {
            int laRespuesta = -1;
            try
            {


                ConfiguracionContexto.InicializarContexto();

                ConfiguracionContexto.ConfigApps["PROCESAT112"]["BDReadAutorizador".ToUpper()] = new CommonProcesador.Entidades.Propiedad
                {
                    ClaveProceso = "PROCESAT112",
                    ID_Config = 421,
                    Nombre = "BDReadAutorizador",
                    Valor = "data source=172.18.1.178;Initial Catalog=AUTO_CACAO;User Id=sa;Password=cacao6996*"

                };

                ConfiguracionContexto.ConfigApps["PROCESAT112"]["BDWriteAutorizador".ToUpper()] = new CommonProcesador.Entidades.Propiedad
                {
                    ClaveProceso = "PROCESAT112",
                    ID_Config = 421,
                    Nombre = "BDWriteAutorizador",
                    Valor = "data source=172.18.1.178;Initial Catalog=AUTO_CACAO;User Id=sa;Password=cacao6996*"

                };

                Movimiento elMovimiento = new Movimiento();

                string elEventoToEjecutar = "PB78      ";
                string LaCadenaComercial = "53392204";
                string TipoOperacionCheckOut_O_Normal = "";
                string elMOvimientoJSON = @"{
                    'IDFichero': 20363,
	                'NombreArchivo': 'T112_V2_MI112202010167',
	                'Ruta': '\\\\172.18.1.196\\FTP\\LocalUser\\ftp_cacao_in\\T112_files\\EN_PROCESO_T112_V2_MI112202010167.txt',
	                'IDFicheroDetalle': 14707605,
	                'IdEvento': 0,
	                'ClaveEvento': '00',
	                'ClaveMA': '5339220472846000',
	                'TipoMA': 'TAR',
	                'MonedaOriginal': '484',
	                'IdColectiva': 0,
	                'IdTipoColectiva': 0,
	                'ClaveColectiva': null,
	                'Importe': '298',
	                'ImporteMonedaOriginal': '298',
	                'Concepto': null,
	                'Observaciones': 'PROCESAMIENTO AUTOMATICO T112',
	                'Autorizacion': '963312',
	                'ReferenciaNumerica': '14707605',
	                'FechaOperacion': '20201015',
	                'Ticket': '',
	                'T112_ImporteCompensadoPesos': '298',
	                'T112_ImporteCompensadoDolar': '0',
	                'T112_ImporteCompensadoLocal': '298',
	                'T112_CodigoMonedaLocal': '484',
	                'T112_CuotaIntercambio': '2.24',
	                'T112_IVA': '0.36',
	                'T112_NombreArchivo': 'T112_V2_MI112202010167',
	                'T112_FechaPresentacion': '20201016',
	                'T112_CodigoTx': '00',
	                'T112_Comercio': 'INM RECAUDACIONMU        ',
	                'T112_Ciudad': 'CIUDAD DE MEX',
	                'T112_Pais': 'MEX',
	                'T112_MCC': '9311',
	                'T112_Moneda1': '484',
	                'T112_Moneda2': '484',
	                'T112_Referencia': '75412910290941886723509',
	                'T112_FechaProc': '2020 - 11 - 20 00:13:29',
	                'T112_FechaJuliana': '0290',
	                'T112_FechaConsumo': '20201015',
	                'T112_Ciclo': '7'
                }";

                elMovimiento = Newtonsoft.Json.JsonConvert.DeserializeObject<Movimiento>(elMOvimientoJSON);


                using (SqlConnection conn = new SqlConnection(PNConfig.Get("PROCESAT112", "BDWriteAutorizador")))
                {
                    conn.Open();
                    using (SqlConnection connFichero = new SqlConnection(PNConfig.Get("PROCESAT112", "BDWriteArchivos")))
                    {
                        connFichero.Open();

                        using (SqlTransaction transaccionSQL = conn.BeginTransaction("T112_PROCESS"))
                        {
                            Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                            LNArchivoProcesador.obtieneParametros(ref losParametros, elMovimiento, TipoOperacionCheckOut_O_Normal, conn, transaccionSQL);
                            losParametros.Add("@ID_EstatusOperacion", new Parametro {
                                Nombre = "@ID_EstatusOperacion",
                                Valor = "1"
                            });

                            losParametros.Add("@ID_EstatusPostOperacion", new Parametro
                            {
                                Nombre = "@ID_EstatusPostOperacion",
                                Valor = "0"
                            });


                            losParametros.Add("@ID_Operacion", new Parametro
                            {
                                Nombre = "@ID_Operacion",
                                Valor = "5127550"
                            });

                            losParametros.Add("@ID_EstatusCompensacionNew", new Parametro
                            {
                                Nombre = "@ID_EstatusCompensacionNew",
                                Valor = "2"
                            });

                            laRespuesta =
                                    LNProcesaMovimiento.ProcesarMovimiento(
                                        elMovimiento,
                                        elEventoToEjecutar,
                                        LaCadenaComercial,
                                        elMovimiento.ClaveEvento,
                                        TipoOperacionCheckOut_O_Normal,
                                        losParametros,
                                        conn,
                                        transaccionSQL,
                                        connFichero);


                            transaccionSQL.Rollback();

                            Assert.AreEqual(0, laRespuesta);
                        }
                    }

                }
            }
            catch(Exception ex)
            {
                Assert.Fail();
            }
        }


        //Pertenencia 00 , EstOp = 1 , EstPostOp = 0
        //Script validaEstatusCta = true
        //cta CAN
        //update ScriptContable set ValidaEstatusCuenta = 1 where id_Script = 1678
        [TestMethod]
        public void Caso2()
        {
            int laRespuesta = -1;
            try
            {
                ConfiguracionContexto.InicializarContexto();

                Movimiento elMovimiento = new Movimiento();

                string elEventoToEjecutar = "PB87      ";
                string LaCadenaComercial = "53392298";
                string TipoOperacionCheckOut_O_Normal = "";
                string elMOvimientoJSON = @"{
	            'IDFichero': 20363,
	            'NombreArchivo': 'T112_V2_MI112202010167',
	            'Ruta': '\\\\172.18.1.196\\FTP\\LocalUser\\ftp_cacao_in\\T112_files\\EN_PROCESO_T112_V2_MI112202010167.txt',
	            'IDFicheroDetalle': 14707602,
	            'IdEvento': 0,
	            'ClaveEvento': '00',
	            'ClaveMA': '5339229875961001',
	            'TipoMA': 'TAR',
	            'MonedaOriginal': '484',
	            'IdColectiva': 0,
	            'IdTipoColectiva': 0,
	            'ClaveColectiva': null,
	            'Importe': '700',
	            'ImporteMonedaOriginal': '700',
	            'Concepto': null,
	            'Observaciones': 'PROCESAMIENTO AUTOMATICO T112',
	            'Autorizacion': '969229',
	            'ReferenciaNumerica': '14707602',
	            'FechaOperacion': '20201015',
	            'Ticket': '',
	            'T112_ImporteCompensadoPesos': '700',
	            'T112_ImporteCompensadoDolar': '0',
	            'T112_ImporteCompensadoLocal': '700',
	            'T112_CodigoMonedaLocal': '484',
	            'T112_CuotaIntercambio': '7.7',
	            'T112_IVA': '1.23',
	            'T112_NombreArchivo': 'T112_V2_MI112202010167',
	            'T112_FechaPresentacion': '20201016',
	            'T112_CodigoTx': '00',
	            'T112_Comercio': 'RECREATIVOS ONLINE2MU    ',
	            'T112_Ciudad': 'CIUDAD DE MEX',
	            'T112_Pais': 'MEX',
	            'T112_MCC': '7999',
	            'T112_Moneda1': '484',
	            'T112_Moneda2': '484',
	            'T112_Referencia': '75412910290941706082763',
	            'T112_FechaProc': '2020-11-20 14:06:50',
	            'T112_FechaJuliana': '0290',
	            'T112_FechaConsumo': '20201015',
	            'T112_Ciclo': '7'
            }";

                elMovimiento = Newtonsoft.Json.JsonConvert.DeserializeObject<Movimiento>(elMOvimientoJSON);


                using (SqlConnection conn = new SqlConnection(PNConfig.Get("PROCESAT112", "BDWriteAutorizador")))
                {
                    conn.Open();
                    using (SqlConnection connFichero = new SqlConnection(PNConfig.Get("PROCESAT112", "BDWriteArchivos")))
                    {
                        connFichero.Open();

                        using (SqlTransaction transaccionSQL = conn.BeginTransaction("T112_PROCESS"))
                        {
                            Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                            LNArchivoProcesador.obtieneParametros(ref losParametros, elMovimiento, TipoOperacionCheckOut_O_Normal, conn, transaccionSQL);
                            losParametros.Add("@ID_EstatusOperacion", new Parametro
                            {
                                Nombre = "@ID_EstatusOperacion",
                                Valor = "1"
                            });

                            losParametros.Add("@ID_EstatusPostOperacion", new Parametro
                            {
                                Nombre = "@ID_EstatusPostOperacion",
                                Valor = "0"
                            });


                            losParametros.Add("@ID_Operacion", new Parametro
                            {
                                Nombre = "@ID_Operacion",
                                Valor = "5136815"
                            });

                            losParametros.Add("@ID_EstatusCompensacionNew", new Parametro
                            {
                                Nombre = "@ID_EstatusCompensacionNew",
                                Valor = "2"
                            });

                            laRespuesta =
                                    LNProcesaMovimiento.ProcesarMovimiento(
                                        elMovimiento,
                                        elEventoToEjecutar,
                                        LaCadenaComercial,
                                        elMovimiento.ClaveEvento,
                                        TipoOperacionCheckOut_O_Normal,
                                        losParametros,
                                        conn,
                                        transaccionSQL,
                                        connFichero);


                            transaccionSQL.Rollback();

                            Assert.AreNotEqual(0, laRespuesta);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }


        //Pertenencia 00 , EstOp = 1 , EstPostOp = 0
        //Script validaEstatusCta = FALSE
        //Componente no afectado
        //cta 15 CAN
        //previamente se tiene que actualizar en Script COntable 
        //update ScriptContable set ValidaEstatusCuenta = 0 where id_Script = 1678
        [TestMethod]
        public void Caso3()
        {
            int laRespuesta = -1;
            try
            {
                ConfiguracionContexto.InicializarContexto();

                Movimiento elMovimiento = new Movimiento();

                string elEventoToEjecutar = "PB87      ";
                string LaCadenaComercial = "53392298";
                string TipoOperacionCheckOut_O_Normal = "";
                string elMOvimientoJSON = @"{
	            'IDFichero': 20363,
	            'NombreArchivo': 'T112_V2_MI112202010167',
	            'Ruta': '\\\\172.18.1.196\\FTP\\LocalUser\\ftp_cacao_in\\T112_files\\EN_PROCESO_T112_V2_MI112202010167.txt',
	            'IDFicheroDetalle': 14707602,
	            'IdEvento': 0,
	            'ClaveEvento': '00',
	            'ClaveMA': '5339229875961001',
	            'TipoMA': 'TAR',
	            'MonedaOriginal': '484',
	            'IdColectiva': 0,
	            'IdTipoColectiva': 0,
	            'ClaveColectiva': null,
	            'Importe': '700',
	            'ImporteMonedaOriginal': '700',
	            'Concepto': null,
	            'Observaciones': 'PROCESAMIENTO AUTOMATICO T112',
	            'Autorizacion': '969229',
	            'ReferenciaNumerica': '14707602',
	            'FechaOperacion': '20201015',
	            'Ticket': '',
	            'T112_ImporteCompensadoPesos': '700',
	            'T112_ImporteCompensadoDolar': '0',
	            'T112_ImporteCompensadoLocal': '700',
	            'T112_CodigoMonedaLocal': '484',
	            'T112_CuotaIntercambio': '7.7',
	            'T112_IVA': '1.23',
	            'T112_NombreArchivo': 'T112_V2_MI112202010167',
	            'T112_FechaPresentacion': '20201016',
	            'T112_CodigoTx': '00',
	            'T112_Comercio': 'RECREATIVOS ONLINE2MU    ',
	            'T112_Ciudad': 'CIUDAD DE MEX',
	            'T112_Pais': 'MEX',
	            'T112_MCC': '7999',
	            'T112_Moneda1': '484',
	            'T112_Moneda2': '484',
	            'T112_Referencia': '75412910290941706082763',
	            'T112_FechaProc': '2020-11-20 14:06:50',
	            'T112_FechaJuliana': '0290',
	            'T112_FechaConsumo': '20201015',
	            'T112_Ciclo': '7'
            }";

                elMovimiento = Newtonsoft.Json.JsonConvert.DeserializeObject<Movimiento>(elMOvimientoJSON);


                using (SqlConnection conn = new SqlConnection(PNConfig.Get("PROCESAT112", "BDWriteAutorizador")))
                {
                    conn.Open();
                    using (SqlConnection connFichero = new SqlConnection(PNConfig.Get("PROCESAT112", "BDWriteArchivos")))
                    {
                        connFichero.Open();

                        using (SqlTransaction transaccionSQL = conn.BeginTransaction("T112_PROCESS"))
                        {
                            Dictionary<String, Parametro> losParametros = new Dictionary<string, Parametro>();

                            LNArchivoProcesador.obtieneParametros(ref losParametros, elMovimiento, TipoOperacionCheckOut_O_Normal, conn, transaccionSQL);
                            losParametros.Add("@ID_EstatusOperacion", new Parametro
                            {
                                Nombre = "@ID_EstatusOperacion",
                                Valor = "1"
                            });

                            losParametros.Add("@ID_EstatusPostOperacion", new Parametro
                            {
                                Nombre = "@ID_EstatusPostOperacion",
                                Valor = "0"
                            });


                            losParametros.Add("@ID_Operacion", new Parametro
                            {
                                Nombre = "@ID_Operacion",
                                Valor = "5136815"
                            });

                            losParametros.Add("@ID_EstatusCompensacionNew", new Parametro
                            {
                                Nombre = "@ID_EstatusCompensacionNew",
                                Valor = "2"
                            });

                            laRespuesta =
                                    LNProcesaMovimiento.ProcesarMovimiento(
                                        elMovimiento,
                                        elEventoToEjecutar,
                                        LaCadenaComercial,
                                        elMovimiento.ClaveEvento,
                                        TipoOperacionCheckOut_O_Normal,
                                        losParametros,
                                        conn,
                                        transaccionSQL,
                                        connFichero);


                            transaccionSQL.Rollback();

                            Assert.AreEqual(0, laRespuesta);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }


        //
        [TestMethod]
        public void Caso4()
        {
            try
            {
                ConfiguracionContexto.InicializarContexto();

                ConfiguracionContexto.ConfigApps["PROCESAVENCPARABILIA"]["BDReadAutorizador".ToUpper()] = new CommonProcesador.Entidades.Propiedad
                {
                    ClaveProceso = "PROCESAVENCPARABILIA",
                    ID_Config = 421,
                    Nombre = "BDReadAutorizador",
                    Valor = "data source=172.18.1.178;Initial Catalog=AUTO_CACAO_COMP;User Id=sa;Password=cacao6996*"

                };

                ConfiguracionContexto.ConfigApps["PROCESAVENCPARABILIA"]["BDWriteAutorizador".ToUpper()] = new CommonProcesador.Entidades.Propiedad
                {
                    ClaveProceso = "PROCESAVENCPARABILIA",
                    ID_Config = 421,
                    Nombre = "BDWriteAutorizador",
                    Valor = "data source=172.18.1.178;Initial Catalog=AUTO_CACAO_COMP;User Id=sa;Password=cacao6996*"

                };

                LNProcesaVencimeintos.ejecutaAplicacionDevoluciones();
            } 
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }
    }
}
