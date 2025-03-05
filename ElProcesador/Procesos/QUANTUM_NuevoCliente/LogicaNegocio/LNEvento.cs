using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interfases.Entidades;
using QUANTUM_NuevoCliente.Entidades;
using QUANTUM_NuevoCliente.BaseDatos;
using CommonProcesador;
using System.Data.SqlClient;

namespace QUANTUM_NuevoCliente.LogicaNegocio
{
   public  class LNEvento
    {
       public static Respuesta EjecuarEvento(String CadenaComercial, String ClaveEvento, String IDCliente, String Tarjeta, Decimal Importe, Parametro[] losParamentros, Int64 RefNumerica, String Concepto, String Observaciones, String elUsuario)
       {
           Respuesta laRespuesta = new Respuesta();
           int Resp = -1;

           try
           {

               Parametro unParametro = new Parametro() { Nombre = "@Importe", Valor = Importe.ToString(), Descripcion = "Importe" };


               Dictionary<String, Parametro> TodosLosParametros = DAOEvento.ListaDeParamentrosContrato(CadenaComercial,Tarjeta, ClaveEvento, elUsuario);

               TodosLosParametros.Add(unParametro.Nombre, unParametro);

               foreach (Parametro elParamentro in losParamentros)
               {
                   TodosLosParametros.Add(elParamentro.Nombre, elParamentro);
               }


               int ID_Evento = TodosLosParametros.ContainsKey("@ID_Evento") ? Int32.Parse(TodosLosParametros["@ID_Evento"].Valor) : 0;
               int ID_Contrato = TodosLosParametros.ContainsKey("@ID_Contrato") ? Int32.Parse(TodosLosParametros["@ID_Contrato"].Valor) : 0;
               //Int64 RefNumerica = TodosLosParametros.ContainsKey("@ReferenciaNumerica") ? Int64.Parse(TodosLosParametros["@ReferenciaNumerica"].Valor) : 0;
               //String Concepto = TodosLosParametros.ContainsKey("@Concepto") ? TodosLosParametros["@Concepto"].Valor : "";
               //String Observaciones = TodosLosParametros.ContainsKey("@Observaciones") ? TodosLosParametros["@Observaciones"].Valor : "SIN OBSERVACIONES";



               if (ID_Contrato == 0 | ID_Evento == 0)
               {
                   throw new Exception("No se pudo obtener un ID_Evento o Contraro valido");
               }

               using (SqlConnection conn = new SqlConnection(BDNuevoCliente.strBDEscritura))
               {
                   conn.Open();

                   using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                   {
                       try
                       {

                           //Genera y Aplica la Poliza
                           Executer.EventoManual aplicador = new Executer.EventoManual(ID_Evento, ID_Contrato, Concepto, false, TodosLosParametros, conn, transaccionSQL, Observaciones, RefNumerica);
                           Resp = aplicador.AplicaContablilidad();

                           //Guardar el Lote
                           if (Resp != 0)
                           {
                               throw new Exception("No se Genero la Póliza del Evento Seleccionado");
                           }


                       }

                       catch (Exception err)
                       {
                           Logueo.Error(err.Message + " NuevoCliente.LNEventos.EjecuarEvento()");
                       }

                       finally
                       {
                           StringBuilder unXML = new StringBuilder();

                           if (Resp == 0)
                           {
                               transaccionSQL.Commit();

                               laRespuesta.Autorizacion = DateTime.Now.ToString("HHmmss");
                               laRespuesta.CodigoRespuesta = 0;
                               laRespuesta.Tarjeta = Tarjeta;

                               unXML.Append(" <Respuesta>");
                               unXML.Append(" <IDCliente>" + IDCliente + "</IDCliente>");
                               unXML.Append(" <Autorizacion>" + laRespuesta.Autorizacion + "</Autorizacion>");
                               unXML.Append(" <Tarjeta>" + Tarjeta + "</Tarjeta>");
                               unXML.Append(" <SaldoActual>" + laRespuesta.Saldo + "</SaldoActual>");
                               unXML.Append(" </Respuesta>");

                               laRespuesta.XmlExtras = unXML.ToString();
                           }
                           else
                           {
                               transaccionSQL.Rollback();

                               laRespuesta.Autorizacion = "000000";
                               laRespuesta.CodigoRespuesta = 92;
                               laRespuesta.Tarjeta = Tarjeta;
                           }


                       }
                   }
               }
           }
           catch (Exception err)
           {
               Logueo.Error(err.Message + " NuevoCliente.LNEventos.EjecuarEvento()");
           }

           return laRespuesta;

       }
 
    }
}
