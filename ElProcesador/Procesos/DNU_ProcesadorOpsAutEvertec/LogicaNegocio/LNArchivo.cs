using CommonProcesador;
using DNU_ProcesadorOpsAutEvertec.BaseDatos;
using DNU_ProcesadorOpsAutEvertec.Entidades;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace DNU_ProcesadorOpsAutEvertec.LogicaNegocio
{
    class LNArchivo
    {
        public static Boolean enProceso = false;
        private static readonly object balanceLock = new object();
        private static FileSystemWatcher elObservador = new FileSystemWatcher(PNConfig.Get("OPAUTEVERTEC", "DirectorioEntrada"));

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool EjecutaProceso()
        {
            Logueo.Evento("INICIA PROCESO - OPAUTEVERTEC");

            try
            {
                crearDirectorio();

                Archivo configArchivo = DAOArchivo.ObtieneParametrosArchivos();

                DirectoryInfo directory = new DirectoryInfo(configArchivo.RutaLectura);
                FileInfo[] files = directory.GetFiles(configArchivo.Nombre + "*.*");

                Logueo.Evento("ARCHIVOS A PROCESAR: " + files.Length.ToString());

                for (int i = 0; i < files.Length; i++)
                {
                    String elpathFile = (((FileInfo)files[i]).FullName);
                    configArchivo.RutaEscritura = elpathFile;

                    //cambia el nombre del archivo para evitar que el proceso automatico y el proceso programadado choquen
                    Logueo.Evento("Iniciar el renombramiento del Archivo procesado: " + elpathFile);
                    string filename = Path.GetFileName(elpathFile);

                    configArchivo.NombreArchivoDetectado = filename;
                    string root1 = Path.GetDirectoryName(elpathFile);

                    File.Move(elpathFile, root1 + "\\EN_PROCESO_" + filename);

                    elpathFile = root1 + "\\EN_PROCESO_" + filename;

                    Logueo.Evento("Procesar archivo desde hora programada [" + ((FileInfo)files[i]).FullName + "] ");

                    //Crea el registro del archivo
                    Int64 idFile = CreaRegistroArchivo(filename);
                    Logueo.Evento("CREACION DE ARCHIVO OK. ID_FILE = " + idFile.ToString());

                    if (idFile == -1)
                    {
                        Logueo.Error("EjecutaProceso(): NO SE PUDO OBTENER UN ID DE ARCHIVO VALIDO. ARCHIVO: " + filename);
                        //Restablece el nombre del archivo, para que se procese en la siguiente ejecución
                        renombrarArchivo(elpathFile, root1 + filename);
                    }
                    else
                    {
                        DataTable archivo = new DataTable();

                        try
                        {
                            //Limpia el archivo y genera el objeto table
                            archivo = ObtieneArchivoLimpio(idFile, elpathFile, configArchivo);

                            //El archivo no trae registros
                            if (archivo.Rows.Count == 0)
                            {
                                RegistraArchivoVacio(idFile);
                                EstableceEstatusAlArchivo(idFile, elpathFile, filename, true);
                            }
                            else
                            {
                                //Inserta el detalle del archivo en BD
                                bool inserta = InsertaNuevoArchivodetalle(archivo, idFile);
                                EstableceEstatusAlArchivo(idFile, elpathFile, filename, true);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logueo.Error(ex.Message);
                            EstableceEstatusAlArchivo(idFile, elpathFile, filename, false);
                        }
                    }
                }

                Logueo.Evento("FIN PROCESO - OPAUTEVERTEC");
                return true;
            }
            catch (Exception Ex)
            {
                Logueo.Error("OCURRIO UN ERROR AL EJECUTAR EL PROCESO OPAUTEVERTEC.\nMSJ:" + Ex.Message +
                    "\nSOURCE: " + Ex.Source + "\nSTACKTRACE: " + Ex.StackTrace);
                return false;
            }
        }

        //
        public static void crearDirectorio()
        {
            string directorioEscucha = PNConfig.Get("OPAUTEVERTEC", "DirectorioEntrada");
            string directorioSalida = PNConfig.Get("OPAUTEVERTEC", "DirectorioSalida");

            if (!Directory.Exists(directorioEscucha))
                Directory.CreateDirectory(directorioEscucha);

            if (!Directory.Exists(directorioSalida))
                Directory.CreateDirectory(directorioSalida);
        }

        //
        public static void EscucharDirectorio()
        {
            try
            {
                Logueo.Evento("Inicia Escucha de Carpeta ");

                crearDirectorio();

                Logueo.Evento("Inicia la escucha de la carpeta: " + PNConfig.Get("OPAUTEVERTEC", "DirectorioEntrada") + " en espera de archivos. OPAUTEVERTEC");

                elObservador.NotifyFilter = (NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);

                elObservador.Created += alCambiar;
                elObservador.Error += alOcurrirUnError;

                elObservador.EnableRaisingEvents = true;

            }
            catch (Exception err)
            {
                Logueo.Error("EscucharDirectorio(): " + err.Message);
            }
        }

        //
        public static void DetenerDirectorio()
        {
            try
            {
                Logueo.Evento("Se Detiene a escucha del Directorio: " + PNConfig.Get("OPAUTEVERTEC", "DirectorioEntrada"));

                elObservador.NotifyFilter = (NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);

                elObservador.Created -= alCambiar;
                elObservador.Error -= alOcurrirUnError;

                elObservador.EnableRaisingEvents = false;

            }
            catch (Exception err)
            {
                Logueo.Error("EscucharDirectorio(): " + err.Message);
            }
        }

        //
        public static void alCambiar(object source, FileSystemEventArgs el)
        {
            try
            {
                //detener el file wathcer despues de que se detecto un cambio para evitar varios disparadores
                DetenerDirectorio();

                enProceso = true;
                WatcherChangeTypes elTipoCambio = el.ChangeType;

                //Espera un tiempo determinado para darle tiempo a que se copie el archivo y no lo encuentre ocupado por otro proceso.
                Logueo.Evento("Hubo un Cambio [" + elTipoCambio.ToString() + "] en el directorio: " + PNConfig.Get("OPAUTEVERTEC", "DirectorioEntrada") + " el se recibio el archivo : " + el.FullPath);
                Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");

                try
                {
                    Logueo.Evento("Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                    Thread.Sleep(1000 * 60 * Int32.Parse(PNConfig.Get("OPAUTEVERTEC", "MinEsperaProceso")));

                    Logueo.Evento("Esperando" + (1000 * 60 * Int32.Parse(PNConfig.Get("OPAUTEVERTEC", "MinEsperaProceso"))).ToString() + " Minutos.");

                }
                catch (Exception err)
                {
                    Logueo.Evento("Inicia espera defualt por no tener configuracion en 'MinEsperaProceso'. Espera a que se copie el archivo completo y lo libere el proceso de copiado");
                    Thread.Sleep(1000 * 60 * 10);
                    Logueo.Evento("Esperando 10 Minutos.");
                }

                Logueo.Evento("INICIO DE PROCESO DEL ARCHIVO:" + el.FullPath);

                try
                {
                    lock (balanceLock)
                    {
                        EjecutaProceso();
                    }

                }

                catch (Exception ex)
                {
                    Logueo.Error("alCambiar(): " + ex.Message + ",  " + ex.ToString());
                    Console.WriteLine(ex.ToString());

                }
                finally
                {
                    enProceso = false;
                }


            }
            catch (Exception err)
            {
                Logueo.Error("alCambiar(): " + err.Message + ",  " + err.ToString());
            }
            finally
            {
                //poner a escuchar otra vez el file watcher
                EscucharDirectorio();
            }
        }

        //
        public static void alOcurrirUnError(object source, ErrorEventArgs e)
        {
            try
            {
                Logueo.Error("alOcurrirUnError(): e: " + e.GetException().Message);
            }
            catch (Exception err)
            {
                Logueo.Error("alOcurrirUnError(): " + err.Message);
            }
        }

        /// <summary>
        /// Establece el estatus del archivo, según haya sido el resultado de la inserción, logueando el evento
        /// y moviendo el archivo a PROCESADOS
        /// </summary>
        /// <param name="IdArchivo">Identificador del archivo</param>
        /// <param name="RutaOrigen">Ruta origen, donde se encuentra el archivo</param>
        /// <param name="Archivo">Nombre del archivo</param>
        /// <param name="ProcExitoso">Bandera con el resultado de la inserción</param>
        public static void EstableceEstatusAlArchivo(Int64 IdArchivo, String RutaOrigen, String Archivo, bool ProcExitoso)
        {
            //Loguea el estatus del procesamiento
            Logueo.Evento("Inserción del archivo con ID: " + IdArchivo.ToString() + ", nombre del archivo: " + Archivo +
                (ProcExitoso ? " EXITOSA" : " NO EXITOSA"));

            //Actualiza el estatus del archivo
            ActualizaEstatusArchivo(IdArchivo, ProcExitoso ? 1 : 0);

            moverArchivo(RutaOrigen, PNConfig.Get("OPAUTEVERTEC", "DirectorioSalida") + "\\PROCESADO_" + Archivo.Replace("EN_PROCESO_", ""));
        }

        /// <summary>
        /// Procesa el archivo de texto para obtener sólo los registros de las operaciones, dejándolos
        /// en un objeto de tipo DataTable
        /// </summary>
        /// <param name="IdArchivo">Identificador del archivo en base de datos</param>
        /// <param name="elArchivo">Cadena con el archivo de texto completo</param>
        /// <param name="cfg">Parámetros de configuración de archivo</param>
        /// <returns>Datatable con los registros procesados</returns>
        public static DataTable ObtieneArchivoLimpio(Int64 IdArchivo, String elArchivo, Archivo cfg)
        {
            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            String FechaAut = String.Empty;
            int offset = 0;

            try
            {
                using (StreamReader sr = new StreamReader(elArchivo, Encoding.UTF8))
                {
                    while (!sr.EndOfStream)
                    {
                        String line = sr.ReadLine();

                        if (Regex.Match(line, @"^Fecha:[\s]{3}[0-9]{2}[/]").Success)
                        {
                            //Fecha:   01/10/19
                            FechaAut = line.Substring(9, 6).Trim() + "20" + line.Substring(15, 2).Trim();
                        }

                        else if (Regex.Match(line, @"^[\s]{1}[0-9]{7}[\s]{1,2}[A-Z]{1}[\s]{1,15}").Success)
                        {
                            if (dt.Rows.Count == 0)
                            {
                                dt.Columns.Add("ID_Archivo");
                                dt.Columns.Add("FechaAut");
                                dt.Columns.Add("NumAut");
                                dt.Columns.Add("EstatusAut");
                                dt.Columns.Add("Monto");
                                dt.Columns.Add("Moneda");
                                dt.Columns.Add("TipoOp");
                                dt.Columns.Add("Afiliado");
                                dt.Columns.Add("Cuenta");
                                dt.Columns.Add("Tarjeta");
                                dt.Columns.Add("Remision");
                                dt.Columns.Add("Usuario");
                            }

                            DataRow row = dt.NewRow();
                            row["ID_Archivo"] = IdArchivo.ToString();
                            row["FechaAut"] = FechaAut;
                            offset = cfg.LongitudC1;

                            try
                            {
                                row["NumAut"] = line.Substring(0, offset).Trim();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("LimpiarArchivo()_AUT", ex);
                            }

                            try
                            {
                                row["EstatusAut"] = line.Substring(offset, cfg.LongitudC2).Trim();
                                offset += cfg.LongitudC2;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("LimpiarArchivo()_estatusAut", ex);
                            }

                            try
                            {
                                row["Monto"] = line.Substring(offset, cfg.LongitudC3).Trim().Replace(",", "");
                                offset += cfg.LongitudC3;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("LimpiarArchivo()_Monto", ex);
                            }

                            try
                            {
                                row["Moneda"] = line.Substring(offset, cfg.LongitudC4).Trim();
                                offset += cfg.LongitudC4;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("LimpiarArchivo()_Moneda", ex);
                            }

                            try
                            {
                                row["TipoOp"] = line.Substring(offset, cfg.LongitudC5).Trim();
                                offset += cfg.LongitudC5;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("LimpiarArchivo()_tipoOp", ex);
                            }

                            try
                            {
                                row["Afiliado"] = line.Substring(offset, cfg.LongitudC6).Trim().Replace(" ", "");
                                offset += cfg.LongitudC6;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("LimpiarArchivo()_Afiliado", ex);
                            }

                            try
                            {
                                row["Cuenta"] = line.Substring(offset, cfg.LongitudC7).Trim().Replace(" ", "");
                                offset += cfg.LongitudC7;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("LimpiarArchivo()_Cuenta", ex);
                            }

                            try
                            {
                                row["Tarjeta"] = line.Substring(offset, cfg.LongitudC8).Trim().Replace(" ", "");
                                offset += cfg.LongitudC8;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("LimpiarArchivo()_Tarjeta", ex);
                            }

                            try
                            {
                                row["Remision"] = line.Substring(offset, cfg.LongitudC9).Trim();
                                offset += cfg.LongitudC9;
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("LimpiarArchivo()_Remision", ex);
                            }

                            try
                            {
                                row["Usuario"] = line.Substring(offset, line.Length - offset).Trim();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("LimpiarArchivo()_Usuario", ex);
                            }

                            dt.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logueo.Error(ex.Message);
                throw new Exception("OCURRIO UN ERROR AL DEPURAR REGISTROS DEL ARCHIVO", ex);
            }

            return dt;
        }

        /// <summary>
        /// Establece las condiciones de validación para crear una nuevo registro de archivo en base de datos
        /// controlando la transacción (commit o rollback)
        /// </summary>
        /// <param name="NombreComercial">Nombre comercial</param>
        /// <param name="elUsuario">Usuario en sesión</param>
        public static Int64 CreaRegistroArchivo(string NombreArchivo)
        {
            Int64 id = -1;
            SqlConnection conn = null;

            SqlConnection cx = new SqlConnection(BDOperacionesEvertec.strBDEscrituraArchivo);
            if (null != cx && ConnectionState.Open == cx.State)
            {
                cx.Close();
            }

            try
            {
                conn = BDOperacionesEvertec.BDEscritura;

                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        id = DAOArchivo.InsertaArchivoEnProceso(NombreArchivo, conn, transaccionSQL);
                        transaccionSQL.Commit();
                    }

                    catch (Exception err)
                    {
                        transaccionSQL.Rollback();
                        Logueo.Error("ERROR: CreaRegistroArchivo()|ARCHIVO: " + NombreArchivo + ": " + err.Message);
                    }
                }
            }

            catch (Exception ex)
            {
                Logueo.Error("CreaRegistroArchivo():" + ex.Message);
            }

            finally
            {
                if (null != conn && ConnectionState.Open == conn.State)
                {
                    conn.Close();
                }
            }

            return id;
        }

        /// <summary>
        /// Establece las condiciones de validación para registrar en bitácora de base de datos
        /// un archivo recibido vacío, controlando la transacción (commit o rollback)
        /// </summary>
        /// <param name="IdArchivo">Identificador del archivo</param>
        public static void RegistraArchivoVacio(Int64 IdArchivo)
        {
            SqlConnection conn = null;

            SqlConnection cx = new SqlConnection(BDOperacionesEvertec.strBDEscrituraArchivo);
            if (null != cx && ConnectionState.Open == cx.State)
            {
                cx.Close();
            }

            try
            {
                conn = BDOperacionesEvertec.BDEscritura;

                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        DAOArchivo.InsertaArchivoVacioEnBitacora(IdArchivo, conn, transaccionSQL);
                        transaccionSQL.Commit();
                    }

                    catch (Exception err)
                    {
                        transaccionSQL.Rollback();
                        Logueo.Error("ERROR AL REGISTRAR ARCHIVO VACIO EN BITACORA| ID_ARCHIVO: " +
                            IdArchivo.ToString() + ": " + err.Message);
                    }
                }
            }

            catch (Exception ex)
            {
                Logueo.Error("RegistraArchivoVacio():" + ex.Message);
            }

            finally
            {
                if (null != conn && ConnectionState.Open == conn.State)
                {
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Establece las condiciones de validación para actualizar el estatus de procesamiento 
        /// de un archivo en base de datos, controlando la transacción (commit o rollback)
        /// </summary>
        /// <param name="unArchivo">DataTable con los registros del archivo</param>
        /// <param name="IdArchivo">Identificador del archivo</param>
        public static bool InsertaNuevoArchivodetalle(DataTable unArchivo, Int64 IdArchivo)
        {
            SqlConnection conn = null;
            bool insertaOK = false;

            SqlConnection cx = new SqlConnection(BDOperacionesEvertec.strBDEscrituraArchivo);
            if (null != cx && ConnectionState.Open == cx.State)
            {
                cx.Close();
            }

            try
            {
                conn = BDOperacionesEvertec.BDEscritura;

                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        DAOArchivo.InsertaDetalleArchivo(unArchivo, IdArchivo, conn, transaccionSQL);
                        transaccionSQL.Commit();
                        insertaOK = true;
                    }

                    catch (Exception err)
                    {
                        transaccionSQL.Rollback();
                        Logueo.Error("ERROR AL INSERTAR NUEVO ARCHIVO DETALLE:| ID_ARCHIVO: " +
                            IdArchivo.ToString() + ": " + err.Message);
                    }
                }
            }

            catch (Exception ex)
            {
                Logueo.Error("InsertaNuevoArchivodetalle():" + ex.Message);
            }

            finally
            {
                if (null != conn && ConnectionState.Open == conn.State)
                {
                    conn.Close();
                }
            }

            return insertaOK;
        }


        /// <summary>
        /// Establece las condiciones de validación para actualizar el estatus de procesamiento 
        /// de un archivo en base de datos, controlando la transacción (commit o rollback)
        /// </summary>
        /// <param name="IdArchivo">Identificador del archivo</param>
        /// <param name="Estatus">Bandera con el estatus de procesamiento del archivo</param>
        public static void ActualizaEstatusArchivo(Int64 IdArchivo, int Estatus)
        {
            SqlConnection conn = null;

            SqlConnection cx = new SqlConnection(BDOperacionesEvertec.strBDEscrituraArchivo);
            if (null != cx && ConnectionState.Open == cx.State)
            {
                cx.Close();
            }

            try
            {
                conn = BDOperacionesEvertec.BDEscritura;

                using (SqlTransaction transaccionSQL = conn.BeginTransaction())
                {
                    try
                    {
                        DAOArchivo.ActualizaEstatusArchivo(IdArchivo, Estatus, conn, transaccionSQL);
                        transaccionSQL.Commit();
                    }

                    catch (Exception err)
                    {
                        transaccionSQL.Rollback();
                        Logueo.Error("ERROR AL ACTUALIZAR EL ESTATUS DEL ARCHIVO:| ID_ARCHIVO: " +
                            IdArchivo.ToString() + ": " + err.Message);
                    }
                }
            }

            catch (Exception ex)
            {
                Logueo.Error("ActualizaEstatusArchivo():" + ex.Message);
            }

            finally
            {
                if (null != conn && ConnectionState.Open == conn.State)
                {
                    conn.Close();
                }
            }
        }

        //
        protected static void moverArchivo(string pathOrigen, string pathDestino)
        {

            if (!File.Exists(pathDestino))
            {
                File.Move(pathOrigen, pathDestino);
            }
            else
            {
                int numero = 0;
                bool validador = true;
                while (validador)
                {
                    if (numero == 0)
                    {
                        string[] list = pathDestino.Split('\\');
                        string nombreEliminar = list.Last();
                        string[] nom = nombreEliminar.Split('.');
                        string nombre = nom.First() + " - copia." + nom.Last();

                        string nvoPath = pathDestino.Replace(nombreEliminar, nombre);
                        validador = !renombrarArchivo(pathOrigen, nvoPath);
                        numero = numero + 1;
                    }
                    else
                    {
                        numero = numero + 1;
                        string[] list = pathDestino.Split('\\');
                        string nombreEliminar = list.Last();
                        string[] nom = nombreEliminar.Split('.');
                        string nombre = nom.First() + " - copia (" + numero + ")." + nom.Last();

                        string nvoPath = pathDestino.Replace(nombreEliminar, nombre);
                        validador = !renombrarArchivo(pathOrigen, nvoPath);
                    }
                }
            }
        }

        //
        protected static bool renombrarArchivo(string pathOrigen, string pathDestino)
        {
            if (!File.Exists(pathDestino))
            {
                File.Move(pathOrigen, pathDestino);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
