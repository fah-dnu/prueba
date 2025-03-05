using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ImportarEmpleados.Entidades;
using Framework;
using System.Diagnostics;
using ImportarEmpleados.Utilidades;
using ImportarEmpleados.BaseDatos;
using CommonProcesador;

namespace ImportarEmpleados.LogicaNegocio
{
    public class LNEmpleado
    {
        public static int CrearEnClubEscala(Empleado elEmpleado)
        {

            try
            {
                Cliente objetoCliente = Cliente.Get("");
                string emailLogIn;

                if (elEmpleado.Baja.Equals("1")) // si es una operacion de baja solo inactiva el usuario en la DB
                {
                    //TODO: Inactivar el Operador en ClubEscala
                    try
                    {
                        Cliente.ActualizaEstatus(elEmpleado.EmailPersonal, 2);
                    }
                    catch (Exception err)
                    {
                        try
                        {
                           Cliente.ActualizaEstatus(elEmpleado.EmailPersonal, 2);
                        }
                        catch (Exception err2)
                        {
                            throw new Exception("No se pudo eliminar el usuario de Club Escala: " + elEmpleado.NumeroEmpleado + " " + elEmpleado.APaterno);
                        }
                    }
                    return 0;
                }

                //validar que venga un email, se privilegia el Email personal
                if (elEmpleado.EmailPersonal == null || elEmpleado.EmailPersonal.Length == 0)
                {
                    if (elEmpleado.EmailEmpresarial != null || elEmpleado.EmailEmpresarial.Length != 0)
                    {
                        if (Cliente.EstaEmailEnBaseDatos(elEmpleado.EmailEmpresarial))
                        {
                            throw new Exception("El Email " + elEmpleado.EmailEmpresarial + "  Ya se encuentra en la BD");
                        }
                        else
                        {
                            emailLogIn = elEmpleado.EmailEmpresarial;
                        }
                    }
                    else
                    {
                        throw new Exception("Es Necesario un Email para agregar el Usuario en Club Escala");
                    }
                }
                else
                {
                    if (Cliente.EstaEmailEnBaseDatos(elEmpleado.EmailPersonal))
                    {
                        throw new Exception("El Email " + elEmpleado.EmailPersonal + " Ya se encuentra en la BD");
                    }
                    else
                    {
                        emailLogIn = elEmpleado.EmailPersonal;
                    }
                }

                if (elEmpleado.TelefonoMovil != null || elEmpleado.TelefonoMovil.Length == 0)
                {
                    if (Cliente.EstaTelefonoEnBaseDatos(elEmpleado.TelefonoMovil))
                    {
                        throw new Exception("El Telefono Movil " + elEmpleado.TelefonoMovil + " Ya se encuentra en la BD");
                    }
                }
                else
                {
                    throw new Exception("Es Necesario un Telefono para agregar el Usuario en Club Escala");
                }




                Cliente.TipoSexo elSexo = new Cliente.TipoSexo();

                String confirmacionSitio = Guid.NewGuid().ToString().ToUpper();
                String ConfirmacionCel = getNumero();

                objetoCliente.Add(Framework.ContextoInicial.adminServicio,
                    ContextoInicial.ContextoServicio,
                    emailLogIn,
                    ConfirmacionCel,
                    elEmpleado.Nombre,
                    elEmpleado.APaterno + " " + elEmpleado.AMaterno,
                    elEmpleado.FechaNacimiento.Year.ToString(),
                    Convert.ToInt32(elEmpleado.FechaNacimiento.Month),
                    Convert.ToInt32(elEmpleado.FechaNacimiento.Day),
                    elEmpleado.TelefonoMovil,
                     elSexo, true,
                    Cliente.ReferenciaCliente.Servicio, false, confirmacionSitio, ConfirmacionCel, "", "");

                Framework.Log.EscribirEventoProceso("Error 5152. El usuario fue creado.",
                    5310,
                    "CreateUser.btnAdd_Click",
                    EventLogEntryType.Information,
                    emailLogIn);
                Logueo.Evento("Se Creo el Usuario en Club Escala :" + emailLogIn);

                //genera el correo electronico de Bienvenida
                GeneraMailConfirmacion(confirmacionSitio, objetoCliente);

                //cambiamos el Estatus del cliente, para que la proxima vez que se registre solicitte cambio de password.
                Cliente.ActualizaEstatus(emailLogIn, 2);

                //Agregar medios de pago default
                agregarMedioPagoDefault(objetoCliente, elEmpleado.EmailPersonal, (string)Mensajeria.Configuracion.GetConfiguracion("DescripcionCredito"), (string)Mensajeria.Configuracion.GetConfiguracion("TipoCuentaCredito"));
                //agregarMedioPagoDefault(objetoCliente, elEmpleado.EmailPersonal, (string)Mensajeria.Configuracion.GetConfiguracion("DescripcionMonedero"), (string)Mensajeria.Configuracion.GetConfiguracion("TipoCuentaMonedero"));



                Logueo.Evento("Envio de Correo de confirmacion ");
                Framework.Log.EscribirEventoProceso("Se Envio Correo con clave de activacion",
                5310,
                "CreateUser.btnAccept_Click",
                EventLogEntryType.Information,
                emailLogIn);


                DAOEmpleado.Actualiza(elEmpleado, EstatusEmpleado.Procesada, elEmpleado.ID_Detalle);

                return 0;

            }
            catch (Exception err)
            {
                Logueo.Error("ProcesarEnAutorizador() Empleado: " + elEmpleado.NumeroEmpleado + " " + elEmpleado.Nombre + " " + elEmpleado.APaterno + ": " + err.Message);
                DAOEmpleado.Actualiza(elEmpleado, EstatusEmpleado.ErrorAlCrearClubEscala, elEmpleado.ID_Detalle);
                return -1;
            }

        }

