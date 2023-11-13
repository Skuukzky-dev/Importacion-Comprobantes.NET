using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APIImportacionComprobantes.BO
{
    public class ComprobanteAplicado
    {
        private int _ComprobanteID;
        private int _Numero;
        private String _Serie;
        private int _PuntoDeVentaID;
        private int _Vto_ComprobanteID;
        private decimal _ImporteAplicado;

        public int ComprobanteID { get => _ComprobanteID; set => _ComprobanteID = value; }
        public string Serie { get => _Serie; set => _Serie = value; }
        public int PuntoDeVentaID { get => _PuntoDeVentaID; set => _PuntoDeVentaID = value; }
        public int Vto_ComprobanteID { get => _Vto_ComprobanteID; set => _Vto_ComprobanteID = value; }
        public decimal ImporteAplicado { get => _ImporteAplicado; set => _ImporteAplicado = value; }
        public int Numero { get => _Numero; set => _Numero = value; }
    }
}