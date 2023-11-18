using Importacion_Comprobantes.NET.BO;
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
            LogSucesosAPI.LoguearErrores(message, 0);
            
            APIImportacionComprobantes.BO.Error error = new APIImportacionComprobantes.BO.Error();
            error.Code = code;
            error.Message = message;

            return error;
        }


        /// <summary>
        /// Setea todas los datos iniciales previo a la importacion de Cobros o Pedidos
        /// </summary>
        /// <param name="MiAPISessionMgr"></param>
        /// <returns></returns>
        public static VariablesIniciales DevolverVariablesIniciales(APIImportacionComprobantes.BO.APISessionManager MiAPISessionMgr)
        {
            try
            {
                VariablesIniciales oVariablesIniciales = new VariablesIniciales();
                #region SessionManagers
                GESI.GESI.BLL.TablasGeneralesGESIMgr.SessionManager = MiAPISessionMgr.SessionMgr;
                GESI.CORE.BLL.ConfiguracionesBaseMgr.SessionManager = MiAPISessionMgr.SessionMgr;
                GESI.GESI.BLL.ReferenciasContablesMgr.SessionManager = MiAPISessionMgr.SessionMgr;
                GESI.CORE.BLL.Verscom2k.ComprobantesMgr.sessionMgr = MiAPISessionMgr.SessionMgr;
                GESI.CORE.BLL.TablasGeneralesMgr.SessionManager = MiAPISessionMgr.SessionMgr;
                GESI.CORE.BLL.Verscom2k.AlmacenesMgr.SessionManager = MiAPISessionMgr.SessionMgr;
                GESI.CORE.BLL.EmpresasMgr.SessionManager = MiAPISessionMgr.SessionMgr;
                GESI.CORE.BLL.Verscom2k.FormasDePagoMgr.sessionMgr = MiAPISessionMgr.SessionMgr;

                #endregion

                switch (TipoDeAPI) // Determino que tipo de API se esta utilizando
                {
                    case "PEDIDOS":
                        #region PEDIDOS
                        oVariablesIniciales.LstAlicuotasImpuestos = GESI.CORE.DAL.Verscom2k.TablasGeneralesGESIDB.GetListAlicuotasImpuestos();
                        oVariablesIniciales.LstFormaDePago = GESI.CORE.BLL.Verscom2k.FormasDePagoMgr.GetList();
                        oVariablesIniciales.OEmpresa = GESI.CORE.BLL.EmpresasMgr.GetItem(MiAPISessionMgr.SessionMgr.EmpresaID);
                        oVariablesIniciales.Comprobante = GESI.CORE.BLL.Verscom2k.ComprobantesMgr.GetItem(MiAPISessionMgr.ComprobanteID);
                        oVariablesIniciales.LstCanalesDeAtencion = GESI.GESI.BLL.TablasGeneralesGESIMgr.CanalesDeAtencionGetList();
                        oVariablesIniciales.LstConfiguraciones = GESI.CORE.BLL.ConfiguracionesBaseMgr.GetList();
                        oVariablesIniciales.LstAlmacenes = GESI.CORE.BLL.Verscom2k.AlmacenesMgr.GetList();
                        oVariablesIniciales.LstCanalesDeVenta = GESI.GESI.BLL.TablasGeneralesGESIMgr.CanalesDeVentaGetList();
                        oVariablesIniciales.LstEstadosComprobantesVenta = GESI.GESI.BLL.TablasGeneralesGESIMgr.EstadosComprobantesDeVentaGetList();
                        oVariablesIniciales.LstComprobantes = GESI.CORE.BLL.Verscom2k.ComprobantesMgr.GetList();
                        oVariablesIniciales.LstReferenciasContables = GESI.GESI.BLL.ReferenciasContablesMgr.GetList(MiAPISessionMgr.SessionMgr.EmpresaID);
                        List<GESI.CORE.BO.ConfiguracionBase> oConfiguracionBaseaUX = oVariablesIniciales.LstConfiguraciones.Where(x => x.GrupoID == "VENTAS" && x.SeccionID == "Comprobante_" + MiAPISessionMgr.SessionMgr.EmpresaID + "_" + oVariablesIniciales.Comprobante.ComprobanteID).ToList();

                        if (oConfiguracionBaseaUX != null)
                        {
                            if (oConfiguracionBaseaUX.Count > 0)
                            {
                                foreach (GESI.CORE.BO.ConfiguracionBase oConfiguracion in oConfiguracionBaseaUX)
                                {

                                    if (oConfiguracion.ItemID.Equals("SubdiarioID"))
                                    {
                                        oVariablesIniciales.SubdiarioID = Convert.ToInt32(oConfiguracion.Valor);
                                    }

                                    if (oConfiguracion.ItemID.Equals("TipoDeOperacionID"))
                                    {
                                        oVariablesIniciales.TipoDeOperacion = Convert.ToInt32(oConfiguracion.Valor);

                                    }
                                }
                            }
                        }
                        #endregion
                        break;


                    case "COBROS":
                        #region COBROS
                        oVariablesIniciales.LstCajasYBancos = GESI.GESI.BLL.TablasGeneralesGESIMgr.GetListCajasYBancos();
                        oVariablesIniciales.LstValores = GESI.GESI.BLL.TablasGeneralesGESIMgr.GetListValores(false);
                        oVariablesIniciales.LstConfiguraciones = GESI.CORE.BLL.ConfiguracionesBaseMgr.GetList();
                        oVariablesIniciales.Comprobante = GESI.CORE.BLL.Verscom2k.ComprobantesMgr.GetItem(MiAPISessionMgr.ComprobanteID);
                        List<GESI.CORE.BO.ConfiguracionBase> oConfiguracionBase = oVariablesIniciales.LstConfiguraciones.Where(x => x.GrupoID == "VENTAS" && x.SeccionID == "Comprobante_" + MiAPISessionMgr.SessionMgr.EmpresaID + "_" + oVariablesIniciales.Comprobante.ComprobanteID).ToList();

                        GESI.GESI.BO.ListaBancos lstBancos = new GESI.GESI.BO.ListaBancos();
                        lstBancos = GESI.GESI.BLL.TablasGeneralesGESIMgr.GetListBancos();

                        if (oConfiguracionBase != null)
                        {
                            if (oConfiguracionBase.Count > 0)
                            {
                                foreach (GESI.CORE.BO.ConfiguracionBase oConfiguracion in oConfiguracionBase)
                                {

                                    if (oConfiguracion.ItemID.Equals("SubdiarioID"))
                                    {
                                        oVariablesIniciales.SubdiarioID = Convert.ToInt32(oConfiguracion.Valor);
                                    }

                                    if (oConfiguracion.ItemID.Equals("TipoDeOperacionID"))
                                    {
                                        oVariablesIniciales.TipoDeOperacion = Convert.ToInt32(oConfiguracion.Valor);

                                    }
                                }
                            }
                        }

                        if (oVariablesIniciales.TipoDeOperacion == 0)
                        {
                            oVariablesIniciales.TipoDeOperacion = 1;
                        }
                        #endregion
                        break;

                }

                return oVariablesIniciales;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public const string AltaCobros = "AltaCobros";
        public const string AltaPedidos = "AltaPedidos";

        public enum PermisosOperaciones
        {
            pCobro = 12700,
            pPedido = 12100
        }

        public enum tErrores
        {
            ePermisosCobros = 400,
            ePermisosPedidos = 401,
            eFormatoIncorrectoSolicitud = 415,
            eCantidadDeComprobantesExcedida = 420,
            eDatoVacioONull = 403,
            eErrorInternoAplicacion = 500,
            eNuevoToken = 416
        }

      

    }
}
