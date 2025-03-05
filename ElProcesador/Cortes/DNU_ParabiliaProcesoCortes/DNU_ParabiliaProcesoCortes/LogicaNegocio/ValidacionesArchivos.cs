using CommonProcesador;
using CsvHelper;
using DNU_ParabiliaProcesoCortes.CapaDatos;
using DNU_ParabiliaProcesoCortes.Common.Funciones;
using DNU_ParabiliaProcesoCortes.dataService;
using DNU_ParabiliaProcesoCortes.Entidades;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class ValidacionesArchivos
    {
        private string directorioEntrada;
        private static DataTable dtContenidoFile;
        private static DataTable dtContenidoFileProducto;
        private string directorioSalida;
        private static string nomArchivoProcesar;

        public ValidacionesArchivos(string directorioEntrada, string directorioSalida)
        {
            this.directorioEntrada = directorioEntrada;
            this.directorioSalida = directorioSalida;
        }
        public ValidacionesArchivos() { }

        internal bool validarContenido(FileInfo pInfoArchivo, Archivo archivoLog=null)
        {

            bool respuesta = false;
            int counter = 0, contadorAltasTotales = 0, contadorEmpleadoras = 0, contadorAltas = 0;
            string line, fechaEnvio = null, claveCliente = null, claveBIN = null;
            bool resultHeaderA = false;
            //bool resultHeaderB = false;
            bool resultDetalle = false;
            bool resultTrailerA = false;
            //bool resultTrailerB = false;
            dtContenidoFile = crearDataTable();
            dtContenidoFileProducto = crearDataTableProducto();

            Hashtable htFile = new Hashtable();
            string tipoArchivo = "";

            if (pInfoArchivo.Name.ToUpper().Contains("EDOCTADATOSCTE"))
            {
                htFile.Add("@descripcion", "Archivo de Alta Datos ECTA");
                tipoArchivo = "Datos";
            }

            if (pInfoArchivo.Name.ToUpper().Contains("EDOCTADETACTA"))
            {
                htFile.Add("@descripcion", "Archivo de Alta ECTA");
                tipoArchivo = "Detalle";
            }

            if (pInfoArchivo.Name.ToUpper().Contains("EDOCTARESUMCTA"))
            {
                htFile.Add("@descripcion", "Archivo de Alta Suma ECTA");
                tipoArchivo = "Sumatoria";
            }

            htFile.Add("@claveProceso", "PROCESAEDOCUENTA");
            htFile.Add("@nombre", pInfoArchivo.Name);
            htFile.Add("@tipoArchivo", ".csv");

            string idFile = null;
            using (SqlConnection conn2 = new SqlConnection(DBProcesadorArchivo.strBDEscrituraArchivo))
            {
                conn2.Open();
                try
                {
                    idFile = DAOArchivo.EjecutarSP("procnoc_travel_InsertaDatosArchivo", htFile, conn2).Tables["Table"].Rows[0]["ID_Archivo"].ToString();


                }
                catch (Exception ex)
                {
                    Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [error al insertae archivo en base de datos]");

                }
            }

            if (idFile == null)
                return false;
            Hashtable ht = new Hashtable();
            ht.Add("@idArchivo", idFile);

            int numeroColumna = 0;
            int numeroFilas = 0;
            String nombrecolumna = "";
            try
            {
                int fila = 1;
                using (var reader = new StreamReader(pInfoArchivo.FullName, Encoding.Default))
                {
                    bool header = true;
                    string rowHeader = "";
                    while (!reader.EndOfStream)
                    {
                        rowHeader = reader.ReadLine();
                        String[] arrayString = rowHeader.Split(';');
                        if (header)
                        {
                            //  rowHeader = csv.GetField(0);
                            //string[] arrayString = rowHeader.Split(';');
                            dtContenidoFile = crearDataTable(arrayString);
                            header = false;
                            continue;
                        }

                        if (archivoLog != null) {
                            archivoLog.Columnas = arrayString.Length;
                        }
                        var row = dtContenidoFile.NewRow();
                        //rowHeader = csv.GetField(0);
                        //string[] arrayStringDatos = rowHeader.Split(';');
                        foreach (DataColumn column in dtContenidoFile.Columns)
                        {
                            nombrecolumna = column.ColumnName;
                            numeroColumna = column.Ordinal;
                            row[column.ColumnName] = string.IsNullOrEmpty(arrayString[column.Ordinal])?null: arrayString[column.Ordinal];//csv.GetField(column.DataType, column.ColumnName);
                            if (column.ColumnName == "SaldoInicial" || column.ColumnName == "Deposito" || column.ColumnName == "Retiro" || column.ColumnName == "Saldo") {
                                try {
                                    Convert.ToDecimal(row[column.ColumnName]);
                                } catch (Exception ex) {
                                    String deteccionError = "";
                                    Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [error al insertae archivo en base de datos]" +"fila:"+ numeroFilas+" columna "+ column.ColumnName);

                                }

                            }
                        }
                        dtContenidoFile.Rows.Add(row);
                        numeroFilas++;
                    }
                    dtContenidoFile.AcceptChanges();
                }

                if (archivoLog != null)//esto es para registrar en log
                {
                    archivoLog.Registros = numeroFilas;
                }
                //using (TextReader fileReader = File.OpenText(pInfoArchivo.FullName))//new StreamReader(pInfoArchivo.FullName);
                //{
                //    CsvReader csv = new CsvReader(fileReader);
                //    csv.Configuration.HasHeaderRecord = true;
                //    bool header = true;
                //    string rowHeader = "";
                //    while (csv.Read())
                //    {

                //        if (header) {
                //            rowHeader = csv.GetField(0);
                //            string[] arrayString = rowHeader.Split(';');
                //            dtContenidoFile = crearDataTable(arrayString);
                //            header = false;
                //            continue;
                //        }
                //        var row = dtContenidoFile.NewRow();
                //        rowHeader = csv.GetField(0);
                //        string[] arrayStringDatos = rowHeader.Split(';');
                //        foreach (DataColumn column in dtContenidoFile.Columns)
                //        {
                //            nombrecolumna = column.ColumnName;
                //            numeroColumna = column.Ordinal;
                //            row[column.ColumnName] = arrayStringDatos[column.Ordinal];//csv.GetField(column.DataType, column.ColumnName);
                //        }
                //        dtContenidoFile.Rows.Add(row);
                //    }
                //}
            

                //insertar bulk
                respuesta = DAOArchivo.bulkInsertarDatosArchivoDetalle(dtContenidoFile, DBProcesadorArchivo.strBDEscrituraArchivo, "ArchivosDetalleEdoCtaExternoTmp", idFile, null,tipoArchivo);
                if (respuesta) {
                    Hashtable ht2 = new Hashtable();
                    ht2.Add("@tipoArchivo", tipoArchivo);
                    DataTable datosRespuesta = DAOArchivo.EjecutarSP("[procnoc_travel_ActualizarOInsertarDatosCTA]", ht2, DBProcesadorArchivo.strBDEscrituraArchivo).Tables[0];
                    if (datosRespuesta.Rows[0][0].ToString() == "0")
                    {
                        respuesta = true;

                    }
                    else {
                        respuesta = false; 
                        Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [PROCESADORNOCTURNO] [error al insertae archivo en base de datos]"+ datosRespuesta.Rows[0][1].ToString());

                    }
                }
                //while (csv.Read())
                //{
                //    string row = csv.GetField(0);
                //    string[] arrayString = row.Split(',');

                //    if (arrayString.Length == 60 || arrayString.Length == 15 || arrayString.Length == 23)
                //    {

                //        DataRow registro = dtContenidoFile.NewRow();
                //        registro["cAfiliacion"] = Convert.ToString(arrayString[0]);
                //        registro("nId_archivo") = Id_Fichero
                //                            registro("dfecha_Pago") = ConvertirFecha(Array(1)).ToString("yyyy/MM/dd HH:mm:ss")
                //                            registro("nReferencia") = Convert.ToString(Array(2))
                //                            registro("nSecuencia") = Convert.ToString(Array(4))
                //                            registro("nTipo_Impuesto") = Convert.ToString(Array(6))

                //    }
                //    else
                //    {
                //        Logueo.Error("[GeneraReporteEstadoCuentaCargaArchivos] [validarArchivos] [numero de columnas incorrecto]");

                //    }

                //    //while ((line = file.ReadLine()) != null)
                //    //{
                //    //if (line.Count() != 361 && line.Count() != 372)
                //    //{
                //    //    Logueo.Error("[AltaEmpleado] [ValidarContenido] [La línea numero: " + (counter + 1) + " no tinen los 361 caracteres] [\\LNAltaEmpleado.cs: línea 156]");
                //    //    file.Close();
                //    //    return false;
                //    //}
                //    //if (line.StartsWith("11"))
                //    //{
                //    //    resultHeaderA = validaHeaderA(line);
                //    //    fechaEnvio = line.Substring(9, 8);
                //    //    claveCliente = line.Substring(17, 10);
                //    //    claveBIN = line.Substring(27, 8);
                //    //    if (!resultHeaderA) { file.Close(); return false; }
                //    //}
                //    //if (line.StartsWith("21"))
                //    //{
                //    //    resultDetalle = validarDetalle(line, counter, claveCliente, fechaEnvio, claveBIN);
                //    //    contadorAltasTotales += 1;
                //    //    contadorAltas += 1;
                //    //    if (!resultDetalle) { file.Close(); return false; }
                //    //}
                //    //    if (line.StartsWith("92"))
                //    //    {
                //    //        resultTrailerA = validarTrailerA(line, contadorAltasTotales, contadorEmpleadoras);
                //    //        contadorAltasTotales = 0;
                //    //        contadorEmpleadoras = 0;
                //    //        if (!resultTrailerA) { file.Close(); return false; }
                //    //    }
                //    //    counter++;
                //    //}
                //    //file.Close();

                //    //Se obtienen los datos del contrato.
                //    //Contrato.contrato =
                //    //    BDsps.ObtenerDatosContrato(dtContenidoFile.Rows[0]["ClaveCliente"].ToString());
                //}
                //}
            }
            catch (Exception ex)
            {
                Logueo.Evento("[GeneraReporteEstadoCuentaCargaArchivos] Error al leer el csv"+ex.Message);
                return false;
            }

            return respuesta;
        }

        private DataTable crearDataTable(String[] arregloDatos)
        {

            DataTable dtDatosnew = new DataTable();

            foreach (String columna in arregloDatos)
            {
                dtDatosnew.Columns.Add(columna);
            }


            return dtDatosnew;
        }

        private DataTable crearDataTable()
        {

            DataTable dtDatosnew = new DataTable();
            TypeInfo t = typeof(ArchivosDetalleEdoCuentaExterno).GetTypeInfo();
            var pList = t.DeclaredFields.ToList();

            foreach (FieldInfo columna in pList)
            {
                dtDatosnew.Columns.Add(columna.Name);
            }


            //DataTable dtDatosnew = new DataTable("DetalleAltaTarjetasNominativas");
            //var dc = new DataColumn("FechaEnvio", Type.GetType("System.String"));
            //var dc1 = new DataColumn("ClaveCliente", Type.GetType("System.String"));
            //var dc2 = new DataColumn("ClaveBIN", Type.GetType("System.String"));
            //var dc3 = new DataColumn("ClaveEmpleadora", Type.GetType("System.String"));
            //var dc4 = new DataColumn("NumeroEmpleado", Type.GetType("System.String"));
            //var dc5 = new DataColumn("Nombre", Type.GetType("System.String"));
            //var dc6 = new DataColumn("PrimerApellido", Type.GetType("System.String"));
            //var dc7 = new DataColumn("SegundoApellido", Type.GetType("System.String"));
            //var dc8 = new DataColumn("NombreEmbozado", Type.GetType("System.String"));
            //var dc9 = new DataColumn("Telefono", Type.GetType("System.String"));
            //var dc10 = new DataColumn("Correo", Type.GetType("System.String"));
            //var dc11 = new DataColumn("TipoMedioAcceso", Type.GetType("System.String"));
            //var dc12 = new DataColumn("MedioAcceso", Type.GetType("System.String"));
            //var dc13 = new DataColumn("TipoTarjeta", Type.GetType("System.String"));
            //var dc14 = new DataColumn("TarjetaTitular", Type.GetType("System.String"));
            //var dc15 = new DataColumn("TipoMedioAccesoTitular", Type.GetType("System.String"));
            //var dc16 = new DataColumn("MedioAccesoTitular", Type.GetType("System.String"));
            //var dc17 = new DataColumn("ID_EstatusCACAO", Type.GetType("System.String"));
            //var dc18 = new DataColumn("ReintentosCACAO", Type.GetType("System.String"));


            //dtDatosnew.Columns.Add(dc);
            //dtDatosnew.Columns.Add(dc1);
            //dtDatosnew.Columns.Add(dc2);
            //dtDatosnew.Columns.Add(dc3);
            //dtDatosnew.Columns.Add(dc4);
            //dtDatosnew.Columns.Add(dc5);
            //dtDatosnew.Columns.Add(dc6);
            //dtDatosnew.Columns.Add(dc7);
            //dtDatosnew.Columns.Add(dc8);
            //dtDatosnew.Columns.Add(dc9);
            //dtDatosnew.Columns.Add(dc10);
            //dtDatosnew.Columns.Add(dc11);
            //dtDatosnew.Columns.Add(dc12);
            //dtDatosnew.Columns.Add(dc13);
            //dtDatosnew.Columns.Add(dc14);
            //dtDatosnew.Columns.Add(dc15);
            //dtDatosnew.Columns.Add(dc16);
            //dtDatosnew.Columns.Add(dc17);
            //dtDatosnew.Columns.Add(dc18);


            return dtDatosnew;
        }

        private DataTable crearDataTableProducto()
        {
            DataTable dtDatosnew = new DataTable("DetalleAltaTarjetasNominativas");
            var dc = new DataColumn("FechaEnvio", Type.GetType("System.String"));
            var dc1 = new DataColumn("ClaveCliente", Type.GetType("System.String"));
            var dc2 = new DataColumn("ClaveBIN", Type.GetType("System.String"));
            var dc3 = new DataColumn("ClaveEmpleadora", Type.GetType("System.String"));
            var dc4 = new DataColumn("NumeroEmpleado", Type.GetType("System.String"));
            var dc5 = new DataColumn("Nombre", Type.GetType("System.String"));
            var dc6 = new DataColumn("PrimerApellido", Type.GetType("System.String"));
            var dc7 = new DataColumn("SegundoApellido", Type.GetType("System.String"));
            var dc8 = new DataColumn("NombreEmbozado", Type.GetType("System.String"));
            var dc9 = new DataColumn("Telefono", Type.GetType("System.String"));
            var dc10 = new DataColumn("Correo", Type.GetType("System.String"));
            var dc11 = new DataColumn("TipoMedioAcceso", Type.GetType("System.String"));
            var dc12 = new DataColumn("MedioAcceso", Type.GetType("System.String"));
            var dc13 = new DataColumn("TipoTarjeta", Type.GetType("System.String"));
            var dc14 = new DataColumn("TarjetaTitular", Type.GetType("System.String"));
            var dc15 = new DataColumn("TipoMedioAccesoTitular", Type.GetType("System.String"));
            var dc16 = new DataColumn("MedioAccesoTitular", Type.GetType("System.String"));
            var dc17 = new DataColumn("ID_EstatusCACAO", Type.GetType("System.String"));
            var dc18 = new DataColumn("ReintentosCACAO", Type.GetType("System.String"));
            var dc19 = new DataColumn("ClaveSubproducto", Type.GetType("System.String"));

            dtDatosnew.Columns.Add(dc);
            dtDatosnew.Columns.Add(dc1);
            dtDatosnew.Columns.Add(dc2);
            dtDatosnew.Columns.Add(dc3);
            dtDatosnew.Columns.Add(dc4);
            dtDatosnew.Columns.Add(dc5);
            dtDatosnew.Columns.Add(dc6);
            dtDatosnew.Columns.Add(dc7);
            dtDatosnew.Columns.Add(dc8);
            dtDatosnew.Columns.Add(dc9);
            dtDatosnew.Columns.Add(dc10);
            dtDatosnew.Columns.Add(dc11);
            dtDatosnew.Columns.Add(dc12);
            dtDatosnew.Columns.Add(dc13);
            dtDatosnew.Columns.Add(dc14);
            dtDatosnew.Columns.Add(dc15);
            dtDatosnew.Columns.Add(dc16);
            dtDatosnew.Columns.Add(dc17);
            dtDatosnew.Columns.Add(dc18);
            dtDatosnew.Columns.Add(dc19);

            return dtDatosnew;
        }



    }
}
