using CommonProcesador;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace DNU_ConciliadorCacao.Entidades
{
    public class Archivo
    {
        public FilaConfig laConfiguracionHeaderLectura { get; set; }
        public FilaConfig laConfiguracionDetalleLectura { get; set; }
        public FilaConfig laConfiguracionDetalleExtraLectura { get; set; }
        public FilaConfig laConfiguracionFooterLectura { get; set; }

        public FilaConfig laConfiguracionHeaderEscritura { get; set; }
        public FilaConfig laConfiguracionDetalleEscritura { get; set; }
        public FilaConfig laConfiguracionFooterEscritura { get; set; }

        public ConfigRespuesta laConfiguracionHeaderRespuesta { get; set; }
        public ConfigRespuesta laConfiguracionDetalleRespuesta { get; set; }
        public ConfigRespuesta laConfiguracionFooterRespuesta { get; set; }


        private List<Fila> losDatos = new List<Fila>();

        public List<Fila> LosDatos
        {
            get { return losDatos; }
            set { losDatos = value; }
        }

        public Fila Header { get; set; }
        public Fila Footer { get; set; }

        public Int64 ID_Archivo { get; set; }
        public String ClaveArchivo { get; set; }
        public String DescripcionArchivo { get; set; }
        public Int32 ID_CodificacionRead { get; set; }
        public Int32 ID_CodificacionWrite { get; set; }
        public String URLEscritura { get; set; }
        public Int64 ID_ColectivaCCM { get; set; }
        public Int64 ID_ConsultaBD { get; set; }
        public Int32 ID_TipoProceso { get; set; }
        public String Nombre { get; set; }

        public String UrlArchivo { get; set; }
        public String Prefijo { get; set; }
        public String Sufijo { get; set; }
        public String FormatoFecha { get; set; }

        public DateTime FechadeOperaciones
        {
            get
            {
                DateTime laFecha = DateTime.Now;
                try
                {
                    String elNombreREal = Path.GetFileName(UrlArchivo);

                    String[] lasPartesNombreARchivo = elNombreREal.Split(Char.Parse(SeparadorFecha));


                    if (DateTime.TryParseExact(lasPartesNombreARchivo[Int32.Parse(PosicionFecha)-1],
                                               FormatoFecha,
                                               System.Globalization.CultureInfo.InvariantCulture,
                                               System.Globalization.DateTimeStyles.None,
                                               out laFecha))
                    {
                        //valid
                    }
                    else
                    {
                        throw new Exception ("LA POSICION INDICADA EN EL ARCHIVO: [" + elNombreREal +"] NO ES FORMATO VALIDO DE FECHA: POSICION [" +PosicionFecha + "] , SEPARADOR: [" + SeparadorFecha + "], VALOR: ["+  lasPartesNombreARchivo[Int32.Parse(PosicionFecha)] + "]" );
                    }

                    laFecha = laFecha.AddDays(this.SumaDiasFechaOperaciones);


                }
                catch (Exception err)
                {
                    Logueo.Error("Error al Obtener la Fecha del Nombre del Archivo:" + err);
                    return DateTime.Now;
                    //throw err;
                }
                return laFecha;
            }
        }
        public String SeparadorFecha { get; set; }
        public String PosicionFecha { get; set; }
        public String TipoArchivo { get; set; }
        public Int32 ID_Respuesta { get; set; }
        public Int32 SumaDiasFechaOperaciones { get; set; }
        public String FTPUser { get; set; }
        public String FTPPass { get; set; }
        public String FTPUbicacion { get; set; }
        public String FTPIP { get; set; }
        public String FTPPuerto { get; set; }
        public String FTPTipoSeguridad { get; set; }
        public String DistribucionEmail { get; set; }
        public String SoloOK { get; set; }
        public Int64 EID_Header { get; set; }
        public Int64 EID_Detail { get; set; }
        public Int64? EID_Footer { get; set; }
        public Int64 LID_Header { get; set; }
        public Int64 LID_Detail { get; set; }
        public Int64? LID_Footer { get; set; }
        public Int64 LID_DetailExtra { get; set; }

        public String ClaveProceso { get; set; }
        public String DescripcionProceso { get; set; }
        public String EventoTipoProceso { get; set; }


        public String ClaveHeader { get; set; }
        public String ClaveRegistro { get; set; }
        public String ClaveFooter { get; set; }

    }
}
