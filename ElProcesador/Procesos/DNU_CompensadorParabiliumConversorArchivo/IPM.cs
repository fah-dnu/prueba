using DNU_CompensadorParabiliumCommon.Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumConversorArchivo
{
    public class IPM
    {

        #region "Declara??o de atributos"

        const Int16 TAM_BMP = 16;
        const Int16 TAM_MTI = 4;
        const Int16 TAM_FIELD = 4;
        private string activaLog;


        #endregion

        #region "Contrutor"

        /// <summary>
        /// Construtor
        /// </summary>
        public IPM(string activaLog)
        {
            this.activaLog = activaLog;
        }

        #endregion

        #region "Metodos - Regras de negocio"

        /// <summary>
        /// Metodo para comprobar que el archivo a procesar incluya los codigos ISO aceptados en el proceso
        /// </summary>
        /// <param name="bytes">Bytes archivo IPM (EBCDIC)</param>
        /// <param name="patternIpmCodes">Expresión regular de los códigos IPM validos para el proceso</param>
        /// <returns>Bool (archivo valido)</returns>
        private bool ValidarIPM(byte[] bytes, string patternIpmCodes)
        {
            try
            {
                // Crear un StreamReader para convertir los bytes en texto y utilizar el encoding deseado (por ejemplo, UTF-8)
                using (StreamReader reader = new StreamReader(new MemoryStream(bytes), Encoding.GetEncoding("IBM037")))
                {
                    string lineaArchivo = string.Empty;
                    while ((lineaArchivo = reader.ReadLine()) != null)
                    {
                        Encoding _ascii = Encoding.ASCII;
                        Encoding _ebcdic = Encoding.GetEncoding("IBM037");                  // Crear los Encoding.

                        byte[] ebcdicBytes = _ebcdic.GetBytes(lineaArchivo);
                        byte[] asciiBytes = Encoding.Convert(_ebcdic, _ascii, ebcdicBytes); // Convertir la codificación de los bytes de EBCDIC A ASCII

                        string valorASCII = _ascii.GetString(asciiBytes);                 // Obtener la cadena de bytes en ASCII
                        string ValorLimpio = string.Empty;

                        foreach (char caracter in valorASCII)
                        {
                            if (caracter >= 32 && caracter <= 126)
                                ValorLimpio += caracter;                                    // El carácter es un carácter ASCII imprimible.
                        }

                        if (string.IsNullOrEmpty(ValorLimpio))
                            return false;

                        var matches = Regex.Matches(ValorLimpio, patternIpmCodes);
                        if (matches.Count > 0)
                        {   //foreach (Match item in matches)
                            return true;                                                    // El archivo contiene N cantidad de registros validos
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        List<byte> RemoveBytes(List<byte> originalList, int Cantidad, int posiciones)
        {
            List<byte> result = new List<byte>();
            int length = originalList.Count;
            bool OmitirDespuesDeAgregar = true;
            
            //var verCaracteres = string.Empty;

            for (int i = 0; i < length; i++)
            {
                // Omitir cada X posiciones los siguientes 2 bytes
                if (result.Count % posiciones == 0 && i > 0 && OmitirDespuesDeAgregar)
                {
                    #region Dbug

                    //var verCaracter = originalList[i];
                    //var verCaracter2 = originalList[(i+1)];
                    //;
                    //Encoding _ascii = Encoding.ASCII;
                    //Encoding _ebcdic = Encoding.GetEncoding("IBM037");                  // Crear los Encoding.

                    //byte[] ebcdicBytes = new byte[] { verCaracter, verCaracter2 };
                    //byte[] asciiBytes = Encoding.Convert(_ebcdic, _ascii, ebcdicBytes); // Convertir la codificación de los bytes de EBCDIC A ASCII

                    //string valorASCII = _ascii.GetString(asciiBytes);                 // Obtener la cadena de bytes en ASCII
                    //string ValorLimpio = string.Empty;

                    //foreach (char caracter in valorASCII)
                    //{
                    //    if (caracter >= 32 && caracter <= 126)
                    //        ValorLimpio += caracter;                                    // El carácter es un carácter ASCII imprimible.
                    //}

                    //if (string.IsNullOrEmpty(ValorLimpio) || !ValorLimpio.Contains("  "))
                    //{
                    //    verCaracteres += ValorLimpio;
                    //}

                    #endregion

                    i = i + Cantidad - 1;  // Saltar los X(Cantidad) bytes - 1 por el for
                    OmitirDespuesDeAgregar = false;
                    if (i >= length) break;
                }
                else
                {
                    result.Add(originalList[i]);
                    OmitirDespuesDeAgregar = true;
                }
            }

            return result;
        }

        /// <summary>
        /// L? um arquivo no formato IPM (Mastercard) e grava os registros
        /// lidos em um arquivo texto
        /// </summary>
        /// <param name="pathIPM">Local e nome do arquivo IPM</param>
        /// <param name="pathTXT">Local e nome do arquivo Texto</param>
        /// <param name="logueoActivo">Desconocido</param>
        /// <param name="removerCaracter">Bandera para quitar caracteres no legibles en archivo IPM</param>
        /// <param name="cantidadCaracteres">Cantidad de caracteres a remover del archivo IPM</param>
        /// <param name="longitudCarateres">Posición en la que aparecen los caracteres no deseados, debe ser la misma en cada ciclo</param>
        /// <returns>Retorna a qtde de registro lidos no arquivo IPM</returns>
        public Int32 LeerIPM(string pathIPM, string pathTXT, string logueoActivo, bool removerCaracter = false, int cantidadCaracteres = 0, int longitudCarateres = 0, string exprecionCodigosIPM = "")
        {

            byte[] bt = null;

            string str = string.Empty;
            string msg = string.Empty;
            string mti = string.Empty;
            string bitmap = string.Empty;

            string[] bmp = new string[TAM_BMP];

            ISO _iso;

            int regresar = 0; // Devuelve un codigo de proceso que indica: -100 con posibles registros incorrectos, -99 sin registros detectados, -98 error en el proceso
            int idx = 0;
            int idx_aux = 0;
            int idx_bmp = 0;
            int tam_reg = 0;
            int qt_reg = 0;
            StreamWriter sw = null;
            BinaryReader br = null;

            bool ContienteRegistros = false;
            bool ContienteRegistrosProceso = false;

            try
            {
                // testa os parametros do metodo
                if (String.IsNullOrEmpty(pathIPM) || String.IsNullOrEmpty(pathTXT))
                {
                    throw new Exception("Paramétros inválidos para la función");
                }

                // Abre o arquivo e realiza a leitura em formato Binario
                br = new BinaryReader(File.Open(@pathIPM, FileMode.Open));
                //br = new BinaryReader(File.Open(@pathIPM, FileMode.Open), Encoding.UTF8);
                bt = br.ReadBytes(Int32.Parse(br.BaseStream.Length.ToString()));


                // Prepara o arquivo texto que sera gerado com os registros do 
                // arquivo IPM
                FileStream fs = File.Create(@pathTXT);
                sw = new StreamWriter(fs, Encoding.ASCII);
                sw.Flush();


                if (!string.IsNullOrEmpty(exprecionCodigosIPM))
                    ContienteRegistros = ValidarIPM(bt, exprecionCodigosIPM);         //Validación de archivos sin reistros

                #region Quitar caracteres

                // Esta sección omite caracteres que aparecen en una determida posición en el archivo
                // Debe de ser recurrente, por ejemplo con Volcan cada 1012 caracteres aparecen 2 arrobas (@@) que deben ser removidos para un correcto procesamiento

                if (removerCaracter)
                {
                    List<byte> lstNewBt = RemoveBytes(bt.ToList(), cantidadCaracteres, longitudCarateres); // Remover los caracteres no deseados

                    bt = lstNewBt.ToArray();    // Reemplazar por el nuevo array sin los caracteres indicados
                    //File.WriteAllBytes(@"D:\DNU\Actividades\Volcan\Conversor Archivos\Nuevo_EBCDIC_Sin_arrobas.ipm", bt);    // Verificar archivo de salida
                }
                #endregion Quitar caracteres

                // L? os registros do arquivo IPM
                do
                {
                    // Inicializa as variaveis a cada registro lido
                    str = string.Empty;
                    mti = string.Empty;
                    bitmap = string.Empty;
                    msg = string.Empty;

                    tam_reg = 0;
                    idx_bmp = 0;

                    bmp.Initialize();



                    //
                    // L? o tamanho do arquivo
                    ///
                    /// Primeros 4 digitos indican el tamaño de la cadena
                    /// MTI siempre presente en posición 1-4
                    /// Bit Map Primario siempre presente posición 5-12
                    /// Bit Map Secundario siempre presente posición 13-20
                    ///
                    //
                    for (int i = idx; i < (idx + TAM_FIELD); i++)
                    {
                        if (i >= bt.Length)
                        {
                            break;//Verificar
                        }
                        var valor = bt[i].ToString();
                        str += Conversor.ConvertDecimalToHex(valor);
                        idx_aux++;
                    }
                    tam_reg = Int32.Parse(Conversor.ConvertHexToDecimal((str)).ToString());

                    // Se o tamanho do arquivo for igual a 0 para a leitura
                    if (tam_reg == 0)
                    {
                        break;
                    }

                    //
                    // L? o MTI da Mensagem
                    ///
                    /// Formar código MTI
                    /// Ejemplos: 1240, 1740, 1644, 1442
                    ///
                    //
                    idx = idx_aux;
                    for (int i = idx; i < (idx + TAM_MTI); i++)
                    {
                        if (i >= bt.Length)
                        {
                            break;//Verificar
                        }
                        str = bt[i].ToString();

                        // Verifica se o caracter em EBCDIC ? numerico ou alfa
                        if (Int32.Parse(str) < 240 || Int32.Parse(str) > 249)
                        {
                            str = Conversor.convertEBCDICtoASCII(Convert.ToChar(Int32.Parse(str)).ToString());
                        }
                        else
                        {
                            str = Conversor.convertASCIItoEBCDIC(str);
                            str = Conversor.convertEBCDICtoASCII(str.Substring(2, 1)).ToString();
                        }
                        mti += str;
                        idx_aux++;
                    }

                    ///
                    /// L? o Bitmap da mensagem
                    /// Se forma Bit Map primario
                    ///
                    idx = idx_aux;

                    for (int i = idx; i < (idx + TAM_BMP); i++)
                    {
                        if (i >= bt.Length)
                        {
                            break;//Verificar
                        }
                        var valor = bt[i];
                        var valor2 = Convert.ToString(valor, 16).ToString();
                        bmp[idx_bmp] = valor2;
                        idx_bmp++;
                        idx_aux++;
                    }

                    for (int i = 0; i < bmp.Length; i++)
                    {
                        if (i >= bt.Length)
                        {
                            break;//Verificar
                        }
                        if (bmp[i].Length == 1)
                            bitmap += "0" + bmp[i];
                        else
                            bitmap += bmp[i].ToString();
                    }

                    //
                    // L? o conteudo da mensagem
                    //
                    idx = idx_aux;
                    _iso = new ISO(mti, bitmap, bt, idx);


                    try
                    {
                        _iso.MontaMensagemISO();
                    }
                    catch (Exception ex)
                    {
                        qt_reg++;
                        throw new Exception(ex.Message + "-Erro no tratamento da linha:" + qt_reg);
                    }


                    idx = _iso.IDX;
                    idx_aux = _iso.IDX;
                    if (activaLog.Equals("1"))
                        Log.Evento("[VALOR MTI] " + mti);

                    // Grava o registro lido no arquivo TXT
                    switch (mti)
                    {
                        case "1240":
                        case "1740":
                            ContienteRegistrosProceso = true;
                            sw.WriteLine(MastercardToEvertec(_iso));
                            break;

                    }

                    qt_reg++;

                } while (idx < bt.Length);

                // Fecha os arquivos
                sw.Close();
                br.Close();

                regresar = ContienteRegistrosProceso ? qt_reg : (ContienteRegistros ? -100 : -99);

                return regresar;

            }
            catch (Exception ex)
            {
                regresar = -98;
                return regresar;
                throw ex;
            }
            finally
            {
                sw.Close();
                br.Close();
            }

        }




        #endregion



        public string MastercardToEvertec(ISO iso)
        {
            var sb = new StringBuilder();

            //C001 - Código Proceso
            int dataElement = 3;
            int lenDataElemnt = 2;
            int pdsElement = 0;
            var valor           = String.IsNullOrEmpty(iso.GetBit(dataElement).conteudo) ? "" : iso.GetBit(dataElement).conteudo.Trim();
            var valorTruncado   = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C002 y C003 - Tarjeta
            dataElement = 2;
            lenDataElemnt = 19;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C004 - Referencia
            dataElement = 31;
            lenDataElemnt = 23;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C005 - Fecha consumo
            dataElement = 12;
            lenDataElemnt = 6;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = "20" +  valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);
            //var aux = iso.GetBit(12).conteudo;
            //LogueoDE(12, iso);
            //sb.Append("20" + (aux.Length >= 8 ? aux.Substring(0, 6) : aux.PadRight(6)));

            //C006 - Monto moneda origen
            dataElement = 4;
            lenDataElemnt = 12;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C007 - Moneda Origen
            dataElement = 49;
            lenDataElemnt = 3;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C008 - Monto moneda destino
            dataElement = 6;
            lenDataElemnt = 12;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C009 - Moneda destino
            dataElement = 51;
            lenDataElemnt = 3;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C010, C011, C012 - Nombre del negocio (25, 13, 3)
            string[] merchant = iso.GetBit(43).conteudo.Split('\\');
            dataElement = 43;
            LogueoDE(dataElement, iso);

            if (merchant.Length < 4)
            {
                lenDataElemnt = 41;
                valor = "".PadRight(41);
                valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
                sb.Append(valorTruncado);
            }
            else
            {

                lenDataElemnt = 25;
                valor = merchant[0].Trim();
                valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
                sb.Append(valorTruncado);   // Merchant Name//C010 - Nombre del negocio

                lenDataElemnt = 13;
                valor = merchant[2].Trim();
                valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
                sb.Append(valorTruncado);   // Merchabt City//C011 - Ciudad del negocio

                lenDataElemnt = 3;
                valor = merchant[3].Trim();
                valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
                sb.Append(valorTruncado);   //Merchant Country//C012 - Pais del negocio
                //sb.Append(merchant[3].Substring(merchant[3].Length - 3).PadRight(3)); 
            }

            //C013 - Categoria del negocio
            dataElement = 26;
            lenDataElemnt = 4;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C014 - Codigo postal del negocio
            sb.Append(merchant.Length >= 4 ? (merchant[3].Length > 5 ? merchant[3].Substring(0, 5).PadRight(5) : "".PadRight(5)) : "".PadRight(5));//Merchant ZipCode
            //C015 - Estado del negocio
            sb.Append(merchant.Length >= 4 ? merchant[3].Length >= 16 ? merchant[3].Substring(merchant[3].Length - 6, 3).PadRight(3) : "".PadRight(3) : "".PadRight(3));//Merchant State

            //C016 ID mensaje
            lenDataElemnt = 4;
            valor = iso.MTI.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);

            //C017 - Función
            dataElement = 24;
            lenDataElemnt = 3;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C018 - Autorización
            dataElement = 38;
            lenDataElemnt = 6;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C019 - Fecha de proceso
            dataElement = 31;
            lenDataElemnt = 4;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(valor.Length >= 11 ? 7 : 0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C020 - Comision
            dataElement = 48;
            pdsElement = 146;
            lenDataElemnt = 12;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(valor.Length >= 12 ? 24 : 0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C021 - Hora de transacción
            dataElement = 12;
            lenDataElemnt = 6;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(valor.Length >= 6 ? 6 : 0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C022 - Fecha de expiración
            dataElement = 14;
            lenDataElemnt = 4;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C023 - Razon de mensaje
            dataElement = 25;
            lenDataElemnt = 4;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C024 - Monto original transacción
            dataElement = 30;
            lenDataElemnt = 12;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C025 - Moneda original transacción
            sb.Append("".PadRight(3));

            //C026 - Institición remite
            dataElement = 32;
            lenDataElemnt = 11;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C027 - Entidad que acepta
            dataElement = 42;
            lenDataElemnt = 15;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C028 - Ciclo de vida trans
            dataElement = 63;
            lenDataElemnt = 16;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C029 - Fecha de acción
            dataElement = 73;
            lenDataElemnt = 6;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C030 - Destino transacción
            dataElement = 93;
            lenDataElemnt = 11;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C031 - Origen transacción
            dataElement = 94;
            lenDataElemnt = 11;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C032 - Referencia emisor
            dataElement = 95;
            lenDataElemnt = 10;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);
			
            //C033 - Terminal
            dataElement = 48;
            pdsElement = 23;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C034 - Número control cobro
            dataElement = 125;
            pdsElement = 137;
            lenDataElemnt = 20;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C035 - Exponentes para moneda
            dataElement = 48;
            pdsElement = 148;
            lenDataElemnt = 20;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C036 - Indicador liquidación
            dataElement = 48;
            pdsElement = 165;
            lenDataElemnt = 1;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C037 - Codigo recuperación documentación
            sb.Append(" ");

            //C038 - Indicador documentación
            dataElement = 125;
            pdsElement = 262;
            lenDataElemnt = 1;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C039 - Actividad negocio
            dataElement = 48;
            pdsElement = 158;
            lenDataElemnt = 31;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C040 - Reverso mensaje
            dataElement = 48;
            pdsElement = 25;
            lenDataElemnt = 7;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);
            //sb.Append("".PadRight(7));

            //C041 - Monto Trans parcial, C042 - Moneda Trans parcial
            dataElement = 48;
            pdsElement = 268;
            lenDataElemnt = 15;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C043 - Taza convención conc
            dataElement = 9;
            lenDataElemnt = 8;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C044 - Mensaje contracargo
            sb.Append("".PadRight(100));

            //C045 - MCCR Monto
            dataElement = 111;
            lenDataElemnt = 12;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C046 - Datos del POS
            dataElement = 22;
            lenDataElemnt = 12;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C047 - Cross boarder
            dataElement = 48;
            pdsElement = 177;
            lenDataElemnt = 2;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C048 - ID Prodcuto
            dataElement = 48;
            pdsElement = 2;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C049 - MC Assigned ID
            dataElement = 48;
            pdsElement = 176;
            lenDataElemnt = 6;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C050 - Monto, impuestos
            sb.Append("".PadRight(60));

            //C051 - Seguridad comercio electronico
            dataElement = 48;
            pdsElement = 52;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C052 - Circuito integrado
            dataElement = 55;
            lenDataElemnt = 350;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C053 - Retrieval reference
            dataElement = 37;
            lenDataElemnt = 12;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C054 - Card Sequence numbeer
            dataElement = 23;
            lenDataElemnt = 3;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C055 - DE 54 monto Adiconal
            dataElement = 54;
            lenDataElemnt = 121;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C056 - DE 51 Cardholder Bill
            dataElement = 51;
            lenDataElemnt = 3;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C057 - PDS 180 Domestic card
            dataElement = 48;
            pdsElement = 180;
            lenDataElemnt = 20;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C058 - PDS 206 Late present
            dataElement = 48;
            pdsElement = 206;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C059 - PDS 145 ALT Trans
            dataElement = 48;
            pdsElement = 145;
            lenDataElemnt = 8;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C060 - PDS 0044 Prog part int
            dataElement = 48;
            pdsElement = 44;
            lenDataElemnt = 20;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C061 - PDS 0043 Prog Rec ID
            dataElement = 48;
            pdsElement = 43;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C062 - PDS 0001 Serv Account
            dataElement = 48;
            pdsElement = 1;
            lenDataElemnt = 21;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C063 - Código IATA cliete
            dataElement = 48;
            pdsElement = 717;
            lenDataElemnt = 17;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C064 - Numero de boleto
            dataElement = 48;
            pdsElement = 506;
            lenDataElemnt = 15;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C065 - Transportadora emisor
            sb.Append("".PadRight(4));

            //C066 - Código agencia viaje
            dataElement = 48;
            pdsElement = 510;
            lenDataElemnt = 8;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C067 - Nombre de la agencia
            dataElement = 48;
            pdsElement = 511;
            lenDataElemnt = 25;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C068 - Indicador procesado
            sb.Append(" ");

            //C069 - PDS 0211 Term Comp ID
            dataElement = 48;
            pdsElement = 211;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C070 - PDS 0058 Token Level
            dataElement = 48;
            pdsElement = 58;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C071 - PDS 0059 Token ID
            dataElement = 48;
            pdsElement = 59;
            lenDataElemnt = 11;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C072 - Addit mercahnt data
            dataElement = 48;
            pdsElement = 208;
            lenDataElemnt = 26;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C073 - Indep sales org ID
            dataElement = 48;
            pdsElement = 209;
            lenDataElemnt = 21;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C074 - PDS 0002 GCMS
            dataElement = 48;
            pdsElement = 2;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C075 - DE 40 Cod servicio 
            dataElement = 40;
            lenDataElemnt = 3;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C076 - PDS 0072 Authenticas
            dataElement = 48;
            pdsElement = 72;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C077 - DE 5 Fact Tarjeta
            dataElement = 5;
            lenDataElemnt = 12;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C078 - DE 32 Codigo ident ad
            dataElement = 32;
            lenDataElemnt = 11;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C079 - DE 100 Cod ident re 
            dataElement = 100;
            lenDataElemnt = 11;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C080 - PDS 0005 ind error MJ
            dataElement = 48;
            pdsElement = 5;
            lenDataElemnt = 140;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C081 - PDS 0146 Mont Car tra
            dataElement = 48;
            pdsElement = 146;
            lenDataElemnt = 144;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C082 - PDS 0170 Inf cont tarj
            dataElement = 48;
            pdsElement = 170;
            lenDataElemnt = 57;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C083 - PDS 0175 URL Acept tj
            dataElement = 48;
            pdsElement = 175;
            lenDataElemnt = 125;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C084 - PDS 0191 For mj ori
            dataElement = 48;
            pdsElement = 191;
            lenDataElemnt = 1;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C085 - PDS 0202 PAN ERR SINT
            dataElement = 48;
            pdsElement = 202;
            lenDataElemnt = 19;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C086 - PDS 0204 Mont err sint
            dataElement = 48;
            pdsElement = 204;
            lenDataElemnt = 12;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C087 - PDS 0205 Dev sint mj
            dataElement = 48;
            pdsElement = 205;
            lenDataElemnt = 14;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C088 - PDS 0225 Dev cod mot
            dataElement = 48;
            pdsElement = 225;
            lenDataElemnt = 4;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C089 - PDS 0446 Mont Tran er
            dataElement = 48;
            pdsElement = 446;
            lenDataElemnt = 144;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C090 - PDS 0501 Des Transac
            dataElement = 48;
            pdsElement = 501;
            lenDataElemnt = 16;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C091 - PDS 0663 Des for libr
            dataElement = 48;
            pdsElement = 663;
            lenDataElemnt = 125;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C092 - DE 10 Conver rate
            dataElement = 10;
            lenDataElemnt = 10;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C093 - PDS 0071 Magn strip
            dataElement = 48;
            pdsElement = 71;
            lenDataElemnt = 4;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C094 - PDS 0004 - Font Acc l
            dataElement = 48;
            pdsElement = 4;
            lenDataElemnt = 36;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C095 - PDS 0207 Wallet Iden
            dataElement = 48;
            pdsElement = 207;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C096 - PDS 0502 Custom Iden
            dataElement = 48;
            pdsElement = 502;
            lenDataElemnt = 82;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C097 - DE 71 Message number 
            dataElement = 71;
            lenDataElemnt = 8;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoDE(dataElement, iso);

            //C098 - PDS 0670 Payer name
            dataElement = 48;
            pdsElement = 670;
            lenDataElemnt = 104;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C099 - PDS 0671 Dat fu re
            dataElement = 48;
            pdsElement = 671;
            lenDataElemnt = 6;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C100 - PDS 0673 Dat an re
            dataElement = 48;
            pdsElement = 673;
            lenDataElemnt = 6;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C101 - PDS 0674 AD Tra Re
            dataElement = 48;
            pdsElement = 674;
            lenDataElemnt = 19;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C102 - PDS 0675 Ad Tra DE
            dataElement = 48;
            pdsElement = 675;
            lenDataElemnt = 15;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C103 - PDS 0765 Mon re da
            dataElement = 48;
            pdsElement = 765;
            lenDataElemnt = 169;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C104 - PDS 0181 paym data
            dataElement = 48;
            pdsElement = 181;
            lenDataElemnt = 68;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C105 - PDS 0014 AC Ref N
            dataElement = 48;
            pdsElement = 14;
            lenDataElemnt = 19;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C106 - PDS 0147 Extended percision
            dataElement = 48;
            pdsElement = 147;
            lenDataElemnt = 144;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C107 - PDS 0715 Ancillary service charges
            dataElement = 48;
            pdsElement = 715;
            lenDataElemnt = 56;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C108 - PDS 0674  Addtl trace / reference numb
            dataElement = 48;
            pdsElement = 674;
            lenDataElemnt = 19;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C109 - PDS 0018  mPOS Accepance device type
            dataElement = 48;
            pdsElement = 18;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C110 - PDS 0184 Directory server transaction
            dataElement = 48;
            pdsElement = 184;
            lenDataElemnt = 36;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C111 - PDS 0185 AccountHoder Autentication 
            dataElement = 48;
            pdsElement = 185;
            lenDataElemnt = 32;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C112 - PDS 0186 Program Protocol
            dataElement = 48;
            pdsElement = 186;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C113 - PDS 0198 Device Type
            dataElement = 48;
            pdsElement = 198;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C114 - PDS ???? Claim cbk ID, C115 - Cbk ID, C116 - Trasaction Status
            sb.Append("".PadRight(45));

            //C117 - PDS 0025 Revesal Indicator
            dataElement = 48;
            pdsElement = 25;
            lenDataElemnt = 5;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C118 - PDS ???? Reason for rejection
            sb.Append("".PadRight(100));

            //C119 - PDS 0015
            dataElement = 48;
            pdsElement = 15;
            lenDataElemnt = 7;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C120 - PDS 0222
            dataElement = 48;
            pdsElement = 222;
            //lenDataElemnt = 126;//////???????????????????????????????????????
			lenDataElemnt = 65;//////???????????????????????????????????????
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C121 - PDS 0213
            dataElement = 48;
            pdsElement = 213;
            lenDataElemnt = 3;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //C122 - PDS 0218
            dataElement = 48;
            pdsElement = 218;
            lenDataElemnt = 4;
            valor = iso.GetPds(pdsElement, dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            LogueoPDS(pdsElement, dataElement, iso);

            //Filler C123 Longitud 21, C124 longitud 26, C125 longitud 63, C126 longitud 29
            sb.Append("".PadRight(139));

            //Campo DE 105 - New
            dataElement = 105;
            lenDataElemnt = 999;
            valor = iso.GetBit(dataElement).conteudo.Trim();
            valorTruncado = valor.Substring(0, valor.Length > lenDataElemnt ? lenDataElemnt : valor.Length).PadRight(lenDataElemnt);
            sb.Append(valorTruncado);
            if (!string.IsNullOrEmpty(valorTruncado.Trim()))
            {

            }
            LogueoDE(dataElement, iso);

            return sb.ToString();

        }

        private void LogueoPDS(int number, int bit, ISO iso)
        {
            if (activaLog.Equals("1"))
                Log.Evento("[CONVERSOR PDS " + number + ", " + bit + " ] " + iso.GetPds(number, bit).conteudo);
        }

        private void LogueoDE(int number, ISO iso)
        {
            if (activaLog.Equals("1"))
                Log.Evento("[CONVERSOR DE " + number + "] " + iso.GetBit(number).conteudo);
        }
    }
}
