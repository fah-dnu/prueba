using CommonProcesador;
using DNU_ParabiliaProcesoAnualidad.Entidades;
using Executer.Entidades;
using Interfases.Entidades;
using Interfases.Enums;
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

namespace DNU_ParabiliaProcesoAnualidad.dataService
{
    public class servicioDatosAnualidad
    {
        public SqlConnection DataConnection;
        public SqlDatabase database;
        public int numeroCuentas { get; set; }

        public servicioDatosAnualidad(SqlConnection connConsulta, string cadenaConexion)
        {
            DataConnection = connConsulta;
            database = new SqlDatabase(cadenaConexion);
        }

        public servicioDatosAnualidad(SqlConnection connConsulta, string cadenaConexion, SqlTransaction transaccionSQL)
        {
            DataConnection = connConsulta;
            database = new SqlDatabase(cadenaConexion);
        }

        public List<Cuentas> Obtiene_Set_deCuentas(string fecha = null, string idCorte = null, bool activo = false)
        {


            List<Cuentas> laRespuesta = new List<Cuentas>();

            //instancia de clase que pertenece a dataService, para conectarse y obtener datos de B.D.
            System.Data.Common.DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneCuentasPorProcesarAnualidad");
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
                cuenta.ID_Corte = Int64.Parse((laTabla.Rows[k]["ID_Corte"]).ToString());
                cuenta.ID_Cuenta = Int64.Parse((laTabla.Rows[k]["ID_Cuenta"]).ToString());
                cuenta.id_CadenaComercial = Int64.Parse((laTabla.Rows[k]["id_cadenaComercial"]).ToString());
                cuenta.Fecha_Corte = DateTime.Parse((laTabla.Rows[k]["FechaProximoCorte"]).ToString());
                cuenta.ClaveCorteTipo = (laTabla.Rows[k]["ClaveCorteTipo"]).ToString();
                if (cuenta.ClaveCorteTipo != "MTD001")
                {
                    numeroCuentas++;
                }
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
                cuenta.id_colectivaCliente = Convert.ToInt64((laTabla.Rows[k]["ID_ColectivaCliente"]).ToString());
                //cuenta.CP = (laTabla.Rows[k]["CP"]).ToString();
                //cuenta.Subproducto = (laTabla.Rows[k]["Subproducto"]).ToString();
                cuenta.CumpleAnio = (laTabla.Rows[k]["CumpleAnio"]).ToString();
                laRespuesta.Add(cuenta);

            }

            return laRespuesta;
        }

