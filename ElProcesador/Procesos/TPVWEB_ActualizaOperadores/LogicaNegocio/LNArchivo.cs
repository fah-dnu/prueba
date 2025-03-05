using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonProcesador;
using TPVWEB_ActualizaOperadores.Entidades;
using System.IO;
using Framework;
using System.Data.SqlClient;
using DALAutorizador.BaseDatos;
using DALAutorizador.Entidades;
using System.Configuration;
using DALAutorizador.LogicaNegocio;
using System.Threading;

namespace TPVWEB_ActualizaOperadores.LogicaNegocio
{
    public static class LNArchivo
    {
        delegate void EnviarDetalle(String elDetalle);
        public static Dictionary<String, IAsyncResult> ThreadsUsuarios = new Dictionary<string, IAsyncResult>();


        public static Boolean ProcesaUsuarios(String TipoArchivo, String path)
        {
            try
            {
                switch (TipoArchivo)
                {

                    case "EN_": //Datos Extras como cusursal
                    case "EMP": //Datos Extras como cusursal

                        //bloquea a todos los Operadores
                        try
                        {
                            Cliente.BloquearTodoslosOperadoresDeCadena(PNConfig.Get("FILEMNTR", "UserSufijo"));
                        }
                        catch (Exception err)
                        {
                        }

                        foreach (String elDetalle in obtieneRegistros(path))
                        {
                            //LNCreaActualiza uno = new LNCreaActualiza();
                            //uno.ActualizaOper(elDetalle);

                            LNCreaActualiza EjetutaActualizar = new LNCreaActualiza();
                            //Inicia los delegados para Ejecutar los batch de las diferentes acciones.
                            EnviarDetalle ProcesoAEjecutar = EjetutaActualizar.ActualizaOper;


                            //Ejecuta el metodo asincrono

                            ProcesoAEjecutar.BeginInvoke(elDetalle,
                            delegate(IAsyncResult ar1)
                            {
                                try
                                {
                                    ProcesoAEjecutar.EndInvoke(ar1);
                                }
                                catch (Exception ex)
                                {

                                }
                            }, null);
                        }

                        break;
                    case "UC": //Solo Usuario y Password

                        break;
                }

                return true;

            }
            catch (Exception err)
            {
                Logueo.Error("getUsuarios(): " + err.Message);
                return false;
            }
        }

        public static List<String> obtieneRegistros(String elPath)
        {
            StreamReader objReader = new StreamReader(elPath);
            string sLine = "";
            List<String> losDetalles = new List<String>();

            while (sLine != null)
            {
                sLine = objReader.ReadLine();
                if (sLine != null)
                {
                    losDetalles.Add(sLine);
                }
            }
            objReader.Close();

            return losDetalles;
        }


    }
}
