using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.LogicaNegocio
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

        public void UploadFileSFTP(string from)
        {
            using (var client = new SftpClient(this._host, Convert.ToInt32(this._port), this._user, this._password))
            {
                client.Connect();
                if (client.IsConnected)
                {
                    Log.Evento("Conectado SFTP " + this._host);

                    using (var fileStream = new FileStream(from, FileMode.Open))
                    {
                        Log.Evento("[RUTA SFTP]" + this._path + Path.GetFileName(from));
                        client.UploadFile(fileStream, this._path + Path.GetFileName(from));
                    }
                }
                else
                {
                    Log.Error("Ocurrio error al conectar SFTP " + this._host);
                }
            }
        }

        public void UploadFileSFTPKeyEvent(string from)
        {
            KeyboardInteractiveAuthenticationMethod keybAuth = new KeyboardInteractiveAuthenticationMethod(this._user);
            keybAuth.AuthenticationPrompt += new EventHandler<AuthenticationPromptEventArgs>(HandleKeyEvent);

            ConnectionInfo conInfo = new ConnectionInfo(this._host, Convert.ToInt32(this._port), this._user, keybAuth);

            using (var client = new SftpClient(conInfo))
            {
                client.Connect();
                if (client.IsConnected)
                {
                    Log.Evento("Conectado SFTP " + this._host);

                    using (var fileStream = new FileStream(from, FileMode.Open))
                    {
                        Log.Evento("[RUTA SFTP]" + this._path + Path.GetFileName(from));
                        client.UploadFile(fileStream, this._path + Path.GetFileName(from), null);
                    }
                }
                else
                {
                    Log.Error("Ocurrio error al conectar SFTP " + this._host);
                }
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

        public void UploadFileSFTPKeyFile(string from, string keyFile)
        {
            var key = File.ReadAllText(keyFile);
            var buf = new MemoryStream(Encoding.UTF8.GetBytes(key));
            var privateKeyFile = new PrivateKeyFile(buf);
            var connectionInfo = new ConnectionInfo(this._host, Convert.ToInt32(this._port), this._user,
                                        new PrivateKeyAuthenticationMethod(this._user, privateKeyFile));

            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                if (client.IsConnected)
                {
                    Log.Evento("Conectado SFTP " + this._host);

                    using (var fileStream = new FileStream(from, FileMode.Open))
                    {
                        Log.Evento("[RUTA SFTP]" + this._path + Path.GetFileName(from));
                        client.UploadFile(fileStream, this._path + Path.GetFileName(from));
                    }
                }
                else
                {
                    Log.Error("Ocurrio error al conectar SFTP " + this._host);
                }
            }

        }
    }
}
