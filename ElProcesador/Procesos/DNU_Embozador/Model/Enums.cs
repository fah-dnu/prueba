using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNU_Embozador.Model
{
    public enum ORIGIN
    {
        PL,
        PLF,
        CHR,
        F,
        C,
        TS,//para tarjeta seccionada 0000 0000 0000 0000
        PLT//Para formato que necesite un trim
    }


    public enum ParameterTypeKeys
    {
        fileContent = 1,
        fileName = 2
    }


    public enum EstatusGeneracionArchivo
    {
        NUEVO,
        GENERANDO,
        GENERANDO_ERR,
        ENVIANDO,
        ENVIANDO_ERR,
        ENVIADO,
        TERMINADO
    }




}
