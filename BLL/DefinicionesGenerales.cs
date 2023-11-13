using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APIImportacionComprobantes.BLL
{

    public enum DefinicionesErrores
    {
        eNoEncontrado = 404,
        eInternoAplicacion = 500,
        eFaltaAtributo = 400,
        eImportadoConErrores = 401,
        eYaCargadoEnSistema = 402,
        eDatoVacioONull = 403,
        eSinAutorizacion = 405
    }

    

    public static class DefinicionesGenerales
    {
        private static int _DivisaID = 1;

        public static int DivisaID
        {
            get { return _DivisaID; }
            set { _DivisaID = value; }
        }

        private static String _SubTipoPedido = "P";

        public static String SubTipoPedido
        {
            get { return _SubTipoPedido; }
            set { _SubTipoPedido = value; }
        }
    }
}