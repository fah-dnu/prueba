using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DNU_CompensadorParabiliumCommon.Utilidades
{
   public enum TipoFilaConfig
    { 
        Header=1,
        Detail=2,
        Footer=3
    }

    public enum EstatusFichero
    {
        NEW ,
        OK,
        ERROR,
        IN_PROC

    }


    public enum EstatusCompensacion
    {
        NEW = 15
        ,OK = 16
        ,PAYCARD = 17
        ,MISC = 18
        ,DOUBLE_OK =19
        ,RTRN = 20
        ,RTRN_NONE = 21
        ,RTRN_TO_CLRF = 22
        ,RTRN_CLRF = 23
        ,BACK = 24
        ,BACK_NONE = 25
        ,BACK_TO_CLRF = 26
        ,BACK_CLRF = 27
        ,NONE = 28
        ,NOT = 29
        ,NOT_FOUND = 30
    }

}
