using CommonProcesador;
using DNU_ParabiliaProcesoCortes.Entidades;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
namespace DNU_ParabiliaProcesoCortes.LogicaNegocio
{
    class LNCreacionExcel
    {

        public static bool escribirEnExcel()
        {

            Excel.Application myexcelApplication = new Excel.Application();
            if (myexcelApplication != null)
            {
                Excel.Workbook myexcelWorkbook = myexcelApplication.Workbooks.Add();
                Excel.Worksheet myexcelWorksheet = (Excel.Worksheet)myexcelWorkbook.Sheets.Add();

                myexcelWorksheet.Cells[1, 1] = "Value 1";
                myexcelWorksheet.Cells[2, 1] = "Value 2";
                myexcelWorksheet.Cells[3, 1] = "Value 3";

                myexcelApplication.ActiveWorkbook.SaveAs(@"C:\Users\Osvi\Documents\DNU\requerimientos\Trafalgar\documentos\PruebasExcel\abc.xls", Excel.XlFileFormat.xlWorkbookNormal);

                myexcelWorkbook.Close();
                myexcelApplication.Quit();
            }
            return true;
        }

        public static bool escribirEnExcelDatosEdocuenta(ArchivoXLSX datosArchivoXLSX, string rutaArchivo)
        {
            try
            {
                SpreadsheetDocument myexcelApplication = SpreadsheetDocument.Create(rutaArchivo + datosArchivoXLSX.Nombre + ".xlsx", SpreadsheetDocumentType.Workbook);
                WorkbookPart workbookpart = myexcelApplication.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();
                WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(new SheetData());
                Sheets sheets = myexcelApplication.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());
                Sheet sheet = new Sheet()
                {
                    Id = myexcelApplication.WorkbookPart.GetIdOfPart(worksheetPart),
                    SheetId = 1,
                    Name = "DetallesEdosCuentaTimbrados"
                };
                sheets.Append(sheet);

                Worksheet worksheet = worksheetPart.Worksheet;
                SheetData sheetData = worksheet.GetFirstChild<SheetData>();

                Row row = new Row();
                Cell cell = new Cell()
                {

                    DataType = CellValues.String,
                    CellValue = new CellValue("AnioMes")
                };
                row.Append(cell);
                cell = new Cell();
                cell.CellValue = new CellValue("ClienteID");

                row.Append(cell);
                cell = new Cell();
                cell.CellValue = new CellValue("NombreCompleto");

                row.Append(cell);
                cell = new Cell();
                cell.CellValue = new CellValue("RFCReceptor");

                row.Append(cell);
                cell = new Cell();
                cell.CellValue = new CellValue("DireccionCompleta");

                row.Append(cell);
                cell = new Cell();
                cell.CellValue = new CellValue("FechaTimbrado");

                row.Append(cell);
                cell = new Cell();
                cell.CellValue = new CellValue("FechaRegistro");

                row.Append(cell);
                cell = new Cell();
                cell.CellValue = new CellValue("UUID");

                row.Append(cell);
                cell = new Cell();
                cell.CellValue = new CellValue("Folio");

                row.Append(cell);
                cell = new Cell();
                cell.CellValue = new CellValue("IVA");

                row.Append(cell);
                cell = new Cell();
                cell.CellValue = new CellValue("SubTotal");

                row.Append(cell);
                cell = new Cell();
                cell.CellValue = new CellValue("ImporteTotal");

                row.Append(cell);
                cell = new Cell();
                cell.CellValue = new CellValue("NombreEmisor");
                row.Append(cell);

                cell = new Cell();
                cell.CellValue = new CellValue("RFCEmisor");
                row.Append(cell);

                sheetData.Append(row);


                char[] reference = "ABCDEFGHIJKLMN".ToCharArray();

                foreach (DetalleArchivoXLSX detalle in datosArchivoXLSX.Detalle)
                {
                    row = new Row();
                    Cell celda = new Cell()
                    {
                        CellValue = new CellValue(detalle.AnioMes),

                    };
                    row.Append(celda);
                    celda = new Cell();
                    celda.CellValue = new CellValue(detalle.ClienteID);
                    row.Append(celda);

                    celda = new Cell();
                    celda.CellValue = new CellValue(detalle.NombreCompleto);
                    row.Append(celda);

                    celda = new Cell();
                    celda.CellValue = new CellValue(detalle.RFCReceptor);
                    row.Append(celda);

                    celda = new Cell();
                    celda.CellValue = new CellValue(detalle.DireccionCompleta);
                    row.Append(celda);

                    celda = new Cell();
                    celda.CellValue = new CellValue(detalle.FechaTimbrado.ToString("dd-MM-yyyy hh:mm:ss"));
                    celda.DataType = CellValues.Date;
                    row.Append(celda);

                    celda = new Cell();
                    celda.CellValue = new CellValue(detalle.FechaRegistro.ToString("dd-MM-yyyy hh:mm:ss"));
                    celda.DataType = CellValues.Date;
                    row.Append(celda);

                    celda = new Cell();
                    celda.CellValue = new CellValue(detalle.UUID);
                    celda.DataType = CellValues.String;
                    row.Append(celda);

                    celda = new Cell();
                    celda.CellValue = new CellValue(detalle.Folio);
                    celda.DataType = CellValues.String;
                    row.Append(celda);

                    celda = new Cell();
                    celda.CellValue = new CellValue(detalle.IVA);
                    celda.DataType = CellValues.Number;
                    row.Append(celda);

                    celda = new Cell();

                    celda.CellValue = new CellValue(detalle.SubTotal);
                    celda.DataType = CellValues.Number;
                    row.Append(celda);

                    celda = new Cell();
                    celda.CellValue = new CellValue(detalle.ImporteTotal);
                    celda.DataType = CellValues.Number;
                    row.Append(celda);

                    celda = new Cell();
                    celda.CellValue = new CellValue(detalle.NombreEmisor);
                    row.Append(celda);

                    celda = new Cell();
                    celda.CellValue = new CellValue(detalle.RFCEmisor);
                    row.Append(celda);

                    sheetData.Append(row);
                }



                //segunda hoja

                WorksheetPart worksheetPart2 = workbookpart.AddNewPart<WorksheetPart>();
                Worksheet workSheet2 = new Worksheet();
                SheetData sheetData2 = new SheetData();

                Row content = new Row();
                Cell celdaResumen = new Cell()
                {
                    CellValue = new CellValue("Resumen")
                };
                content.Append(celdaResumen);
                sheetData2.Append(content);

                content = new Row();


                celdaResumen = new Cell();
                celdaResumen.CellValue = new CellValue("Fecha Generacion");
                content.Append(celdaResumen);
                celdaResumen = new Cell();
                celdaResumen.CellValue = new CellValue(DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"));
                celdaResumen.DataType = CellValues.Date;
                content.Append(celdaResumen);
                celdaResumen = new Cell();
                sheetData2.Append(content);
                content = new Row();


                celdaResumen.CellValue = new CellValue("Estados De Cuenta Totales");
                content.Append(celdaResumen);
                celdaResumen = new Cell();
                celdaResumen.CellValue = new CellValue(datosArchivoXLSX.EstadosDeCuentaTotales);
                content.Append(celdaResumen);
                sheetData2.Append(content);
                content = new Row();

                celdaResumen = new Cell();
                celdaResumen.CellValue = new CellValue("Estados De Cuenta Correctos");
                content.Append(celdaResumen);
                celdaResumen = new Cell();
                celdaResumen.CellValue = new CellValue(datosArchivoXLSX.EstadosDeCuentaCorrectos);
                content.Append(celdaResumen);
                sheetData2.Append(content);
                content = new Row();

                celdaResumen = new Cell();
                celdaResumen.CellValue = new CellValue("Estados De Cuenta Erroneos");
                content.Append(celdaResumen);
                celdaResumen = new Cell();
                celdaResumen.CellValue = new CellValue(datosArchivoXLSX.EstadosDeCuentaErroneos);
                content.Append(celdaResumen);
                sheetData2.Append(content);
                content = new Row();

                celdaResumen = new Cell();
                celdaResumen.CellValue = new CellValue("Estados De Cuenta Timbrados");
                content.Append(celdaResumen);
                celdaResumen = new Cell();
                celdaResumen.CellValue = new CellValue(datosArchivoXLSX.EstadosDeCuentaTimbrados);
                content.Append(celdaResumen);
                sheetData2.Append(content);


                workSheet2.AppendChild(sheetData2);
                worksheetPart2.Worksheet = workSheet2;

                Sheet myexcelWorksheet = new Sheet()
                {
                    Id = myexcelApplication.WorkbookPart.GetIdOfPart(worksheetPart2),
                    SheetId = 2,
                    Name = "ResumenEdoCuenta"
                };
                sheets.Append(myexcelWorksheet);

                //save document.
                worksheetPart.Worksheet.Save();
                // Close the document.
                myexcelApplication.Close();


            }
            catch (Exception ex)
            {


                Logueo.Error("[GeneraEstadoCuentaCredito] error al generar Xlsx:" + ex.Message + " " + ex.StackTrace);

            }
            return true;
        }

