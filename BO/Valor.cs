using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APIImportacionComprobantes.BO
{
    public class Valor
    {
        private int _CajaBancoID;
        private int _ValorID;
        private String _NumeroDeValor;
        private decimal _Importe;
        private String _FechaDC;
        private String _Emisor;
        private int _BancoID;

        public int CajaBancoID { get => _CajaBancoID; set => _CajaBancoID = value; }
        public int ValorID { get => _ValorID; set => _ValorID = value; }
        public string NumeroDeValor { get => _NumeroDeValor; set => _NumeroDeValor = value; }
        public decimal Importe { get => _Importe; set => _Importe = value; }
        public String FechaDC { get => _FechaDC; set => _FechaDC = value; }
        public string Emisor { get => _Emisor; set => _Emisor = value; }
        public int BancoID { get => _BancoID; set => _BancoID = value; }
    }
}