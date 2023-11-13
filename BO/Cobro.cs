using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APIImportacionComprobantes.BO
{
    /// <summary>
    /// Objeto Cobro
    /// </summary>
    public class Cobro
    {
        private int _CobroID;
        private Cliente? _Cliente;
        private String? _FechaRegistracion;       
        private String? _Notas;
        private decimal _TotalCobrado;
        private List<ComprobanteAplicado>? _ComprobantesAplicados;
        private List<Valor>? _Valores;

        /// <summary>
        /// ID del Cobro
        /// </summary>
        public int CobroID { get => _CobroID; set => _CobroID = value; }
        public Cliente? Cliente { get => _Cliente; set => _Cliente = value; }

        /// <summary>
        /// Fecha en la que se registra la Operacion
        /// </summary>
        public string FechaRegistracion { get => _FechaRegistracion; set => _FechaRegistracion = value; }
        public string Notas { get => _Notas; set => _Notas = value; }
        public decimal TotalCobrado { get => _TotalCobrado; set => _TotalCobrado = value; }
        public List<ComprobanteAplicado>? ComprobantesAplicados { get => _ComprobantesAplicados; set => _ComprobantesAplicados = value; }
        public List<Valor>? Valores { get => _Valores; set => _Valores = value; }
    }
}