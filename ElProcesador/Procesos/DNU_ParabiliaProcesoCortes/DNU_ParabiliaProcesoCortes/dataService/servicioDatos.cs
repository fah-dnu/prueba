using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.Utilidades;
using Executer.Entidades;
using Interfases.Entidades;
using Interfases.Enums;
using log4net;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.dataService
{

    interface parametroSP
    {
        List<Evento> Obtiene_EventosAgrupadores(Int64 IDCUENTA, String ClaveCorteTipo);
        List<Evento> Obtiene_EventosAgrupadores(Int64 IDCUENTA, String ClaveCorteTipo, Int64 idCorte, string numeroMesCobranza);

    }



    class servicioDatos : parametroSP
    {

        public SqlConnection DataConnection;
        public SqlDatabase database;

        public servicioDatos(SqlConnection connConsulta, string cadenaConexion)
        {
            DataConnection = connConsulta;
            database = new SqlDatabase(cadenaConexion);
        }
        public servicioDatos(SqlConnection connConsulta, string cadenaConexion, SqlTransaction transaccionSQL)
        {
            DataConnection = connConsulta;
            database = new SqlDatabase(cadenaConexion);
        }

        public List<Evento> Obtiene_EventosAgrupadores(Int64 IDCUENTA, string ClaveCorteTipo, Int64 IdCorte, string numeroMesCobranza)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP procnoc_ejecutor_ObtieneEjecucionesEventosAgrupadores]");
            System.Data.Common.DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneEjecucionesEventosAgrupadores");
            database.AddInParameter(command, "@idCuenta", DbType.Int64, IDCUENTA);
            database.AddInParameter(command, "@claveCorteTipo", DbType.String, ClaveCorteTipo);
            database.AddInParameter(command, "@idCorte", DbType.String, IdCorte);
            database.AddInParameter(command, "@numeroMes", DbType.String, numeroMesCobranza);
            DataSet DataSet_ = database.ExecuteDataSet(command);
            DataTable laTabla = null;
            List<Evento> laRespuesta = new List<Evento>();
            laTabla = DataSet_.Tables[0];
            for (int k = 0; k < laTabla.Rows.Count; k++)
            {
                Evento unEvento = new Evento();

                //  unEvento.ID_ConfiguracionCorte = long.Parse((laTabla.Rows[k]["ID_ConfiguracionCorte"]).ToString());
                // unEvento.ID_AgrupadorEvento = long.Parse((laTabla.Rows[k]["ID_AgrupadorEvento"]).ToString());
                unEvento.ID_AgrupadorEvento = Int32.Parse((laTabla.Rows[k]["ID_AgrupadorEvento"]).ToString());

                //unEvento.ID_Evento = Int32.Parse((laTabla.Rows[k]["ID_Evento"]).ToString());
                unEvento.ClaveEventoAgrupador = (laTabla.Rows[k]["ClaveEventoAgrupador"]).ToString();
                // unEvento.Descripcion = (laTabla.Rows[k]["Descripcion"]).ToString();

                //estos si
                unEvento.ID_CadenaComercial = Int64.Parse((laTabla.Rows[k]["ID_CadenaComercial"]).ToString());
                // unEvento.ID_Contrato = Int64.Parse((laTabla.Rows[k]["ID_Contrato"]).ToString());
                unEvento.ClaveCadenaComercial = (laTabla.Rows[k]["ClaveColectiva"]).ToString();
                unEvento.Consecutivo = Int64.Parse((laTabla.Rows[k]["Consecutivo"]).ToString());
                unEvento.Stored_Procedure = (laTabla.Rows[k]["Stored_Procedure"]).ToString();
                unEvento.Descripcion = (laTabla.Rows[k]["Descripcion"]).ToString();
                //
                unEvento.generaPoliza = (laTabla.Rows[k]["generaPoliza"]).ToString();
                unEvento.incluirEnXML = (laTabla.Rows[k]["incluirEnXMl"]).ToString();
                unEvento.eventoPrincipal = (laTabla.Rows[k]["eventoPrincipal"]).ToString();
                unEvento.descripcionEventoEdoCuenta = (laTabla.Rows[k]["descripcionEventoEdoCuenta"]).ToString();
                unEvento.unidadSAT = (laTabla.Rows[k]["unidadSAT"]).ToString();
                unEvento.ClaveProdServSAT = (laTabla.Rows[k]["ClaveProdServSAT"]).ToString();
                unEvento.ClaveUnidadSAT = (laTabla.Rows[k]["ClaveUnidadSAT"]).ToString();
                unEvento.impImpuestoSAT = (laTabla.Rows[k]["impImpuestoSAT"]).ToString();
                unEvento.impTipoFactorSAT = (laTabla.Rows[k]["impTipoFactorSAT"]).ToString();
                unEvento.parametroMonto = (laTabla.Rows[k]["parametroMonto"]).ToString();
                unEvento.id_corteEventoAgrupador = (laTabla.Rows[k]["id_corteEventoAgrupador"]).ToString();


                laRespuesta.Add(unEvento);

            }
            return laRespuesta;
        }

        public List<Evento> Obtiene_EventosAgrupadores(Int64 IDCUENTA, string ClaveCorteTipo)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP procnoc_ejecutor_ObtieneEjecucionesEventosAgrupadores]");
            System.Data.Common.DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneEjecucionesEventosAgrupadores");

            database.AddInParameter(command, "@ID_Cuenta", DbType.Int32, IDCUENTA);
            database.AddInParameter(command, "@ClaveCorteTipo", DbType.String, ClaveCorteTipo);



            DataSet DataSet_ = database.ExecuteDataSet(command);
            DataTable laTabla = null;
            List<Evento> laRespuesta = new List<Evento>();

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


        public Dictionary<String, Parametro> ObtieneParametros_Cuenta_CorteAnterior(Int64 idcuenta, String DiaFechaCorte, DateTime Fecha_Corte, Int64 idCorte)
        {

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            //            string Fecha_Corte_s = Convert.ToString(Fecha_Corte).Substring(0, 10);

            //  DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture);
            //DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            string formattedDate = Fecha_Corte.ToString("yyyy-MM-dd");

            

            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_Ejecutor_ObtieneParametros_Cuenta_CorteAnterior]");
            System.Data.Common.DbCommand command = database.GetStoredProcCommand("ws_Ejecutor_ObtieneParametros_Cuenta_CorteAnterior");

            Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
            DataTable laTabla = null;


            database.AddInParameter(command, "@ID_Cuenta", DbType.Int64, idcuenta);
            database.AddInParameter(command, "@Fecha_Corte", DbType.String, formattedDate);
            database.AddInParameter(command, "@DiaFechaCorte", DbType.String, DiaFechaCorte);
            database.AddInParameter(command, "@ID_Corte", DbType.String, idCorte);

            DataSet DataSet_ = database.ExecuteDataSet(command);

            laTabla = DataSet_.Tables[0];


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



        public Dictionary<String, Parametro> ObtieneParametros_Cuenta(Int64 idcuenta, Int64 idcorte, string tarjeta)
        {

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
            DataTable laTabla = null;

            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_Ejecutor_ObtieneParametros_Cuenta]");
            System.Data.Common.DbCommand command = database.GetStoredProcCommand("ws_Ejecutor_ObtieneParametros_Cuenta");




            database.AddInParameter(command, "@ID_Cuenta", DbType.Int64, idcuenta);

            database.AddInParameter(command, "@ID_Corte", DbType.Int64, idcorte);
            database.AddInParameter(command, "@tarjeta", DbType.String, tarjeta);
            DataSet DataSet_ = database.ExecuteDataSet(command);

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



        public List<Cuentas> Obtiene_Set_deCuentas(string fecha = null, string idCorte = null, bool activo = false)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            List<Cuentas> laRespuesta = new List<Cuentas>();

            //instancia de clase que pertenece a dataService, para conectarse y obtener datos de B.D.
            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP procnoc_ejecutor_ObtieneCuentasPorProcesar]");
            System.Data.Common.DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneCuentasPorProcesar");
            if (!string.IsNullOrEmpty(fecha))
                database.AddInParameter(command, "@fecha", DbType.String, fecha);
            if (!string.IsNullOrEmpty(idCorte))
                database.AddInParameter(command, "@idCorte", DbType.String, idCorte);
            if (activo)
                database.AddInParameter(command, "@activo", DbType.Boolean, activo);

            DataTable laTabla = null;
            DataSet DataSet_ = database.ExecuteDataSet(command);

            laTabla = DataSet_.Tables[0];


            for (int k = 0; k < laTabla.Rows.Count; k++)
            {

                Cuentas cuenta = new Cuentas();
                cuenta.ID_Corte = Int64.Parse((laTabla.Rows[k]["id_corte"]).ToString());
                cuenta.ID_Cuenta = Int64.Parse((laTabla.Rows[k]["id_cuenta"]).ToString());
                cuenta.id_CadenaComercial = Int64.Parse((laTabla.Rows[k]["id_cadenaComercial"]).ToString());
                cuenta.Fecha_Corte = DateTime.Parse((laTabla.Rows[k]["FechaProximoCorte"]).ToString());
                cuenta.ClaveCorteTipo = (laTabla.Rows[k]["ClaveCorteTipo"]).ToString();
                cuenta.ID_CuentaHabiente = Int64.Parse((laTabla.Rows[k]["ID_ColectivaCuentahabiente"]).ToString());
                cuenta.ID_TipoColectiva = Int32.Parse((laTabla.Rows[k]["ID_TipoColectiva"]).ToString());
                cuenta.Tarjeta = (laTabla.Rows[k]["tarjeta"]).ToString();
                cuenta.ClaveCliente = (laTabla.Rows[k]["claveCliente"]).ToString();
                cuenta.ClaveCuentahabiente = (laTabla.Rows[k]["claveCuentaHabiente"]).ToString();
                cuenta.NombreCuentahabiente = (laTabla.Rows[k]["nombreCuentahabiente"]).ToString();
                cuenta.CorreoCuentahabiente = (laTabla.Rows[k]["correo"]).ToString();
                cuenta.RFCCuentahabiente = (laTabla.Rows[k]["RFC"]).ToString();
                cuenta.FechaCorteAnterior = DateTime.Parse((laTabla.Rows[k]["FechaCorte"]).ToString());
                cuenta.NombreORazonSocial = (laTabla.Rows[k]["NombreORazonSocialCliente"]).ToString();
                cuenta.RFCCliente = (laTabla.Rows[k]["RFCCliente"]).ToString();

                laRespuesta.Add(cuenta);

            }

            return laRespuesta;
        }





        public Dictionary<String, Parametro> ObtieneParametros_Contrato(String ClaveCadenaComercial, String Tarjeta,
           String ClaveEvento, String elUsuario)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
            DataTable laTabla = null;

            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP ws_Ejecutor_ObtieneValoresContratos_cuentas]");
            System.Data.Common.DbCommand command = database.GetStoredProcCommand("ws_Ejecutor_ObtieneValoresContratos_cuentas");



            database.AddInParameter(command, "@ClaveCadenaComercial", DbType.String, ClaveCadenaComercial);
            database.AddInParameter(command, "@ClaveEvento", DbType.String, ClaveEvento);
            database.AddInParameter(command, "@Tarjeta", DbType.String, Tarjeta);



            DataSet DataSet_ = database.ExecuteDataSet(command);
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






        public List<Parametro> ObtieneParametrosdeStoredProcedure(String SP, SqlTransaction transaccionSQL)
        {

            //String stored = new String();
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

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
                //  string cadenaConexion = "Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43; MultipleActiveResultSets=true;";


                Parametros = new List<Parametro>();
                SqlParameter param;
                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_ObtieneParametrosdeStoredProcedure]");
                SqlCommand comando2 = new SqlCommand("EJECUTOR_ObtieneParametrosdeStoredProcedure", DataConnection, transaccionSQL);

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
            catch (SqlException ex)
            {
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [EJECUTOR, getReglasOperacion(): " + ex.Message + "]");
                //Logueo.Error("[EJECUTOR] getReglasOperacion(): " + ex.Message);
                throw ex;
            }

            finally
            {
                try
                {
                    if(SqlReader2 != null)
                        SqlReader2.Close();
                }
                catch (Exception err) { }
            }

            return Parametros;
        }







        public Dictionary<String, Parametro> relacionarCorteEventoPolizas(Int64 ID_Corte,
           string ClaveEventoAgrupador, Int64 ID_Cuenta, DateTime Fecha_Corte,
       string DiaFechaCorte, String Fecha_Inicial, String Fecha_Final, string idPoliza, SqlTransaction transaccionSQL, string limteFechaPago)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();
            // string Fecha_Corte_s = Convert.ToString(Fecha_Corte).Substring(0, 10);

            //  DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture);
            // DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
            string formattedDate = Fecha_Corte.ToString("yyyy-MM-dd");

            Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
            DataTable laTabla = null;
            //string Fecha_Corte_string = Fecha_Corte.Date.ToString();
            //instancia de clase que pertenece a dataService, para conectarse y obtener datos de B.D.




            LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_RelacionarCorteEventoPolizas]");
            System.Data.Common.DbCommand command = database.GetStoredProcCommand("EJECUTOR_RelacionarCorteEventoPolizas");

            command.CommandTimeout = 600;

            database.AddInParameter(command, "@ID_Corte", DbType.Int64, ID_Corte);
            database.AddInParameter(command, "@ClaveEventoAgrupador", DbType.String, ClaveEventoAgrupador);
            database.AddInParameter(command, "@ID_Cuenta", DbType.Int64, ID_Cuenta);
            // database.AddInParameter(command, "@ID_CadenaComercial", DbType.Int32, ID_CadenaComercial);
            database.AddInParameter(command, "@Fecha_Corte", DbType.String, formattedDate);
            database.AddInParameter(command, "@DiaFechaCorte", DbType.String, DiaFechaCorte);

            database.AddInParameter(command, "@Fecha_Inicial", DbType.String, Fecha_Inicial);
            database.AddInParameter(command, "@Fecha_Final", DbType.String, Fecha_Final);
            database.AddInParameter(command, "@ID_Poliza", DbType.Int64, idPoliza);
            database.AddInParameter(command, "@FechaLimitePago", DbType.String, limteFechaPago);

            DataSet DataSet_ = database.ExecuteDataSet(command, transaccionSQL);

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



        public DataTable ejectutar_SP_EventoAgrupador(String sp, Dictionary<string, Parametro> todoslosparametros, SqlTransaction transaccionSQL)
        {
            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            List<Parametro> ParametroSP = ObtieneParametrosdeStoredProcedure(sp, transaccionSQL);
            DataTable laTabla = new DataTable();
            try
            {
                // List<Evento> larespuesta = new List<Evento>();
                //  string cadenaConexion = "Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43";
                System.Data.Common.DbCommand command = database.GetStoredProcCommand(sp);
                // command.Transaction =  transaccionSQL;
                //PROCEDE A AGREGAR PARAMETROS AL COMMAND. 
                // SqlParameter param;
                foreach (Parametro valorParametro in ParametroSP)
                {

                    //spEjecutor.s("hola", valorParametro.getNombre());
                    TipoDatoSQL dato = valorParametro.TipoDato;

                    SqlParameter param;



                    switch (dato)
                    {
                        case TipoDatoSQL.VARCHAR:
                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.VarChar);
                            param.Value = todoslosparametros[valorParametro.Nombre.Trim()].Valor;
                            command.Parameters.Add(param);
                            break;
                        case TipoDatoSQL.DATETIME:

                            string Fecha_Corte_s = Convert.ToString(todoslosparametros[valorParametro.Nombre.Trim()].Valor).Substring(0, 10);
                            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
                            DateTime dateFechaLimiteDePago = Convert.ToDateTime(Fecha_Corte_s);
                            //DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            string formattedDate = dateFechaLimiteDePago.ToString("yyyy-MM-dd");
                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.DateTime);
                            param.Value = formattedDate;
                            command.Parameters.Add(param);
                            break;
                        case TipoDatoSQL.CHAR:

                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.VarChar);
                            param.Value = todoslosparametros[valorParametro.Nombre.Trim()].Valor;
                            command.Parameters.Add(param);
                            break;
                        case TipoDatoSQL.INT:

                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.Int);
                            param.Value = todoslosparametros[valorParametro.Nombre.Trim()].Valor;
                            command.Parameters.Add(param);
                            break;
                        case TipoDatoSQL.BIGINT:

                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.BigInt);
                            param.Value = todoslosparametros[valorParametro.Nombre.Trim()].Valor;
                            command.Parameters.Add(param);
                            break;
                        case TipoDatoSQL.BIT:

                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.Bit);
                            param.Value = todoslosparametros[valorParametro.Nombre.Trim()].Valor;
                            command.Parameters.Add(param);
                            break;
                        case TipoDatoSQL.DECIMAL:
                        case TipoDatoSQL.DOUBLE:

                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.Decimal);
                            param.Value = todoslosparametros[valorParametro.Nombre.Trim()].Valor;
                            command.Parameters.Add(param);
                            break;
                        case TipoDatoSQL.FLOAT:

                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.Float);
                            param.Value = todoslosparametros[valorParametro.Nombre.Trim()].Valor;
                            command.Parameters.Add(param);
                            break;
                        case TipoDatoSQL.TINYINT:

                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.TinyInt);
                            param.Value = todoslosparametros[valorParametro.Nombre.Trim()].Valor;
                            command.Parameters.Add(param);
                            break;
                        case TipoDatoSQL.SMALLINT:

                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.SmallInt);
                            param.Value = todoslosparametros[valorParametro.Nombre.Trim()].Valor;
                            command.Parameters.Add(param);
                            break;
                        default:
                            String msjError = "No hay Mapeo de Tipo de Dato : " + valorParametro.Nombre;
                            LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [" + msjError +"]");
                            //Logueo.Error(msjError);
                            break;

                    }

                }
                laTabla = database.ExecuteDataSet(command, transaccionSQL).Tables[0];
            }

            catch (Exception err)
            {
                //   transaccionSQL.Rollback();
                // GeneracionExitosaDePolizas = false;
                LogueProcesaEdoCuenta.Error("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Error al ejectutar SP :" + err.Message + "]");
                //Logueo.Error("Error al ejectutar SP :" + err.Message);
                throw err;

            }

            return laTabla;

        }








        public bool Calculo_NuevoSaldoCorteCuenta(SqlTransaction transaccionSQL, Int64 ID_Corte,
          DateTime Fecha_Corte,
       String DiaFechaCorte,
       String Saldo_PromedioDiario, String Pago_MinimoANT,
       String FactorSaldoInsoluto, String FactorLimiteCredito,
       String SaldoInsoluto,
       String INTORD, String IVAINTORD,
       String INTMOR, String IVAINTMOR,
       String LimiteCredito, string Saldo_Vencido,
        Int64 ID_Cuenta,
       String ClaveCorteTipo, Int64 ID_CorteAnt, decimal sumaComisiones, decimal ivaComision,
       decimal TasaOrdinariaAnual, decimal TasaMoratoriaAnual, int diasTranscurridos,
       decimal ComprasDisposiciones,
       decimal PagosReembolsos, decimal limiteRetiroEfectivo, string FactorSaldoCuenta)
        {

            string log = ThreadContext.Properties["log"].ToString();
            string ip = ThreadContext.Properties["ip"].ToString();

            try
            {
                //string Fecha_Corte_s = Convert.ToString(Fecha_Corte).Substring(0, 10);

                //  DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture);
                //DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                string formattedDate = Fecha_Corte.ToString("yyyy-MM-dd");

                Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
                DataTable laTabla = null;
                //string Fecha_Corte_string = Fecha_Corte.Date.ToString();
                //instancia de clase que pertenece a dataService, para conectarse y obtener datos de B.D.

                LogueProcesaEdoCuenta.Info("[" + ip + "] [ProcesoCortes] [PROCESADORNOCTURNO] [" + log + "] [Ejecuta SP EJECUTOR_Calculo_NuevoSaldoCorteCuenta]");
                System.Data.Common.DbCommand command = database.GetStoredProcCommand("EJECUTOR_Calculo_NuevoSaldoCorteCuenta");




                database.AddInParameter(command, "@ID_Corte", DbType.Int64, ID_Corte);
                database.AddInParameter(command, "@Fecha_Corte", DbType.String, formattedDate);
                database.AddInParameter(command, "@DiaFechaCorte", DbType.String, DiaFechaCorte);
                database.AddInParameter(command, "@Saldo_PromedioDiario", DbType.String, Saldo_PromedioDiario);

                database.AddInParameter(command, "@Pago_MinimoANT", DbType.String, Pago_MinimoANT);

                database.AddInParameter(command, "@FactorSaldoInsoluto", DbType.String, FactorSaldoInsoluto);
                database.AddInParameter(command, "@FactorLimiteCredito", DbType.String, FactorLimiteCredito);
                database.AddInParameter(command, "@SaldoInsoluto", DbType.String, SaldoInsoluto);
                database.AddInParameter(command, "@INTORD", DbType.String, INTORD);
                database.AddInParameter(command, "@IVAINTORD", DbType.String, IVAINTORD);
                database.AddInParameter(command, "@INTMOR", DbType.String, INTMOR);
                database.AddInParameter(command, "@IVAINTMOR", DbType.String, IVAINTMOR);
                database.AddInParameter(command, "@LimiteCredito", DbType.String, LimiteCredito);
                database.AddInParameter(command, "@Saldo_Vencido", DbType.String, Saldo_Vencido);
                database.AddInParameter(command, "@ID_Cuenta", DbType.Int64, ID_Cuenta);
                database.AddInParameter(command, "@ClaveCorteTipo", DbType.String, ClaveCorteTipo);
                database.AddInParameter(command, "@ID_CorteAnt", DbType.Int64, ID_CorteAnt);
                database.AddInParameter(command, "@Comisiones", DbType.Decimal, sumaComisiones);
                database.AddInParameter(command, "@IvaComisiones", DbType.Decimal, ivaComision);
                database.AddInParameter(command, "@TasaOrdinariaAnual", DbType.Decimal, TasaOrdinariaAnual);
                database.AddInParameter(command, "@TasaMoratoriaAnual", DbType.Decimal, TasaMoratoriaAnual);
                database.AddInParameter(command, "@DiasDelPeriodo", DbType.Int64, diasTranscurridos);
                database.AddInParameter(command, "@compasDisposiciones", DbType.Decimal, ComprasDisposiciones);
                database.AddInParameter(command, "@pagosReembolsosDev", DbType.Decimal, PagosReembolsos);
                database.AddInParameter(command, "@retiroEfectivoDisponible", DbType.Decimal, limiteRetiroEfectivo);
                database.AddInParameter(command, "@FactorSaldoCuenta", DbType.String, FactorSaldoInsoluto);


                database.ExecuteDataSet(command, transaccionSQL);

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

    }
}
