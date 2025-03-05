using DNU_CompensadorParabiliumCommon.Utilidades;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DNU_CompensadorParabiliumReportes.LogicaNegocio
{
    public class LNFtp
    {

        private string _host;
        private string _path;
        private string _user;
        private string _password;
        private string _port;

        public LNFtp(string host, string path, string user, string password, string port)
        {
            this._host = host;
            this._path = path;
            this._user = user;
            this._password = password;
            this._port = port;
        }

        public bool DoesFtpDirectoryExist(string dirPath)
        {
            bool isexist = false;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(dirPath);
                request.Credentials = new NetworkCredential(this._user, this._password);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    isexist = true;
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        return false;
                    }
                }
            }
            return isexist;
        }

        public bool CreateFolder(string dirPath)
        {

            bool IsCreated = true;
            try
            {
                WebRequest request = WebRequest.Create(dirPath);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                request.Credentials = new NetworkCredential(this._user, this._password);
                using (var resp = (FtpWebResponse)request.GetResponse())
                {
                    Log.Evento("Generacion Path FTP destino " + resp.StatusCode);
                }
            }
            catch (Exception ex)
            {
                IsCreated = false;
            }
            return IsCreated;
        }

        public void UploadFileFTP(string from, string to)
        {
            string From = from;
            string To = to;

            using (WebClient client = new WebClient())
            {
                client.Credentials = new NetworkCredential(this._user, this._password);

                client.UploadFile(To, WebRequestMethods.Ftp.UploadFile, From);
            }
        }


        /// <summary>
        /// Subir archivo por SFTP modificado para cacao
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>

        public void UploadFileSFTP(string from, string to)
        {
            // KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(this._user);
            // keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);
            try
            {
                using (var client = new SftpClient(this._host, Convert.ToInt32(this._port), this._user, this._password))
                {
                    client.Connect();
                    client.ChangeDirectory(this._path); ///ruta destino dentro del sftp
                    if (client.IsConnected)
                    {
                        Log.Evento("Conectado SFTP " + this._host);

                        using (var fileStream = new FileStream(from, FileMode.Open))
                        {

                            client.BufferSize = 4 * 1024; // bypass Payload error large files
                            client.UploadFile(fileStream, Path.GetFileName(from));
                        }
                    }
                    else
                    {
                        throw new Exception("Ocurrio error al conectar SFTP " + this._host);
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Ocurrio error al conectar SFTP " + this._host + " " + ex.Message.ToString());
            }

        }

        private void HandleKeyEvent(object sender, AuthenticationPromptEventArgs e)
        {
            foreach (AuthenticationPrompt prompt in e.Prompts)
            {
                if (prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    prompt.Response = this._password;
                }
            }
        }
    }
}