        public Dictionary<String, Parametro> ObtieneParametros_Cuenta(Int64 idcuenta, Int64 idcorte, string tarjeta)
        {

            Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
            DataTable laTabla = null;

            System.Data.Common.DbCommand command = database.GetStoredProcCommand("ws_Ejecutor_ObtieneParametros_CuentaCobranza");
            database.AddInParameter(command, "@ID_Cuenta", DbType.Int64, idcuenta);
            database.AddInParameter(command, "@ID_Corte", DbType.Int64, idcorte);
            //parametros always encripted
            SqlParameter paramSSN = (SqlParameter)command.CreateParameter();
            paramSSN.ParameterName = "@tarjeta";
            paramSSN.DbType = DbType.AnsiStringFixedLength;
            paramSSN.Direction = ParameterDirection.Input;
            paramSSN.Value = tarjeta;
            paramSSN.Size = string.IsNullOrEmpty(tarjeta)
                                ? 50
                                : tarjeta.Length;

            command.Parameters.Add(paramSSN);

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

        //public Dictionary<String, Parametro> ObtieneParametros_Cuenta_CorteAnterior(Int64 idcuenta, String DiaFechaCorte, DateTime Fecha_Corte, Int64 idCorte)
        //{
        //    // string Fecha_Corte_s = Convert.ToString(Fecha_Corte).Substring(0, 10);

        //    //  DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy h:m:s tt", System.Globalization.CultureInfo.InvariantCulture);
        //    //DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
        //    string formattedDate = Fecha_Corte.ToString("yyyy-MM-dd");
        //    System.Data.Common.DbCommand command = database.GetStoredProcCommand("ws_Ejecutor_ObtieneParametros_Cuenta_CorteAnterior");

        //    Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
        //    DataTable laTabla = null;
        //    database.AddInParameter(command, "@ID_Cuenta", DbType.Int64, idcuenta);
        //    database.AddInParameter(command, "@Fecha_Corte", DbType.String, formattedDate);
        //    database.AddInParameter(command, "@DiaFechaCorte", DbType.String, DiaFechaCorte);
        //    database.AddInParameter(command, "@ID_Corte", DbType.String, idCorte);

        //    DataSet DataSet_ = database.ExecuteDataSet(command);

        //    laTabla = DataSet_.Tables[0];
        //    for (int k = 0; k < laTabla.Rows.Count; k++)
        //    {
        //        Parametro unParamentro = new Parametro();
        //        unParamentro.Nombre = (laTabla.Rows[k]["NombreParametro"]).ToString();
        //        unParamentro.Valor = (laTabla.Rows[k]["Valor"]).ToString();

        //        larespuesta.Add(unParamentro.Nombre, unParamentro);
        //    }

        //    return larespuesta;
        //}

        public List<Evento> Obtiene_EventosAgrupadores(Int64 IDCUENTA, string ClaveCorteTipo, Int64 IdCorte, string numeroMesCobranza)
        {
            System.Data.Common.DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneEjecucionesEventosAnualidad");

            database.AddInParameter(command, "@idCorte", DbType.String, IdCorte);
            DataSet DataSet_ = database.ExecuteDataSet(command);
            DataTable laTabla = null;
            List<Evento> laRespuesta = new List<Evento>();
            laTabla = DataSet_.Tables[0];
            for (int k = 0; k < laTabla.Rows.Count; k++)
            {
                Evento unEvento = new Evento();


                unEvento.ID_AgrupadorEvento = Int32.Parse((laTabla.Rows[k]["ID_AgrupadorEvento"]).ToString());
                unEvento.ClaveEventoAgrupador = (laTabla.Rows[k]["ClaveEventoAgrupador"]).ToString();

                //estos si
                unEvento.ID_CadenaComercial = Int64.Parse((laTabla.Rows[k]["ID_CadenaComercial"]).ToString());
                unEvento.ClaveCadenaComercial = (laTabla.Rows[k]["ClaveColectiva"]).ToString();
                unEvento.Consecutivo = Int64.Parse((laTabla.Rows[k]["Consecutivo"]).ToString());
                unEvento.Stored_Procedure = (laTabla.Rows[k]["Stored_Procedure"]).ToString();
                unEvento.Descripcion = (laTabla.Rows[k]["Descripcion"]).ToString();
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
                unEvento.parametroMontoCobranza = (laTabla.Rows[k]["parametroMontoCobranza"]).ToString();
                unEvento.id_corteEventoAgrupador = (laTabla.Rows[k]["id_corteEventoAgrupador"]).ToString();


                laRespuesta.Add(unEvento);

            }
            return laRespuesta;
        }

        public DataTable ejectutar_SP_EventoAgrupador(String sp, Dictionary<string, Parametro> todoslosparametros, SqlTransaction transaccionSQL)
        {

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
                            Logueo.Evento("[GeneraEstadoCuentaCreditoSP] sp:" + sp + "," + valorParametro.Nombre + " valor:" + param.Value);
                            break;
                        case TipoDatoSQL.DATETIME:
                            string Fecha_Corte_s = Convert.ToString(todoslosparametros[valorParametro.Nombre.Trim()].Valor).Substring(0, 10);
                            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-MX");
                            DateTime dateFechaLimiteDePago = Convert.ToDateTime(Fecha_Corte_s);
                            Logueo.Evento("[GeneraEstadoCuentaCreditoSP] " + dateFechaLimiteDePago);
                            //DateTime date = DateTime.ParseExact(Fecha_Corte_s, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            string formattedDate = dateFechaLimiteDePago.ToString("yyyy-MM-dd");
                            param = new SqlParameter(valorParametro.Nombre, SqlDbType.DateTime);
                            param.Value = formattedDate;
                            command.Parameters.Add(param);
                            Logueo.Evento("[GeneraEstadoCuentaCreditoSP] sp:" + sp + ", " + valorParametro.Nombre + "valorDate:" + param.Value);
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
                            Logueo.Error(msjError);
                            break;

                    }

                }
                laTabla = database.ExecuteDataSet(command, transaccionSQL).Tables[0];
            }

            catch (Exception err)
            {
                Logueo.Error("Error al ejectutar SP :" + err.Message);
                throw err;
            }
            //ejecuto store procedure
            return laTabla;

        }

        public List<Parametro> ObtieneParametrosdeStoredProcedure(String SP, SqlTransaction transaccionSQL)
        {
            List<Parametro> Parametros;
            SqlDataReader SqlReader2 = null;
            //Obtiene los valores de los parametros tanto de referencia como Operativos a partir del Mensaje ISO

            try
            {
                //  string cadenaConexion = "Data Source=64.34.163.109;initial catalog=Autorizador_Diconsa;user id=Developer04;password=Mochila43; MultipleActiveResultSets=true;";

                Parametros = new List<Parametro>();
                SqlParameter param;
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
                Logueo.Error("[EJECUTOR] getReglasOperacion(): " + ex.Message);
                throw ex;
            }
            finally
            {
                try
                {
                    SqlReader2.Close();
                }
                catch (Exception err) { }
            }

            return Parametros;
        }

        //public DataTable Obtiene_SaldosYPagos(long idCuenta, long idCorte, DateTime dtInicio, DateTime dtFin)
        //{
        //    try
        //    {
        //        System.Data.Common.DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneSaldosYPagosCobranza");
        //        database.AddInParameter(command, "@idCuenta", DbType.Int64, idCuenta);
        //        database.AddInParameter(command, "@idCorte", DbType.Int64, idCorte);
        //        database.AddInParameter(command, "@FechaInicio", DbType.DateTime, dtInicio);
        //        database.AddInParameter(command, "@FechaFin", DbType.DateTime, dtFin);

        //        DataSet DataSet_ = database.ExecuteDataSet(command);

        //        return DataSet_.Tables[0];
        //    }

        //    catch (Exception err)
        //    {
        //        Logueo.Error("Error al ejectutar SP :" + err.Message);
        //        throw err;
        //    }
        //}

        public Dictionary<String, Parametro> relacionarCorteEventoPolizas(Int64 ID_Corte,
           string ClaveEventoAgrupador, Int64 ID_Cuenta, string idPoliza, SqlTransaction transaccionSQL, string fecha)
        {
            Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
            DataTable laTabla = null;

            System.Data.Common.DbCommand command = database.GetStoredProcCommand("EJECUTOR_RelacionarCorteEventoPolizasAnualidad");
            command.CommandTimeout = 600;

            database.AddInParameter(command, "@ID_Corte", DbType.Int64, ID_Corte);
            database.AddInParameter(command, "@ClaveEventoAgrupador", DbType.String, ClaveEventoAgrupador);
            database.AddInParameter(command, "@ID_Cuenta", DbType.Int64, ID_Cuenta);
            database.AddInParameter(command, "@ID_Poliza", DbType.Int64, idPoliza);
            database.AddInParameter(command, "@FechaDia", DbType.String, fecha);

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

        public Dictionary<String, Parametro> ConsultaParametrosTarjeta(string Tarjeta, string MedioAcceso = null, string TipoMedioAcceso = null, SqlConnection conn = null, SqlTransaction transaccionSQL = null)
        {
            Dictionary<String, Parametro> larespuesta = new Dictionary<String, Parametro>();
            DataTable laTabla = null;

            System.Data.Common.DbCommand command = database.GetStoredProcCommand("procnoc_ejecutor_ObtieneParametrosOpenPay");
           
           // database.AddInParameter(command, "@medioAcceso", DbType.String, MedioAcceso);
            SqlParameter paramSSNMA = (SqlParameter)command.CreateParameter();
            paramSSNMA.ParameterName = "@medioAcceso";
            paramSSNMA.DbType = DbType.AnsiStringFixedLength;
            paramSSNMA.Direction = ParameterDirection.Input;
            paramSSNMA.Value = MedioAcceso;
            paramSSNMA.Size = string.IsNullOrEmpty(MedioAcceso)
                                ? 50
                                : MedioAcceso.Length;

            command.Parameters.Add(paramSSNMA);

            database.AddInParameter(command, "@tipoMedioAcceso", DbType.String, TipoMedioAcceso);
            //database.AddInParameter(command, "@tarjeta", DbType.AnsiStringFixedLength, ParameterDirection.Input,null, null, Tarjeta);
            //parametros always encripted
            SqlParameter paramSSN = (SqlParameter)command.CreateParameter();
            paramSSN.ParameterName = "@tarjeta";
            paramSSN.DbType = DbType.AnsiStringFixedLength;
            paramSSN.Direction = ParameterDirection.Input;
            paramSSN.Value = Tarjeta;
            paramSSN.Size = string.IsNullOrEmpty(Tarjeta)
                                ? 50
                                : Tarjeta.Length;

            command.Parameters.Add(paramSSN);

            DataSet DataSet_ = database.ExecuteDataSet(command);

            laTabla = DataSet_.Tables[0];
            for (int k = 0; k < laTabla.Rows.Count; k++)
            {
                Parametro unParamentro = new Parametro();

                unParamentro.Nombre = (laTabla.Rows[k]["clave"]).ToString();
                unParamentro.Valor = (laTabla.Rows[k]["Valor"]).ToString();

                larespuesta.Add(unParamentro.Nombre, unParamentro);

            }

            return larespuesta;
        }
    }
}
