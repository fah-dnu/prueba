using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfases;
using Executer.Entidades;
using Interfases.Entidades;
using System.Linq;
using System.Reflection;
using System.Text;

namespace dataServices_Class.Entidades
{
    class Entidades
    {
    }

    public class Cuentas
    {
        public Int32 ID_Corte { get; set; }
        public Int32 ID_Cuenta { get; set; }
        public Int32 id_CadenaComercial { get; set; }
        public Int32 ID_CuentaHabiente { get; set; }
        public Int32 ID_TipoColectiva { get; set; }
        public DateTime Fecha_Corte { get; set; }
        public string ClaveEventoAgrupador { get; set; }
        public string ClaveCorteTipo { get; set; }

    }

    


    public class EjecutorCadena
    {
        public Int32 ID_Cadena { get; set; }
        public Int32 ID_ConfiguracionCorte { get; set; }
    }

    public class Evento
    {
        public Int64 ID_ConfiguracionCorte { get; set; }

       // public Int64 ID_AgrupadorEvento { get; set; }
        public int ID_AgrupadorEvento { get; set; }
        public String ClaveEventoAgrupador { get; set; }
        public String ClaveEvento { get; set; }
        public int ID_Evento { get; set; }
        public String Descripcion { get; set; }

        public String ClaveCadenaComercial { get; set; }

        public Int64 ID_CadenaComercial { get; set; }

        public Int64 ID_Contrato { get; set; }

        public Int64 Consecutivo { get; set; }
        public String Parametros_Input { get; set; }
        public String Stored_Procedure { get; set; }


       


    }


    //public class Poliza
    //{

    //    public int ID_Operacion { get; set; }
    //    public int ID_Poliza { get; set; }
    //    public int ID_Evento { get; set; }
    //    public Int64 ID_CadenaComercial { get; set; }
    //    public String FechaCreacion { get; set; }
    //    public String Concepto { get; set; }
    //    public String Observaciones { get; set; }
    //    public Int64 ReferenciaNumerica { get; set; }
    //    public float Importe { get; set; }
    //    public List<DetallePoliza> Detalles { get; set; }
    //    public List<Saldo> Saldos { get; set; }
    //    public Boolean isProcesada { get; set; }
    //    public Boolean Cancelacion { get; set; }

    //    public float ImportePremio { get; set; }
    //    //private String _idMensajeISO;

    //    public Poliza()
    //    {
    //        Detalles = new List<DetallePoliza>();
    //        ID_Operacion = 0;
    //        FechaCreacion = "";
    //        Importe = 0;
    //        isProcesada = false;
    //    }



    //    public void addDetalles(DetallePoliza detalle)
    //    {
    //        this.Detalles.Add(detalle);
    //    }

    //}

    //public class DetallePoliza
    //{
    //    public int ID_Colectiva { get; set; }
    //    public int ID_Cuenta { get; set; }
    //    public int ID_TipoCuenta { get; set; }
    //    public int ID_Poliza { get; set; }
    //    public long ID_Polizadetalle { get; set; }
    //    public float Cargo { get; set; }
    //    public float Abono { get; set; }
    //    public ScriptContable Script { get; set; }

    //}

    //public class Saldo
    //{
    //    public String TipoCuenta { get; set; }
    //    public String TipoMonto { get; set; }
    //    public String CodigoMoneda { get; set; }
    //    public String PosicionesDecimales { get; set; }
    //    public String CreditoDebito { get; set; }
    //    public String Monto { get; set; }
    //    public int ID_Colectiva { get; set; }
    //    public int ID_Cuenta { get; set; }


    //    public String toString()
    //    {
    //        StringBuilder saldo = new StringBuilder();

    //        saldo.Append(this.TipoCuenta.PadLeft(2, '0'));
    //        saldo.Append(this.TipoMonto.PadLeft(2, '0'));
    //        saldo.Append(this.CodigoMoneda.PadLeft(3, 'X'));
    //        saldo.Append(this.PosicionesDecimales.PadLeft(1, '0'));
    //        saldo.Append(this.CreditoDebito.PadLeft(1, '0'));
    //        saldo.Append(this.Monto.PadLeft(12, '0'));

    //        return saldo.ToString();
    //    }


    //}

    //public class ScriptContable
    //{
    //    public int ID_TipoColectiva { get; set; }
    //    public int ID_Divisa { get; set; }
    //    public int ID_TipoCuenta { get; set; }
    //    public String Formula { get; set; }
    //    public Boolean esAbono { get; set; }
    //    public Boolean GeneraDetalle { get; set; }
    //    public Boolean ValidaSaldo { get; set; }

    //}

    public class Regla
    {
        public String Nombre { get; set; }
        public StringBuilder CadenaEjecucion { get; set; }
        public String StoredProcedure { get; set; }
        public int OrdenEjecucion { get; set; }
        public Boolean esAccion { get; set; }
        public List<Parametro> Parametros { get; set; }

        /**
         * @return the _cadenaEjecucion
         * Genera la cadena con que se ejecutara el SP
         */
        public String getCadenaEjecucion()
        {

            int i = 0;
            //"{call trx_ObtieneReglasDePertenencia(?)}"
            CadenaEjecucion.Append("{call ");
            CadenaEjecucion.Append(this.Nombre);
            CadenaEjecucion.Append("( ");

            for (i = 0; i < this.Parametros.Count; i++)
            {
                CadenaEjecucion.Append("?,");
            }
            CadenaEjecucion.Append("?,? )}"); //se ponen otros 2 parametro que es el de salida @Respuesta y @Descripcion
            return CadenaEjecucion.ToString();


        }
    }


    public class Respuesta
    {
        public Decimal Saldo { get; set; }
        public String Tarjeta { get; set; }

        public Int64 ID_Poliza { get; set; }
        public String Autorizacion { get; set; }
        public String XmlExtras { get; set; }
        public int CodigoRespuesta { get; set; }
        public String Descripcion { get; set; }

        override
             public string ToString()
        {
            try
            {
                StringBuilder laCadena = new StringBuilder();

                Type type = this.GetType();
                PropertyInfo[] properties = type.GetProperties();

                foreach (PropertyInfo property in properties)
                {
                    // Console.WriteLine("Name: " + property.Name + ", Value: " + property.GetValue(this, null));}

                    String elValor = String.Format("{0}", property.GetValue(this, null));

                    if (elValor.Trim().Length != 0)
                    {
                        laCadena.Append(property.Name);
                        laCadena.Append(":");
                        laCadena.Append(property.GetValue(this, null));
                        laCadena.Append(", ");
                    }

                }

                return laCadena.ToString();
            }
            catch (Exception err)
            {
                return "";
            }
        }


    }





}
