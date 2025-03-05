using DNU_ParabiliaProcesoCobranza.Entidades;
using Interfases.Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCobranza.LogicaNegocio
{
    public class LNValidacionesCampos
    {
        public List<Object> validarCamposObligatorios(List<Object> objetoAValidar, List<string> nombresCampos, Response respuestaError)
        {
            //modificar en caso de que se desee agregar el campo nulo a mostrar, por eso se dejo el list
            List<Object> resultadoValidacion = new List<Object>();
            for (int i = 0; i < objetoAValidar.Count; i++)
            {
                if (objetoAValidar[i] == null)
                {
                    Object objeto = objetoAValidar[i];
                    resultadoValidacion.Add((bool)false);
                    respuestaError.CodRespuesta = "1055";
                    respuestaError.DescRespuesta = "El valor del parámetro " + nombresCampos[i] + " es incorrecto";
                    return resultadoValidacion;
                }
            }
            resultadoValidacion.Add((bool)true);
            return resultadoValidacion;
        }


        public bool validarEnterosObligatorios(string numero)
        {
            bool regresoNumero;
            try
            {
                int convertiEntero = int.Parse(numero);
                regresoNumero = true;
            }
            catch (Exception ex)
            {
                regresoNumero = false;
            }
            return regresoNumero;
        }
        public int validarEnteros(string valor)
        {
            int entero = 0;
            if (string.IsNullOrEmpty(valor))
                return 0;
            if (string.IsNullOrWhiteSpace(valor))
                return 0;
            try
            {
                entero = int.Parse(valor);
            }
            catch (Exception ex)
            {
                entero = 0;
            }
            return entero;

        }
        public long validarLong(string valor)
        {
            long valorLong = 0;
            if (string.IsNullOrEmpty(valor))
                return 0;
            if (string.IsNullOrWhiteSpace(valor))
                return 0;
            try
            {
                valorLong = Convert.ToInt64(valor);
            }
            catch (Exception ex)
            {
                valorLong = 0;
            }
            return valorLong;
        }

        public float validarFlotantes(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return 0;
            if (string.IsNullOrWhiteSpace(valor))
                return 0;

            return float.Parse(valor);
        }
        public double validarDoubles(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return 0;
            if (string.IsNullOrWhiteSpace(valor))
                return 0;

            return double.Parse(valor);
        }

        public decimal validarDecimales(string valor)
        {
            Decimal valorDecimal = 0;
            if (string.IsNullOrEmpty(valor))
                return 0;
            if (string.IsNullOrWhiteSpace(valor))
                return 0;
            try
            {
                valorDecimal = Convert.ToDecimal(valor);
            }
            catch (Exception ex)
            {
                valorDecimal = 0;
            }
            return valorDecimal;
        }

        public bool validarBoleanos(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return false;
            if (string.IsNullOrWhiteSpace(valor))
                return false;

            return bool.Parse(valor);

        }
        public int ConvertirBooleanos(bool valor)
        {
            var a = valor == true ? 1 : 0;

            return a;

        }
        public string validarCadenasNulas(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return string.Empty;
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            return valor.ToString();

        }
        public string validarObjetos(object valor)
        {
            string valorConvertido = "";
            try
            {
                valorConvertido = valor.ToString();
            }
            catch (Exception ex)
            {
                valorConvertido = "";
            }
            return valorConvertido;

        }


        public DateTime validarFecha(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return DateTime.Today;
            if (string.IsNullOrWhiteSpace(valor))
                return DateTime.Today;


            try
            {
                return Convert.ToDateTime(valor);
            }
            catch (Exception ex)
            {
                return DateTime.Today;
            }

        }
        public bool BusquedaSinErrores(Response errores, DataTable tabla, string idLog = null)
        {
            bool retornoBooleano = true;
            if (tabla.Columns.Count == 2 || tabla.Columns.Count == 3 || tabla.Columns.Count == 4)
            {
                if (tabla.Columns.Count == 2 && (tabla.Rows[0][0].ToString() == "error" || tabla.Rows[0][0].ToString() == "errorExcepcion"))
                {
                    if (tabla.Rows[0][0].ToString() == "error")
                    {
                        errores.CodRespuesta = "9999";
                        errores.DescRespuesta = validarCadenasNulas(tabla.Rows[0][1].ToString());
                        //   Logueo.Error(JsonConvert.SerializeObject(errores) + (string.IsNullOrEmpty(idLog) ? "" : " [" + idLog + "]"));
                    }
                    else
                    {
                        errores.CodRespuesta = "9999";
                        errores.DescRespuesta = "Error en base de datos";
                        //     Logueo.Error(JsonConvert.SerializeObject(errores) + " Exception:" + tabla.Rows[0][1].ToString() + (string.IsNullOrEmpty(idLog) ? "" : " [" + idLog + "]"));
                    }
                    retornoBooleano = false;
                }
                else
                {
                    if (tabla.Rows[0][0].ToString() == "error" || tabla.Rows[0][0].ToString() == "errorExcepcion")
                    {
                        try
                        {
                            errores.CodRespuesta = validarCadenasNulas(tabla.Rows[0]["Codigo"].ToString());
                        }
                        catch (Exception ex)
                        {
                            errores.CodRespuesta = "9999";
                        }
                        errores.DescRespuesta = validarCadenasNulas(tabla.Rows[0][1].ToString());
                        //  Logueo.Error(JsonConvert.SerializeObject(errores) + (string.IsNullOrEmpty(idLog) ? "" : " [" + idLog + "]"));
                        retornoBooleano = false;
                    }

                }
            }
            return retornoBooleano;
        }

        public bool validarImporte(Decimal importe, Response errores)
        {
            bool respuesaBooleano = true;
            if (importe <= 0)
            {
                errores.CodRespuesta = "1060";
                errores.DescRespuesta = "El importe no puede ser menor o igual a 0";
                respuesaBooleano = false;
            }
            return respuesaBooleano;
        }

        public string getUserTemp(string pToken)
        {
            var cadenaB64 = pToken.StartsWith("Bearer ") ? pToken.Substring(7) : pToken;
            var cadenaFromB64ToString = Encoding.UTF8.GetString(Convert.FromBase64String(cadenaB64));

            string[] divCadenas = cadenaFromB64ToString.Split('|');

            return divCadenas[1];
        }
        public bool validaParametroDiccionario(Dictionary<String, Parametro> diccionario, string parametro)
        {
            Parametro valor;
            if (diccionario.TryGetValue(parametro, out valor))
            {
                // the key isn't in the dictionary.
                return true; // or whatever you want to do
            }
            return false;
        }
    }
}
