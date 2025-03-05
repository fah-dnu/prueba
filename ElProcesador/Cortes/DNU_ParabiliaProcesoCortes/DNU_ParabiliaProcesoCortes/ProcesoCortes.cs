#define PIEK
using CommonProcesador;
using DNU_ParabiliaProcesoCortes.CapaNegocio;
using DNU_ParabiliaProcesoCortes.Entidades;
using DNU_ParabiliaProcesoCortes.LogicaNegocio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes
{
    public class ProcesoCortes : IProcesoNocturno
    {

        string directorioEntrada;
        string directorioSalida;

        bool IProcesoNocturno.Procesar()
        {//
            try
            {
                Logueo.Evento("Inicio proceso corte credito");
                LNInicioCorte _lnIniciocorte = new LNInicioCorte();
                return _lnIniciocorte.inicio();

            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;

            }
        }

        void IProcesoNocturno.Iniciar()
        {


            Logueo.Evento("inicio alta tarjeta");

#if PIEK
            try
            {
                Logueo.Evento("Inicio proceso corte debito");
                LNInicioCorte _lnIniciocorte = new LNInicioCorte();
                _lnIniciocorte.inicio();

            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
              

            }
#else


            try
            {
                Logueo.Evento("[GeneraReporteEstadoCuenta] [PROCESADORNOCTURNO] [Inicio hilo]");
                LNCorteTrafalgar trafalgar = new LNCorteTrafalgar();
                Task.Run(() => { trafalgar.inicioCiclo(); });

            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraReporteEstadoCuenta]" + ex.Message);
            }
            try
            {
                ConfiguracionContexto.InicializarContexto();
                directorioEntrada = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioEntradaArchivos");
                directorioSalida = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioSalidaArchivos");
                LNCargaArchivo lnCargaArchivo = new LNCargaArchivo(directorioEntrada, String.Empty, directorioSalida);
                lnCargaArchivo.crearDirectorio();
                Logueo.Evento("[GeneraReporteEstadoCuenta] [PROCESADORNOCTURNO] [Inicia Escucha en Directorio " + directorioEntrada + "PROCESAEDOCUENTA]");
                FileSystemWatcher watcher = new FileSystemWatcher(directorioEntrada);
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                // Add event handlers.
                watcher.Created += new FileSystemEventHandler(lnCargaArchivo.NuevoArchivo);
                watcher.Error += new ErrorEventHandler(lnCargaArchivo.OcurrioError);
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex)
            {
                Logueo.Error("[GeneraReporteEstadoCuenta]" + ex.Message);
            }
#endif


        }

        void IProcesoNocturno.Detener()
        {
        }

        public bool InicioLocal(string fecha = null)
        {
            try
            {
                LNInicioCorte _lnIniciocorte = new LNInicioCorte();
                //
                return _lnIniciocorte.inicio(fecha);
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
        }

        public bool InicioLocalWB(string fecha = null)
        {
            try
            {
                LNInicioCorte _lnIniciocorte = new LNInicioCorte();
                //
                return _lnIniciocorte.inicio(fecha);
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
        }

        public bool GenerarEstadoDeCuenta(Int64 idCorte)
        {
            try
            {
                LNInicioCorte _lnIniciocorte = new LNInicioCorte(idCorte.ToString());
                //
                return _lnIniciocorte.inicio();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
        }

        public RespuestaSolicitud GenerarEstadoDeCuentaDebito(Int64 idCorte)
        {
            RespuestaSolicitud respuesta = new RespuestaSolicitud();
            try
            {
                //si envia el id de entrarda va aenviar el correo o generar el pdf sin hacer todo el proceso
                LNInicioCorte _lnIniciocorte = new LNInicioCorte(idCorte.ToString(), null, null, false, respuesta, true);
                //
                bool Procesado = _lnIniciocorte.inicio();
                return respuesta;
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return respuesta;
            }
        }

        public RespuestaSolicitud EnviarEstadoDeCuenta(Int64 idCorte)
        {
            RespuestaSolicitud respuesta = new RespuestaSolicitud();
            try
            {

                LNInicioCorte _lnIniciocorte = new LNInicioCorte(idCorte.ToString(), null, null, true, respuesta);
                //
                bool Procesado = _lnIniciocorte.inicio();
                return respuesta;
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return respuesta;
            }
        }


        public RespuestaSolicitud GenerarEstadoDeCuentaCredito(Int64 idCorte)
        {
            RespuestaSolicitud respuesta = new RespuestaSolicitud();
            try
            {
                //si envia el id de entrarda va aenviar el correo o generar el pdf sin hacer todo el proceso
                LNInicioCorte _lnIniciocorte = new LNInicioCorte(idCorte.ToString(), null, null, false, respuesta, false);
                //
                bool Procesado = _lnIniciocorte.inicio();
                return respuesta;
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return respuesta;
            }
        }

        public bool Inicio()
        {
            try
            {
                Logueo.Evento("Inicio proceso corte debito");
                LNInicioCorte _lnIniciocorte = new LNInicioCorte();
                return _lnIniciocorte.inicio();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
        }

        public bool InicioCiclo()
        {
            try
            {
                Logueo.Evento("Inicio proceso corte debito");
               // LNInicioCorte _lnIniciocorte = new LNInicioCorte();
                try
                {
                    Logueo.Evento("[GeneraReporteEstadoCuenta] [PROCESADORNOCTURNO] [Inicio hilo]");
                    LNCorteTrafalgar trafalgar = new LNCorteTrafalgar();
                    Task.Run(() => { trafalgar.inicioCiclo(); });
                    System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);

                }
                catch (Exception ex)
                {
                    Logueo.Error("[GeneraReporteEstadoCuenta]" + ex.Message);
                }
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
            return true;
        }

        public void IniciarLeerArchivos()
        {
            string direccionIP = System.Net.Dns.GetHostName();
            Guid idLog = Guid.NewGuid();

            Logueo.Evento("inicio alta tarjeta");
            try
            {
                ConfiguracionContexto.InicializarContexto();
                directorioEntrada = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioEntradaArchivos");
                directorioSalida = PNConfig.Get("PROCESAEDOCUENTA", "DirectorioEntradaArchivos");
                LNCargaArchivo lnCargaArchivo = new LNCargaArchivo(directorioEntrada, String.Empty, directorioSalida);
                lnCargaArchivo.crearDirectorio();
                Logueo.Evento("[GeneraReporteEstadoCuenta] [PROCESADORNOCTURNO] [Inicia Escucha en Directorio " + directorioEntrada + "PRALTATARJPARABILIA]");
                lnCargaArchivo.leerArchivos(false, "");
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
            }

        }

    }
}
