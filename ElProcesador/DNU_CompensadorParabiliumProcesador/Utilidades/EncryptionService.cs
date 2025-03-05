using log4net.Repository.Hierarchy;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumProcesador.Utilidades
{
    public  class EncryptionService
    {
        public static void ExecuteCommand(string _Command)
        {
            //while (true)
            //{
                Console.Write("#:");
                _Command = Console.ReadLine();
                //if (_Command == "Salir" || _Command == "salir")
                //{
                //    break;
                //}

                //Indicamos que deseamos inicializar el proceso cmd.exe junto a un comando de arranque. 
                //(/C, le indicamos al proceso cmd que deseamos que cuando termine la tarea asignada se cierre el proceso).
                //Para mas informacion consulte la ayuda de la consola con cmd.exe /? 
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + _Command);
                // Indicamos que la salida del proceso se redireccione en un Stream
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                //Indica que el proceso no despliegue una pantalla negra (El proceso se ejecuta en background)
               // procStartInfo.CreateNoWindow = true;
                //Inicializa el proceso
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                //Consigue la salida de la Consola(Stream) y devuelve una cadena de texto
                string result = proc.StandardOutput.ReadToEnd();
            //Muestra en pantalla la salida del Comando
            Console.WriteLine(result);

            //return result;
            //}
        }


        public static void DecryptFile(
            string inputFileName,
            string keyFileName,
            string passwd,
            string defaultFileName)
        {
            using (Stream input = File.OpenRead(inputFileName),
                   keyIn = File.OpenRead(keyFileName))
            {
                DecryptPgpData(input, keyIn, passwd, defaultFileName);
            }
        }


        public static void DecryptPgpData(Stream inputStream, Stream privateKeyStream, string passPhrase, string defaultFileName)
        {
            try
            {
                PgpObjectFactory pgpFactory = new PgpObjectFactory(PgpUtilities.GetDecoderStream(inputStream));
                // find secret key
                PgpSecretKeyRingBundle pgpKeyRing = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));

                PgpObject pgp = null;
                if (pgpFactory != null)
                {
                    pgp = pgpFactory.NextPgpObject();
                }

                // the first object might be a PGP marker packet.
                PgpEncryptedDataList encryptedData = null;
                if (pgp is PgpEncryptedDataList)
                {
                    encryptedData = (PgpEncryptedDataList)pgp;
                }
                else
                {
                    encryptedData = (PgpEncryptedDataList)pgpFactory.NextPgpObject();
                }

                // decrypt
                PgpPrivateKey privateKey = null;
                PgpPublicKeyEncryptedData pubKeyData = null;
                foreach (PgpPublicKeyEncryptedData pubKeyDataItem in encryptedData.GetEncryptedDataObjects())
                {
                    privateKey = FindSecretKey(pgpKeyRing, pubKeyDataItem.KeyId, passPhrase.ToCharArray());

                    if (privateKey != null)
                    {
                        pubKeyData = pubKeyDataItem;
                        break;
                    }
                }

                if (privateKey == null)
                {
                    throw new ArgumentException("Secret key for message not found.");
                }

                PgpObjectFactory plainFact = null;
                using (Stream clear = pubKeyData.GetDataStream(privateKey))
                {
                    plainFact = new PgpObjectFactory(clear);
                }

                PgpObject message = plainFact.NextPgpObject();

                if (message is PgpCompressedData)
                {
                    PgpCompressedData compressedData = (PgpCompressedData)message;
                    PgpObjectFactory pgpCompressedFactory = null;

                    using (Stream compDataIn = compressedData.GetDataStream())
                    {
                        pgpCompressedFactory = new PgpObjectFactory(compDataIn);
                    }

                    message = pgpCompressedFactory.NextPgpObject();
                    PgpLiteralData literalData = null;
                    if (message is PgpOnePassSignatureList)
                    {
                        message = pgpCompressedFactory.NextPgpObject();
                    }

                    literalData = (PgpLiteralData)message;
                    string outFileName = defaultFileName;

                    Stream fOut = File.Create(outFileName);
                    Stream unc = literalData.GetInputStream();
                    Streams.PipeAll(unc, fOut);
                    fOut.Close();

                }
                else if (message is PgpLiteralData)
                {
                    PgpLiteralData ld = (PgpLiteralData)message;

                    string outFileName = defaultFileName;

                    Stream fOut = File.Create(outFileName);
                    Stream unc = ld.GetInputStream();
                    Streams.PipeAll(unc, fOut);
                    fOut.Close();
                }
                else if (message is PgpOnePassSignatureList)
                {
                    throw new PgpException("Encrypted message contains a signed message - not literal data.");
                }
                else
                {
                    throw new PgpException("Message is not a simple encrypted file - type unknown.");
                }
            }
            catch (Exception ex) 
            {
                throw ex;
            }

        }

        private static PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);
            if (pgpSecKey == null)
            {
                return null;
            }

            return pgpSecKey.ExtractPrivateKey(pass);
        }
    }
}
