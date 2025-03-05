using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Executer.Entidades;
using Executer.BaseDatos;
using System.Data.SqlClient;
using Interfases.Exceptions;
//using Executer.Utilidades;
using Interfases.Enums;
using Microsoft.JScript.Vsa;
using Microsoft.JScript;
using Interfases.Entidades;
using QUANTUM_NuevoCliente.BaseDatos;
using CommonProcesador;

namespace Executer.LogicaNegocio
{
    class LNPoliza
    {
        public Poliza generaPoliza(int IDOperacion, float Importe, String Descripcion, String claveEvento, Boolean Cancela, Dictionary<String, Parametro> parametros, SqlConnection connConsulta, SqlTransaction transaccionSQL)
        {

            DAOPoliza datPoliza = new DAOPoliza(connConsulta, transaccionSQL);
            int IDEvento = new DAOEvento(connConsulta, transaccionSQL).getIDEvento(claveEvento);
            DAOUtilerias util = new DAOUtilerias(connConsulta, transaccionSQL);
            Poliza nuevaPoliza = new Poliza();
            VsaEngine engine = VsaEngine.CreateEngine();
            Decimal respEvaluacion;


            try
            {

                List<ScriptContable> scriptsConta = datPoliza.getScriptsContables(IDEvento);

                nuevaPoliza.FechaCreacion = DateTime.Now.ToString();
                nuevaPoliza.ID_Operacion = IDOperacion;
                nuevaPoliza.Importe = (Importe);
                nuevaPoliza.Concepto = (Descripcion);
                nuevaPoliza.ID_Evento = (IDEvento);
                nuevaPoliza.ID_CadenaComercial = Int64.Parse(parametros["@ID_CadenaComercial"].Valor);
                nuevaPoliza.Cancelacion = (Cancela);


                foreach (ScriptContable elScript in scriptsConta)
                {

                    DetallePoliza nuevoDetalle = new DetallePoliza();

                    //Evalua la Formula
                    //Expresion exp = new Expresion(this.reemplazaValoresFormula(parametros, elScript.Formula));

                    object o = Eval.JScriptEvaluate(this.reemplazaValoresFormula(parametros, elScript.Formula), engine);
                    respEvaluacion = System.Convert.ToDecimal(o);


                    //Inicia a meter valores al detalle de la poliza
                    if (elScript.esAbono)
                    {
                        nuevoDetalle.Abono = float.Parse(decimal.Round(respEvaluacion, 2).ToString());// ((float)exp.evaluar(1));
                        nuevoDetalle.Cargo = (0);
                    }
                    else
                    {
                        nuevoDetalle.Cargo = float.Parse(decimal.Round(respEvaluacion, 2).ToString()); //((float)exp.evaluar(1));
                        nuevoDetalle.Abono = (0);
                    }

                    nuevoDetalle.Script = (elScript);
                    nuevoDetalle.ID_TipoCuenta = (elScript.ID_TipoCuenta);
                    //IParametro param= getParametro(elScript.getIdTipoColectiva());
                    Parametro param = getClaveColectivaDeValores(parametros, elScript.ID_TipoColectiva, connConsulta, transaccionSQL);

                    //                nuevoDetalle.setIdColectiva(util.getIDColectiva(ClaveColectiva));
                    //                
                    String ClaveColectiva = param.Valor;

                    if (param.EsClave)
                    {
                        nuevoDetalle.ID_Colectiva = (util.getIDColectiva(ClaveColectiva));
                    }
                    else
                    {
                        nuevoDetalle.ID_Colectiva = (Int32.Parse(param.Valor));
                    }

                    nuevaPoliza.addDetalles(nuevoDetalle);
                }

                //Valida que la sumatoria de los Cargos sea igual que la sumatoria de los abonos.
                validaCargosAbonos(nuevaPoliza);

            }

            catch (SqlException err)
            {
                Logueo.Error("EJECUTOR: generaPoliza():" + err.Message);
                throw err;
            }
            catch (GenericalException err)
            {
                Logueo.Error("EJECUTOR: generaPoliza():" + err.Mensaje);
                throw err;
            }
            catch (Exception err)
            {
                Logueo.Error("EJECUTOR: generaPoliza():" + err.Message);
                throw err;
            }

            return nuevaPoliza;
        }

