using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APIImportacionComprobantes.BO
{
    public class SucursalCliente
    {
        private int _SucursalID;
        private String _Descripcion;
        private String _Domicilio;
        private String _Localidad;
        private String _DomicilioEntrega;

        public int SucursalID { get => _SucursalID; set => _SucursalID = value; }
        public string Descripcion { get => _Descripcion; set => _Descripcion = value; }
        public string Domicilio { get => _Domicilio; set => _Domicilio = value; }
        public string Localidad { get => _Localidad; set => _Localidad = value; }
        public string DomicilioEntrega { get => _DomicilioEntrega; set => _DomicilioEntrega = value; }
    }
}