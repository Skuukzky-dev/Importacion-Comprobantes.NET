using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace APIImportacionComprobantes.BO
{
   
        public class Cliente
        {
            private int _ClienteID;
          
            private String _RazonSocial;
            private String _Fantasia;
            private String _Domicilio;
            private String _Localidad;
            private int _CondicionIVA;
            private String _NumeroDeDocumento;
            private int _TipoDocumentoID;
            private String _Notas;
            private String _Telefono;
            private String _Email;

            public int ClienteID { get => _ClienteID; set => _ClienteID = value; }
            public string RazonSocial { get => _RazonSocial; set => _RazonSocial = value; }
            public string Fantasia { get => _Fantasia; set => _Fantasia = value; }
            public string Domicilio { get => _Domicilio; set => _Domicilio = value; }
            public string Localidad { get => _Localidad; set => _Localidad = value; }
            public int CondicionIVA { get => _CondicionIVA; set => _CondicionIVA = value; }
            public string NumeroDeDocumento { get => _NumeroDeDocumento; set => _NumeroDeDocumento = value; }
            public int TipoDocumentoID { get => _TipoDocumentoID; set => _TipoDocumentoID = value; }
            public string Notas { get => _Notas; set => _Notas = value; }
            public string Telefono { get => _Telefono; set => _Telefono = value; }
            public string Email { get => _Email; set => _Email = value; }
        }
    
        
}