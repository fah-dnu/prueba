using CommonProcesador;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_SFTP.Services
{
    public class SFTPService
    {
        private static bool CreateDirectoriesRecursively(string directory, SftpClient sftp, string separatorDirectory)
        {
            bool created = false;
            try
            {
                FileInfo fi = new FileInfo(directory);
                directory = directory.Replace(@"\", "/");
                // directory = directory.Split(':')[1];

                String[] spearator = { separatorDirectory };
                Int32 count = 2;

                // using the method
                String[] strlist = directory.Split(spearator, count, StringSplitOptions.RemoveEmptyEntries);
                if (strlist.Length == 1)
                {
                    directory = directory.Split(spearator, count, StringSplitOptions.RemoveEmptyEntries)[0];
                }
                else
                {
                    directory = directory.Split(spearator, count, StringSplitOptions.RemoveEmptyEntries)[1];
                }
                foreach (string dir in directory.Split('/'))
                {
                    // Ignoring leading/ending/multiple slashes
                    if (dir == fi.Name)
                    {
                        break;
                    }
                    if (!string.IsNullOrWhiteSpace(dir))
                    {
                        if (!sftp.Exists(dir))
                        {
                            sftp.CreateDirectory(dir);
                            created = true;
                        }
                        sftp.ChangeDirectory(dir);
                        created = true;
                    }
                }
                // Going back to default directory
              //  sftp.ChangeDirectory("home");
                //sftp.ChangeDirectory("../");
                //sftp.ChangeDirectory("./");
            }
            catch (Exception ex)
            {
                created = false;
                throw new Exception(ex.Message, ex);
            }
            return created;
        }

        public static bool CreateDirectoryWithConnection(string host, int port, string username, string password, string directory, string separatorDirectory)
        {
            bool created = false;

            try
            {
                using (SftpClient sftp = new SftpClient(host, port, username, password))
                {
                    sftp.Connect();

                    created = CreateDirectoriesRecursively(directory, sftp, separatorDirectory);
                    //var files = sftp.ListDirectory(remoteDirectory);

                    //foreach (var file in files)
                    //{
                    //    Console.WriteLine(file.Name);
                    //}

                    sftp.Disconnect();

                }
            }
            catch (Exception e)
            {
                created = false;
            }
            return created;

        }

        public static bool CreateFileAndDirectoryWithConnection(string host, int port, string username, string password, string absoluteFilePath, string separatorDirectory)
        {
            bool created = false;
            // FileInfo fi = new FileInfo(absoluteFilePath);
            try
            {
                using (SftpClient sftp = new SftpClient(host, port, username, password))
                {

                    sftp.Connect();
                    string directoryMain = sftp.WorkingDirectory;
                    created = CreateDirectoriesRecursively(Path.GetFullPath(absoluteFilePath), sftp, separatorDirectory);
                    using (var fileStream = new FileStream(absoluteFilePath, FileMode.Open))
                    {

                        sftp.BufferSize = 4 * 1024; // bypass Payload error large files
                        sftp.UploadFile(fileStream, sftp.WorkingDirectory + "/" + Path.GetFileName(absoluteFilePath));//Path.GetFileName(absoluteFilePath));
                    }
                    sftp.ChangeDirectory(directoryMain);
                    ////forma 2
                    //var fsIn = sftp.OpenRead(absoluteFilePath);
                    //var fsOut = sftp.OpenWrite(absoluteFilePath);

                    //int data;
                    //while ((data = fsIn.ReadByte()) != -1)
                    //    fsOut.WriteByte((byte)data);

                    //fsOut.Flush();
                    //fsIn.Close();
                    //fsOut.Close();

                    //var files = sftp.ListDirectory(remoteDirectory);

                    //foreach (var file in files)
                    //{
                    //    Console.WriteLine(file.Name);
                    //}

                    sftp.Disconnect();

                }
            }
            catch (Exception e)
            {
                created = false;
                throw new Exception(e.Message, e);

            }
            return created;

        }


        public static bool CreateFileAndDirectoryWithoutConnection(SftpClient sftp, string absoluteFilePath, string separatorDirectory)
        {
            bool created = false;
            // FileInfo fi = new FileInfo(absoluteFilePath);
            try
            {
                //using (SftpClient sftp = new SftpClient(host, port, username, password))
                //{

                //    sftp.Connect();

                created = CreateDirectoriesRecursively(Path.GetFullPath(absoluteFilePath), sftp, separatorDirectory);
                using (var fileStream = new FileStream(absoluteFilePath, FileMode.Open))
                {

                    sftp.BufferSize = 4 * 1024; // bypass Payload error large files
                    sftp.UploadFile(fileStream, sftp.WorkingDirectory + "/" + Path.GetFileName(absoluteFilePath));//Path.GetFileName(absoluteFilePath));
                }

                ////forma 2
                //var fsIn = sftp.OpenRead(absoluteFilePath);
                //var fsOut = sftp.OpenWrite(absoluteFilePath);

                //int data;
                //while ((data = fsIn.ReadByte()) != -1)
                //    fsOut.WriteByte((byte)data);

                //fsOut.Flush();
                //fsIn.Close();
                //fsOut.Close();

                //var files = sftp.ListDirectory(remoteDirectory);

                //foreach (var file in files)
                //{
                //    Console.WriteLine(file.Name);
                //}

                //    sftp.Disconnect();

                //}
            }
            catch (Exception e)
            {
                created = false;
                Logueo.Error("[GeneraEstadoCuentaCredito] error sftp:" + e.Message );

                throw new Exception(e.Message, e);

            }
            return created;

        }

        public static bool DownloadFileWithoutConnection(SftpClient sftp, string pathRemoteFile, string pathLocalFile)
        {
            bool download = false;
            // FileInfo fi = new FileInfo(absoluteFilePath);
            try
            {
                using (Stream fileStream = File.OpenWrite(pathLocalFile))
                {
                    sftp.DownloadFile(pathRemoteFile, fileStream);
                }
                download = true;
            }
            catch (Exception e)
            {
                download = false;
                throw new Exception(e.Message, e);

            }
            return download;

        }

        public static bool CreateDirectoryWithoutConnection(string directory, SftpClient sftp)
        {
            bool created = false;
            try
            {
               
              
                    if (!string.IsNullOrWhiteSpace(directory))
                    {
                        if (!sftp.Exists(directory))
                        {
                            sftp.CreateDirectory(directory);
                            created = true;
                        }
                      //  sftp.ChangeDirectory(directory);
                        created = true;
                    }
               
                // Going back to default directory
                //  sftp.ChangeDirectory("home");
                //sftp.ChangeDirectory("../");
                //sftp.ChangeDirectory("./");
            }
            catch (Exception ex)
            {
                created = false;
                throw new Exception(ex.Message, ex);
            }
            return created;
        }


        public static bool CreateFileWithoutConnection(SftpClient sftp, string absoluteFilePath)
        {
            bool created = false;
            // FileInfo fi = new FileInfo(absoluteFilePath);
            try
            {
                //using (SftpClient sftp = new SftpClient(host, port, username, password))
                //{

                //    sftp.Connect();

                using (var fileStream = new FileStream(absoluteFilePath, FileMode.Open))
                {

                    sftp.BufferSize = 4 * 1024; // bypass Payload error large files
                    sftp.UploadFile(fileStream, sftp.WorkingDirectory + "/" + Path.GetFileName(absoluteFilePath));//Path.GetFileName(absoluteFilePath));
                }
                created = true;



            }
            catch (Exception e)
            {
                created = false;
                Logueo.Error("[GeneraEstadoCuentaCredito] error sftp:" + e.Message);

                throw new Exception(e.Message, e);

            }
            return created;

        }


        //private void HandleKeyEvent(object sender, AuthenticationPromptEventArgs e)
        //{
        //    string passUsuarioSFTP = PNConfig.Get("PROCESAEDOCUENTA", "PassUsuarioSFTP");
        //    foreach (AuthenticationPrompt Prompt in e.Prompts)
        //    {
        //        if (Prompt.Request.IndexOf("Password:", StringComparison.InvariantCultureIgnoreCase) != -1)
        //        {
        //            Prompt.Response = passUsuarioSFTP;
        //        }
        //    }
        //}
    }
}

