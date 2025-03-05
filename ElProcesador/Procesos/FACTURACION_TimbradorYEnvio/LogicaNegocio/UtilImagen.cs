using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using DALCentralAplicaciones.Utilidades;

namespace FACTURACION_Timbrador.LogicaNegocio
{
    public class UtilImagen
    {
        public static Byte[] CargarImagen(string rutaArchivo)
        {


            if (rutaArchivo != "")
            {

                try
                {

                    FileStream Archivo = new FileStream(rutaArchivo, FileMode.Open);//Creo el archivo
                    BinaryReader binRead = new BinaryReader(Archivo);//Cargo el Archivo en modo binario

                    Byte[] imagenEnBytes = new Byte[(Int64)Archivo.Length]; //Creo un Array de Bytes donde guardare la imagen

                    binRead.Read(imagenEnBytes, 0, (int)Archivo.Length);//Cargo la imagen en el array de Bytes
                    binRead.Close();

                    Archivo.Close();

                    return imagenEnBytes;//Devuelvo la imagen convertida en un array de bytes

                }


                catch (Exception err)
                {
                    Loguear.Error(err, "FACTURACION");
                    return new Byte[0];
                }

            }

            return new byte[0];

        }
    }
}