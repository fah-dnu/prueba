
namespace DNU_ParabiliaTarjetasStock.LogicaNegocio
{
    using CommonProcesador;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Entidades;
    using DNU_ParabiliaTarjetasStock.BaseDeDatos;
    using System.Threading;
    using System.Text.RegularExpressions;
    using System.Data;
    using DNU_NewRelicNotifications.Services.Wrappers;
    using NewRelic.Api.Agent;

    public class LNArchivoStocks
    {

        private string m_directorio;
        private static string m_directorioSalida;


        public const string CLASE = "LNArchivoStocks";
        const string MENSEJE_SUPRODUCTO_INVALIDO = "EL SUBPRODUCTO  NO ES VALIDO";
        const string MENSAJE_COLECTIVA_INVALIDA = "LA COLECTIVA NO ES VALIDA";
        const string MENSAJE_AFECTACION_BD = "NO SE AFECTO CORRECTAMENTE EN BD";
        const string CODIGO_EXITOSO = "99";
        const string CODIGO_NO_EXITOSO = "00";
        //
        private static string _NombreNewRelic;
        private static string NombreNewRelic
        {
            set
            {

            }
            get
            {
                string ClaveProceso = "PRPROCESARTARSTOCK";
                _NombreNewRelic = PNConfig.Get(ClaveProceso, "NombreNewRelic");
                if (String.IsNullOrEmpty(_NombreNewRelic))
                {
                    _NombreNewRelic = ClaveProceso + "-SINNOMBRE";
                    Logueo.Evento("Se coloco nombre generico para instrumentacion NewRelic al no encontrar el parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                    Logueo.Error("No se encontro parametro NombreNewRelic en tabla Configuraciones [" + ClaveProceso + "]");
                }
                else
                {
                    Logueo.Evento("Se encontro parametro NombreNewRelic: " + _NombreNewRelic + " [" + ClaveProceso + "]");
                }
                return _NombreNewRelic;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public LNArchivoStocks()
        {

        }


        /// <summary>
        /// Constructor sobre carga
        /// </summary>
        /// <param name="directorio"></param>
        public LNArchivoStocks(string directorio)
        {
            this.m_directorio = directorio;
        }

        internal void NuevoArchivo(object sender, FileSystemEventArgs e)
        {
            const string METODO = "NuevoArchivo";
            WatcherChangeTypes elTipoCambio = e.ChangeType;
            Logueo.Evento("[PRPROCESARTARSTOCK ][NuevoArchivo]Hubo un Cambio [" + elTipoCambio.ToString() + "] en el directorio: " + PNConfig.Get("PRPROCESARTARSTOCK", "DirectorioEntrada") + " el se recibio el archivo : " + e.FullPath);

            Logueo.Evento("[PRPROCESARTARSTOCK] Inicia Proceso Ruta:" + e.FullPath);
            validarArchivos(true);
        }



        public void crearDirectorio()
        {
            m_directorioSalida = PNConfig.Get("PRPROCESARTARSTOCK", "DirectorioSalida");

            if (!Directory.Exists(m_directorio))
                Directory.CreateDirectory(m_directorio);
            if (!Directory.Exists(m_directorio + "\\Procesados"))
                Directory.CreateDirectory(m_directorio + "\\Procesados");
            if (!Directory.Exists(m_directorioSalida))
                Directory.CreateDirectory(m_directorioSalida);
            if (!Directory.Exists(m_directorioSalida + "\\Erroneos"))
                Directory.CreateDirectory(m_directorioSalida + "\\Erroneos");
            if (!Directory.Exists(m_directorioSalida + "\\Correctos"))
                Directory.CreateDirectory(m_directorioSalida + "\\Correctos");
        }

        [Transaction]
        internal void OcurrioError(object sender, ErrorEventArgs e)
        {
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("OcurrioError");
            ApmNoticeWrapper.NoticeException(e.GetException());
            throw new NotImplementedException();
        }

        [Transaction]
        internal bool validarArchivos(bool copiaArchivos)
        {
            ApmNoticeWrapper.SetAplicationName(NombreNewRelic);
            ApmNoticeWrapper.SetTransactionName("validarArchivos");
            bool esCredito = false;
            bool colectivaValida = false;
            bool subproductoValido = false;
            string subproducto = string.Empty;
            string nuevoNombre = string.Empty;
            string tipoManufactura = string.Empty;
            bool afectacionBD = false;
            DataTable ArchivoLectura = new DataTable();


            const string METODO = "validarArchivos";
            ArchivoConfiguracion elArchivos = new ArchivoConfiguracion();

            try
            {
                ConfiguracionContexto.InicializarContexto();
                List<ArchivoConfiguracion> losArchivos = DAORegistrarTarjetasStock.ObtenerArchivosConfigurados();

                Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Obteniendo configuracion de archivos...", CLASE, METODO));

                if (losArchivos != null)
                {
                    elArchivos = losArchivos.
                       Where(m => m.ClaveArchivo.Contains("PRPROCESARTARSTOCK")).FirstOrDefault();
                    if (elArchivos == null)
                    {

                        Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  No se obtuvo  la configuracion para (PRPROCESARTARSTOCK)", CLASE, METODO));
                        return false;
                    }
                }
                else
                {

                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  No se obtuvo ninguna configuracion...", CLASE, METODO));
                    return false;
                }

                if (copiaArchivos)
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Inicia el  proceso de espera para el copiado...", CLASE, METODO));
                    Thread.Sleep(1000 * 60 * 1);
                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  termina el proceso de espera para el copiado ", CLASE, METODO));
                }


                DirectoryInfo directory = new DirectoryInfo(m_directorio);
                FileInfo[] files = directory.GetFiles(elArchivos.Nombre + "*.*");



                if (files.Length == 0)
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  No  hay archivos para procesar", CLASE, METODO));
                    Logueo.Evento("[PRPROCESARTARSTOCK] +[" + elArchivos.Nombre + "*.*]");
                    return false;

                }

                foreach (var item in files)
                {

                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Obtener informacion del archivo :  ", CLASE, METODO));
                    string elpathFile = (((FileInfo)item).FullName);


                    string filename1 = Path.GetFileName(elpathFile);
                    string root1 = Path.GetDirectoryName(elpathFile);

                    string[] componentesTitulo = filename1.Split('_');




                    String claveColectiva = filename1.Substring(filename1.IndexOf('_') + 1,
                            filename1.LastIndexOf('_') - (filename1.IndexOf('_') + 1)).Trim();



                    nuevoNombre = filename1.Replace("_V_", "_");

                    ///Obtiene la clave colectiva del archivo
                    claveColectiva = LNArchivoStocks.ObtenerClaveEmisor(nuevoNombre);

                    if (claveColectiva.Length > 9)
                    {

                        Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  CLAVE EMISOR Validada  en nombre:  ", CLASE, METODO));

                        nuevoNombre = nuevoNombre.Replace("_" + componentesTitulo[componentesTitulo.Length - 1], "");
                        claveColectiva = nuevoNombre.Substring(nuevoNombre.IndexOf('_') + 1,
                        nuevoNombre.LastIndexOf('_') - (nuevoNombre.IndexOf('_') + 1)).Trim();

                        subproducto = nuevoNombre.Split('_').Last();


                        if (filename1.ToUpper().Contains("_V_"))
                        {
                            tipoManufactura = "V";
                        }

                        var tmpNameFile = LNArchivoStocks.LimpiarArchivo(((FileInfo)item).FullName, directory.FullName);

                        File.Move(tmpNameFile, elpathFile);
                        File.Move(elpathFile, root1 + "\\EN_PROCESO_" + filename1);
                        elpathFile = root1 + "\\EN_PROCESO_" + filename1;

                        Logueo.Evento("Procesar archivo desde hora programada [" + ((FileInfo)item).FullName + "] ");

                        if (!string.IsNullOrEmpty(subproducto))
                        {
                            Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Inicia validacion de subProducto ", CLASE, METODO));
                            DataTable resultadoConsultaSubproducto = DAORegistrarTarjetasStock.validaExistenciaSubproducto(subproducto);
                            if (resultadoConsultaSubproducto.Rows.Count > 0)
                            {
                                if (resultadoConsultaSubproducto.Rows[0]["tipo"].ToString() == "correcto")
                                {
                                    if (resultadoConsultaSubproducto.Rows[0]["esPrepago"].ToString() == "0")
                                    {
                                        Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Prepago Valido  ", CLASE, METODO));
                                        esCredito = true;
                                    }
                                    subproductoValido = true;
                                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  SubProducto Valido :  ", CLASE, METODO));
                                }

                            }

                        }
                        if (subproductoValido)
                        {
                            Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Inicia Validacion de Colectiva:  ", CLASE, METODO));
                            colectivaValida = DAORegistrarTarjetasStock.ColectivaValida(claveColectiva);
                        }
                        else
                            Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  SubProducto Invalido:  ", CLASE, METODO));

                        ArchivoLectura = LNArchivoStocks.LecturaArchivo(elpathFile, subproducto);

                        if (colectivaValida)
                        {
                            Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Colectiva Valida :  ", CLASE, METODO));
                            if (ArchivoLectura.Rows.Count > 0)
                            {
                                if (DAORegistrarTarjetasStock.afectacionTablas(ArchivoLectura,item.Name))
                                {
                                    afectacionBD = true;
                                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Se afecto correctamente en BD ", CLASE, METODO));
                                }

                                else
                                    Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Ocurrio un error al insertar en BD ", CLASE, METODO));


                            }
                            else
                            {

                                Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Error al leer el archivo revise la infomacion :  ", CLASE, METODO));
                                return false;

                            }

                        }

                        if (LNArchivoStocks.GenerarArchivo(ArchivoLectura, subproductoValido, colectivaValida, afectacionBD, elpathFile)) 
                        {
                            Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  GENERACION DE ARCHIVO ...Correcta ", CLASE, METODO));

                        }



                    }
                    else
                    {

                        Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Error en el formato del  nombre del archivo :  ", CLASE, METODO));
                        return false;
                    }

                }

            }
            catch (Exception ex)
            {
                Logueo.Evento(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Error : " + ex.InnerException, CLASE, METODO));
                Logueo.Error(string.Format("[PRPROCESARTARSTOCK].{0}.{1}()  Error:" + ex.Message, CLASE, METODO));
                ApmNoticeWrapper.NoticeException(ex);
            }

            return true;
        }

        private static DataTable LecturaArchivo(string elPath, string subProducto)
        {
            const string METODO = "LecturaArchivo";
            DataTable dt = new DataTable("Tarjetas");
            string sLine = string.Empty;

            Logueo.Evento(string.Format("[PRPROCESARTASTOCK].{0}.{1}() Inicia el proceso de lectura del archivo", CLASE, METODO));

            dt.Columns.Add(new DataColumn("Id_ArchivoDG"));
            dt.Columns.Add(new DataColumn("ID_Solicitud"));
            dt.Columns.Add(new DataColumn("ClaveEmisor"));
            dt.Columns.Add(new DataColumn("Subproducto"));
            dt.Columns.Add(new DataColumn("ValorD1"));//Cuenta
            dt.Columns.Add(new DataColumn("ValorD2")); //Tarjeta
            dt.Columns.Add(new DataColumn("ValorD3"));///Tipo Tarjeta
            dt.Columns.Add(new DataColumn("ValorD4"));  //Fecha de Vencimiento;
            dt.Columns.Add(new DataColumn("EsElegible"));
            try
            {
                //
                using (StreamReader objReader = new StreamReader(elPath))
                {
                    while (objReader.Peek() > -1)
                    {
                        sLine = objReader.ReadLine();

                        if (string.IsNullOrEmpty(sLine))
                        {
                            break;
                        }
                        var splitted = sLine.Split('|');
                        DataRow row = dt.NewRow();
                        row["Id_ArchivoDG"] = null;
                        row["ID_Solicitud"] = string.Empty;
                        row["ClaveEmisor"] = splitted[0];
                        row["Subproducto"] = subProducto;
                        row["ValorD1"] = string.Format("{0}{1}", splitted[0], splitted[1]);
                        row["ValorD2"] = splitted[2];//Tarjeta
                        row["ValorD3"] = splitted[3];///Tipo Tarjeta
                        row["ValorD4"] = splitted[4];  //Fecha de Vencimiento;
                        row["EsElegible"] = Convert.ToBoolean(1);

                        dt.Rows.Add(row);
                    }
                }

            }
            catch (Exception err)
            {
                Logueo.Evento(string.Format("[PRPROCESARTASTOCK].{0}.{1}()  ERROR" + err.Message, CLASE, METODO));
            }
            return dt;
        }



        private static string ObtenerClaveEmisor(string Filename)
        {

            const string METODO = "ObtenerClaveEmisor";
            Logueo.Evento(string.Format("[PRPROCESARTASTOCK].{0}.{1}()  Inicia el  proceso de espera para el copiado...", CLASE, METODO));
            string claveColectiva = string.Empty;
            try
            {
                claveColectiva = Filename.Substring(Filename.IndexOf('_') + 1,
                        Filename.LastIndexOf('_') - (Filename.IndexOf('_') + 1)).Trim();


            }
            catch (Exception ex)
            {
                Logueo.Evento(string.Format("[PRPROCESARTASTOCK].{0}.{1}()  ERROR:" + ex.Message, CLASE, METODO));
                return string.Empty;
            }
            return claveColectiva;

        }
        /// <summary>
        /// limpiar Archivo obtener 
        /// </summary>
        /// <param name="elArchivo"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static string LimpiarArchivo(String elArchivo, string path)
        {
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(elArchivo, Encoding.UTF8))
            {
                String claveMov = String.Empty;
                String descMov = String.Empty;
                String account = String.Empty;
                String total = String.Empty;
                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();


                    if (Regex.Match(line, @"^[0-9]{8}[\s][0-9]{5,15}[\s]{11}").Success)
                    {

                        var limiteInicial = 9;
                        var emisor = string.Empty;
                        try
                        {
                            emisor =
                                line.Substring(0, limiteInicial).Trim();

                        }
                        catch (Exception ex)
                        {

                        }

                        var Cuenta = string.Empty;
                        var segundolimite = limiteInicial + 16;
                        try
                        {
                            Cuenta = line.Substring(limiteInicial, (segundolimite - limiteInicial)).Trim();

                        }
                        catch (Exception ex)
                        {

                        }

                        var tercerlimite = segundolimite + 29;
                        var NumeroTarjeta = string.Empty;
                        try
                        {
                            NumeroTarjeta = line.Substring(segundolimite, (tercerlimite - segundolimite)).Trim();
                        }
                        catch (Exception ex)
                        {

                        }

                        var cuartolimite = tercerlimite + 24;
                        var Tarjeta = string.Empty;
                        try
                        {
                            Tarjeta = line.Substring(tercerlimite, (cuartolimite - tercerlimite)).Trim();
                        }
                        catch (Exception ex)
                        {

                        }

                        var quintoimite = cuartolimite + 10;
                        Logueo.Error("FECHA:" + line.Substring(cuartolimite, (quintoimite - cuartolimite)).Trim());

                        var vencimientoTmp = string.Empty;
                        var Vencimiento = string.Empty;
                        try
                        {
                            vencimientoTmp = line.Substring(cuartolimite).Trim();


                            Vencimiento = String.Format("{0}-{1}-{2}", vencimientoTmp.Substring(6, 4),
                                vencimientoTmp.Substring(3, 2),
                                vencimientoTmp.Substring(0, 2));
                        }
                        catch (Exception ex)
                        {

                        }


                        Logueo.Evento("Registro: " + String.Format("{0}|{1}|{2}|{3}|{4}", emisor, Cuenta, NumeroTarjeta, Tarjeta,
                            Vencimiento));

                        sb.AppendLine(String.Format("{0}|{1}|{2}|{3}|{4}", emisor, Cuenta, NumeroTarjeta, Tarjeta,
                             Vencimiento));
                    }
                }
            }
            File.Delete(elArchivo);
            using (StreamWriter sw = File.CreateText(elArchivo))
            {
                sw.WriteLine(sb.ToString());

            }

            return elArchivo;
        }
        private static bool GenerarArchivo(DataTable archivoLeeido, bool subProducto, bool colectiva, bool afectacionBD,string path)
        {
            const string METODO = "GenerarArchivo";
            string mensajeSalida = "CORRECTO";
            string codigoSalida = "00";


            Logueo.Evento(string.Format("[PRPROCESARTASTOCK].{0}.{1}()   Inicia el  proceso de generacion de archivo.. : ", CLASE, METODO));
             StringBuilder sb = new StringBuilder();

            string emisor = string.Empty;
            string cuenta = string.Empty;
            string numTarjeta = string.Empty;
            string tarjeta = string.Empty;
            string vencimiento = string.Empty;
            bool ejecucionExitosa = true;
        
            try 
            {

                sb.AppendLine(String.Format("{0}{1}{2}{3}{4}{5}{6}",
                    "Emisor".PadRight(9),
                    "Cuenta".PadRight(16),
                    "Número Tarjeta".PadRight(29),
                    "Tarjeta".PadRight(24),
                    "Vencimiento".PadRight(15),
                    "Código Resultado".PadRight(20),
                    "Mensaje Resultado"));
                sb.AppendLine("------------------------------------------------------------------------------------------------------------------------------------");

                if (!subProducto)
                {
                    mensajeSalida = MENSEJE_SUPRODUCTO_INVALIDO;
                    codigoSalida = "99";
                    ejecucionExitosa = false;
                }
                else if (!colectiva)
                {
                    mensajeSalida = MENSAJE_COLECTIVA_INVALIDA;
                    codigoSalida = "99";
                    ejecucionExitosa = false;
                }
                else if (!afectacionBD)
                {
                    mensajeSalida = MENSAJE_AFECTACION_BD;
                    codigoSalida = "99";
                    ejecucionExitosa = false;
                }


                foreach (DataRow   row in archivoLeeido.Rows)
                {
                    emisor = row["ClaveEmisor"].ToString().PadRight(9);
                    cuenta = row["ValorD1"].ToString().PadRight(16);
                    numTarjeta = row["ValorD2"].ToString().PadRight(29);
                    tarjeta = row["ValorD3"].ToString().PadRight(24);
                    vencimiento = row["ValorD4"].ToString().PadRight(15);



                    sb.AppendLine(
                        string.Format
                        ("{0}{1}{2}{3}{4}{5}{6}",
                    emisor,
                    cuenta,
                    numTarjeta,
                    tarjeta,
                    vencimiento,
                    codigoSalida.PadRight(20),
                    mensajeSalida
                    ));


                }
                LNArchivoStocks.renombraArchivo(path,ejecucionExitosa, sb);
                 
               
            }
            catch (Exception ex) 
            {
                Logueo.Evento(string.Format("[PRPROCESARTASTOCK].{0}.{1}()  Error :  " + ex.Message, CLASE, METODO));

            }


            return true;
        }

