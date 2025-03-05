using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DNU_ProcesadorArchivos510.LogicaNegocio
{
    public class LNFTP
    {

         public Boolean EnviarFTP(string strServer, string strUser, string strPassword, string strFileNameLocal, string strPathFTP)
        {
            try
            {
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(string.Format("ftp://{0}/{1}", strServer, Path.Combine(strPathFTP, Path.GetFileName(strFileNameLocal))));
                request.Method = WebRequestMethods.Ftp.UploadFile;

                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(strUser, strPassword);

                // Copy the contents of the file to the request stream.
                StreamReader sourceStream = new StreamReader(strFileNameLocal);
                byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
                sourceStream.Close();
                request.ContentLength = fileContents.Length;

                Stream requestStream = request.GetRequestStream();
                requestStream.Write(fileContents, 0, fileContents.Length);
                requestStream.Close();

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

                response.Close();

                return true;
            }catch (Exception err)
            {
                return false;
            }


        }

        //public  Boolean EnviarFTP(string strServer, string strUser, string strPassword, string strFileNameLocal, string strPathFTP)
        //{
        //    try
        //    {
        //        FtpWebRequest ftpRequest;

        //        // Crea el objeto de conexión del servidor FTP
        //        ftpRequest = (FtpWebRequest)WebRequest.Create(string.Format("ftp://{0}/{1}", strServer,Path.Combine(strPathFTP, Path.GetFileName(strFileNameLocal))));
        //        // Asigna las credenciales
        //        ftpRequest.Credentials = new NetworkCredential(strUser, strPassword);
        //        // Asigna las propiedades
        //        ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
        //        ftpRequest.UsePassive = true;
        //        ftpRequest.UseBinary = true;
        //        ftpRequest.KeepAlive = false;
        //        // Lee el archivo y lo envía
        //        using (FileStream stmFile = File.OpenRead(strFileNameLocal))
        //        { // Obtiene el stream sobre la comunicación FTP
        //            using (Stream stmFTP = ftpRequest.GetRequestStream())
        //            {
        //                int cnstIntLengthBuffer = 0;
        //                byte[] arrBytBuffer = new byte[cnstIntLengthBuffer];
        //                int intRead;

        //                // Lee y escribe el archivo en el stream de comunicaciones
        //                while ((intRead = stmFile.Read(arrBytBuffer, 0, cnstIntLengthBuffer)) != 0)
        //                    stmFTP.Write(arrBytBuffer, 0, intRead);
        //                // Cierra el stream FTP
        //                stmFTP.Close();
        //            }
        //            // Cierra el stream del archivo
        //            stmFile.Close();
        //        }



        //    }
        //    catch (Exception err)
        //    {
        //        return false;
        //    }

        //    return true;
        //}

    }
}
