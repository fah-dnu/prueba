using CommonProcesador;
using DNU_ParabiliumProcesoMSI.Modelos.Clases;
using DNU_ParabiliumProcesoMSI.Servicios.SQLServer;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliumProcesoMSI.CapaDeDatos
{
    class DAOOperaciones
    {
        private ConexionBD consultaBDRead;
        private ConexionBD consultaBDWrite;
        private DataTable dtResultado;
        public string nuevaPoliza { get; set; }
        public DAOOperaciones(String cadenaconexion)
        {

            consultaBDRead = new ConexionBD(cadenaconexion);
            consultaBDWrite = new ConexionBD(cadenaconexion);

        }

        public DataTable ObtieneOperacionesADiferir(SqlConnection conn)
        {
            List<Operaciones> laRespuesta = new List<Operaciones>();
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
          
                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_MSI_ObtieneOperacionesPorDiferir]", parametros, "Procnoc_MSI_ObtieneOperacionesPorDiferir", "", conn);
               // return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[ProcesoMSI]error al obtener corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
            return dtResultado;
        }

        public DataTable ObtieneMesesPorOperacion(SqlConnection conn, Dictionary<String, Parametro> diccionarioDatos)
        {
            List<Operaciones> laRespuesta = new List<Operaciones>();
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                consultaBDRead.verificarParametrosNulosString(parametros, "@meses", diccionarioDatos["@Meses"].Valor);
                consultaBDRead.verificarParametrosNulosString(parametros, "@importe", diccionarioDatos["@ImporteOperacion"].Valor);
                consultaBDRead.verificarParametrosNulosString(parametros, "@fechaCorte", diccionarioDatos["@FechaCorte"].Valor);
                consultaBDRead.verificarParametrosNulosString(parametros, "@fechaCompra", diccionarioDatos["@FechaCompra"].Valor);
                if (diccionarioDatos.ContainsKey("@MSI_FechaValor"))
                {
                    consultaBDRead.verificarParametrosNulosString(parametros, "@msiFechaValor", diccionarioDatos["@MSI_FechaValor"].Valor);
                }
                consultaBDRead.verificarParametrosNulosString(parametros, "@clavePromocion", diccionarioDatos["@ClavePromocion"].Valor);
                consultaBDRead.verificarParametrosNulosString(parametros, "@diferimiento", diccionarioDatos["@Diferimiento"].Valor);
                consultaBDRead.verificarParametrosNulosString(parametros, "@tasaInteresMSI", diccionarioDatos["@TasaInteresMSI"].Valor);
                consultaBDRead.verificarParametrosNulosString(parametros, "@iva", diccionarioDatos["@IVA"].Valor);
                consultaBDRead.verificarParametrosNulosString(parametros, "@idProducto", diccionarioDatos["@IdProducto"].Valor);



                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_MSI_ObtieneMesesPorOperacion]", parametros, "Procnoc_MSI_ObtieneOperacionesPorDiferir", "", conn);
                // return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[ProcesoMSI]error al obtener corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
            return dtResultado;
        }

        public DataTable ActualizaFechaPoliza(string idPoliza,DateTime fecha,string idOperacion,bool ultimoMes ,SqlConnection conn, SqlTransaction transaccionSQL,decimal tasaTransaccion)
        {
            List<Operaciones> laRespuesta = new List<Operaciones>();
           
            try
            {
                string formattedDate = fecha.ToString("yyyy-MM-dd HH:mm:ss");
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                consultaBDRead.verificarParametrosNulosString(parametros, "@idPoliza", idPoliza);
                consultaBDRead.verificarParametrosNulosString(parametros, "@fecha", formattedDate);
                consultaBDRead.verificarParametrosNulosString(parametros, "@idOperacion", idOperacion);
                consultaBDRead.verificarParametrosNulosString(parametros, "@ultimoMes", ultimoMes?"1":"0");
                consultaBDRead.verificarParametrosNulosString(parametros, "@tasaDiferimiento", tasaTransaccion.ToString());
                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_MSI_ActualizarFechaPoliza]", parametros, "Procnoc_MSI_ObtieneOperacionesPorDiferir", "", conn,transaccionSQL);
                // return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[ProcesoMSI] error al obtener corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
            return dtResultado;
        }

        public DataTable ReversaPoliza(string id_Poliza, string idRegistro, string pUserTemp, SqlConnection conn = null, SqlTransaction transaccionSQL = null, Guid? idLog = null, string nuevaPoliza = null)
        {
            nuevaPoliza = "0";
            try
            {
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                List<ParametrosSalidaProcedimiento> parametrosSalida = new List<ParametrosSalidaProcedimiento>();
                parametros.Add(new ParametrosProcedimiento { Nombre = "@ID_Poliza", parametro = id_Poliza });
                parametrosSalida.Add(new ParametrosSalidaProcedimiento { Nombre = "@IDNewPoliza", parametro = "0", tipo = SqlDbType.BigInt });
                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_MSI_ReversaPoliza]", parametros, "Procnoc_MSI_ReversaPoliza", "", conn, transaccionSQL, "", parametrosSalida);
                foreach (ParametrosSalidaProcedimiento parametro in parametrosSalida)
                {
                    if (parametro.Nombre == "@IDNewPoliza")
                    {
                        nuevaPoliza = parametro.parametro.ToString();
                    }
                }
                if (dtResultado is null)
                {
                    DataTable dt = new DataTable();
                    dt.Clear();
                    dt.Columns.Add("tipo");
                    DataRow _nuevaTabla = dt.NewRow();
                    _nuevaTabla["tipo"] = nuevaPoliza;
                    dt.Rows.Add(_nuevaTabla);
                    return dt;
                }
                return dtResultado;
            }
            catch (Exception ex)
            {
                return dtResultado;
            }
        }

        public DataTable ActualizaPolizaOperacion(string idPoliza,  string idOperacion,string idPolizaCancelada, SqlConnection conn, SqlTransaction transaccionSQL)
        {
            List<Operaciones> laRespuesta = new List<Operaciones>();

            try
            {
               
                List<ParametrosProcedimiento> parametros = new List<ParametrosProcedimiento>();
                consultaBDRead.verificarParametrosNulosString(parametros, "@idPoliza", idPoliza);
                consultaBDRead.verificarParametrosNulosString(parametros, "@idOperacion", idOperacion);
                consultaBDRead.verificarParametrosNulosString(parametros, "@idPolizaCancelada", idPolizaCancelada);

                dtResultado = consultaBDRead.ConstruirConsulta("[Procnoc_MSI_ActualizarPolizaOperacion]", parametros, "Procnoc_MSI_ObtieneOperacionesPorDiferir", "", conn, transaccionSQL);
                // return dtResultado;
            }
            catch (Exception ex)
            {
                Logueo.Error("[ProcesoMSI] error al obtener corte:" + ex.Message + ex.StackTrace);
                return dtResultado;
            }
            return dtResultado;
        }
    }
}