        private static Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            string cellReference = columnName + rowIndex;

            // If the worksheet does not contain a row with the specified row index, insert one.
            Row row;
            if (sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).Count() != 0)
            {
                row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
            }
            else
            {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            // If there is not a cell with the specified column name, insert one.  
            if (row.Elements<Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0)
            {
                return row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).First();
            }
            else
            {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                Cell refCell = null;
                foreach (Cell cell in row.Elements<Cell>())
                {
                    if (cell.CellReference.Value.Length == cellReference.Length)
                    {
                        if (string.Compare(cell.CellReference.Value, cellReference, true) > 0)
                        {
                            refCell = cell;
                            break;
                        }
                    }
                }

                Cell newCell = new Cell() { CellReference = cellReference };
                row.InsertBefore(newCell, refCell);

                worksheet.Save();
                return newCell;
            }
        }

        //    public static bool escribirEnExcelDatosEdocuenta2(ArchivoXLSX datosArchivoXLSX, string rutaArchivo)
        //    {
        //        try
        //        {
        //            Excel.Application myexcelApplication = new Excel.Application();
        //            if (myexcelApplication != null)
        //            {


        //                Excel.Workbook myexcelWorkbook = myexcelApplication.Workbooks.Add();


        //                Excel.Worksheet myexcelWorksheetEdoCuenta = (Excel.Worksheet)myexcelWorkbook.Sheets.Add();
        //                myexcelWorksheetEdoCuenta.Name = "DetallesEdosCuentaTimbrados";//primero va esta proque conforme se van agregando hojas las va colocando al incio

        //                //cambiando formato
        //                Excel.Range oRngDecimalSubtotal = myexcelWorksheetEdoCuenta.get_Range("J2", "J" + datosArchivoXLSX.EstadosDeCuentaTotales + 1);
        //                oRngDecimalSubtotal.NumberFormat = "$###,###,##0.00";
        //                Excel.Range oRngDecimalIva = myexcelWorksheetEdoCuenta.get_Range("K2", "K" + datosArchivoXLSX.EstadosDeCuentaTotales + 1);
        //                oRngDecimalIva.NumberFormat = "$###,###,##0.00";
        //                Excel.Range oRngDecimalImporteTotal = myexcelWorksheetEdoCuenta.get_Range("L2", "L" + datosArchivoXLSX.EstadosDeCuentaTotales + 1);
        //                oRngDecimalImporteTotal.NumberFormat = "$###,###,##0.00";
        //                Excel.Range oRngCadena = myexcelWorksheetEdoCuenta.get_Range("I2", "I" + datosArchivoXLSX.EstadosDeCuentaTotales + 1);
        //                oRngCadena.NumberFormat = "@";

        //                myexcelWorksheetEdoCuenta.Cells[1, 1] = "AnioMes";
        //                myexcelWorksheetEdoCuenta.Cells[1, 2] = "ClienteID";
        //                myexcelWorksheetEdoCuenta.Cells[1, 3] = "NombreCompleto";
        //                myexcelWorksheetEdoCuenta.Cells[1, 4] = "RFCReceptor";
        //                myexcelWorksheetEdoCuenta.Cells[1, 5] = "DireccionCompleta";
        //                myexcelWorksheetEdoCuenta.Cells[1, 6] = "FechaTimbrado";
        //                myexcelWorksheetEdoCuenta.Cells[1, 7] = "FechaRegistro";
        //                myexcelWorksheetEdoCuenta.Cells[1, 8] = "UUID";
        //                myexcelWorksheetEdoCuenta.Cells[1, 9] = "Folio";
        //                myexcelWorksheetEdoCuenta.Cells[1, 10] = "IVA";
        //                myexcelWorksheetEdoCuenta.Cells[1, 11] = "SubTotal";
        //                myexcelWorksheetEdoCuenta.Cells[1, 12] = "ImporteTotal";
        //                myexcelWorksheetEdoCuenta.Cells[1, 13] = "NombreEmisor";
        //                myexcelWorksheetEdoCuenta.Cells[1, 14] = "RFCEmisor";


        //                int fila = 2;
        //                int columna = 1;
        //                foreach (DetalleArchivoXLSX detalle in datosArchivoXLSX.Detalle)
        //                {
        //                    columna = 1;
        //                    if (detalle.Timbrado)
        //                    {
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.AnioMes;
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.ClienteID;
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.NombreCompleto;
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.RFCReceptor;
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.DireccionCompleta;
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.FechaTimbrado;
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.FechaRegistro;
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.UUID;
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.Folio;
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.IVA;
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.SubTotal;
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.ImporteTotal;
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.NombreEmisor;
        //                        myexcelWorksheetEdoCuenta.Cells[fila, columna++] = detalle.RFCEmisor;
        //                    }
        //                    fila++;
        //                }


        //                Excel.Worksheet myexcelWorksheet = (Excel.Worksheet)myexcelWorkbook.Sheets.Add();
        //                myexcelWorksheet.Name = "ResumenEdoCuenta";

        //                myexcelWorksheet.Cells[1, 1] = "Resumen";
        //                myexcelWorksheet.Cells[3, 1] = "Fecha Generacion";
        //                myexcelWorksheet.Cells[3, 2] = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss");
        //                myexcelWorksheet.Cells[4, 1] = "Estados De Cuenta Totales";
        //                myexcelWorksheet.Cells[4, 2] = datosArchivoXLSX.EstadosDeCuentaTotales;
        //                myexcelWorksheet.Cells[5, 1] = "Estados De Cuenta Correctos";
        //                myexcelWorksheet.Cells[5, 2] = datosArchivoXLSX.EstadosDeCuentaCorrectos;
        //                myexcelWorksheet.Cells[6, 1] = "Estados De Cuenta Erroneos";
        //                myexcelWorksheet.Cells[6, 2] = datosArchivoXLSX.EstadosDeCuentaErroneos;
        //                myexcelWorksheet.Cells[7, 1] = "Estados De Cuenta Timbrados";
        //                myexcelWorksheet.Cells[7, 2] = datosArchivoXLSX.EstadosDeCuentaTimbrados;


        //                //myexcelApplication.ActiveWorkbook.SaveAs(rutaArchivo + datosArchivoXLSX.Nombre + ".xlsx", Excel.XlFileFormat.xlWorkbookNormal);

        //                myexcelApplication.ActiveWorkbook.SaveAs(rutaArchivo + datosArchivoXLSX.Nombre + ".xlsx", Excel.XlFileFormat.xlOpenXMLWorkbook, Missing.Value,
        //                     Missing.Value, false, false, Excel.XlSaveAsAccessMode.xlNoChange,
        //                     Excel.XlSaveConflictResolution.xlUserResolution, true,
        //                      Missing.Value, Missing.Value, Missing.Value);

        //                myexcelWorkbook.Close();
        //                myexcelApplication.Quit();
        //            }
        //        }
        //        catch (Exception ex)
        //        {

        //            Logueo.Error("[GeneraEstadoCuentaCredito] error al generar Xlsx:" + ex.Message + " " + ex.StackTrace);

        //        }
        //        return true;
        //    }
        //}
    }
}
