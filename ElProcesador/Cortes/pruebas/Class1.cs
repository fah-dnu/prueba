using Dnu.AutorizadorParabiliaAzure.Models;
using Dnu.AutorizadorParabiliaAzure.Services;
using DNU_ParabiliaProcesoAnualidad;
using DNU_ParabiliaProcesoCobranza;
using DNU_ParabiliaProcesoCortes;
using DNU_ParabiliumProcesoMSI;
using DNU_SFTP.Services;
using System;
using System.Configuration;

namespace pruebas
{
    public class Class1
    {
        static void Main(string[] args)
        {
            new Class1().iniciar();
        }

        public void iniciar()
        {//

            string appAKV = ConfigurationManager.AppSettings["applicationId"].ToString();
            string clave = ConfigurationManager.AppSettings["clientKey"].ToString();
            bool activarAzure = Convert.ToBoolean(ConfigurationManager.AppSettings["enableAzure"]);

           

            //edo cuenta sftp
            //bool prueba=SFTPService.CreateFileAndDirectoryWithConnection("45.32.4.114", 22, "UserEmbozoSFTP", "4bhuzrrW3WeFNr", @"C:\Users\Osvi\Documents\DNU\requerimientos\PIEK\documentos\credito\estadosGenerados130\nov\EstadoDeCuentaPruebas.pdf", "credito");

            // CFDI c = new CFDI("");
            //string datos = ConfigurationManager.AppSettings["rutaXMLSalida"].ToString();
            ProcesoCortes proc = new ProcesoCortes();
        



            responseAzure respuesta = KeyVaultProvider.RegistrarProvedorCEK(appAKV, clave);

           // responseAzure respuesta1 = KeyVaultProvider.RegistrarProvedorCEK(appAKV, clave);
            string app = ConfigurationManager.AppSettings["applicationId"].ToString();
            string appPass = ConfigurationManager.AppSettings["clientKey"].ToString();
            string claveCadena = ConfigurationManager.ConnectionStrings["DataBaseProcesos"].ToString();

            ProcesoCobranza procC = new ProcesoCobranza();
           //  procC.InicioLocal("");
           // procC.InicioLocal("2016-12-19");

            ProcesoAnualidad_ proca = new ProcesoAnualidad_();
          //  proca.InicioLocal();

            responseAzure respuestaObtenerCadena = KeyVaultProvider.ObtenerCadenasDeConexionAzure(app, appPass, claveCadena);// "CACAO-DESA-PN-PN-W");
            responseAzure respuestaObtenerCadena1 = KeyVaultProvider.ObtenerValoresClaveAzureKeyVault(app, appPass, claveCadena);// "CACAO-DESA-PN-PN-W");
                                                                                                                                 //ProcesoMSI procMSI= new ProcesoMSI();
                                                                                                                                 //procMSI.InicioLocal();

            // proc.InicioLocal();

            //genrando estado de cuenta credito timbrar o sin timbrar
             proc.GenerarEstadoDeCuentaCredito(10339);
             proc.Inicio();

            //pruebas carga archivos
            //proc.IniciarLeerArchivos();

            //prueba traf
            // proc.Inicio();

            //prueba traf ciclo
            //proc.InicioCiclo();

            // proc.InicioLocal("2016-11-26");// sin facturar
            //proc.InicioLocal("2016-12-26");//facturando
            // proc.InicioLocal("2017-02-26");//debito

            //pruebas isamel
            // proc.Inicio();

            //ProcesoCobranza procC = new ProcesoCobranza();
            //procC.InicioLocal();




            //Timbrado timbrado = new Timbrado();
            //var res = timbrado.Facturama();
            //timbrado.SaveXMLFacturama("PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0idXRmLTgiPz48Y2ZkaTpDb21wcm9iYW50ZSB4bWxuczp4c2k9Imh0dHA6Ly93d3cudzMub3JnLzIwMDEvWE1MU2NoZW1hLWluc3RhbmNlIiB4bWxuczp0ZmQ9Imh0dHA6Ly93d3cuc2F0LmdvYi5teC9UaW1icmVGaXNjYWxEaWdpdGFsIiB4c2k6c2NoZW1hTG9jYXRpb249Imh0dHA6Ly93d3cuc2F0LmdvYi5teC9jZmQvMyBodHRwOi8vd3d3LnNhdC5nb2IubXgvc2l0aW9faW50ZXJuZXQvY2ZkLzMvY2ZkdjMzLnhzZCIgVmVyc2lvbj0iMy4zIiBGb2xpbz0iMTEwIiBGZWNoYT0iMjAyMC0xMS0wMlQxODoxNToxMiIgU2VsbG89IlpETXFnUUdCOVJ2SC9OaFJnLzJtcnFjcnNiTmpoSUh4YmM0TnJKcm5rYWdVS3VQSmxmaGkwTUdkZFd0ejhsRzdjaWhta3RHeERvSTQyaVBaMXJYK2hUWlRnOEM1STNGQ0JXd3dzMlQzbDBzSHBWU2Q0dzFRQklMMnlLRFFHcVV6aVFFUUkzQklPem1SQkVjcHJENVF5L2FFV2RkRWM1UDhaT1E3bXNzOTBSdzhFTmNJa0JITFYwUnI1OUlCQUpwa1ZuYVpDVXdDNmpPbHF4UUYzUzNJYm1HdXVCMW9vRmdoVTVnVTI1dlpQcFhGcjJReWh6TkJ2T01vdHZZNnFFWXRTRk80VmVMaGhYeGtkUTNhMUlWZXRlZDdYQlE5RDJEZ05iSzFZSlJBem9senFneDd6VWlHelBVTjVMSU0vTFJyRDg4RG5GRklpUmdaSWROV2ZQMDZIQT09IiBGb3JtYVBhZ289IjAzIiBOb0NlcnRpZmljYWRvPSIzMDAwMTAwMDAwMDMwMDAyMzcwOCIgQ2VydGlmaWNhZG89Ik1JSUYrVENDQStHZ0F3SUJBZ0lVTXpBd01ERXdNREF3TURBek1EQXdNak0zTURnd0RRWUpLb1pJaHZjTkFRRUxCUUF3Z2dGbU1TQXdIZ1lEVlFRRERCZEJMa011SURJZ1pHVWdjSEoxWldKaGN5ZzBNRGsyS1RFdk1DMEdBMVVFQ2d3bVUyVnlkbWxqYVc4Z1pHVWdRV1J0YVc1cGMzUnlZV05wdzdOdUlGUnlhV0oxZEdGeWFXRXhPREEyQmdOVkJBc01MMEZrYldsdWFYTjBjbUZqYWNPemJpQmtaU0JUWldkMWNtbGtZV1FnWkdVZ2JHRWdTVzVtYjNKdFlXTnB3N051TVNrd0p3WUpLb1pJaHZjTkFRa0JGaHBoYzJsemJtVjBRSEJ5ZFdWaVlYTXVjMkYwTG1kdllpNXRlREVtTUNRR0ExVUVDUXdkUVhZdUlFaHBaR0ZzWjI4Z056Y3NJRU52YkM0Z1IzVmxjbkpsY204eERqQU1CZ05WQkJFTUJUQTJNekF3TVFzd0NRWURWUVFHRXdKTldERVpNQmNHQTFVRUNBd1FSR2x6ZEhKcGRHOGdSbVZrWlhKaGJERVNNQkFHQTFVRUJ3d0pRMjk1YjJGanc2RnVNUlV3RXdZRFZRUXRFd3hUUVZRNU56QTNNREZPVGpNeElUQWZCZ2txaGtpRzl3MEJDUUlNRWxKbGMzQnZibk5oWW14bE9pQkJRMFJOUVRBZUZ3MHhOekExTVRnd016VTBOVFphRncweU1UQTFNVGd3TXpVME5UWmFNSUhsTVNrd0p3WURWUVFERXlCQlEwTkZUU0JUUlZKV1NVTkpUMU1nUlUxUVVrVlRRVkpKUVV4RlV5QlRRekVwTUNjR0ExVUVLUk1nUVVORFJVMGdVMFZTVmtsRFNVOVRJRVZOVUZKRlUwRlNTVUZNUlZNZ1UwTXhLVEFuQmdOVkJBb1RJRUZEUTBWTklGTkZVbFpKUTBsUFV5QkZUVkJTUlZOQlVrbEJURVZUSUZORE1TVXdJd1lEVlFRdEV4eEJRVUV3TVRBeE1ERkJRVUVnTHlCSVJVZFVOell4TURBek5GTXlNUjR3SEFZRFZRUUZFeFVnTHlCSVJVZFVOell4TURBelRVUkdVazVPTURreEd6QVpCZ05WQkFzVUVrTlRSREF4WDBGQlFUQXhNREV3TVVGQlFUQ0NBU0l3RFFZSktvWklodmNOQVFFQkJRQURnZ0VQQURDQ0FRb0NnZ0VCQUpkVWNzSElFSWd3aXZ2QWFudEduWVZJTzMrN3lUZEQxdGtLb3BiTCt0S1NqUkZvMUVyUGRHSnhQM2d4VDVPK0FDSURRWE4rSFM5dU1XRFluYVVSYWxTSUY5Q09GQ2RoL09IMlBuK1Vta040Y3VscjJEYW5LenRWSU84aWRYTTZjOWFIbjVoT283aER4WE1DM3VPdUdWM0ZTNE9ia3hUVis5TnN2T0FWMmxNZTI3U0hyU0IwRGh1THVyVWJad1htKy9yNGR0ejNiMnVMZ0JjK0RpeTk1UEcrTUl1N29OS004OWFCTkdjalRKdys5aytXekppUGQzWnBRZ0llZFlCRCs4UVd4bFlDZ3hobnRhM2s5eWxnWEtZWENZazBrMHFhdXZCSjFqU1JWZjVCampJVWJPc3RhUXA1OW5rZ0hoNDVjOWdud0pSVjYxOE5XMGZNZUR6dUtSMENBd0VBQWFNZE1Cc3dEQVlEVlIwVEFRSC9CQUl3QURBTEJnTlZIUThFQkFNQ0JzQXdEUVlKS29aSWh2Y05BUUVMQlFBRGdnSUJBQktqMERDTkwxbGg0NHkrT2NXRnJUMmljbktGN1d5U09WaWh4MG9SK0hQcldLQk1YeG85S3Ryb2RuQjF0Z0l4OGYrWGpxeXBoaGJ3K2p1RFNlRHJiOTlQaEM0K0U2SmVYT2tkUWNKdDUwS3lvZGw5VVJwQ1ZXTldqVWIzRi95cGE4b1RjZmYvZU1mdFFaVDdNUTFMcWh0K3htM1FoVm94VElBU2NlMGpqc25CVEdEMkpRNHVUM29DZW04Ym1vTVhWL2ZrOWFKM3YwK1pJTDQyTXBZNFBPR1VhL2lUYWF3a2xLUkFMMVhqOUlkSVIwNlJLNjhSUzZ4ckdrNmp3YkRURUt4SnBtWjNTUEx0bHNtUFVUTzFrcmFUUElvOUZDbVUvelprV0dwZDhaRUFBRncrWmZJK2JkWEJmdmREd2FNMmlNR1RRWlRURWdVNUtLVEl2a0FuSG85TzQ1U3FTSndxVjlOTGZQQXhDbzVlUlIyT0dpYmQ5amhIZTgxelVzcDVHZEUxbVppU3FKVTgySDNjdTZCaUUrRDNZYlplWm5qck5TeEJnS1RJZjh3K0tOWVBNNGFXbnVVTWwwbUxndE94VFVYaTlNS25VY2NxM0daTEE3Yng3Wm4yMTF5UFJxRWpTQXF5YlVNVklPaG82YXF6a2ZjM1dMWjZMbkdVK2h5SHVaVWZQd2JuQ2xiN29GRnoxUGx2R09wTkRzVWIwcVA0MlFDR0JpVFVzZUd1Z0F6cU9QNkVZcFZQQzczZ0ZvdXJtZEJRZ2ZheWFFdmkzeGpOYW5Ga1BsVzFYRVlOcllKQjR5TmpwaEZydld3VFk4NnZMMm84Z1pOMFV0bWM1Zm5vQlRmTTlyMnpWS21FaTZGVWVKMWlhRGFWTnY0N3RlOWlTMWFpNFY0dkJZOHIiIFN1YlRvdGFsPSI2MDUwLjAwIiBEZXNjdWVudG89IjEwLjAwIiBNb25lZGE9Ik1YTiIgVG90YWw9IjcwMDYuNDAiIFRpcG9EZUNvbXByb2JhbnRlPSJJIiBNZXRvZG9QYWdvPSJQVUUiIEx1Z2FyRXhwZWRpY2lvbj0iNzgyMTYiIHhtbG5zOmNmZGk9Imh0dHA6Ly93d3cuc2F0LmdvYi5teC9jZmQvMyI+PGNmZGk6RW1pc29yIFJmYz0iQUFBMDEwMTAxQUFBIiBOb21icmU9IkVYUFJFU0lPTiBFTiBTT0ZUV0FSRSIgUmVnaW1lbkZpc2NhbD0iNjAxIiAvPjxjZmRpOlJlY2VwdG9yIFJmYz0iWEFYWDAxMDEwMTAwMCIgTm9tYnJlPSJFbnRpZGFkIHJlY2VwdG9yYSIgVXNvQ0ZEST0iUDAxIiAvPjxjZmRpOkNvbmNlcHRvcz48Y2ZkaTpDb25jZXB0byBDbGF2ZVByb2RTZXJ2PSI4NDExMTUwNiIgTm9JZGVudGlmaWNhY2lvbj0iMjMiIENhbnRpZGFkPSIxMDAiIENsYXZlVW5pZGFkPSJFNDgiIFVuaWRhZD0iVW5pZGFkIGRlIHNlcnZpY2lvIiBEZXNjcmlwY2lvbj0iIEFQSSBmb2xpb3MgYWRpY2lvbmFsZXMiIFZhbG9yVW5pdGFyaW89IjAuNTAiIEltcG9ydGU9IjUwLjAwIiBEZXNjdWVudG89IjEwLjAwIj48Y2ZkaTpJbXB1ZXN0b3M+PGNmZGk6VHJhc2xhZG9zPjxjZmRpOlRyYXNsYWRvIEJhc2U9IjQwLjAwIiBJbXB1ZXN0bz0iMDAyIiBUaXBvRmFjdG9yPSJUYXNhIiBUYXNhT0N1b3RhPSIwLjE2MDAwMCIgSW1wb3J0ZT0iNi40MCIgLz48L2NmZGk6VHJhc2xhZG9zPjwvY2ZkaTpJbXB1ZXN0b3M+PC9jZmRpOkNvbmNlcHRvPjxjZmRpOkNvbmNlcHRvIENsYXZlUHJvZFNlcnY9Ijg0MTExNTA2IiBOb0lkZW50aWZpY2FjaW9uPSIyMSIgQ2FudGlkYWQ9IjEiIENsYXZlVW5pZGFkPSJFNDgiIFVuaWRhZD0iVW5pZGFkIGRlIHNlcnZpY2lvIiBEZXNjcmlwY2lvbj0iIEFQSSBJbXBsZW1lbnRhY2nDs24gIiBWYWxvclVuaXRhcmlvPSI2MDAwLjAwIiBJbXBvcnRlPSI2MDAwLjAwIiBEZXNjdWVudG89IjAiPjxjZmRpOkltcHVlc3Rvcz48Y2ZkaTpUcmFzbGFkb3M+PGNmZGk6VHJhc2xhZG8gQmFzZT0iNjAwMC4wMCIgSW1wdWVzdG89IjAwMiIgVGlwb0ZhY3Rvcj0iVGFzYSIgVGFzYU9DdW90YT0iMC4xNjAwMDAiIEltcG9ydGU9Ijk2MC4wMCIgLz48L2NmZGk6VHJhc2xhZG9zPjwvY2ZkaTpJbXB1ZXN0b3M+PC9jZmRpOkNvbmNlcHRvPjwvY2ZkaTpDb25jZXB0b3M+PGNmZGk6SW1wdWVzdG9zIFRvdGFsSW1wdWVzdG9zVHJhc2xhZGFkb3M9Ijk2Ni40MCI+PGNmZGk6VHJhc2xhZG9zPjxjZmRpOlRyYXNsYWRvIEltcHVlc3RvPSIwMDIiIFRpcG9GYWN0b3I9IlRhc2EiIFRhc2FPQ3VvdGE9IjAuMTYwMDAwIiBJbXBvcnRlPSI5NjYuNDAiIC8+PC9jZmRpOlRyYXNsYWRvcz48L2NmZGk6SW1wdWVzdG9zPjxjZmRpOkNvbXBsZW1lbnRvPjx0ZmQ6VGltYnJlRmlzY2FsRGlnaXRhbCB4c2k6c2NoZW1hTG9jYXRpb249Imh0dHA6Ly93d3cuc2F0LmdvYi5teC9UaW1icmVGaXNjYWxEaWdpdGFsIGh0dHA6Ly93d3cuc2F0LmdvYi5teC9zaXRpb19pbnRlcm5ldC9jZmQvVGltYnJlRmlzY2FsRGlnaXRhbC9UaW1icmVGaXNjYWxEaWdpdGFsdjExLnhzZCIgVmVyc2lvbj0iMS4xIiBVVUlEPSIzOTI0ODQyNC0zYjc3LTQ1M2YtYWViMy04NjY2NWJmOWQxNTYiIEZlY2hhVGltYnJhZG89IjIwMjAtMTEtMDNUMTg6MDI6MDMiIFJmY1Byb3ZDZXJ0aWY9IlNQUjE5MDYxM0k1MiIgU2VsbG9DRkQ9IlpETXFnUUdCOVJ2SC9OaFJnLzJtcnFjcnNiTmpoSUh4YmM0TnJKcm5rYWdVS3VQSmxmaGkwTUdkZFd0ejhsRzdjaWhta3RHeERvSTQyaVBaMXJYK2hUWlRnOEM1STNGQ0JXd3dzMlQzbDBzSHBWU2Q0dzFRQklMMnlLRFFHcVV6aVFFUUkzQklPem1SQkVjcHJENVF5L2FFV2RkRWM1UDhaT1E3bXNzOTBSdzhFTmNJa0JITFYwUnI1OUlCQUpwa1ZuYVpDVXdDNmpPbHF4UUYzUzNJYm1HdXVCMW9vRmdoVTVnVTI1dlpQcFhGcjJReWh6TkJ2T01vdHZZNnFFWXRTRk80VmVMaGhYeGtkUTNhMUlWZXRlZDdYQlE5RDJEZ05iSzFZSlJBem9senFneDd6VWlHelBVTjVMSU0vTFJyRDg4RG5GRklpUmdaSWROV2ZQMDZIQT09IiBOb0NlcnRpZmljYWRvU0FUPSIzMDAwMTAwMDAwMDQwMDAwMjQ5NSIgU2VsbG9TQVQ9IlgvMUhhZDlKVHltQmVXcVJFQkRTOGQwNGxaKzlIeFhjRUVleTRRZGhVRmF5VklWZnl1dDd5K0dqdTZTaHVPK21aSmRpRkowdWFVeW1ibWJvRmFHMm5XZWFUTVZ3clEwb1d6SVV0TGkrdnFBcC83TWVQK2E3UStDazA2U0N2NElpSGpPWEVTZ3dmbXNzRC9xaksrbGhsejVvdlZhZHFoODEydUYvWkczRlJ2aEVta1hwS0RMc05wZ1RhUXU2QnpXTU5DSzhQM1E0bFdzem1NSmVpc3NORWRyb3R4ckZJZFpKTWVwY1BQTHFRSnJ6aEUzZ0RvOHg5Y2E3U3FGbk5xZm9SanQ1RlE2djFOV3VXQmhMaVdsOHQxWHF5Y3V3QkcwL1U3RUNaaGk3REFiL3ppc0U3U0Vpa3JYZ1czajl3WDc5SXJKSHorT1BlWVc2ZTRpNmlHTXlVQT09IiAvPjwvY2ZkaTpDb21wbGVtZW50bz48L2NmZGk6Q29tcHJvYmFudGU+");
            // proc.InicioLocal("2017-02-26");
            //     proc.InicioLocal("2016-12-26");

            //  proc.InicioLocal("2017-02-26");
            //   proc.GenerarEstadoDeCuentaDebito(10135);

            //DNU_ParabiliaOperacionesProcesoCortes.ProcesoCortes proc2 = new DNU_ParabiliaOperacionesProcesoCortes.ProcesoCortes();
            //DNU_ParabiliaOperacionesProcesoCortes.Entidades.RespuestaSolicitud respuesta =proc2.EnviarEstadoDeCuenta(10141);
            ////RespuestaSolicitud respuesta = proc.EnviarEstadoDeCuenta2(10141);
            //Console.ReadLine();


            // ProcesoAnualidad_ pa = new ProcesoAnualidad_();
            // pa.InicioLocal();
        }
    }
}
