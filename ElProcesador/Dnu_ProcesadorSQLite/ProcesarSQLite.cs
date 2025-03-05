using CommonProcesador;
using Dnu_ProcesadorSQLite.LogicaNegocio;
using System;

namespace Dnu_ProcesadorSQLite
{

    public class ProcesarSQLite : IProcesoNocturno

    {
        public void Detener()
        {
            try
            {
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }

        public void Iniciar()
        {
            try
            {
                //LNUsuario.EscucharDirectorio();
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
            }
        }

        public bool Procesar()
        {
            try
            {
                int resp;

                resp = 0;

                try
                {
                    ConfiguracionContexto.InicializarContexto();
                    var config = PNConfig.Get("PROCGENSQLITEDB", "BDReadData");

                    var tempPath = PNConfig.Get("PROCGENSQLITEDB", "Directorio");
                    var newVersion = LNArchivo.ExisteVersionNueva(config);
                    if (!String.IsNullOrEmpty(newVersion.Version))
                    {
                        if(LNArchivo.ProcesaGeneracionSQLiteDB(config, newVersion, tempPath)){
                            if(LNArchivo.ActualizaEstatusVersionSqlite(config,newVersion) <= 0)
                            {
                                resp = 1;
                            }
                        }
                        else
                        {
                            resp = 1;
                        }
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


                if (resp == 0)
                {
                    return true;
                }
                else
                    return false;
            }
            catch (Exception err)
            {
                Logueo.Error(err.Message);
                return false;
            }
        }
    }
}