        public static int CrearCuentas(Empleado elEmpleado)
        {
            try
            {
                if (elEmpleado.Baja == "1")
                {
                    return 0;
                }

                if (elEmpleado.TelefonoMovil.Trim().Length == 0 || (elEmpleado.EmailEmpresarial.Trim().Length == 0 && elEmpleado.EmailPersonal.Trim().Length == 0))
                {
                    Logueo.Error("Empleado: " + elEmpleado.NumeroEmpleado + " " + elEmpleado.Nombre + " " + elEmpleado.APaterno + " -Para Crear las Cuentas es necesario que un Email y un Telefono para asignarle a los medios de acceso");
                    return -1;
                }
               
                String Evento = Config.Get(elEmpleado.ID_CadenaComercial, "WsEvento");

                ws_CEscala.WS_Administracion webService = new ws_CEscala.WS_Administracion();

                int resp = webService.CrearCuentasaEmpleadoClubEscala(Config.Get(elEmpleado.ID_CadenaComercial, "wsUser"), Config.Get(elEmpleado.ID_CadenaComercial, "WsPass"),
                    Evento, Config.Get(elEmpleado.ID_CadenaComercial, "WsCveTipoCol"), "", elEmpleado.NumeroEmpleado, 0, elEmpleado.Nombre, elEmpleado.APaterno, 
                    elEmpleado.AMaterno,elEmpleado.FechaNacimiento.ToString("yyyy-MM-dd"), elEmpleado.TelefonoMovil, elEmpleado.EmailPersonal == "" ? elEmpleado.EmailEmpresarial : elEmpleado.EmailPersonal,
                    Config.Get(elEmpleado.ID_CadenaComercial, "wsCveGpoMA"), getNumero(), int.Parse(Config.Get(elEmpleado.ID_CadenaComercial, "wsAniosExpira")),
                    Config.Get(elEmpleado.ID_CadenaComercial, "wsCveGpoCta"), int.Parse(elEmpleado.ID_CadenaComercial.ToString()),
                    Config.Get(elEmpleado.ID_CadenaComercial, "wsCveTipoCta"), elEmpleado.LimiteCompra.ToString(), elEmpleado.Baja,
                    elEmpleado.CicloNominal, elEmpleado.DiaPago);

                if (resp == 0)
                {
                    DAOEmpleado.Actualiza(elEmpleado, EstatusEmpleado.CuentaCreada, elEmpleado.ID_Detalle);
                }
                else
                {
                    DAOEmpleado.Actualiza(elEmpleado, EstatusEmpleado.ColectivaCreada, elEmpleado.ID_Detalle);
                }

              
                return resp;
            }
            catch (Exception err)
            {
                DAOEmpleado.Actualiza(elEmpleado, EstatusEmpleado.ErrorAlCrearCuentas, elEmpleado.ID_Detalle);
                Logueo.Error("ProcesarEnAutorizador() Empleado: " + elEmpleado.NumeroEmpleado + " " + elEmpleado.Nombre + " " + elEmpleado.APaterno + ": " + err.Message);
                return -1;
            }
        }

