using Microsoft.Extensions.Logging;

namespace APIImportacionComprobantes.BLL
{
    public class APIHelper
    {

        public static string TipoDeAPI = "";

        /// <summary>
        /// Devuelve el Mgr a Utilizar
        /// </summary>
        /// <param name="usuarioID"></param>
        /// <returns></returns>
        public static APIImportacionComprobantes.BO.APISessionManager SetearMgrAPI(string usuarioID)
        {
            BO.APISessionManager MiAPISessionMgr = new BO.APISessionManager();

            try
            {
                MiAPISessionMgr.SessionMgr = new GESI.CORE.BLL.SessionMgr();
                MiAPISessionMgr.ERPSessionMgr = new GESI.ERP.Core.SessionManager();
                MiAPISessionMgr.Habilitado = false;

                List<GESI.CORE.BO.Verscom2k.HabilitacionesAPI> lstHabilitacionesAPI = GESI.CORE.BLL.Verscom2k.HabilitacionesAPIMgr.GetList(usuarioID);

                if(lstHabilitacionesAPI?.Count > 0)
                {
                    foreach(GESI.CORE.BO.Verscom2k.HabilitacionesAPI oHabilitacion in lstHabilitacionesAPI)
                    {
                        if (oHabilitacion.TipoDeAPI.Equals(TipoDeAPI))
                        {
                            MiAPISessionMgr.SessionMgr.EmpresaID = oHabilitacion.EmpresaID;
                            MiAPISessionMgr.SessionMgr.UsuarioID = oHabilitacion.UsuarioID;
                            MiAPISessionMgr.SessionMgr.SucursalID = oHabilitacion.SucursalID;
                            MiAPISessionMgr.SessionMgr.EntidadID = 1;
                            MiAPISessionMgr.ComprobanteID = oHabilitacion.ComprobanteID;
                            MiAPISessionMgr.UsuarioID = oHabilitacion.UsuarioID;
                            MiAPISessionMgr.ERPSessionMgr.EmpresaID = (uint)oHabilitacion.EmpresaID;
                            MiAPISessionMgr.ERPSessionMgr.UsuarioID = oHabilitacion.UsuarioID;

                            MiAPISessionMgr.ComprobanteID = oHabilitacion.ComprobanteID;
                            MiAPISessionMgr.SucursalID = oHabilitacion.SucursalID;

                            MiAPISessionMgr.Habilitado = true;
                        }

                    }
                }
                return MiAPISessionMgr;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Devuelve un objeto Error de la API
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public static APIImportacionComprobantes.BO.Error DevolverErrorAPI(int code, string message, string tipo, string usuario, string endpoint)
        {
            //Logger.LoguearErrores(message, tipo, usuario, endpoint);
            
            APIImportacionComprobantes.BO.Error error = new APIImportacionComprobantes.BO.Error();
            error.Code = code;
            error.Message = message;

            return error;
        }


        public const string AltaCobros = "AltaCobros";
        public const string AltaPedidos = "AltaPedidos";

        public enum PermisosOperaciones
        {
            pCobro = 12700,
            pPedido = 111
        }

        public enum tErrores
        {
            ePermisosCobros = 400,
            ePermisosPedidos = 401,
            eFormatoIncorrectoSolicitud = 415,
            eCantidadDeComprobantesExcedida = 420,
            eDatoVacioONull = 403,
            eErrorInternoAplicacion = 500
        }

      

    }
}
