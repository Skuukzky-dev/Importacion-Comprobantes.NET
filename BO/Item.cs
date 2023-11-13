using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APIImportacionComprobantes.BO
{
    public class Item
    {
        private String _ProductoID;
        private decimal _UnidadU1;
        private decimal _UnidadU2;
        private decimal _Precio;
        private Double _DescuentoImporte;
        private String? _DescuentoPorcentajes;
        private String? _Comentario;

        public string ProductoID { get => _ProductoID; set => _ProductoID = value; }
        public decimal UnidadU1 { get => _UnidadU1; set => _UnidadU1 = value; }
        public decimal UnidadU2 { get => _UnidadU2; set => _UnidadU2 = value; }
        public decimal Precio { get => _Precio; set => _Precio = value; }
        public double DescuentoImporte { get => _DescuentoImporte; set => _DescuentoImporte = value; }
        public string? Comentario { get => _Comentario; set => _Comentario = value; }
        public string? DescuentoPorcentajes { get => _DescuentoPorcentajes; set => _DescuentoPorcentajes = value; }
    }
}