        public Poliza generaPolizaManual(float Importe, String Descripcion, Int32 IDEvento, Boolean Cancela, Dictionary<String, Parametro> parametros, SqlConnection connConsulta, SqlTransaction transaccionSQL, String Observaciones, Int64 RefNum)
        {

            DAOPoliza datPoliza = new DAOPoliza(connConsulta, transaccionSQL);
            //int IDEvento = new DAOEvento(connConsulta, transaccionSQL).getIDEvento(claveEvento);
            DAOUtilerias util = new DAOUtilerias(connConsulta, transaccionSQL);
            Poliza nuevaPoliza = new Poliza();
            VsaEngine engine = VsaEngine.CreateEngine();
            Decimal respEvaluacion;


            try
            {

                List<ScriptContable> scriptsConta = datPoliza.getScriptsContables(IDEvento);

                nuevaPoliza.FechaCreacion = DateTime.Now.ToString();
                //nuevaPoliza.ID_Operacion = IDOperacion;
                nuevaPoliza.Importe = (Importe);
                nuevaPoliza.Concepto = (Descripcion);
                nuevaPoliza.ID_Evento = (IDEvento);
                nuevaPoliza.ID_CadenaComercial = Int64.Parse(parametros["@ID_CadenaComercial"].Valor);
                nuevaPoliza.Cancelacion = (Cancela);
                nuevaPoliza.ReferenciaNumerica = RefNum;
                nuevaPoliza.Observaciones = Observaciones;


                foreach (ScriptContable elScript in scriptsConta)
                {

                    DetallePoliza nuevoDetalle = new DetallePoliza();

                    //Evalua la Formula
                    //Expresion exp = new Expresion(this.reemplazaValoresFormula(parametros, elScript.Formula));

                    object o = Eval.JScriptEvaluate(this.reemplazaValoresFormula(parametros, elScript.Formula), engine);
                    respEvaluacion = System.Convert.ToDecimal(o);


                    //Inicia a meter valores al detalle de la poliza
                    if (elScript.esAbono)
                    {
                        nuevoDetalle.Abono = float.Parse(decimal.Round(respEvaluacion, 2).ToString());// ((float)exp.evaluar(1));
                        nuevoDetalle.Cargo = (0);
                    }
                    else
                    {
                        nuevoDetalle.Cargo = float.Parse(decimal.Round(respEvaluacion, 2).ToString()); //((float)exp.evaluar(1));
                        nuevoDetalle.Abono = (0);
                    }

                    nuevoDetalle.Script = (elScript);
                    nuevoDetalle.ID_TipoCuenta = (elScript.ID_TipoCuenta);
                    //IParametro param= getParametro(elScript.getIdTipoColectiva());
                    Parametro param = getClaveColectivaDeValores(parametros, elScript.ID_TipoColectiva, connConsulta, transaccionSQL);

                    //                nuevoDetalle.setIdColectiva(util.getIDColectiva(ClaveColectiva));
                    //                
                    String ClaveColectiva = param.Valor;

                    if (param.EsClave)
                    {
                        nuevoDetalle.ID_Colectiva = (util.getIDColectiva(ClaveColectiva));
                    }
                    else
                    {
                        nuevoDetalle.ID_Colectiva = (Int32.Parse(param.Valor));
                    }

                    nuevaPoliza.addDetalles(nuevoDetalle);
                }

                //Valida que la sumatoria de los Cargos sea igual que la sumatoria de los abonos.
                validaCargosAbonos(nuevaPoliza);

            }

            catch (SqlException err)
            {
                Logueo.Error("EJECUTOR: generaPoliza():" + err.Message);
                throw err;
            }
            catch (GenericalException err)
            {
                Logueo.Error("EJECUTOR: generaPoliza():" + err.Mensaje);
                throw err;
            }
            catch (Exception err)
            {
                Logueo.Error("EJECUTOR: generaPoliza():" + err.Message);
                throw err;
            }

            return nuevaPoliza;
        }

