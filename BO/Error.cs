using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APIImportacionComprobantes.BO
{
    public class Error
    {
        private int _code;
        private String _message;

        public int Code { get => _code; set => _code = value; }
        public string Message { get => _message; set => _message = value; }
    }
}