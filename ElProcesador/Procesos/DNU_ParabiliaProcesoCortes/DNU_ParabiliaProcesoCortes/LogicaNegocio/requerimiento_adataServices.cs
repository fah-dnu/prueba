using CommonProcesador;
using DNU_ParabiliaProcesoCortes.dataService;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.Utilidades;
using Executer.Entidades;
using Interfases.Entidades;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.CapaNegocio
{
    public class requerimiento_adataServices
    {

        static List<Cuentas> Obtiene_Set_deCuentas(String cadenaConexion)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            List<Cuentas> laRespuesta = new List<Cuentas>();

            //instancia de clase que pertenece a dataService, para conectarse y obtener datos de B.D.
            acceso_aDatos accessData = new acceso_aDatos();

            DataTable laTabla = null;
            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP procnoc_ejecutor_ObtieneCuentasPorProcesar]");
            DataSet DataSet_ = accessData.GetSqlDataSet(cadenaConexion, "procnoc_ejecutor_ObtieneCuentasPorProcesar",

                (SqlDatabase database, System.Data.Common.DbCommand command, object value, object value2,
                object value3, object value4, object value5, object value6, object val7, object val8, object val9, object val10, object val11,
                object val12, object val13) =>
                {
                    int var_;
                    var_ = (int)value;
                },

           1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13);



            laTabla = DataSet_.Tables[0];



            for (int k = 0; k < laTabla.Rows.Count; k++)
            {

                Cuentas cuenta = new Cuentas();

                cuenta.ID_Cuenta = Int32.Parse((laTabla.Rows[k]["id_cuenta"]).ToString());
                cuenta.id_CadenaComercial = Int32.Parse((laTabla.Rows[k]["id_cadenaComercial"]).ToString());
                cuenta.Fecha_Corte = DateTime.Parse((laTabla.Rows[k]["FechaProximoCorte"]).ToString());
                cuenta.ClaveCorteTipo = (laTabla.Rows[k]["ClaveCorteTipo"]).ToString();
                laRespuesta.Add(cuenta);

            }

            return laRespuesta;
        }



        static List<Evento> ObtieneSet_deEventosAgrupadores(String cadenaConexion, int idcuenta, string ClaveCorteTipo)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            List<Evento> laRespuesta = new List<Evento>();

            //instancia de clase que pertenece a dataService, para conectarse y obtener datos de B.D.
            acceso_aDatos accessData = new acceso_aDatos();

            DataTable laTabla = null;

            // pasa un metodo anonimo a un  parametro tipo delegado
            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP procnoc_ejecutor_ObtieneEjecucionesEventosAgrupadores]");
            DataSet DataSet_ = accessData.GetSqlDataSet(cadenaConexion, "procnoc_ejecutor_ObtieneEjecucionesEventosAgrupadores",
                (SqlDatabase database, System.Data.Common.DbCommand command, object IDCUENTA, object value2,
                     object value3, object value4, object value5, object value6, object val7, object val8, object val9, object val10, object val11,
                     object val12, object val13) =>
                {
                    database.AddInParameter(command, "@ID_Cuenta", DbType.Int32, IDCUENTA);
                    database.AddInParameter(command, "@ClaveCorteTipo", DbType.String, ClaveCorteTipo);
                },
                idcuenta,
                ClaveCorteTipo,
               2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);

            laTabla = DataSet_.Tables[0];
            for (int k = 0; k < laTabla.Rows.Count; k++)
            {
                Evento unEvento = new Evento();

                //  unEvento.ID_ConfiguracionCorte = long.Parse((laTabla.Rows[k]["ID_ConfiguracionCorte"]).ToString());
                // unEvento.ID_AgrupadorEvento = long.Parse((laTabla.Rows[k]["ID_AgrupadorEvento"]).ToString());
                unEvento.ID_AgrupadorEvento = Int32.Parse((laTabla.Rows[k]["ID_AgrupadorEvento"]).ToString());

                // unEvento.ID_Evento = Int32.Parse((laTabla.Rows[k]["ID_Evento"]).ToString());
                unEvento.ClaveEventoAgrupador = (laTabla.Rows[k]["ClaveEventoAgrupador"]).ToString();
                // unEvento.Descripcion = (laTabla.Rows[k]["Descripcion"]).ToString();

                //estos si
                unEvento.ID_CadenaComercial = Int64.Parse((laTabla.Rows[k]["ID_CadenaComercial"]).ToString());
                // unEvento.ID_Contrato = Int64.Parse((laTabla.Rows[k]["ID_Contrato"]).ToString());
                unEvento.ClaveCadenaComercial = (laTabla.Rows[k]["ClaveColectiva"]).ToString();
                unEvento.Consecutivo = Int64.Parse((laTabla.Rows[k]["Consecutivo"]).ToString());
                unEvento.Stored_Procedure = (laTabla.Rows[k]["Stored_Procedure"]).ToString();
                unEvento.Descripcion = (laTabla.Rows[k]["Descripcion"]).ToString();



                laRespuesta.Add(unEvento);

            }
            return laRespuesta;

        }









        public static Dictionary<String, Parametro> ObtieneParametros_Cuenta(String cadenaConexion, int idcuenta)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
            DataTable laTabla = null;


            //instancia de clase que pertenece a dataService, para conectarse y obtener datos de B.D.
            acceso_aDatos accessData = new acceso_aDatos();


            //System.Data.Common.DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneCadenasPorProcesar");



            // pasa un metodo anonimo a un  parametro tipo delegado
            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_Ejecutor_ObtieneParametros_Cuenta]");
            DataSet DataSet_ = accessData.GetSqlDataSet(cadenaConexion, "ws_Ejecutor_ObtieneParametros_Cuenta",
                (SqlDatabase database, System.Data.Common.DbCommand command, object IDCUENTA, object value2,
                    object value3, object value4, object value5, object value6, object val7, object val8, object val9, object val10, object val11,
                    object val12, object val13) =>
                {
                    database.AddInParameter(command, "@ID_Cuenta", DbType.Int32, IDCUENTA);
                },
                idcuenta,
                1,
               2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);


            laTabla = DataSet_.Tables[0];
            for (int k = 0; k < laTabla.Rows.Count; k++)
            {
                Parametro unParamentro = new Parametro();

                unParamentro.Nombre = (laTabla.Rows[k]["Clave_ParametroMultiasignacion"]).ToString();
                unParamentro.Valor = (laTabla.Rows[k]["Valor"]).ToString();

                larespuesta.Add(unParamentro.Nombre, unParamentro);

            }

            return larespuesta;

        }

        //  ObtieneParametros_Cuenta_Corte_Anterior
        public static Dictionary<String, Parametro> ObtieneParametros_Cuenta_CorteAnterior(String cadenaConexion, int idcuenta)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
            DataTable laTabla = null;



            //instancia de clase que pertenece a dataService, para conectarse y obtener datos de B.D.
            acceso_aDatos accessData = new acceso_aDatos();


            //System.Data.Common.DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneCadenasPorProcesar");



            // pasa un metodo anonimo a un  parametro tipo delegado
            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_Ejecutor_ObtieneParametros_Cuenta_CorteAnterior]");
            DataSet DataSet_ = accessData.GetSqlDataSet(cadenaConexion, "ws_Ejecutor_ObtieneParametros_Cuenta_CorteAnterior",
                 //DELEGADO
                 (SqlDatabase database, System.Data.Common.DbCommand command, object IDCUENTA, object value2,
                     object value3, object value4, object value5, object value6, object val7, object val8, object val9, object val10, object val11,
                     object val12, object val13) =>
                 {
                     database.AddInParameter(command, "@ID_Cuenta", DbType.Int32, IDCUENTA);
                 },

                idcuenta,
                1,
                  2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);


            laTabla = DataSet_.Tables[0];
            for (int k = 0; k < laTabla.Rows.Count; k++)
            {
                Parametro unParamentro = new Parametro();

                unParamentro.Nombre = (laTabla.Rows[k]["NombreParametro"]).ToString();

                unParamentro.Valor = (laTabla.Rows[k]["Valor"]).ToString();

                larespuesta.Add(unParamentro.Nombre, unParamentro);

            }

            return larespuesta;

        }




        //public static Int64 InsertaCorte(String ClaveCorteTipo, DateTime Fecha_corte, int ID_cuenta, SqlConnection conn, SqlTransaction transaccionSQL)
        //{
        //    string Fecha_Corte_s = Convert.ToString(Fecha_corte).Substring(0, 10);

        //    DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
        //    string formattedDate = date.ToString("yyyy-MM-dd");

        //    SqlDataReader SqlReader = null;
        //    Int64 ID_Lote = 0;

        //    try
        //    {
        //        string cadenaConexion = "Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43; MultipleActiveResultSets=true;";
        //        using (SqlConnection conn2 = new SqlConnection(cadenaConexion))
        //        {

        //            conn2.Open();



        //            using (SqlTransaction transaccionSQL2 = conn2.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
        //            {


        //                SqlParameter param;
        //                SqlCommand comando2 = new SqlCommand("EJECUTOR_InsertaCorteCuenta", conn2);
        //                comando2.Transaction = transaccionSQL2;
        //                comando2.CommandType = CommandType.StoredProcedure;

        //                param = new SqlParameter("@ClaveCorteTipo", SqlDbType.VarChar);
        //                param.Value = ClaveCorteTipo;
        //                comando2.Parameters.Add(param);

        //                param = new SqlParameter("@Fecha_Corte", SqlDbType.DateTime);
        //                param.Value = formattedDate;
        //                comando2.Parameters.Add(param);


        //                param = new SqlParameter("@ID_Cuenta", SqlDbType.Int);
        //                param.Value = ID_cuenta;
        //                comando2.Parameters.Add(param);


        //                SqlReader = comando2.ExecuteReader();


        //                if (null != SqlReader)
        //                {

        //                    while (SqlReader.Read())
        //                    {


        //                         ID_Lote = ((Int64)SqlReader["ID_Corte"]);

        //                    }
        //                }

        //                transaccionSQL2.Commit();


        //            }

        //        }

        //    }

        //    catch (Exception err)
        //    {
        //        //   transaccionSQL.Rollback();
        //        // GeneracionExitosaDePolizas = false;
        //        Logueo.Error("GenerarLasPolizasDeCortes() Error se deshacen la polizas para la cadena comercial ");

        //    }
        //    return ID_Lote;

        //}


        //*********************



        //public static Int64 InsertaCorte(String ClaveCorteTipo, DateTime Fecha_corte, int ID_cuenta, SqlConnection conn, SqlTransaction transaccionSQL)
        //{
        //    string Fecha_Corte_s = Convert.ToString(Fecha_corte).Substring(0, 10);

        //    DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
        //    string formattedDate = date.ToString("yyyy-MM-dd");

        //    SqlDataReader SqlReader = null;
        //    Int64 ID_Lote = 0;

        //    try
        //    {


        //        SqlParameter param;
        //        SqlCommand comando2 = new SqlCommand("EJECUTOR_InsertaCorteCuenta", conn);
        //        comando2.Transaction = transaccionSQL;
        //        comando2.CommandType = CommandType.StoredProcedure;

        //        param = new SqlParameter("@ClaveCorteTipo", SqlDbType.VarChar);
        //        param.Value = ClaveCorteTipo;
        //        comando2.Parameters.Add(param);

        //        param = new SqlParameter("@Fecha_Corte", SqlDbType.DateTime);
        //        param.Value = formattedDate;
        //        comando2.Parameters.Add(param);


        //        param = new SqlParameter("@ID_Cuenta", SqlDbType.Int);
        //        param.Value = ID_cuenta;
        //        comando2.Parameters.Add(param);

        //        //  comando2.ExecuteNonQuery();
        //        SqlReader = comando2.ExecuteReader();


        //        if (null != SqlReader)
        //        {

        //            while (SqlReader.Read())
        //            {


        //                ID_Lote = ((Int64)SqlReader["ID_Corte"]);

        //            }
        //        }




        //    }

        //    catch (Exception err)
        //    {
        //        //   transaccionSQL.Rollback();
        //        // GeneracionExitosaDePolizas = false;
        //        Logueo.Error("GenerarLasPolizasDeCortes() Error se deshacen la polizas para la cadena comercial ");

        //    }
        //    return ID_Lote;

        //}



        //************
















        public static Dictionary<String, Parametro> ObtieneParametros_Contrato(String cadenaConexion, String ClaveCadenaComercial, String Tarjeta,
            String ClaveEvento, String elUsuario)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
            DataTable laTabla = null;





            //instancia de clase que pertenece a dataService, para conectarse y obtener datos de B.D.
            acceso_aDatos accessData = new acceso_aDatos();



            //System.Data.Common.DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneCadenasPorProcesar");





            // pasa un metodo anonimo a un  parametro tipo delegado
            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_Ejecutor_ObtieneValoresContratos]");
            DataSet DataSet_ = accessData.GetSqlDataSet(cadenaConexion, "ws_Ejecutor_ObtieneValoresContratos",
                //DELEGADO ES EL 2
                (SqlDatabase database, System.Data.Common.DbCommand command,

                    object claveCadena,
                    object claveEvento,
                    object tarjeta, object value4, object value5, object value6, object val7, object val8, object val9, object val10, object val11,
                    object val12, object val13) =>
                {
                    database.AddInParameter(command, "@ClaveCadenaComercial", DbType.String, claveCadena);
                    database.AddInParameter(command, "@ClaveEvento", DbType.String, claveEvento);
                    database.AddInParameter(command, "@Tarjeta", DbType.String, tarjeta);

                },

                ClaveCadenaComercial,
              ClaveEvento,
              Tarjeta, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);


            laTabla = DataSet_.Tables[0];
            for (int k = 0; k < laTabla.Rows.Count; k++)
            {
                Parametro unParamentro = new Parametro();

                unParamentro.Nombre = (laTabla.Rows[k]["Nombre"]).ToString();
                unParamentro.ID_TipoColectiva = Int16.Parse((laTabla.Rows[k]["ID_TipoColectiva"]).ToString());
                unParamentro.Valor = (laTabla.Rows[k]["valor"]).ToString();

                larespuesta.Add(unParamentro.Nombre, unParamentro);

            }

            return larespuesta;

        }







































        public static Dictionary<String, Parametro> ObtieneSaldoPromedioDiario_Cuenta(String cadenaConexion, int ID_Cuenta,
            int ID_CadenaComercial,
            DateTime Fecha_Corte,
            string ClaveEventoAgrupador,
            string Saldo_Inicial,
            string DiaFechaCorte)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            string Fecha_Corte_s = Convert.ToString(Fecha_Corte).Substring(0, 10);

            //string str = Fecha_Corte_s.Substring(0, 2) + Fecha_Corte_s.Substring(3, 2) +
            //    Fecha_Corte_s.Substring(6, 4);




            //  DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture);
            DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            string formattedDate = date.ToString("yyyy-MM-dd");



            Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
            DataTable laTabla = null;
            //string Fecha_Corte_string = Fecha_Corte.Date.ToString();
            //instancia de clase que pertenece a dataService, para conectarse y obtener datos de B.D.
            acceso_aDatos accessData = new acceso_aDatos();

            //System.Data.Common.DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneCadenasPorProcesar");

            // pasa un metodo anonimo a un  parametro tipo delegado
            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_Ejecutor_Calcula_SaldoPromedio_Diario_Cuenta]");
            DataSet DataSet_ = accessData.GetSqlDataSet(cadenaConexion, "ws_Ejecutor_Calcula_SaldoPromedio_Diario_Cuenta",

                //parametro dos   es para el DELEGADO QUE USA 8 PARAMETROS
                (SqlDatabase database,
                    System.Data.Common.DbCommand command,
                    //los siguientes son 6
                    object ID_cuenta,
                    object ID_cadenaComercial,
                    object Fecha_corte,
                    object ClaveEventoagrupador,
                    object Saldo_inicial,
                    object DiafechaCorte, object val7, object val8, object val9, object val10, object val11, object val12, object val13
                    ) =>
                {
                    database.AddInParameter(command, "@ID_Cuenta", DbType.Int32, ID_cuenta);
                    database.AddInParameter(command, "@ID_CadenaComercial", DbType.Int32, ID_cadenaComercial);
                    //database.AddInParameter(command, "@Fecha_Corte", DbType.DateTime, (DateTime)Fecha_corte);
                    database.AddInParameter(command, "@Fecha_Corte", DbType.String, Fecha_corte);
                    database.AddInParameter(command, "@ClaveEventoAgrupador", DbType.String, ClaveEventoagrupador);
                    database.AddInParameter(command, "@Saldo_Inicial", DbType.Decimal, Saldo_inicial);
                    database.AddInParameter(command, "@DiaFechaCorte", DbType.Int32, DiafechaCorte);


                },


                //tres
                ID_Cuenta,
              //cuatro
              ID_CadenaComercial,
            //parametro CINCO
            // Fecha_Corte,
            formattedDate,
              //PARAMETRO SEIS
              ClaveEventoAgrupador,
              //PARAMETRO SIETE
              Saldo_Inicial,
              //PARAMETRO OCHO
              DiaFechaCorte, 7, 8, 9, 10, 11, 12, 13
              );


            laTabla = DataSet_.Tables[0];
            for (int k = 0; k < laTabla.Rows.Count; k++)
            {
                Parametro unParamentro = new Parametro();

                unParamentro.Nombre = (laTabla.Rows[k]["NombreParametro"]).ToString();

                unParamentro.Valor = (laTabla.Rows[k]["Valor"]).ToString();

                larespuesta.Add(unParamentro.Nombre, unParamentro);

            }

            return larespuesta;
        }



        public static Dictionary<String, Parametro> relacionarCorteEventoPolizas(String cadenaConexion, Int64 ID_Corte,
              string ClaveEventoAgrupador, int ID_Cuenta, int ID_CadenaComercial, DateTime Fecha_Corte,
          string DiaFechaCorte)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            string Fecha_Corte_s = Convert.ToString(Fecha_Corte).Substring(0, 10);

            //  DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture);
            DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            string formattedDate = date.ToString("yyyy-MM-dd");

            Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
            DataTable laTabla = null;
            //string Fecha_Corte_string = Fecha_Corte.Date.ToString();
            //instancia de clase que pertenece a dataService, para conectarse y obtener datos de B.D.
            acceso_aDatos accessData = new acceso_aDatos();


            // pasa un metodo anonimo a un  parametro tipo delegado
            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_RelacionarCorteEventoPolizas]");
            DataSet DataSet_ = accessData.GetSqlDataSet(cadenaConexion, "EJECUTOR_RelacionarCorteEventoPolizas",

                //parametro dos   es para el DELEGADO QUE USA 8 PARAMETROS
                (SqlDatabase database,
                    System.Data.Common.DbCommand command,
                    //los siguientes son 6
                    object ID_Corte_,
                    object ClaveEventoAgrupador_,
                    object ID_Cuenta_,
                    object ID_CadenaComercial_,
                    object Fecha_Corte_,
                    object DiaFechaCorte_, object val7, object val8, object val9, object val10, object val11, object val12, object val13
                    ) =>
                {
                    database.AddInParameter(command, "@ID_Corte", DbType.Int32, ID_Corte_);
                    database.AddInParameter(command, "@ClaveEventoAgrupador", DbType.String, ClaveEventoAgrupador_);
                    database.AddInParameter(command, "@ID_Cuenta", DbType.Int32, ID_Cuenta_);
                    database.AddInParameter(command, "@ID_CadenaComercial", DbType.Int32, ID_CadenaComercial_);
                    database.AddInParameter(command, "@Fecha_Corte", DbType.String, Fecha_Corte_);
                    database.AddInParameter(command, "@DiaFechaCorte", DbType.String, DiaFechaCorte_);


                },


                //tres
                ID_Corte,
              //cuatro
              ClaveEventoAgrupador,
              ID_Cuenta,
            //parametro CINCO
            ID_CadenaComercial,
            formattedDate,
              DiaFechaCorte, 7, 8, 9, 10, 11, 12, 13
              );


            laTabla = DataSet_.Tables[0];
            for (int k = 0; k < laTabla.Rows.Count; k++)
            {
                Parametro unParamentro = new Parametro();

                unParamentro.Nombre = (laTabla.Rows[k]["NombreParametro"]).ToString();

                unParamentro.Valor = (laTabla.Rows[k]["Valor"]).ToString();

                larespuesta.Add(unParamentro.Nombre, unParamentro);

            }

            return larespuesta;
        }




        public static bool Calculo_NuevoSaldoCorteCuenta(String cadenaConexion, Int64 ID_Corte,
            DateTime Fecha_Corte,
         String DiaFechaCorte,
         String Saldo_PromedioDiario, String Pago_MinimoANT,
         String FactorSaldoInsoluto, String FactorLimiteCredito,
         String SaldoInsoluto,
         String INTORD, String IVAINTORD,
         String INTMOR, String IVAINTMOR,
         String LimiteCredito)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            try
            {
                string Fecha_Corte_s = Convert.ToString(Fecha_Corte).Substring(0, 10);

                //  DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture);
                DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                string formattedDate = date.ToString("yyyy-MM-dd");

                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;
                //string Fecha_Corte_string = Fecha_Corte.Date.ToString();
                //instancia de clase que pertenece a dataService, para conectarse y obtener datos de B.D.
                acceso_aDatos accessData = new acceso_aDatos();


                // pasa un metodo anonimo a un  parametro tipo delegado
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_Calculo_NuevoSaldoCorteCuenta]");
                DataSet DataSet_ = accessData.GetSqlDataSet(cadenaConexion, "EJECUTOR_Calculo_NuevoSaldoCorteCuenta",

                    //parametro dos   es para el DELEGADO QUE USA 8 PARAMETROS
                    (SqlDatabase database,
                        System.Data.Common.DbCommand command,
                        //los siguientes son 6
                        object ID_Corte_,
                        object Fecha_Corte_,
                        object DiaFechaCorte_,
                        object Saldo_PromedioDiario_,
                        object Pago_MinimoANT_,
                        object FactorSaldoInsoluto_,
                        object FactorLimiteCredito_,
                        object SaldoInsoluto_,
                        object INTORD_,
                        object IVAINTORD_,
                        object LimiteCredito_,
                        object INTMOR_,
                        object IVAINTMOR_


                        ) =>
                    {
                        database.AddInParameter(command, "@ID_Corte", DbType.Int32, ID_Corte_);
                        database.AddInParameter(command, "@Fecha_Corte", DbType.String, Fecha_Corte_);
                        database.AddInParameter(command, "@DiaFechaCorte", DbType.String, DiaFechaCorte_);
                        database.AddInParameter(command, "@Saldo_PromedioDiario", DbType.String, Saldo_PromedioDiario_);

                        database.AddInParameter(command, "@Pago_MinimoANT", DbType.String, Pago_MinimoANT_);

                        database.AddInParameter(command, "@FactorSaldoInsoluto", DbType.String, FactorSaldoInsoluto_);
                        database.AddInParameter(command, "@FactorLimiteCredito", DbType.String, FactorLimiteCredito_);
                        database.AddInParameter(command, "@SaldoInsoluto", DbType.String, SaldoInsoluto_);
                        database.AddInParameter(command, "@INTORD", DbType.String, INTORD_);

                        database.AddInParameter(command, "@IVAINTORD", DbType.String, IVAINTORD_);

                        database.AddInParameter(command, "@INTMOR", DbType.String, INTMOR_);

                        database.AddInParameter(command, "@IVAINTMOR", DbType.String, IVAINTMOR_);

                        database.AddInParameter(command, "@LimiteCredito", DbType.String, LimiteCredito_);


                    },


                    ID_Corte,
                        formattedDate,
                        DiaFechaCorte,
                         Saldo_PromedioDiario,
                        Pago_MinimoANT,
                         FactorSaldoInsoluto,
                         FactorLimiteCredito,
                        SaldoInsoluto,
                        INTORD,
                         IVAINTORD,
                         LimiteCredito,
                         INTMOR,
                         IVAINTMOR
                  );


                //laTabla = DataSet_.Tables[0];
                //for (int k = 0; k < laTabla.Rows.Count; k++)
                //{
                //    Parametro unParamentro = new Parametro();

                //    unParamentro.Nombre = (laTabla.Rows[k]["NombreParametro"]).ToString();

                //    unParamentro.Valor = (laTabla.Rows[k]["Valor"]).ToString();

                //    larespuesta.Add(unParamentro.Nombre, unParamentro);

                //}
                return true;
            }
            catch (Exception err)
            {
                return false;
            }

        }






        public static List<Parametro> ObtieneParametrosdeStoredProcedure(String SP)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            //String stored = new String();

            List<Parametro> Parametros;
            //SqlConnection connConsulta, SqlTransaction transaccionSQL
            //  DAOUtilerias obtenParametros = new DAOUtilerias(_connConsulta, _transaccionSQL);





            SqlDataReader SqlReader2 = null;

            //Obtiene los valores de los parametros tanto de referencia como Operativos a partir del Mensaje ISO
            //  obtenParametros.getParametrosReferidosPertenencia(ID_TipoContrato,ID_CadenaComercial, ref losParametros);



            //CallableStatement spEjecutor = null;
            //ResultSet Resultado=null;

            try
            {
                string cadenaConexion = "Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43; MultipleActiveResultSets=true;";
                using (SqlConnection conn2 = new SqlConnection(cadenaConexion))
                {

                    conn2.Open();



                    using (SqlTransaction transaccionSQL = conn2.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {

                        Parametros = new List<Parametro>();
                        SqlParameter param;
                        LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtieneParametrosdeStoredProcedure]");
                        SqlCommand comando2 = new SqlCommand("EJECUTOR_ObtieneParametrosdeStoredProcedure", conn2);
                        comando2.Transaction = transaccionSQL;
                        comando2.CommandType = CommandType.StoredProcedure;

                        param = new SqlParameter("@StoredProcedure", SqlDbType.VarChar);
                        param.Value = SP;
                        comando2.Parameters.Add(param);

                        SqlReader2 = comando2.ExecuteReader();

                        if (null != SqlReader2)
                        {
                            while (SqlReader2.Read())
                            {



                                //Si el SP no regresa ni una regla no debe agregar nada.
                                Parametro parametro = new Parametro();
                                parametro.Nombre = (string)SqlReader2["NombreParametro"];
                                parametro.TipoDato = TipoDato.getTipoDatoSQL((string)SqlReader2["TipoDato"]);


                                Parametros.Add(parametro);


                            }
                        }



                    }
                }

            }
            catch (SqlException ex)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, getReglasOperacion(): " + ex.Message + "]");
                throw ex;
            }

            finally
            {
                try
                {
                    if(SqlReader2 != null)
                        SqlReader2.Close();
                }
                catch (Exception err)
                { }
            }

            return Parametros;
        }



    }
}