        private static void renombraArchivo(string elpathFile, bool ejecucionCorrecta, StringBuilder sb)
        {
            const string METODO = "renombraArchivo";
            if (ejecucionCorrecta)
            {
                Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                string filename = Path.GetFileName(elpathFile);
                //string root = Path.GetDirectoryName(elpathFile);
                string finalFileName = LNArchivoStocks.m_directorioSalida+ "\\Correctos\\" + "\\PROCESADO_" + filename.Replace("EN_PROCESO_", "");



                File.Delete(elpathFile);
                if (File.Exists(finalFileName))
                {
                    var tempName = Path.GetFileName(finalFileName);
                
                    var directorio = PNConfig.Get("PRPROCESARTARSTOCK", "DirectorioEntrada");
                    
                    DirectoryInfo directory = new DirectoryInfo(directorio);
                    var consecutivo = directory.GetFiles().Where(w => w.Name.Contains(tempName.Replace("PROCESADO_", ""))).Count() + 1;
                    finalFileName = finalFileName.Replace("PROCESADO_", string.Format("PROCESADO_{0}_{1}_", consecutivo, DateTime.Now.Hour));
                }

                Logueo.Evento(string.Format("[PRPROCESARTASTOCK].{0}.{1}() Archivo de Salida : " +
                    "", CLASE, METODO));
                using (StreamWriter sw = File.CreateText(finalFileName))
                {
                    sw.WriteLine(sb.ToString());
                }
            }
            else
            {
                Logueo.Evento("Iniciar el renombramiento del Archivo procesado con error: " + elpathFile);
                string filename = Path.GetFileName(elpathFile);
                //string root = Path.GetDirectoryName(elpathFile);
                string finalFileName = m_directorioSalida + "\\Erroneos\\" + "\\PROCESADO_CON_ERROR_" + filename.Replace("EN_PROCESO_", "");
                File.Delete(elpathFile);

                if (File.Exists(finalFileName))
                {
                    var tempName = Path.GetFileName(finalFileName);

         
                    var directorio = PNConfig.Get("PRPROCESARTARSTOCK", "DirectorioEntrada");
         
                    DirectoryInfo directory = new DirectoryInfo(directorio);
                    var consecutivo = directory.GetFiles().Where(w => w.FullName.Contains(tempName.Replace("PROCESADO_CON_ERROR_", ""))).Count() + 1;
                    finalFileName = finalFileName.Replace("PROCESADO_CON_ERROR_", string.Format("PROCESADO_CON_ERROR_{0}_{1}_", consecutivo, DateTime.Now.Hour));
                }


                using (StreamWriter sw = File.CreateText(finalFileName))
                {
                    sw.WriteLine(sb.ToString());
                }
            }

        }


    }
}