        public static int CrearColectiva(Empleado elEmpleado)
        {
            try
            {
                String Evento = Config.Get(elEmpleado.ID_CadenaComercial, "WsEvento");

             

                if (elEmpleado.Nombre.Trim().Length == 0 || elEmpleado.NumeroEmpleado.Trim().Length == 0 || elEmpleado.APaterno.Trim().Length == 0)
                {
                    Logueo.Error("Empleado: " + elEmpleado.NumeroEmpleado + " " + elEmpleado.Nombre + " " + elEmpleado.APaterno + " -No se Puede crear la Colectiva con los datos proporcionados");
                    return -1;
                }

                ws_CEscala.WS_Administracion webService = new ws_CEscala.WS_Administracion();

                int resp = webService.CrearEmpleadoClubEscala(Config.Get(elEmpleado.ID_CadenaComercial, "wsUser"), Config.Get(elEmpleado.ID_CadenaComercial, "WsPass"),
                    Evento, Config.Get(elEmpleado.ID_CadenaComercial, "WsCveTipoCol"), "", elEmpleado.NumeroEmpleado, elEmpleado.Nombre, elEmpleado.APaterno,
                    elEmpleado.AMaterno, elEmpleado.TelefonoMovil, elEmpleado.EmailPersonal == "" ? elEmpleado.EmailEmpresarial : elEmpleado.EmailPersonal,
                     int.Parse(elEmpleado.ID_CadenaComercial.ToString()), elEmpleado.Baja);

               // DAOEmpleado.Actualiza(elEmpleado, EstatusEmpleado.CreadoAutorizadorSinAbono, elEmpleado.ID_Detalle);
                if (resp == 0)
                {
                    DAOEmpleado.Actualiza(elEmpleado, EstatusEmpleado.ColectivaCreada, elEmpleado.ID_Detalle);
                }
                else
                {
                    DAOEmpleado.Actualiza(elEmpleado, EstatusEmpleado.SinProcesar, elEmpleado.ID_Detalle);
                }

                return resp;
            }
            catch (Exception err)
            {
                DAOEmpleado.Actualiza(elEmpleado, EstatusEmpleado.ErrorAlCrearColectiva, elEmpleado.ID_Detalle);
                Logueo.Error("ProcesarEnAutorizador() Empleado: " + elEmpleado.NumeroEmpleado + " " + elEmpleado.Nombre + " " + elEmpleado.APaterno + ": " + err.Message);
                return -1;
            }
        }
        
        private static string getNumero()
        {
            Random r = new Random(DateTime.Now.Millisecond);
            int num = r.Next(0, 9999);

            return num.ToString().PadLeft(6, '0');

        }

