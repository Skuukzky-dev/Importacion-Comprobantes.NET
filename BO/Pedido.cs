using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APIImportacionComprobantes.BO
{
    public class Pedido
    {
        private int _PedidoID;
        private int _ComprobanteID;
        private Cliente _Cliente;
        private String _FechaRegistracion;
        private String _FechaEntrega;
        private String _Notas;
        private List<Item> _Items;
        private SucursalCliente _SucursalCliente;
        private String _TipoPrecio;
        private String _DescuentoPorcentajes;
        private decimal _DescuentoImporte;
        private String _DescuentoTexto;
        private int _VendedorID;
        private String _TipoDeComprobante;
        public int PedidoID { get => _PedidoID; set => _PedidoID = value; }
        public String FechaRegistracion { get => _FechaRegistracion; set => _FechaRegistracion = value; }
        public String FechaEntrega { get => _FechaEntrega; set => _FechaEntrega = value; }
        public string Notas { get => _Notas; set => _Notas = value; }
        public List<Item> Items { get => _Items; set => _Items = value; }
        public Cliente Cliente { get => _Cliente; set => _Cliente = value; }
        public SucursalCliente SucursalCliente { get => _SucursalCliente; set => _SucursalCliente = value; }
        public string TipoPrecio { get => _TipoPrecio; set => _TipoPrecio = value; }
        public string DescuentoPorcentajes { get => _DescuentoPorcentajes; set => _DescuentoPorcentajes = value; }
        public decimal DescuentoImporte { get => _DescuentoImporte; set => _DescuentoImporte = value; }
        public string DescuentoTexto { get => _DescuentoTexto; set => _DescuentoTexto = value; }
        public int VendedorID { get => _VendedorID; set => _VendedorID = value; }
        public string TipoDeComprobante { get => _TipoDeComprobante; set => _TipoDeComprobante = value; }
        public int ComprobanteID { get => _ComprobanteID; set => _ComprobanteID = value; }
    }
}