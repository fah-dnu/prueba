using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumConversorArchivo
{
    public class Bit
    {
        public int bit;
        public int tam;
        public string tipo;
        public string formato;
        public string conteudo;
        public List<PDS> pds;


    }

    public class PDS
    {
        public int number;
        public int tam;
        public string conteudo;
    }

    public class ISO
    {

        private int _idx;
        private string _mti;
        private string _bmp;
        private Bit[] _bits;
        private List<Bit> _lstBits;
        private string[] _bitmap;
        private byte[] _bt;

        public string MTI
        {
            get { return this._mti; }
        }

        public string BMP
        {
            get { return this._bmp; }
        }

        public Bit[] BITS
        {
            get { return this._bits; }
        }

        public string[] BITMAP
        {
            get { return this._bitmap; }
        }

        public int IDX
        {
            get { return this._idx; }
        }

        public ISO(string mti, string bitmap, byte[] bt, int idx)
        {
            this._idx = idx;
            this._mti = mti;
            this._bmp = bitmap;
            this._bt = bt;

            CarregarMapaBits();
        }

        #region "--- Metodos"


        /// <summary>
        /// Monta a estrutura de Bits da Mensagem
        /// </summary>
        private void CarregarMapaBits()
        {
            int i;
            int idx;
            string str;

            i = 2;
            idx = 1;
            _bitmap = new string[130];
            str = string.Empty;

            this._bits = new Bit[128];
            var b = new Bit();
            b.bit = 2;
            b.tam = 0;
            b.tipo = "LL";
            b.formato = "EBCDIC";
            b.pds = null;
            this._bits[i] = b;

            b = new Bit();
            b.bit = 2;
            b.tam = 0;
            b.tipo = "LL";
            b.formato = "EBCDIC";
            b.pds = null;
            this._bits[i] = b;


            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 6;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 12;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 12;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 12;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 9;
            b = new Bit();
            b.bit = i;
            b.tam = 8;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 8;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 12;
            b = new Bit();
            b.bit = i;
            b.tam = 12;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 14;
            b = new Bit();
            b.bit = i;
            b.tam = 4;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 22;
            b = new Bit();
            b.bit = i;
            b.tam = 12;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 3;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 3;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 4;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 4;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 30;
            b = new Bit();
            b.bit = i;
            b.tam = 24;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 37;
            b = new Bit();
            b.bit = i;
            b.tam = 12;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 6;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 40;
            b = new Bit();
            b.bit = i;
            b.tam = 3;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 8;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 15;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 48;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LLL";
            b.formato = "EBCDIC";
            b.pds = new List<PDS>();
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 3;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 3;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 3;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 54;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LLL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LLL";
            b.formato = "HEXA";
            this._bits[i] = b;

            i = 62;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LLL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LLL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 71;
            b = new Bit();
            b.bit = i;
            b.tam = 8;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LLL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 6;
            b.tipo = "";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 93;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 100;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 105;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LLL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 111;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LLL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 123;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LLL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i++;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LLL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            i = 127;
            b = new Bit();
            b.bit = i;
            b.tam = 0;
            b.tipo = "LLL";
            b.formato = "EBCDIC";
            this._bits[i] = b;

            _lstBits = this._bits.ToList();

            // Seta os bits que estao ligado
            for (i = 0; i < this._bmp.Length; i++)
            {
                str = Conversor.Right("0000" + Convert.ToString(Convert.ToInt32(Conversor.ConvertHexToDecimal(this._bmp.Substring(i, 1)).ToString()), 2), 4);

                for (int z = 0; z < str.Length; z++)
                {
                    this._bitmap[idx] = str.Substring(z, 1);
                    idx++;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ISO MontaMensagemISO()
        {
            int tamLVAR = 0;
            int tamBIT = 0;
            string aux = string.Empty;
            string aux2 = string.Empty;

            _lstBits = new List<Bit>();
            int count = 0;
            int i = 0;
            try
            {
                // Le Bit a Bit
                for (i = 2; i < 128; i++)
                {
                    try
                    {
                        aux = string.Empty;
                        aux2 = string.Empty;

                        // Se o bit n?o estiver presente, vai para o proximo
                        if (this._bitmap[i].CompareTo("0") == 0)
                            continue;

                        count = i;
                        // Recupera o tamanho do BIT
                        // O tamanho do Bit pode se variavel ou fixo
                        if (this._bits[i].tam == 0)
                        {
                            if (this._bits[i].tipo.CompareTo("LL") == 0) // LLVAR
                                tamLVAR = 2;
                            else // LLLVAR
                                tamLVAR = 3;

                            // Recupera o tamanho do bit
                            for (int ii = 0; ii < tamLVAR; ii++)
                            {
                                aux = this._bt[this._idx].ToString();
                                if (Int32.Parse(aux) < 240 || Int32.Parse(aux) > 249)
                                {
                                    aux = Conversor.convertEBCDICtoASCII(Convert.ToChar(Int32.Parse(aux)).ToString());
                                }
                                else
                                {
                                    aux = Conversor.convertASCIItoEBCDIC(aux);
                                    aux = Conversor.convertEBCDICtoASCII(aux.Substring(2, 1)).ToString();
                                }
                                aux2 += aux;
                                this._idx++;
                            }
                            tamBIT = Convert.ToInt32(aux2);
                            this._bits[i].tam = tamBIT;
                        }
                        else // Tamanho Fixo
                        {
                            tamBIT = this._bits[i].tam;
                        }

                        aux = string.Empty;
                        aux2 = string.Empty;

                        // Le o conteudo do Bit
                        for (int ii = 0; ii < tamBIT; ii++)
                        {
                            aux = this._bt[this._idx].ToString();

                            if (this._bits[i].formato.CompareTo("EBCDIC") == 0)
                            {

                                if (Int32.Parse(aux) < 240 || Int32.Parse(aux) > 249)
                                {
                                    aux = Conversor.convertEBCDICtoASCII(Convert.ToChar(Int32.Parse(aux)).ToString());
                                }
                                else
                                {
                                    aux = Conversor.convertASCIItoEBCDIC(aux);
                                    aux = Conversor.convertEBCDICtoASCII(aux.Substring(2, 1)).ToString();
                                }
                            }
                            else
                            {
                                aux = Conversor.ConvertDecimalToHex(aux);
                            }

                            aux2 += aux;
                            this._idx++;
                        }

                        this._bits[i].conteudo = aux2;

                        if (i == 48 || i == 125)
                        {
                            this._bits[i].pds = NewPDS(this._bits[i].conteudo);
                        }
                        _lstBits.Add(this._bits[i]);
                    }
                    catch (Exception e)
                    {
                    }
                }
                return this;
            }

            catch (Exception ex)
            {

                throw new Exception("Erro no tratamento do bit:" + i);
            }
        }

        public List<PDS> NewPDS(string conteudo)
        {
            var offset = 0;
            var pds = new List<PDS>();

            while (offset < conteudo.Length)
            {

                var p = new PDS();

                p.number = Int32.Parse(conteudo.Substring(offset, 4));
                offset += 4;
                p.tam = Int32.Parse(conteudo.Substring(offset, 3));
                offset += 3;
                p.conteudo = conteudo.Substring(offset, p.tam);
                offset += p.tam;
                pds.Add(p);

            }

            return pds;
        }

        public PDS GetPds(int number, int bit)
        {
            var bitP = _lstBits.Where(b => b.bit == bit)
                 .DefaultIfEmpty(new Bit { bit = number, conteudo = string.Empty, tam = 0, pds = new List<PDS>() })
                 .FirstOrDefault();


            var pds = bitP.pds.Where(p => p.number == number)
                .DefaultIfEmpty(new PDS { number = number, tam = 0, conteudo = string.Empty })
                .FirstOrDefault();


            return pds;
        }


        public Bit GetBit(int number)
        {

            var bit = _lstBits.Where(b => b.bit == number)
                .DefaultIfEmpty(new Bit { bit = number, conteudo = string.Empty, tam = 0 })
                .FirstOrDefault();

            return bit;


        }
        #endregion
    }
}