        private Boolean validaCargosAbonos(Poliza laPoliza)
        {
            List<DetallePoliza> losDetalles = laPoliza.Detalles;
            float cargos = 0, abonos = 0;


            foreach (DetallePoliza dp in losDetalles)
            {
                cargos = cargos + dp.Cargo;
                abonos = abonos + dp.Abono;
            }
            if (cargos != abonos)
            {
                throw new GenericalException(CodRespuesta03.DIFERENCIAS_EN_CARGOS_Y_ABONOS, "LA SUMA DE LOS CARGOS Y ABONOS NO ES IGUAL, FAVOR REVISAR CONFIGURACION DE SCRIPTS CONTABLES DEL EVENTO");
            }

            //validamos que la poliza no se este generando en ceros
            if ((cargos == abonos) && (cargos == 0))
            {
                throw new GenericalException(CodRespuesta03.DIFERENCIAS_EN_CARGOS_Y_ABONOS, "El Total de Cargos y de los Abonos es igual a cero, favor de verificar la configuracion del Script contable");
            }

            return true;
        }

        private String reemplazaValoresFormula(Dictionary<String, Parametro> param, String formula)
        {

            //String[] tokens = formula.Split('*','+','-','%','/','(',')');//new StringTokenizer(formula,"*,+,-,%,/,(,)",true); //La expresión partida en tokens

            string seps = @"(\%)|(\+)|(-)|(\*)|(/)|(\()|(\))";
            string[] tokens = System.Text.RegularExpressions.Regex.Split(formula, seps);

            //String tokenActual; //El token que se procesa actualmente
            String formulaEvaluada = "";

            if (param.Count == 0)
            {
                Logueo.Error("EJECUTOR: No existen parametros para Reemplazar en la formula");
                throw new GenericalException(CodRespuesta03.NO_HAY_PARAMETROS, "No existen parametros para Reemplazar en la formula");
            }

            foreach (String tokenActual in tokens)
            {
                //float result;
                //float.TryParse(tokenActual, out result);

                if (param.ContainsKey(tokenActual))
                {
                    Parametro valorParametro = (Parametro)param[tokenActual];

                    if (valorParametro != null)
                    {
                        if (tokenActual == valorParametro.Nombre)
                        {
                            formulaEvaluada = formulaEvaluada + valorParametro.Valor;
                            //tokenActual="";
                        }
                    }
                }
                else
                {
                    formulaEvaluada = formulaEvaluada + tokenActual.ToString();
                }
            }
            return formulaEvaluada;
        }


        public Parametro getClaveColectivaDeValores(Dictionary<String, Parametro> parametros, int IdTipoColectiva, SqlConnection conn, SqlTransaction transaccionSQL)
        {
            //Collection<Parametro> params = parametros.values();

            //Iterator it = params.iterator();
            DAOUtilerias util = new DAOUtilerias(conn, transaccionSQL);

            //while (it.hasNext()) {
            //    Parametro unParametro = (Parametro) it.next();
            foreach (Parametro unParametro in parametros.Values)
            {

                if (unParametro.ClaveTipoColectiva != null)
                {
                    if (util.getIDTipoColectiva(unParametro.ClaveTipoColectiva) == IdTipoColectiva)
                    {
                        if ((unParametro.Valor == null) || (unParametro.Valor == "null") || (unParametro.Valor.Trim() == ""))
                        {
                            throw new GenericalException(CodRespuesta03.MAL_CONFIGURACION_SCRIPTS, "EJECUTOR: El Parametro con el ID_TipoColectiva: " + IdTipoColectiva + " regreso valor Nulo.");
                        }
                        return unParametro;
                    }

                }
                else
                {
                    if (unParametro.ID_TipoColectiva == IdTipoColectiva)
                    {
                        return unParametro;
                    }
                }
            }

            throw new GenericalException(CodRespuesta03.MAL_CONFIGURACION_SCRIPTS, "EJECUTOR: No hay Parametro con el ID_TipoColectiva Requerida en la Generacion de la Poliza: " + IdTipoColectiva);


        }


    }
}