        private static void GeneraMailConfirmacion(String CveConfirmacion, Cliente clienteNuevo)
        {

            StringBuilder strMessage = new StringBuilder();
            try
            {

                //Cliente.TipoSexo sexo= ( this.RadioButtonFemale.Checked ) ? Cliente.TipoSexo.Femenino : Cliente.TipoSexo.Masculino;
                if (clienteNuevo.Sexo == Cliente.TipoSexo.Femenino)
                {
                    strMessage.AppendFormat("Estimada ");
                }
                else
                {
                    strMessage.AppendFormat("Estimado ");
                }

                strMessage.AppendFormat(clienteNuevo.Nombre);
                strMessage.AppendFormat(" ");
                strMessage.AppendFormat(clienteNuevo.Apellido);
                strMessage.AppendFormat(": \n");
                strMessage.AppendFormat("Te enviamos la clave para activar la venta de Tiempo Aire Electronico a traves de nuestro Sitio.\n");
                strMessage.AppendFormat("Esta clave te sera solicitada al realizar tu primer compra\n");
                strMessage.AppendFormat("\n");
                strMessage.AppendFormat("\n");
                strMessage.AppendFormat(String.Format("CLAVE DE ACTIVACION DE RECARGA WEB: {0}", CveConfirmacion.ToUpper()));
                strMessage.AppendFormat("\n");
                try
                {
                    strMessage.AppendFormat("Usuario: {0} \nContraseña : {1}",
                                   clienteNuevo.Mail,
                                   FreezeCode.IceCubes.Security.Encryption.AccessEncryption.DecrypPassword(ContextoInicial.idAplicacion, clienteNuevo.User.UserPassword));
                    strMessage.AppendFormat("Usuario SMS-IVR: {0} \nNIP SMS-IVR : {1}",
                                clienteNuevo.NumeroTelefono,
                                clienteNuevo.CodigoConfirmacionSMS);
                }
                catch (Exception err)
                {
                }
                strMessage.AppendFormat("\n");

                strMessage.AppendFormat("Muchas gracias por confiar en nosotros y disfruta de nuestro servicio.");

                //Enviamos el correo para confirmacion de Email
                Mail.Send(clienteNuevo.Mail, ContextoInicial.AppSettings["MailContacto"],
                       strMessage.ToString(), "Clave de Activacion de Recarga Web",false);
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }

        public static int CrearDepositoInicial(Empleado elEmpleado)
        {
            try
            {

                //si es baja genera otro tipo de Operacion Transaccional o deja los saldos como estan.
                if (elEmpleado.Baja == "1")
                {
                    return 0;
                }

                //no genera transaccion porque no tiene saldo inicial
                if (elEmpleado.LimiteCompra == 0)
                {
                    return 0;
                }

                RespuestaTransaccional laRespTrx = new RespuestaTransaccional();
                TrxOperacion laOperacion = null;

                laOperacion = new TrxOperacion
                {
                    Afiliacion = Config.Get(elEmpleado.ID_CadenaComercial, "Afiliacion"), //DALCentralAplicaciones.Utilidades.Configuracion.Get(AppId, "Afiliacion").Valor,
                    Beneficiario = Config.Get(elEmpleado.ID_CadenaComercial, "Beneficiario"),
                    FechaTransaccion = DateTime.Now,
                    HoraTransaccion = DateTime.Now,
                    Monto = elEmpleado.LimiteCompra.ToString(),
                    Operador = elEmpleado.NumeroEmpleado,
                    ProccesingCode = Config.Get(elEmpleado.ID_CadenaComercial, "ProccesingCode"),
                    Referencia = elEmpleado.ID_Empleado.ToString(),
                    Sucursal = Config.Get(elEmpleado.ID_CadenaComercial, "Sucursal"),
                    Terminal = Config.Get(elEmpleado.ID_CadenaComercial, "Terminal"),
                    Adquirente = Config.Get(elEmpleado.ID_CadenaComercial, "Adquirente"),
                    Ticket = Guid.NewGuid().ToString().Replace('-', '0').Substring(0, 12),
                    NIP = "0000",
                    CodigoMoneda = "MXN",
                    Track2 = "0000000000000000=0000",
                    MedioAcceso = elEmpleado.TelefonoMovil,// DALCentralAplicaciones.Utilidades.Configuracion.Get(AppID, "MedioAccesoCajero").Valor,
                    TipoMedioAcceso = "TEL",// "IDCTA"
                    elTipoOperacion = TipoOperacion.Requerimiento
                };

                laRespTrx = LNTransaccional.ProcesaOperacion(laOperacion, DateTime.Now.ToString("yyyyMMddHHmmss"));

                if (int.Parse(laRespTrx.CodigoRespuesta) == 0)
                {
                    DAOEmpleado.Actualiza(elEmpleado, EstatusEmpleado.AbonoRealizado, elEmpleado.ID_Detalle);
                }
                else
                {
                    DAOEmpleado.Actualiza(elEmpleado, EstatusEmpleado.ColectivaCreada, elEmpleado.ID_Detalle);
                }

                return int.Parse(laRespTrx.CodigoRespuesta);
            }
            catch (Exception err)
            {
                DAOEmpleado.Actualiza(elEmpleado, EstatusEmpleado.ErrorAlAbonar, elEmpleado.ID_Detalle);
                Logueo.Error("ProcesarEnAutorizador() Empleado: " + elEmpleado.NumeroEmpleado + " " + elEmpleado.Nombre + " " + elEmpleado.APaterno + ": " + err.Message);
                return -1;
            }
        }

        private static void agregarMedioPagoDefault(Cliente clienteNuevo, string emailUsuario, string DescTipoCuenta, string TipoCuenta)
        {
            Tarjeta medioPago = null;
            string Procescode = "14" + TipoCuenta + "00";

            medioPago = Tarjeta.Inicializa(emailUsuario
                , "000"
                , DateTime.Now.AddYears(10).ToString("MMyy")
                , clienteNuevo.Nombre + " " + clienteNuevo.Apellido
                , DescTipoCuenta
                , Tarjeta.EstadoTarjeta.Activa
                , Guid.Empty
                , clienteNuevo.Mail
                , "conocido"
                , "conocido"
                , "conocido"
                , "conocido"
                , "{799CFB5D-F9CE-4B87-B537-6230EFE39897}" //Distrito Federal
                , "00000"
                , "conocido", TipoCuenta, Procescode, false, "", emailUsuario);

            clienteNuevo.TarjetasBanco.AgregarABaseDatos(medioPago);

            Framework.Log.EscribirEventoProceso("Error 5151. Se agregó el medio de pago tipo : " + DescTipoCuenta + ": " + medioPago.Numero + ".",
                    5301,
                    "CreateUser.btnAccept_Click",
                    EventLogEntryType.Information,
                    clienteNuevo.Mail);
        }
	


    }
}
