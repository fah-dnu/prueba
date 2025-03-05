using CommonProcesador;
using DNU_CompensadorParabiliumCommon.BaseDatos;
using DNU_CompensadorParabiliumCommon.Constants;
using DNU_CompensadorParabiliumCommon.Utilidades;
using DNU_CompensadorParabiliumMigration.LogicaNegocio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTestProcesador
{
    [TestClass]
    public class MigrationTest
    {

        [TestMethod]
        public void ValidaLogs()
        {

            Log.Evento("EVENTO");
            Log.EventoDebug("EVENTO DEBUG");
            Log.EventoInfo("EVENTO INFO");
            Log.Error("ERROR");

            Assert.IsTrue(true);

        }


        //Caso con transaccion activa
        //
        [TestMethod]
        public void ValidacionZerosTran()
        {
            ConfiguracionContexto.InicializarContexto();

            int timeoutMigracion = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_MIGRACION.ToString(), "TimeOutEjecutaMigracion") ?? "240");
            int timeoutMigracionAutoCacao = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_MIGRACION.ToString(), "TimeOutEjecutaMigracionAutoCacao") ?? "240");
            int daysBack = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_MIGRACION.ToString(), "daysBackToCheck") ?? "30");

            daysBack = 45;

            using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
            {

                conn.Open();
                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlConnection connAuto = new SqlConnection(DBProcesadorArchivo.strBDEscrituraAutorizador))
                        {

                            connAuto.Open();

                            using (SqlTransaction tranAuto =  connAuto.BeginTransaction())
                            {

                                Log.Evento("[MIGRACION - TEST] INICIA ValidaOperaciones ");
                                if (!ManageMigration.validaOperaciones(daysBack, timeoutMigracionAutoCacao, connAuto, tranAuto))
                                {
                                    Log.Evento("[MIGRACION - TEST] NO se completo  ValidaOperaciones ");
                                    throw new Exception();
                                }


                                Log.Evento("[MIGRACION - TEST] ROLLBACK ");
                                tranAuto.Rollback();

                            }


                        }

                        transaccionSQL.Rollback();


                        Assert.IsTrue(true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(new Exception("[MIGRACION] Error en la ejecución del proceso de migracion ", ex));
                        transaccionSQL.Rollback();

                        Assert.Fail();
                    }


                    
                }
            }
        }


        //Caso con transaccion null
        /* 
            UPDATE Operaciones set ID_EstatusPostOperacion = NULL where ID_Operacion IN (
            5274967,
            5274988,
            5275006,
            5275048,
            5275289,
            5275298,
            5275336,
            5275346,
            5275498,
            5275653,
            5275671,
            5275693,
            5275719,
            5275721)

         * 
         * 
         * */
        [TestMethod]
        public void ValidacionZerosNoTran()
        {
            ConfiguracionContexto.InicializarContexto();

            int timeoutMigracion = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_MIGRACION.ToString(), "TimeOutEjecutaMigracion") ?? "240");
            int timeoutMigracionAutoCacao = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_MIGRACION.ToString(), "TimeOutEjecutaMigracionAutoCacao") ?? "240");
            int daysBack = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_MIGRACION.ToString(), "daysBackToCheck") ?? "30");

            daysBack = 45;

            using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
            {

                conn.Open();
                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlConnection connAuto = new SqlConnection(DBProcesadorArchivo.strBDEscrituraAutorizador))
                        {

                            connAuto.Open();

                            Log.Evento("[MIGRACION - TEST] INICIA ValidaOperaciones ");
                            if (!ManageMigration.validaOperaciones(daysBack, timeoutMigracionAutoCacao, connAuto))
                            {
                                Log.Evento("[MIGRACION - TEST] NO se completo  ValidaOperaciones ");
                                throw new Exception();
                            }

                        }

                        transaccionSQL.Rollback();


                        Assert.IsTrue(true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(new Exception("[MIGRACION] Error en la ejecución del proceso de migracion ", ex));
                        transaccionSQL.Rollback();

                        Assert.Fail();
                    }



                }
            }
        }


        [TestMethod]
        public void ValidaOutdated()
        {
            ConfiguracionContexto.InicializarContexto();

            int timeoutMigracion = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_MIGRACION.ToString(), "TimeOutEjecutaMigracion") ?? "240");
            int timeoutMigracionAutoCacao = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_MIGRACION.ToString(), "TimeOutEjecutaMigracionAutoCacao") ?? "240");
            int daysBack = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_MIGRACION.ToString(), "daysBackToCheck") ?? "30");

            using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
            {

                conn.Open();
                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {


                        Log.Evento("[MIGRACION] INICIA MigraFicheroMaestro_FicheroMaestroComp ");
                        if (!DAOMigracion.MigraFicheroMaestro_FicheroMaestroComp(conn, transaccionSQL, timeoutMigracion))
                        {
                            Log.Evento("[MIGRACION] NO se completo  MigraFicheroMaestro_FicheroMaestroComp ");
                            throw new Exception();
                        }

                        using (SqlConnection connAuto = new SqlConnection(DBProcesadorArchivo.strBDEscrituraAutorizador))
                        {

                            connAuto.Open();

                            using (SqlTransaction tranAuto = connAuto.BeginTransaction())
                            {

                                Log.Evento("[MIGRACION - TEST] INICIA ValidaOperaciones ");
                                if (!ManageMigration.validaOperaciones(daysBack, timeoutMigracionAutoCacao, connAuto))
                                {
                                    Log.Evento("[MIGRACION - TEST] NO se completo  ValidaOperaciones ");
                                    throw new Exception();
                                }


                                Log.Evento("[MIGRACION - TEST] ROLLBACK ");
                                tranAuto.Rollback();

                            }

                        }


                        Log.Evento("[MIGRACION] INICIA ValidaOutdated ");
                        if (!DAOMigracion.ValidaOutdated(conn, transaccionSQL, timeoutMigracion))
                        {
                            Log.Evento("[MIGRACION] NO se completo  ValidaOutdated ");
                            throw new Exception();
                        }


                        transaccionSQL.Rollback();

                        

                        Assert.IsTrue(true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(new Exception("[MIGRACION] Error en la ejecución del proceso de migracion ", ex));
                        transaccionSQL.Rollback();

                        Assert.Fail();
                    }
                }
            }
        }



        [TestMethod]
        public void ValidaError()
        {
            ConfiguracionContexto.InicializarContexto();

            int timeoutMigracion = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_MIGRACION.ToString(), "TimeOutEjecutaMigracion") ?? "240");
            int timeoutMigracionAutoCacao = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_MIGRACION.ToString(), "TimeOutEjecutaMigracionAutoCacao") ?? "240");
            int daysBack = Convert.ToInt32(PNConfig.Get(Processes.PROCESA_MIGRACION.ToString(), "daysBackToCheck") ?? "30");

            using (SqlConnection conn = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
            {

                conn.Open();
                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {



                        Log.Evento("[MIGRACION] INICIA ValidaError ");
                        var rows = DAOMigracion.ValidaError(conn, transaccionSQL, timeoutMigracion);


                        transaccionSQL.Rollback();



                        Assert.IsTrue(rows == 200);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(new Exception("[MIGRACION] Error en la ejecución del proceso de migracion ", ex));
                        transaccionSQL.Rollback();

                        Assert.Fail();
                    }
                }
            }
        }
    }
}
