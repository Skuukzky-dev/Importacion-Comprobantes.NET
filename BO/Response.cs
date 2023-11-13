using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace APIImportacionComprobantes.BO
{
    public class Response
    {
        private String _data;
        private int pagination;

        public String Data { get => _data; set => _data = value; }
        public int Pagination { get => pagination; set => pagination = value; }
    }
}