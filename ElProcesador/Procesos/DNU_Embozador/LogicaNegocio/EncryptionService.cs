using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.LogicaNegocio
{
    public class EncryptionService
    {
        public PgpPublicKeyRing asciiPublicKeyToRing(string ascfilein)
        {
            using (Stream pubFis = File.OpenRead(ascfilein))
            {
                var pubArmoredStream = new ArmoredInputStream(pubFis);

                PgpObjectFactory pgpFact = new PgpObjectFactory(pubArmoredStream);
                Object opgp = pgpFact.NextPgpObject();
                var pkr = opgp as PgpPublicKeyRing;
                return pkr;
            }
        }

        public PgpPublicKey getFirstPublicEncryptionKeyFromRing(PgpPublicKeyRing pkr)
        {
            foreach (PgpPublicKey k in pkr.GetPublicKeys())
            {
                if (k.IsEncryptionKey)
                    return k;
            }
            throw new ArgumentException("Can't find encryption key in key ring.");
        }

        public static void EncryptFile(string inputFile, string outputFile, PgpPublicKey encKey, bool armor,
            bool withIntegrityCheck)
        {
            using (MemoryStream bOut = new MemoryStream())
            {
                PgpCompressedDataGenerator comData = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Zip);
                PgpUtilities.WriteFileToLiteralData(comData.Open(bOut), PgpLiteralData.Binary,
                    new FileInfo(inputFile));

                comData.Close();
                PgpEncryptedDataGenerator cPk = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Aes256,
                    withIntegrityCheck, new SecureRandom());

                cPk.AddMethod(encKey);
                byte[] bytes = bOut.ToArray();

                using (Stream outputStream = File.Create(outputFile))
                {
                    if (armor)
                    {
                        using (ArmoredOutputStream armoredStream = new ArmoredOutputStream(outputStream))
                        using (Stream cOut = cPk.Open(armoredStream, bytes.Length))
                        {
                            cOut.Write(bytes, 0, bytes.Length);
                        }
                    }
                    else
                    {
                        using (Stream cOut = cPk.Open(outputStream, bytes.Length))
                        {
                            cOut.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
            }
        }
    }
}
