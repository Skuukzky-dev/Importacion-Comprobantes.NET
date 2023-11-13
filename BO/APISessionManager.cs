using GESI.CORE.BLL;
using GESI.ERP.Core;

namespace APIImportacionComprobantes.BO
{
    public class APISessionManager
    {
        private GESI.CORE.BLL.SessionMgr _SessionMgr;
        private GESI.ERP.Core.SessionManager _ERPSessionMgr;
        private int _ComprobanteID;
        private string _UsuarioID;
        private int _SucursalID;
        private bool _Habilitado;


        public SessionMgr SessionMgr { get => _SessionMgr; set => _SessionMgr = value; }
        public SessionManager ERPSessionMgr { get => _ERPSessionMgr; set => _ERPSessionMgr = value; }
        public int ComprobanteID { get => _ComprobanteID; set => _ComprobanteID = value; }
        public string UsuarioID { get => _UsuarioID; set => _UsuarioID = value; }
        public int SucursalID { get => _SucursalID; set => _SucursalID = value; }
        public bool Habilitado { get => _Habilitado; set => _Habilitado = value; }
    }
}
