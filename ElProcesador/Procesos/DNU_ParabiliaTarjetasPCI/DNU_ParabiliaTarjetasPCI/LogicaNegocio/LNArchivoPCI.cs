
// -----------------------------------------------------------------------
// <copyright file="LNArchivoPCI.cs" company="DNU">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DNU_ParabiliaTarjetasPCI.LogicaNegocio
{
    using CommonProcesador;
    using DNU_ParabiliaTarjetasPCI.Entidades;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Text;
    using System.Threading;
    using BaseDatos;
    using System.Collections.Generic;
    using System.Linq;
    using System.Globalization;

    public class LNArchivoPCI
    {
        private string m_directorio;
        private string m_directorioSalida;

        private string m_tipoArchivo;
        private string m_nombreArchivo;


        const string SALIDA = "PROCESASDOS";


        private const string TERMINACION1 = "a";
        private const string TERMINACION2 = "b";
        private const string TERMINACION3 = "c";
        private const string TERMINACION4 = "d";

         private const string  CLASE  = "LNArchivoPCI";

        public string TipoArchivo { get => m_tipoArchivo; set => m_tipoArchivo = value; }
        public string NombreArchivo { get => m_nombreArchivo; set => m_nombreArchivo = value; }

        /// <summary>
        /// Constructor sobrecarga
        /// </summary>
        public LNArchivoPCI(string directorio)
        {
            this.m_directorio = directorio;

        }

        /// <summary>
        /// Constructor 
        /// </summary>
        public LNArchivoPCI()
        {

        }


        /// <summary>
        /// Metodo que detona el inicio de proceso
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void NuevoArchivo(object sender, FileSystemEventArgs e)
        {
            const string METODO = "NuevoArchivo";
            WatcherChangeTypes elTipoCambio = e.ChangeType;
            Logueo.Evento("[PRPROCESARTARPCI ]Hubo un Cambio [" + elTipoCambio.ToString() + "] en el directorio: " + PNConfig.Get("PRPROCESARTARPCI", "DirectorioEntrada") + " el se recibio el archivo : " + e.FullPath);
            
            Logueo.Evento("[PRPROCESARTARPCI] INICIO DE PROCESO DEL ARCHIVO:" + e.FullPath);
            validarArchivos(true);
        }


        /// <summary>
        /// Valida la existencia de Archivos
        /// </summary>
        /// <param name="copiaArchivos"></param>
        /// <returns></returns>
        internal bool validarArchivos(bool copiaArchivos)
        {
            const string METODO = "validarArchivos";
            Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  Inicia Proceso.. Validacion de archivos en el directorio", CLASE, METODO));

  
            ArchivoConfiguracion elArchivos = new ArchivoConfiguracion();
            
            Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  Inicializa contexto Procesador", CLASE, METODO));
            
            


            TipoArchivo = elArchivos.TipoArchivo;
            NombreArchivo = elArchivos.Nombre;

            int numArchivos = 0;


            if (copiaArchivos)
            {
                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  Inicia el  proceso de espera para el copiado...", CLASE, METODO));
                Thread.Sleep(1000 * 60 * 1);
                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  termina el proceso de espera para el copiado ", CLASE, METODO));
            }


            DirectoryInfo directory = new DirectoryInfo(m_directorio);
            FileInfo[] files = directory.GetFiles(elArchivos.Nombre + "*" + elArchivos.TipoArchivo);

            string archivo1 = elArchivos.Nombre + TERMINACION1 + elArchivos.TipoArchivo;
            string archivo2 = elArchivos.Nombre + TERMINACION2 + elArchivos.TipoArchivo;
            string archivo3 = elArchivos.Nombre + TERMINACION3 + elArchivos.TipoArchivo;
            string archivo4 = elArchivos.Nombre + TERMINACION4 + elArchivos.TipoArchivo;

            bool banderaArchivo1 = false;
            bool banderaArchivo2 = false;
            bool banderaArchivo3 = false;
            bool banderaArchivo4 = false;

            if (files.Length == 4 || files.Length == 3 )
            {
                foreach (var item in files)
                {
                    if (archivo1 == item.Name) 
                     { 
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Archivo Encontrado..{2}", CLASE, METODO,item.Name));
                        numArchivos += 1;
                        banderaArchivo1 = true;
                    }
                        

                    if (archivo2 == item.Name) 
                    {
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Archivo Encontrado..{2}", CLASE, METODO, item.Name));
                        numArchivos += 1;
                        banderaArchivo2 = true;

                    }

                    if (archivo3 == item.Name) 
                        {
                        
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Archivo Encontrado..{2}", CLASE, METODO, item.Name));
                        numArchivos += 1;
                        banderaArchivo3 = true;

                    }
                    if (archivo4 == item.Name)
                    {

                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Archivo Encontrado..{2}", CLASE, METODO, item.Name));
                        numArchivos += 1;
                        banderaArchivo4 = true;

                    }

                }
                if (!banderaArchivo3) 
                           Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()No se deposito el archivo IDS Externos ... archivo Opcional", CLASE, METODO));
                
                if (numArchivos == 4 || numArchivos == 3  && banderaArchivo1 == true && banderaArchivo2 ==true  && banderaArchivo4 ==true  )
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()Cantidad de archivos correctos  y validacion satisfactoria..", CLASE, METODO));
                    
                }
                else 
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() No se puedieron encontrar todos los archivos verifique que existan", CLASE, METODO));
                    return false;
                }




                //LimpiarDirectorio en los procesados
                System.IO.DirectoryInfo di = new DirectoryInfo(m_directorioSalida);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }


                ///Mover los archivos a una ruta para inicial  ReProceso sin que exista algun problema....
                string fechaEntrada = DateTime.Now.ToString("yyyy-mm-dd-h:mm:ss").Replace(":", ".");

                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Se moveran los archivos de directorio para inicial el proceso de lectura", CLASE, METODO));

                for (int i = 0; i < files.Length; i++)

                {
                    try
                    {
                        if (files[i].Name == archivo1)
                        {
                            File.Move(files[i].FullName, m_directorioSalida + "\\" + elArchivos.Nombre + TERMINACION1 + "_PROCESADO_" + fechaEntrada + elArchivos.TipoArchivo);
                        }
                        if (files[i].Name == archivo2)
                        {
                            File.Move(files[i].FullName, m_directorioSalida + "\\" + elArchivos.Nombre + TERMINACION2 + "_PROCESADO_" + fechaEntrada + elArchivos.TipoArchivo);
                        }

                        if (files[i].Name == archivo3)
                        {
                            File.Move(files[i].FullName, m_directorioSalida + "\\" + elArchivos.Nombre + TERMINACION3 + "_PROCESADO_" + fechaEntrada + elArchivos.TipoArchivo);
                        }
                        if (files[i].Name == archivo4)
                        {
                            File.Move(files[i].FullName, m_directorioSalida + "\\" + elArchivos.Nombre + TERMINACION4 + "_PROCESADO_" + fechaEntrada + elArchivos.TipoArchivo);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() Error : al mover los archivos de directorio ", CLASE, METODO));
                        Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() Error :{2} ", CLASE, METODO, ex.Message));

                        return false;
                    }
                }

                DirectoryInfo directory2 = new DirectoryInfo(m_directorioSalida);
                FileInfo[] files2 = directory2.GetFiles(elArchivos.Nombre + "*" + elArchivos.TipoArchivo);



                if (procesarArchivos(files2, archivo1, archivo2, archivo3, archivo4, elArchivos.Nombre, banderaArchivo3))
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  Proceso de Archivos completado con exito..", CLASE, METODO));
                    return true;
                }
                else
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() No se encuentran los archivos necesarios para el proceso..", CLASE, METODO));
                    return false;
                }
            }
            else 
            {
                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() No se pudo completar el procesado de los archivos...", CLASE, METODO));
                return false;
            }



            

        }

        /// <summary>
        /// Metodo que  inicia el ´proceso de los archivoss
        /// </summary>
        /// <param name="files"></param>
        /// <param name="nombreArchivo1"></param>
        /// <param name="nombreArchivo2"></param>
        /// <param name="nombreArchivo3"></param>
        /// <returns></returns>
        public bool procesarArchivos(FileInfo[] files, string nombreArchivo1, string nombreArchivo2, string nombreArchivo3,string nombreArchivo4, string nombre, bool banderaArchivo3)
        {
            const string METODO = "procesarArchivos";
            #region Variables
            

            BaseDatos.DAORegistrarTarjetas Registro = new BaseDatos.DAORegistrarTarjetas();
            EntidadesContratos contratos = new EntidadesContratos();

            DataTable InfTarjetasArchivo = new DataTable();  // informacion de tarjetas
            DataTable Consecutivo = new DataTable();    ///consecutivo para el insert

            string claveEmisor = string.Empty;
            string[] contenidos = new string[1];    ///  emisor y numero de tarjetas

            DataTable IDClientes = new DataTable();

            DataTable NIPS = new DataTable();
            #endregion

            Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Inicia el  procesamiento  de los archivos.", CLASE, METODO));
            try
            {
                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Obtener consecutivo de BD.", CLASE, METODO));
                Consecutivo = Registro.ObtenerRegistroArchivoConsecutivo();

                if (Consecutivo == null || Consecutivo.Rows.Count == 0) 
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() No se pudo obtener el consecutivo.", CLASE, METODO));
                    return false;
                }
                   

                for (int i = 0; i < files.Length; i++)
                {

                    if (obtenername(files[i].Name) == nombreArchivo1)
                    {
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Inicia Lectura. {2}", CLASE, METODO,files[i].Name));
                        InfTarjetasArchivo = LecturaArchivoTarjetas(files[i], Consecutivo);

                    }
                    else if (obtenername(files[i].Name) == nombreArchivo2)
                    {
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Inicia Lectura. {2}", CLASE, METODO, obtenername(files[i].Name)));
                        contenidos = lecturaContenido(files[i]);

                    }
                    else if (obtenername(files[i].Name) == nombreArchivo3)
                    {
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Inicia Lectura. {2}", CLASE, METODO, obtenername(files[i].Name)));
                        IDClientes = lecturaClientes(files[i], out claveEmisor, Consecutivo.Rows[0]["Consecutivo"].ToString());
                    }
                    else if (obtenername(files[i].Name) == nombreArchivo4)
                    {
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Inicia Lectura. {2}", CLASE, METODO, obtenername(files[i].Name)));
                        NIPS = lecturaNip(files[i], Consecutivo.Rows[0]["Consecutivo"].ToString());
                    }
                }
                if (banderaArchivo3) 
                    if(IDClientes.Rows.Count > 0)
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Lectura Correcta  PCI-Tarjetas_c", CLASE, METODO));
                    else 
                    {
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Erro al obtener  PCI-Tarjetas_c", CLASE, METODO));
                        return false;
                    }
                else
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() PCI-Tarjetas_c ,No se encuentra el archivo (Opcional)", CLASE, METODO));


                if (InfTarjetasArchivo != null && contenidos != null  && NIPS.Rows.Count > 0)
                {
                

                    DataTable tableContratos = Registro.ConsultarParametrosPorClaveEmpresa(contenidos[0]);
                    if (!BusquedaSinErrores(tableContratos)) 
                    {
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Error al obtener los parametros de los contratos", CLASE, METODO));
                        return false;
                    }
                     contratos = ObtenerDatosContratos(tableContratos);
                    
                    
                    if (banderaArchivo3)
                        if (IDClientes.Rows.Count  == InfTarjetasArchivo.Rows.Count)
                            Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Afectar tablas PCI-Tarjetas_c  (MATCH)  - PCI-tarjetas_a", CLASE, METODO));
                        else
                        {
                            Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Afectar tablas PCI-Tarjetas_c  (NO MATCH) - PCI-tarjetas_a", CLASE, METODO));
                            return false;
                        }
                    else
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() PCI-Tarjetas_c ,No se encuentra el archivo (Opcional)", CLASE, METODO));

                    if (Convert.ToInt32(InfTarjetasArchivo.Rows.Count) == Convert.ToInt32(contenidos[1])  && NIPS.Rows.Count == Convert.ToInt32(InfTarjetasArchivo.Rows.Count))
                    {
                       if(realizarMatchTarjetas(NIPS, InfTarjetasArchivo)){
                            Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Inicia el proceso de afectar tablas", CLASE, METODO));
                            if (Registro.AfectarTablas(InfTarjetasArchivo, contenidos, Consecutivo.Rows[0]["Consecutivo"].ToString(), IDClientes, NIPS,banderaArchivo3))
                            {

                                if (generarArchivo(InfTarjetasArchivo, contratos, IDClientes, NIPS, nombre))
                                {
                                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()EL ARCHIVO SE GENERO CORRECTAMENTE...", CLASE, METODO));
                                    return true;
                                }
                                else
                                {
                                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()Error al generar el archivo", CLASE, METODO));
                                    return false;
                                }

                            }
                        }
                        else 
                        {
                            return false;
                        }

                    }
                    else
                    {
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() Las tarjetas  no coinciden entre archivos verifique el numero de registro", CLASE, METODO));
                        return false;
                    }

                }
                else
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() No se obtuvo correctamnete la informacion de los archivos, valide formato", CLASE, METODO));
                    return false;
                }

            }
            catch (Exception ex)
            {
                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() ERROR : {2}", CLASE, METODO,ex.Message));
                return false;
            }
            return true;
        }
        /// <summary>
        /// Metodo que valida cadenas vacias
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        public string validarCadenasNulas(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return string.Empty;
            if (string.IsNullOrWhiteSpace(valor))
                return string.Empty;

            return valor.ToString();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public  string obtenername(string name) 
        {
            const string METODO = "obtenername";
            try 
            {

                string nuevoName = string.Empty;

                nuevoName = (name.Substring(0, name.IndexOf("_")+2))+TipoArchivo;
                return nuevoName;
            }
            catch(Exception Ex) 
            {
                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}()  Error  : {2} ", CLASE, METODO,Ex));
            }
            return string.Empty;
        
        }


         private bool realizarMatchTarjetas(DataTable   nips , DataTable tarjetas) 
            {
            const string METODO = "realizarMatchTarjetas";
            string tarjeta = string.Empty;
            string nip = string.Empty;
            int correctas = 0;
            if(tarjetas.Rows.Count == nips.Rows.Count) 
            {
                for (int i = 0; i < tarjetas.Rows.Count; i++)
                {
                    tarjeta = tarjetas.Rows[i]["ClaveMedioAcceso"].ToString().Trim();
                    for (int j = 0; j < nips.Rows.Count; j++)
                    {
                        nip = nips.Rows[j]["ClaveMedioAcceso"].ToString().Trim();   
                        if(tarjeta == nip) 
                        {
                            correctas += 1;
               
                        }

                    }

                }
                 if(tarjetas.Rows.Count == correctas && tarjetas.Rows.Count == correctas) 
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  MATCH DE TARJETAS Y  TARJETAS-NIPS CORRECTO ", CLASE, METODO));

                    return true;
                }
                else 
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() NO HAY MARCH DE ARCHIVOS DE TARJETAS-INFO -NIPS ", CLASE, METODO));
                    return false;
                }

            }
            else 
            {
                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() NO HAY MATCH  ", CLASE, METODO));

                return false;
            }
               
            }   




        /// <summary>
        /// Metodo que obtiene los valores de los contratos
        /// </summary>
        /// <param name="tablaContratos"></param>
        /// <returns></returns>
        private EntidadesContratos ObtenerDatosContratos(DataTable tablaContratos)
        
        {
            const string METODO = "ObtenerDatosContratos";
            Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  Inicia lectura de contratos. ", CLASE, METODO));

            EntidadesContratos contratos = new EntidadesContratos();
            foreach (DataRow renglon in tablaContratos.Rows)
            {
                if (renglon["Nombre"].ToString() == "@RutaSalidaArchivo")
                    contratos.RutaSalida = validarCadenasNulas(renglon["valor"].ToString());
                else if (renglon["Nombre"].ToString() == "@IDCliente")
                    contratos.IDCliente = validarCadenasNulas(renglon["valor"].ToString());
                else if (renglon["Nombre"].ToString() == "@Token")
                    contratos.Token = validarCadenasNulas(renglon["valor"].ToString());
                else if (renglon["Nombre"].ToString() == "@CVV")
                    contratos.Cvv = validarCadenasNulas(renglon["valor"].ToString());
                else if (renglon["Nombre"].ToString() == "@NIP")
                    contratos.Nip = validarCadenasNulas(renglon["valor"].ToString());
                else if (renglon["Nombre"].ToString() == "@CLABE")
                    contratos.Clabe = validarCadenasNulas(renglon["valor"].ToString());
                else if (renglon["Nombre"].ToString() == "@Fecha_vencimiento")
                    contratos.FechaVencimiento = validarCadenasNulas(renglon["valor"].ToString());
                else if (renglon["Nombre"].ToString() == "@CACAO")
                    contratos.Cacao = validarCadenasNulas(renglon["valor"].ToString());
            }

            return contratos;




        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OcurrioError(Object sender, ErrorEventArgs e)
        {
            const string METODO = "OcurrioError";
            Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  ERROR :  "+e.GetException().Message.ToString(), CLASE, METODO));
        }


        /// <summary>
        /// Metodo que realiza  la busqueda de errores en base de datos
        /// </summary>
        /// <param name="tabla"></param>
        /// <returns></returns>
        public bool BusquedaSinErrores(DataTable tabla)
        {
            const string METODO = "BusquedaSinErrores";
            bool retornoBooleano = true;
            if (tabla.Columns.Count == 2 || tabla.Columns.Count == 3 || tabla.Columns.Count == 4)
            {
                if (tabla.Columns.Count == 2 && (tabla.Rows[0][0].ToString() == "error" || tabla.Rows[0][0].ToString() == "errorExcepcion"))
                {
                    if (tabla.Rows[0][0].ToString() == "error")
                    {

                        //     Logueo.Error("[" + idLog + "]" + JsonConvert.SerializeObject(errores));
                    }
                    else
                    {

                        //   Logueo.Error("[" + idLog + "]" + JsonConvert.SerializeObject(errores) + " Exception:" + tabla.Rows[0][1].ToString());
                    }
                    retornoBooleano = false;
                }
                else
                {
                    if (tabla.Rows[0][0].ToString() == "error" || tabla.Rows[0][0].ToString() == "errorExcepcion")
                    {
                        try
                        {

                        }
                        catch (Exception ex)
                        {
                            Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() ERROR {2}", CLASE, METODO,ex.Message));
                        }

                        retornoBooleano = false;
                    }

                }
            }
            return retornoBooleano;
        }



        /// <summary>
        /// Metodo que inicia el proceso del archivo
        /// </summary>
        /// <param name="tarjetas"></param>
        /// <param name="contratos"></param>
        /// <param name="idClientes"></param>
        /// <returns></returns>
        private bool generarArchivo(DataTable tarjetas, EntidadesContratos contratos, DataTable idClientes, DataTable NIPS , string nombre)
        {
            const string METODO = "generarArchivo";
            #region variables
            string consecutivo = string.Empty;
            string fechaVencimiento = string.Empty;
            string clabe = string.Empty;
            string NIP = string.Empty;
            string CVV = string.Empty;
            string Tarjeta = string.Empty;
            string token = string.Empty;
            string idCliente = string.Empty;
            string cacao = string.Empty;
            string tarjeta = string.Empty;

            #endregion

            Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()   Inicia el  proceso de generacion de archivo.. : " , CLASE, METODO));
            try 
            {
                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  Creando encabezado.. ", CLASE, METODO));

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}",
                    "IDConsecutivo".PadRight(15),
                    "Numero Tarjeta".PadRight(30),
                    "Vencimiento".PadRight(15),
                    "CVV".PadRight(10),
                    "NIP".PadRight(10),
                    "No.CACAO".PadRight(30),
                    "No.CLABE".PadRight(30),
                    "IDCliente".PadRight(50),
                    "TOKEN".PadRight(50)
                    ));

                sb.AppendLine("---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");

                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()  Inicia iteracion de archivos leidos. ", CLASE, METODO));
                for (int i = 0; i < tarjetas.Rows.Count; i++)
                {

                    tarjeta = MascaraNumeroTarjeta(tarjetas.Rows[i]["ClaveMedioAcceso"].ToString()).PadRight(30);

                    if (Convert.ToBoolean(contratos.FechaVencimiento))
                        fechaVencimiento = tarjetas.Rows[i]["Fecha"].ToString().PadRight(15);
                    else
                        fechaVencimiento = "".PadRight(15);

                    if (Convert.ToBoolean(contratos.Cvv))
                        CVV = tarjetas.Rows[i]["ValorD1"].ToString().PadRight(10);
                    else
                        CVV = "".PadRight(10);

                    if (Convert.ToBoolean(contratos.Nip))
                        NIP = ObtenerNIP(NIPS, tarjetas.Rows[i]["ClaveMedioAcceso"].ToString()).PadRight(10);
                    else
                        NIP = "".PadRight(10);

                    if (Convert.ToBoolean(contratos.Cacao))
                        cacao = obtieneMedioAccesoDeterminado("CACAO", tarjetas.Rows[i]["ClaveMedioAcceso"].ToString()).PadRight(30);
                    else
                        cacao = "".PadRight(20);

                    if (Convert.ToBoolean(contratos.Clabe))
                        clabe = obtieneMedioAccesoDeterminado("CLABE", tarjetas.Rows[i]["ClaveMedioAcceso"].ToString()).PadRight(30);
                    else
                        clabe = "".PadRight(20);
                        try
                        {
                        if (Convert.ToBoolean(contratos.IDCliente))
                        {
                            idCliente = idClientes.Rows[i]["Id_Externo"].ToString().PadRight(30);
                        }
                        else
                        { 
                                idCliente = "".PadRight(30);
                        }
                    }
                    catch (Exception)
                        {
                        idCliente = "".PadRight(30);
                        }

                    if (Convert.ToBoolean(contratos.Token))
                        token = obtieneTokenTarjeta(tarjetas.Rows[i]["ClaveMedioAcceso"].ToString().PadRight(50));
                    else
                        token = "".PadRight(50);

                    sb.AppendLine(
                        string.Format
                        ("{0}{1}{2}{3}{4}{5}{6}{7}{8}",
                    tarjetas.Rows[i]["Id_ArchivoCP"].ToString().PadRight(15),
                    tarjeta,
                    fechaVencimiento,
                    CVV,
                    NIP,
                    cacao,
                    clabe,
                    idCliente,
                    token));

                }
                string horaProceso = "_"+ DateTime.Now.ToString("yyyy-MM-dd-h:mm:ss").Replace(":", "."); 
                string rutaSalida = @contratos.RutaSalida;
                try
                {
                    if (!Directory.Exists(rutaSalida))
                        Directory.CreateDirectory(rutaSalida);

                    using (StreamWriter sw = File.CreateText(contratos.RutaSalida +
                                                                 NombreArchivo +
                                                                 SALIDA +
                                                                 horaProceso +
                                                                 TipoArchivo)
                                            )
                    {

                        sw.WriteLine(sb.ToString());
                     

                    }
                }
                catch (Exception ex)
                {
                    Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() ERROR al generar el archivo  : {2}", CLASE, METODO, ex.Message));
                    return false;


                }
                finally
                {
                    sb.Clear();
                }
            }
            catch (Exception ex) 
            {
                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() ERROR  : {2}", CLASE, METODO, ex.Message));
                return false;
            }
           

            return true;
        }





        /// <summary>
        /// Metodo que obtiene el nip aunque no este secuenciado dentro del archivo
        /// </summary>
        /// <param name="NIP"></param>
        /// <param name="valorBuscado"></param>
        /// <returns></returns>
        private  string ObtenerNIP(DataTable NIP , string valorBuscado) 
        {
            string nipBuscado = string.Empty;
            DataRow[] rows = NIP.Select("ClaveMedioAcceso = "+valorBuscado);

            if (rows.Length > 0)
            {
                DataRow row = rows[0];
                nipBuscado = row["ValorD2"].ToString().Trim();
                return nipBuscado;
            }
            return string.Empty;

        }


        /// <summary>
        /// Metodo que inicial la lectura del archivo conn la informacion de las tarjetas
        /// </summary>
        /// <param name="files"></param>
        public DataTable LecturaArchivoTarjetas(FileInfo files, DataTable Consecutivo) 
        {
            const string METODO = "LecturaArchivoTarjetas";
            string line = string.Empty;
            string[] arregloFechas ;
            DataTable dt = new DataTable("Tarjetas");
            
            dt.Columns.Add(new DataColumn("Id_ArchivoCP"));
            dt.Columns.Add(new DataColumn("ClaveMedioAcceso"));
            dt.Columns.Add(new DataColumn("Fecha"));
            dt.Columns.Add(new DataColumn("ValorD1")); //CVV
            dt.Columns.Add(new DataColumn("ID_Registro"));
            dt.DefaultView.Sort = "Id_ArchivoCP  ASC";
            try
            {
                StreamReader file = new System.IO.StreamReader(files.FullName);
                while ((line = file.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {

                        DataRow row = dt.NewRow();
                        row["Id_ArchivoCP"] = Convert.ToInt32(line.Substring(0, line.IndexOf("$")));

                        row["ClaveMedioAcceso"] = line.Substring(
                                                                 PosicionInicial(line.IndexOf("$")),
                                                                 PosicionFinal(line.IndexOf("$"), line.IndexOf("*"))
                                                                                                                  ).Replace(" ", "");
                        //"22/11/2009","yyyy-MM-dd",null
                        int dia = 30;
                        arregloFechas = line.Substring(PosicionInicial(line.IndexOf("*")),
                                                                PosicionFinal(line.IndexOf("*"),
                                                                line.IndexOf(")"))).Split(new string[] { "/" }, StringSplitOptions.None);



                        int mes = Convert.ToInt32(arregloFechas[0]);
                        int anio = CultureInfo.CurrentCulture.Calendar.ToFourDigitYear(Convert.ToInt32(arregloFechas[1]));

                        DateTime fecha = new DateTime(anio, mes, dia);

                        string fechaConversa = DateTime.ParseExact(fecha.ToString("yyyy-MM-dd"), "yyyy-MM-dd", null).ToString("yyyy-MM-dd");

                        row["Fecha"] = fechaConversa.ToString();

                        row["ValorD1"] = line.Substring(
                                                    PosicionInicial(line.IndexOf(":")),
                                                    PosicionFinal(line.IndexOf(":"), line.IndexOf("\"")));




                        row["ID_Registro"] = Convert.ToInt32(Consecutivo.Rows[0]["Consecutivo"]);


                        dt.Rows.Add(row);
                    }
                }
                return dt;

            }
            catch (Exception ex)
            {
                Logueo.Error (string.Format("[PRPROCESARTARPCI].{0}.{1}() ERROR : {2}", CLASE, METODO, ex.Message));
                dt = null;
                return dt;
            }
            finally 
            {
            
            
            }

        }

        /// <summary>
        /// metodo que realiza la lectura del archivo donde esta el emisor y el numero de tarjetas contenidas en el mismo archivo
        /// </summary>
        /// <param name="files"></param>
        public string[] lecturaContenido(FileInfo files)
        {
            string line = string.Empty;
            string[] arrayContenido = new string[1];

            StreamReader file = new System.IO.StreamReader(files.FullName);
            
            while ((line = file.ReadLine()) != null)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    arrayContenido = line.Split(new string[] {","},StringSplitOptions.None);
                }
            }
            return arrayContenido;
        }
        public DataTable lecturaNip(FileInfo files, string registro) 
        {
            const string METODO = "lecturaNip";
            string line = string.Empty;
           
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("ClaveMedioAcceso"));
            dt.Columns.Add(new DataColumn("ValorD2"));
            dt.Columns.Add(new DataColumn("Id_Registro"));
            try
            {
                StreamReader file = new System.IO.StreamReader(files.FullName);
                while ((line = file.ReadLine()) != null)
                {
                    

                    if (!string.IsNullOrEmpty(line))
                    {
                        DataRow row = dt.NewRow();

                        row["ClaveMedioAcceso"] = line.Split(',')[0].ToString().Trim().Replace(" ","");
                        row["ValorD2"] = line.Split(',')[1].ToString().Trim(); //NIP
                        row["Id_Registro"] = registro.Trim();

                        dt.Rows.Add(row);

                    
                    }
                }
            }
            catch (Exception ex)
            {
                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}() ERROR : Al leer el archivo ", CLASE, METODO));
                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() ERROR : {2}", CLASE, METODO, ex.Message));
                
            }
            return dt;


        }
        public  DataTable lecturaClientes(FileInfo files , out string  claveEmisor ,string  registro)
        {
            const string METODO = "lecturaClientes";
            int primeraLinea = 1;
            int consecutivo = 1;
            string line = string.Empty;
            claveEmisor = string.Empty;
            StreamReader file = new System.IO.StreamReader(files.FullName);


            DataTable dt = new DataTable("NumerosIdClienteTarjeta");

            dt.Columns.Add(new DataColumn("Id_ArchivoCIE"));
            dt.Columns.Add(new DataColumn("Id_Externo"));
            dt.Columns.Add(new DataColumn("Id_Registro"));
            try 
            {
                while ((line = file.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        DataRow row = dt.NewRow();

                        if (primeraLinea == 1)
                            claveEmisor = line.Trim();
                        else
                        {
                         
                            row["Id_ArchivoCIE"] = consecutivo.ToString();
                            row["Id_Externo"] = line.Trim();
                            row["Id_Registro"] = registro.Trim();
                            dt.Rows.Add(row);
                            consecutivo += 1;
                        }
                        primeraLinea = 2;
                        

                  
                    }
                }

            }
            catch (Exception ex) 
            {
                dt = null;
                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() ERROR  al leer el archivo: {2}", CLASE, METODO, ex.Message));
                Logueo.Error(string.Format("[PRPROCESARTARPCI].{0}.{1}() ERROR : {2}", CLASE, METODO, ex.Message));
            }
            finally
            {
                file.Close();
            }
            return dt;
        }


        /// <summary>
        /// Metiodo que determina la poscion final dentro de los substring
        /// </summary>
        /// <param name="inicial"></param>
        /// <param name="final"></param>
        /// <returns></returns>
        public static int PosicionFinal( int inicial , int final) 
        {
            return final - (inicial+1);
        
        }

        /// <summary>
        /// Metodo que determina la posicion inicial dentro del substring
        /// </summary>
        /// <param name="inicial"></param>
        /// <returns></returns>
        public static  int PosicionInicial( int inicial) 
        {

            return inicial + 1;
        }




         private string obtieneMedioAccesoDeterminado(string claveMAObtener , string tarjeta) 
        {
            const String METODO = "obtieneMedioAccesoDeterminado";

            Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()   Obteniendo  No CACAO  de tarjeta : "+MascaraNumeroTarjeta(tarjeta), CLASE, METODO));
            
            string[] resultado;

            if (claveMAObtener == "CLABE" || claveMAObtener == "CACAO" || claveMAObtener == "NOCTA")
            {
                resultado = DAORegistrarTarjetas.ObtenerClabeTarjeta(tarjeta, claveMAObtener);

                if (resultado[0] == "00")
                {
                    if (!string.IsNullOrEmpty(resultado[1]))
                        return resultado[1];
                    else
                    {
                    
                        Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()   Obteniendo  No CACAO  de tarjeta MA-NO ASIGNADO : " + MascaraNumeroTarjeta(tarjeta), CLASE, METODO));
                        return "MA-NO ASIGNADO";
                    }
                }
                else
                {
                    Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()   No Exitoso al obtener el No Cacao : " + MascaraNumeroTarjeta(tarjeta), CLASE, METODO));
                    return string.Empty;
                }


            }
            else
                return string.Empty;
        }


        /// <summary>
        /// Enmascara con el numero de la tarjeta
        /// </summary>
        /// <param name="numeroTarjeta"></param>
        /// <returns></returns>
        public string MascaraNumeroTarjeta(string numeroTarjeta)
        {
            const string METODO = "MascaraNumeroTarjeta";
            try
            {
                int longitud = numeroTarjeta.Length;
                string mascara = numeroTarjeta.Substring((longitud - 4), 4);
                return "************" + mascara;
            }
            catch (Exception ex)
            {
                Logueo.Evento(string.Format("[PRPROCESARTARPCI].{0}.{1}()Error al enmascarar "+ex.Message, CLASE, METODO));
                return numeroTarjeta;
            }


        }

        /// <summary>
        /// Metodo que obtiene el token de una tarjeta determinada
        /// </summary>
        /// <param name="tarjeta"></param>
        /// <returns></returns>
        private string obtieneTokenTarjeta(string tarjeta)
        {
            string[] resultado;

            if (!string.IsNullOrEmpty(tarjeta))
            {
                resultado = DAORegistrarTarjetas.ObtenerTokenByTarjeta(tarjeta);

                if (resultado[0] == "00")
                {
                    if (!string.IsNullOrEmpty(resultado[1]))
                        return resultado[1];
                    else
                    {
                        return "NO-TOKEN";
                    }
                }
                else
                {
                    return string.Empty;
                }


            }
            else
                return string.Empty;
        }



        /// <summary>
        /// Crea los directorios para la salidas de archivos
        /// </summary>
        /// <param name="directorio"></param>

        public void crearDirectorio(string directorio)
            {
            if (!Directory.Exists(directorio))
                Directory.CreateDirectory(directorio);

            m_directorioSalida = directorio+ "PROCESADOS\\";
            if (!Directory.Exists(m_directorioSalida))
                    Directory.CreateDirectory(m_directorioSalida);
            }
    }
}
