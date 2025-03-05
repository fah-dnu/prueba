namespace DNU_CompensadorParabiliumCommon.BaseDatos
{
    using Interfases.Enums;
    using System;

    /// <summary>
    /// /clase que contiene la definicion para el tipo de dato
    /// </summary>
    /// <creation>Cruz Mejia Raul - 07/10/2022</creation>
    public class TipoDato
    {
        /// <summary>
        /// metodo que obtiene el tipo de dato sql
        /// </summary>
        /// <param name="TipoDato"></param>
        /// <returns></returns>
        public static TipoDatoSQL getTipoDatoSQL(string TipoDato)
        {
            return (!TipoDato.Equals("Int", StringComparison.CurrentCultureIgnoreCase) ? (!TipoDato.Equals("DateTime", StringComparison.CurrentCultureIgnoreCase) ? (!TipoDato.Equals("BigInt", StringComparison.CurrentCultureIgnoreCase) ? (!TipoDato.Equals("Bit", StringComparison.CurrentCultureIgnoreCase) ? (!TipoDato.Equals("decimal", StringComparison.CurrentCultureIgnoreCase) ? (!TipoDato.Equals("double", StringComparison.CurrentCultureIgnoreCase) ? (!TipoDato.Equals("float", StringComparison.CurrentCultureIgnoreCase) ? (!TipoDato.Equals("tinyint", StringComparison.CurrentCultureIgnoreCase) ? (!TipoDato.Equals("smallint", StringComparison.CurrentCultureIgnoreCase) ? (!TipoDato.Equals("varchar", StringComparison.CurrentCultureIgnoreCase) ? (!TipoDato.Equals("char", StringComparison.CurrentCultureIgnoreCase) ? TipoDatoSQL.NODEFINIDO : TipoDatoSQL.VARCHAR) : TipoDatoSQL.VARCHAR) : TipoDatoSQL.SMALLINT) : TipoDatoSQL.TINYINT) : TipoDatoSQL.FLOAT) : TipoDatoSQL.DOUBLE) : TipoDatoSQL.DECIMAL) : TipoDatoSQL.BIT) : TipoDatoSQL.BIGINT) : TipoDatoSQL.DATETIME) : TipoDatoSQL.INT);
        }
    }
}
