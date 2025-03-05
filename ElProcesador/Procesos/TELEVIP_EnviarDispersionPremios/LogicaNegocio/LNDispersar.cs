using CommonProcesador;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TELEVIP_EnviarDispersionPremios.Entidades;
using TELEVIP_ImportaTagsAfiliados.BaseDatos;

namespace TELEVIP_EnviarDispersionPremios.LogicaNegocio
{
    class LNDispersar
    {
        public static bool LlenarTablaRecompensas()
        {
            try
            {
                // Boolean unaRespo = DAOTagDNU.GenerarDispersion();
                //Inserta recompensas para abonar
                Logueo.Evento("Incializa insertar Recompensas");
                DAOTagDNU.Televia_InsertarEncabezadoRecompensa();
                Logueo.Evento("Finaliza la insercion de Recompensas");

                return true;
            }
            catch (Exception err)
            {
                Logueo.Error("LlenarTablaDispersiones():" + err.Message);
                return false;
            }
        }


        public static bool DispersarPremiosdeTags()
        {

            try
            {
                using (SqlConnection unaConexion = new SqlConnection(BDAutorizador.strBDLectura))
                {
                    unaConexion.Open();

                    using (SqlConnection conn = new SqlConnection(TELEVIP_ImportaTagsAfiliados.BaseDatos.BDTelevip.strBDEscritura))
                    {
                        conn.Open();

                        foreach (Premio unPremio in DAOTagDNU.ObtenerTagsParaDispersion())
                        {


                            try
                            {
                                //PROCESAMIENTO DE LA IMPORTACION
                                //Boolean laRespuestaImportacion = .ProcesaImportacionDelTag(UnTag, conn, transaccionSQL);

                                Boolean laRespuestaImportacion = DAOTagTelevia.ProcesaDispersionDelTag(unPremio, conn);

                                if (laRespuestaImportacion)
                                {
                                    DAOTagDNU.IndicarPremioDispersado(unPremio.ID_Recompensa, unaConexion);

                                    Logueo.Evento("Registro dispersado a la Base Central: " + unPremio.id_tag);

                                    //   transaccionSQL.Commit();
                                }
                                else
                                {
                                    Logueo.Error("No se pudo realizar la dispersado:" + unPremio.id_tag);
                                    //  transaccionSQL.Rollback();

                                }

                            }
                            catch (Exception err)
                            {
                                Logueo.Error("No se pudo realizar la dispersado:" + unPremio.id_tag + ": " + err.Message);
                                // transaccionSQL.Rollback();

                            }


                        }
                    }
                }
                return true;
            }
            catch (Exception err)
            {
                Logueo.Error("DispersarPremiosdeTags():" + err.Message);
                return false;
            }
        }


        public static bool SincronizarPremiosdeTags()
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(TELEVIP_ImportaTagsAfiliados.BaseDatos.BDTelevip.strBDEscritura))
                {
                    conn.Open();

                foreach (Premio unPremio in DAOTagTelevia.ObtenerPremiosParaSincronizar())
                {
                   

                        try
                        {
                            //PROCESAMIENTO DE LA IMPORTACION
                            //Boolean laRespuestaImportacion = .ProcesaImportacionDelTag(UnTag, conn, transaccionSQL);

                            Boolean laRespuestaSincronizacion = DAOTagDNU.ActualizarEstatusRecompensa(unPremio.ID_Recompensa, unPremio.intentos, unPremio.status, unPremio.id_motivo, unPremio.f_aplicacion);

                            if (laRespuestaSincronizacion)
                            {
                                DAOTagTelevia.IndicarPremioSincronizado(unPremio.id);
                                Logueo.Evento("Registro Sincronizado Correctamete a la Base Central: " + unPremio.id_tag);
                            }
                            else
                            {
                                Logueo.Evento("Error al sincrinizar el Registro  a la Base Central: " + unPremio.id_tag);
                            }

                        }
                        catch (Exception err)
                        {
                            Logueo.Error("No se pudo realizar la dispersado:" + unPremio.id_tag + ": " + err.Message);
                            // transaccionSQL.Rollback();

                        }


                    }
                }

                return true;
            }
            catch (Exception err)
            {
                Logueo.Error("DispersarPremiosdeTags():" + err.Message);
                return false;
            }
        }




    }
}
