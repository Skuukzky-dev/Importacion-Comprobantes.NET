using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APIImportacionComprobantes.BO
{
    public class Request
    {
        private bool _success;
        private Error _error;
        private Response _response;

        public bool Success { get => _success; set => _success = value; }
        public Error Error { get => _error; set => _error = value; }
        public Response Response { get => _response; set => _response = value; }
    }

}