using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImportarEmpleados.Entidades;
using ImportarEmpleados.BaseDatos;
using ImportarEmpleados.Utilidades;
using CommonProcesador;
using CommonProcesador.Utilidades;
using System.IO;
using System.Configuration;

namespace ImportarEmpleados.LogicaNegocio
{
    public class LNArchivo
    {
        private static List<Archivo> ObtenerArchivosPorProcesar()
        {
            try
            {
                List<Archivo> losArchivos = new List<Archivo>();

                losArchivos = DAOArchivo.ListaArchivos();

                Logueo.Error("Se Procesarán " + losArchivos.Count + " Archivos.");

                foreach (Archivo unArchivo in losArchivos)
                {
                   
                    unArchivo.Empleados = DAOEmpleado.ObtieneEmpleadosPorProcesar(unArchivo.ID_Archivo);
                     Logueo.Error("Archivo: " + unArchivo.Nombre + ", con " +   unArchivo.Empleados.Count +" Empleados");
                }

                
                
                return losArchivos;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);

                
            }
        }

        public static int ProcesarArchivosPendientes()
        {
            try
            {
                int respuesta = 1;

                foreach (Archivo unArchivo in ObtenerArchivosPorProcesar())
                {
                    int Procesadas = 0;
                    int NoProcesada = 0;

                    Logueo.Evento("Inicia Proceso de Archivo:" + unArchivo.Nombre);

                    foreach (Empleado unEmpleado in unArchivo.Empleados)
                    {
                        //int Resultado = -1;
                        switch (unEmpleado.ID_Estatus)
                        {
                            case 1://Sin Procesar
                            case 8://Error en Crear Colectiva

                                respuesta = LNEmpleado.CrearColectiva(unEmpleado);

                                if (respuesta == 0)
                                    respuesta = LNEmpleado.CrearCuentas(unEmpleado);

                                if (respuesta == 0)
                                    respuesta = LNEmpleado.CrearDepositoInicial(unEmpleado);

                                if (respuesta == 0)
                                    respuesta = LNEmpleado.CrearEnClubEscala(unEmpleado);

                                break;
                            case 2://Colectiva Creada
                            case 9://Error al Crear las Cuentas

                                respuesta = LNEmpleado.CrearCuentas(unEmpleado);

                                if (respuesta == 0)
                                    respuesta = LNEmpleado.CrearDepositoInicial(unEmpleado);

                                if (respuesta == 0)
                                    respuesta = LNEmpleado.CrearEnClubEscala(unEmpleado);
                                break;
                            case 3://Cuentas Creadas
                            case 10://Error al Abonar
                                respuesta = LNEmpleado.CrearDepositoInicial(unEmpleado);

                                if (respuesta == 0)
                                    respuesta = LNEmpleado.CrearEnClubEscala(unEmpleado);
                                break;
                            case 4://Abono Realizado
                            case 11://Error al Crear en Club Escala
                                respuesta = LNEmpleado.CrearEnClubEscala(unEmpleado);
                                break;
                            case 5://Creado en Autorizador sin Abono.


                                break;

                        }
                       // return respuesta;

                        if (respuesta == 0)
                        {
                            Procesadas++;
                        }
                        else
                        {
                            NoProcesada++;
                        }
                    }

                     Logueo.Evento("Termina Proceso de Archivo:" + unArchivo.Nombre);
                    //Enviar Email de resultado  Archivo Seleccionado
                    Logueo.Evento("Se enviará correo de Resultados del Archivo:" + unArchivo.Nombre);
                    LNArchivo.EnviarEmail(unArchivo.ID_Archivo);

                    Logueo.Evento("Se envió correo de Resultados del Archivo:" + unArchivo.Nombre);
                    //poner el archivo como procesado parcialmente.
                    if (NoProcesada > 0)
                    {
                        DAOArchivo.Actualiza(unArchivo.ID_Archivo, EstatusArchivo.ParcialmenteProcesado);
                    }
                    else
                    {
                        DAOArchivo.Actualiza(unArchivo.ID_Archivo, EstatusArchivo.Procesado);
                    }
                }
                return respuesta;
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception(ex.Message);
            }
        }


        public static void EnviarEmail(long ID_Archivo)
        {

            try
            {
                StringBuilder emailHtml = new StringBuilder(File.ReadAllText(ConfigurationManager.AppSettings["HtmlActivaEmpleados"].ToString()));

                ResultadoProceso laRespuesta = DAOArchivo.ObtieneResultado(ID_Archivo);

                emailHtml.Replace("[totRegistros]", laRespuesta.totRegistros);
                emailHtml.Replace("[TotEmpleados]", laRespuesta.TotEmpleados);
                emailHtml.Replace("[NombreArchivo]", laRespuesta.NombreArchivo);
                emailHtml.Replace("[NOMBREUSUARIO]", laRespuesta.NOMBREUSUARIO);
                emailHtml.Replace("[TotsinProcesar]", laRespuesta.TotsinProcesar);
                emailHtml.Replace("[TotEmpleadoCreados]", laRespuesta.TotEmpleadoCreados);
                emailHtml.Replace("[TotEmpleadosCuenta]", laRespuesta.TotEmpleadosCuenta);
                emailHtml.Replace("[TotEmpleadosDeposito]", laRespuesta.TotEmpleadosDeposito);
                emailHtml.Replace("[TotEmpleadosProcesados]", laRespuesta.TotEmpleadosProcesados);
                emailHtml.Replace("[TotEmpleadosErrorCrear]", laRespuesta.TotEmpleadosErrorCrear);
                emailHtml.Replace("[TotEmpleadosErrorCta]", laRespuesta.TotEmpleadosErrorCta);
                emailHtml.Replace("[TotEmpleadosErrorDeposito]", laRespuesta.TotEmpleadosErrorDeposito);
                emailHtml.Replace("[TotEmpleadosErrorClub]", laRespuesta.TotEmpleadosErrorClub);




                LNEmail.Send(laRespuesta.email, "info@dnu.mx", emailHtml.ToString(), "Resultado Procesador Nocturno: " + laRespuesta.NombreArchivo);
            }
            catch (Exception err)
            {
                Logueo.Error("Error al Enviar correo:" + err.Message);
                throw err;
            }
        }
    }
}
