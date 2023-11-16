﻿using APIImportacionComprobantes.BLL;
using APIImportacionComprobantes.BO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Importacion_Comprobantes.NET.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class comprobantesController : ControllerBase
    {

        #region Variables         
        public static int moSucursalID;
        public static int moEmpresaID;
        public static int moComprobanteID;
        public static string mstrUsuarioID;
        public static string mostrTipoAPICobros = "COBROS";
        public static string mostrTipoAPIPedidos = "PEDIDOS";
        public static List<GESI.CORE.BO.Verscom2k.HabilitacionesAPI> moHabilitacionesAPI;
        public static GESI.CORE.BLL.SessionMgr _SessionMgr;
        public static string contenido = "";
        public static bool HabilitadoPorToken = false;
        public static string Token = "";
        #endregion

        [HttpPost("AltaCobros")]
        [EnableCors("MyCorsPolicy")]
        public IActionResult AltaCobros([FromBody] List<Cobro> oCobros)
        {
            List<Request> lstRequests = new List<Request>();

            if (!ModelState.IsValid)
            {
                #region Modelo invalido
                Request objRequest = new Request();
                objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eFormatoIncorrectoSolicitud, "Formato incorrecto de la solicitud.", "E", mstrUsuarioID, APIHelper.AltaCobros);
                objRequest.Success = false;
                lstRequests.Add(objRequest);
                return BadRequest(objRequest);
                #endregion
            }
            else
            {
                try
                {
                    APIHelper.TipoDeAPI = "COBROS";
                    APISessionManager MiAPISessionMgr = APIHelper.SetearMgrAPI(mstrUsuarioID);

                    if (MiAPISessionMgr.Habilitado)
                    {
                        LogSucesosAPI.LoguearErrores("----- Inicio Importacion de Comprobantes de Cobro ----", MiAPISessionMgr.SessionMgr.EmpresaID);
                        LogSucesosAPI.LoguearErrores("Lote en JSON: " + contenido, MiAPISessionMgr.SessionMgr.EmpresaID);

                        bool blAcceso = GESI.CORE.BLL.Verscom2k.V2KAccesoMgr.ValidarPermisos(MiAPISessionMgr.UsuarioID, (int)APIHelper.PermisosOperaciones.pCobro);

                        if (!blAcceso) // Tiene Permisos para Agregar , Editar / Eliminar Cobros
                        {
                            #region Acceso
                            Request objRequest = new Request();
                            objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.ePermisosCobros, "No tiene permisos para dar de alta cobranzas (12700)", "I", MiAPISessionMgr.SessionMgr.UsuarioID, APIHelper.AltaCobros);
                            objRequest.Success = false;
                            return Unauthorized(objRequest);
                            #endregion
                        }
                        else
                        {
                            if (oCobros != null)
                            {
                                if (oCobros.Count > 0)
                                {

                                    int intCantidadMaximaDeComprobantes = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CantidadMaximaDeComprobantesAImportar"]);

                                    if (intCantidadMaximaDeComprobantes > 100)
                                        intCantidadMaximaDeComprobantes = 100;

                                    if (oCobros.Count > intCantidadMaximaDeComprobantes) // Cantidad Maxima de Comprobantes a Importar 
                                    {
                                        Request objRequest = new Request();
                                        objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eCantidadDeComprobantesExcedida, "Se supera la cantidad maxima de comprobantes a Importar", "E", MiAPISessionMgr.SessionMgr.UsuarioID,APIHelper.AltaCobros);
                                        objRequest.Success = false;
                                        lstRequests.Add(objRequest);
                                    }
                                    else
                                    {
                                        #region Levanto Caja, Valores y Ref Contables                                     
                                        int mointSubdiarioID = 0;
                                        int mointTipoOperacionID = 0;
                                        GESI.GESI.BLL.TablasGeneralesGESIMgr.SessionManager = MiAPISessionMgr.SessionMgr;
                                        GESI.CORE.BLL.ConfiguracionesBaseMgr.SessionManager = MiAPISessionMgr.SessionMgr;
                                        GESI.GESI.BLL.ReferenciasContablesMgr.SessionManager = MiAPISessionMgr.SessionMgr;
                                        GESI.CORE.BLL.Verscom2k.ComprobantesMgr.sessionMgr = MiAPISessionMgr.SessionMgr;

                                        GESI.GESI.BO.ListaCajasYBancos lstListaCajasYBancos = GESI.GESI.BLL.TablasGeneralesGESIMgr.GetListCajasYBancos();
                                        GESI.GESI.BO.ListaValores lstListaValores = GESI.GESI.BLL.TablasGeneralesGESIMgr.GetListValores(false);
                                        GESI.CORE.BO.ListaConfiguracionesBase lstListaConfiguraciones = GESI.CORE.BLL.ConfiguracionesBaseMgr.GetList();
                                        GESI.CORE.BO.Verscom2k.Comprobante oComprobante = GESI.CORE.BLL.Verscom2k.ComprobantesMgr.GetItem(MiAPISessionMgr.ComprobanteID);
                                        List<GESI.CORE.BO.ConfiguracionBase> oConfiguracionBase = lstListaConfiguraciones.Where(x => x.GrupoID == "VENTAS" && x.SeccionID == "Comprobante_" + MiAPISessionMgr.SessionMgr.EmpresaID + "_" + oComprobante.ComprobanteID).ToList();

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
                                                        mointSubdiarioID = Convert.ToInt32(oConfiguracion.Valor);
                                                    }

                                                    if (oConfiguracion.ItemID.Equals("TipoDeOperacionID"))
                                                    {
                                                        mointTipoOperacionID = Convert.ToInt32(oConfiguracion.Valor);

                                                    }
                                                }
                                            }
                                        }

                                        if (mointTipoOperacionID == 0)
                                        {
                                            mointTipoOperacionID = 1;
                                        }
                                        #endregion
                                        
                                        if (mointSubdiarioID == 0)
                                        {
                                            Request objRequest = new Request();
                                            objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eDatoVacioONull, "No se encontro el SubdiarioID en el ComprobanteID: " + moComprobanteID, "E", MiAPISessionMgr.SessionMgr.UsuarioID, APIHelper.AltaCobros);
                                            objRequest.Success = false;
                                            lstRequests.Add(objRequest);
                                        }
                                        else
                                        {

                                            GESI.GESI.BO.ListaReferenciasContables lstListaRefContables = GESI.GESI.BLL.ReferenciasContablesMgr.GetList(MiAPISessionMgr.SessionMgr.EmpresaID);
                                            foreach (Cobro oCobro in oCobros)
                                            {
                                                if (oCobro.CobroID > 0 && oCobro.Cliente != null && oCobro.Valores != null)
                                                {
                                                    CobrosMgr._SessionMgr = MiAPISessionMgr.SessionMgr;
                                                    lstRequests.Add(CobrosMgr.ImportarCobro(oCobro, lstListaCajasYBancos, lstListaValores, lstListaConfiguraciones, oComprobante, mointSubdiarioID, mointTipoOperacionID, lstListaRefContables, lstBancos));
                                                }
                                                else
                                                {
                                                    Request objRequest = new Request();
                                                    objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eDatoVacioONull, "No se encontro datos a procesar", "E", mstrUsuarioID, APIHelper.AltaCobros);
                                                    objRequest.Success = false;
                                                    lstRequests.Add(objRequest);                                                    
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Request objRequest = new Request();
                                    objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eDatoVacioONull, "No se encontro datos a procesar", "E", mstrUsuarioID, APIHelper.AltaCobros);
                                    objRequest.Success = false;
                                    lstRequests.Add(objRequest);
                                }
                            }
                            else
                            {
                                Request objRequest = new Request();
                                objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eDatoVacioONull, "No se encontro datos a procesar", "E", mstrUsuarioID, APIHelper.AltaCobros);
                                objRequest.Success = false;
                                lstRequests.Add(objRequest);
                            }
                        }
                    }
                    else
                    {
                        Request objRequest = new Request();
                        objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.ePermisosCobros, "No se encontro datos a procesar", "E", mstrUsuarioID, APIHelper.AltaCobros);
                        objRequest.Success = false;
                        lstRequests.Add(objRequest);
                    }

                    return Ok(lstRequests);
                }
                catch (Exception ex)
                {
                    Request objRequest = new Request();
                    objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eErrorInternoAplicacion, "Error interno de la Aplicacion Descripcion: " + ex.Message, "E", mstrUsuarioID, APIHelper.AltaCobros);
                    objRequest.Success = false;
                    lstRequests.Add(objRequest);
                }
                LogSucesosAPI.LoguearErrores("----- FIN Importacion de Comprobantes de Cobro ----", moEmpresaID);
                return Ok(lstRequests);
            }

        }


        [HttpPost("AltaPedidos")]
        [EnableCors("MyCorsPolicy")]
        public IActionResult AltaPedidos([FromBody] List<Pedido> oPedidos)
        {
            List<Request> lstRequests = new List<Request>();
            _SessionMgr = new GESI.CORE.BLL.SessionMgr();

            if (!ModelState.IsValid)
            {
                #region Modelo invalido
                Request objRequest = new Request();
                objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eFormatoIncorrectoSolicitud, "Formato incorrecto de la solicitud . JSON: " + contenido, "E", mstrUsuarioID, APIHelper.AltaPedidos);
                objRequest.Success = false;               
                return Unauthorized(objRequest);
                #endregion
            }
            else
            {
                #region Modelo Valido
                try
                {
                    APIHelper.TipoDeAPI = "PEDIDOS";
                    APISessionManager MiAPISessionMgr = APIHelper.SetearMgrAPI(mstrUsuarioID);
                   
                    //LogSucesosAPI.LoguearErroresPedidos("EmpresaID: " + _SessionMgr.EmpresaID + " | UsuarioID: " + _SessionMgr.UsuarioID + " | JSON: " + contenido, _SessionMgr.EmpresaID);

                    if (MiAPISessionMgr.Habilitado)
                    {
                        if (oPedidos == null)
                        {
                            #region No se encontraron pedidos a importar
                            Request objRequest = new Request();
                            objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eFormatoIncorrectoSolicitud, "No se encontraron pedidos a importar", "E", mstrUsuarioID, APIHelper.AltaPedidos);
                            objRequest.Success = false;
                            lstRequests.Add(objRequest);
                            return BadRequest(lstRequests);
                            #endregion
                        }
                        else
                        {
                            if (oPedidos.Count == 0)
                            {
                                #region No se encontraron pedidos a importar
                                Request objRequest = new Request();
                                objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eDatoVacioONull, "No se encontraron pedidos a importar", "E", mstrUsuarioID, APIHelper.AltaPedidos);
                                objRequest.Success = false;
                                lstRequests.Add(objRequest);
                                #endregion
                            }
                            else
                            {
                                int intCantidadMaximaDeComprobantes = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CantidadMaximaDeComprobantesAImportar"]);

                                if (intCantidadMaximaDeComprobantes > 100)
                                    intCantidadMaximaDeComprobantes = 100;

                                if (oPedidos.Count > intCantidadMaximaDeComprobantes) // Cantidad Maxima de Comprobantes a Importar 
                                {
                                    #region Cantidad maxima de comprobantes a Importar
                                    Request objRequest = new Request();
                                    objRequest.Error = new APIImportacionComprobantes.BO.Error();
                                    objRequest.Success = false;
                                    objRequest.Response = new Response();
                                    objRequest.Error.Code = (int)DefinicionesErrores.eDatoVacioONull;
                                    objRequest.Error.Message = "Se supera la cantidad maxima de comprobantes a Importar";
                                    lstRequests.Add(objRequest);
                                    #endregion
                                }
                                else
                                {
                                    #region Declarar Variables 
                                    GESI.GESI.BLL.TablasGeneralesGESIMgr.SessionManager = MiAPISessionMgr.SessionMgr;
                                    GESI.CORE.BLL.TablasGeneralesMgr.SessionManager = MiAPISessionMgr.SessionMgr;
                                    GESI.CORE.BLL.Verscom2k.ComprobantesMgr.sessionMgr = MiAPISessionMgr.SessionMgr;
                                    GESI.CORE.BLL.Verscom2k.AlmacenesMgr.SessionManager = MiAPISessionMgr.SessionMgr;
                                    GESI.CORE.BLL.ConfiguracionesBaseMgr.SessionManager = MiAPISessionMgr.SessionMgr;
                                    GESI.CORE.BLL.EmpresasMgr.SessionManager = MiAPISessionMgr.SessionMgr;


                                    GESI.CORE.BO.Verscom2k.ListaAlicuotasImpuestos moAlicuotasImpuestos = new GESI.CORE.BO.Verscom2k.ListaAlicuotasImpuestos();
                                    GESI.CORE.BO.Verscom2k.ListaFormasDePago moFormaDePago = new GESI.CORE.BO.Verscom2k.ListaFormasDePago();
                                    GESI.CORE.BO.Verscom2k.Comprobante oComprobante = new GESI.CORE.BO.Verscom2k.Comprobante();
                                    GESI.CORE.BLL.Verscom2k.FormasDePagoMgr.sessionMgr = MiAPISessionMgr.SessionMgr;
                                    moAlicuotasImpuestos = GESI.CORE.DAL.Verscom2k.TablasGeneralesGESIDB.GetListAlicuotasImpuestos();
                                    moFormaDePago = GESI.CORE.BLL.Verscom2k.FormasDePagoMgr.GetList();
                                    GESI.CORE.BO.Empresa oEmpresa = GESI.CORE.BLL.EmpresasMgr.GetItem(MiAPISessionMgr.SessionMgr.EmpresaID);
                                    oComprobante = GESI.CORE.BLL.Verscom2k.ComprobantesMgr.GetItem(MiAPISessionMgr.ComprobanteID);

                                    List<GESI.CORE.BO.Verscom2k.Comprobante> lstComprobantes = GESI.CORE.BLL.Verscom2k.ComprobantesMgr.GetList();

                                    GESI.CORE.BO.ListaConfiguracionesBase lstListaConfiguraciones = GESI.CORE.BLL.ConfiguracionesBaseMgr.GetList();

                                    GESI.CORE.BO.Verscom2k.ListaAlmacenes lstAlmacenes = new GESI.CORE.BO.Verscom2k.ListaAlmacenes();
                                    lstAlmacenes = GESI.CORE.BLL.Verscom2k.AlmacenesMgr.GetList();

                                    GESI.GESI.BO.ListaCanalesDeVenta lstCanalesDeVenta = new GESI.GESI.BO.ListaCanalesDeVenta();
                                    lstCanalesDeVenta = GESI.GESI.BLL.TablasGeneralesGESIMgr.CanalesDeVentaGetList();

                                    List<GESI.GESI.BO.CanalDeAtencion> lstCanalesDeAtencion = GESI.GESI.BLL.TablasGeneralesGESIMgr.CanalesDeAtencionGetList();

                                    List<GESI.CORE.BO.ConfiguracionBase> oConfiguracionBase = lstListaConfiguraciones.Where(x => x.GrupoID == "VENTAS" && x.SeccionID == "Comprobante_" + MiAPISessionMgr.SessionMgr.EmpresaID + "_" + oComprobante.ComprobanteID).ToList();

                                    GESI.GESI.BO.ListaEstadosComprobantesDeVentas lstListaEstadosComprobantes = GESI.GESI.BLL.TablasGeneralesGESIMgr.EstadosComprobantesDeVentaGetList();

                                    int mointSubdiarioID = 0;
                                    int mointTipoOperacionID = 1;

                                    if (oConfiguracionBase != null)
                                    {
                                        if (oConfiguracionBase.Count > 0)
                                        {
                                            foreach (GESI.CORE.BO.ConfiguracionBase oConfiguracion in oConfiguracionBase)
                                            {

                                                if (oConfiguracion.ItemID.Equals("SubdiarioID"))
                                                {
                                                    mointSubdiarioID = Convert.ToInt32(oConfiguracion.Valor);
                                                }

                                                if (oConfiguracion.ItemID.Equals("TipoDeOperacionID"))
                                                {
                                                    mointTipoOperacionID = Convert.ToInt32(oConfiguracion.Valor);

                                                }
                                            }
                                        }
                                    }


                                    #endregion


                                    if (mointSubdiarioID == 0)
                                    {
                                        #region No tiene asignado un SubdiarioID
                                        Request objRequest = new Request();
                                        objRequest.Error = new APIImportacionComprobantes.BO.Error();
                                        objRequest.Success = false;
                                        objRequest.Response = new Response();
                                        objRequest.Error.Code = 400;
                                        objRequest.Error.Message = "El comprobante no tiene asignado un SubdiarioID";
                                        #endregion
                                    }
                                    else
                                    {
                                        #region Importar Pedidos
                                        PedidosMgr._SessionMgr =MiAPISessionMgr.SessionMgr;
                                        GESI.GESI.BO.ListaReferenciasContables lstListaRefContables = GESI.GESI.BLL.ReferenciasContablesMgr.GetList(MiAPISessionMgr.SessionMgr.EmpresaID);

                                        LogSucesosAPI.LoguearErrores("Lote enviado. JSON: " + contenido, MiAPISessionMgr.SessionMgr.EmpresaID);

                                        foreach (Pedido oPedido in oPedidos)
                                        {
                                            if (oPedido.ComprobanteID <= 0)
                                            {
                                                if (oPedido.TipoDeComprobante != null)
                                                {
                                                    if (oPedido.TipoDeComprobante.Length > 0)
                                                    {
                                                        List<GESI.CORE.BO.Verscom2k.Comprobante> lstComprobantesAux = lstComprobantes.Where(x => x.Descripcion.Contains(oPedido.TipoDeComprobante) && x.ClaseDeComprobanteID == 104 && x.EmpresaID == MiAPISessionMgr.SessionMgr.EmpresaID).ToList();

                                                        if (lstComprobantesAux.Count > 0)
                                                        {
                                                            lstRequests.Add(PedidosMgr.ImportarPedido(oPedido, moAlicuotasImpuestos, moFormaDePago, lstComprobantesAux[0], lstListaConfiguraciones, oEmpresa, mointTipoOperacionID, lstAlmacenes, lstCanalesDeVenta, lstListaEstadosComprobantes, lstListaRefContables, lstCanalesDeAtencion));
                                                        }
                                                        else
                                                        {
                                                            lstRequests.Add(PedidosMgr.ImportarPedido(oPedido, moAlicuotasImpuestos, moFormaDePago, oComprobante, lstListaConfiguraciones, oEmpresa, mointTipoOperacionID, lstAlmacenes, lstCanalesDeVenta, lstListaEstadosComprobantes, lstListaRefContables, lstCanalesDeAtencion));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        lstRequests.Add(PedidosMgr.ImportarPedido(oPedido, moAlicuotasImpuestos, moFormaDePago, oComprobante, lstListaConfiguraciones, oEmpresa, mointTipoOperacionID, lstAlmacenes, lstCanalesDeVenta, lstListaEstadosComprobantes, lstListaRefContables, lstCanalesDeAtencion));
                                                    }
                                                }
                                                else
                                                {
                                                    lstRequests.Add(PedidosMgr.ImportarPedido(oPedido, moAlicuotasImpuestos, moFormaDePago, oComprobante, lstListaConfiguraciones, oEmpresa, mointTipoOperacionID, lstAlmacenes, lstCanalesDeVenta, lstListaEstadosComprobantes, lstListaRefContables, lstCanalesDeAtencion));
                                                }
                                            }
                                            else
                                            {
                                                #region Tiene ComprobanteID
                                                List<GESI.CORE.BO.Verscom2k.Comprobante> lstComprobantesAux = lstComprobantes.Where(x => x.ClaseDeComprobanteID == 104 && x.EmpresaID == MiAPISessionMgr.SessionMgr.EmpresaID && x.ComprobanteID == oPedido.ComprobanteID).ToList();

                                                if (lstComprobantesAux.Count > 0)
                                                {
                                                    lstRequests.Add(PedidosMgr.ImportarPedido(oPedido, moAlicuotasImpuestos, moFormaDePago, lstComprobantesAux[0], lstListaConfiguraciones, oEmpresa, mointTipoOperacionID, lstAlmacenes, lstCanalesDeVenta, lstListaEstadosComprobantes, lstListaRefContables, lstCanalesDeAtencion));
                                                }
                                                else
                                                {
                                                    lstRequests.Add(PedidosMgr.ImportarPedido(oPedido, moAlicuotasImpuestos, moFormaDePago, oComprobante, lstListaConfiguraciones, oEmpresa, mointTipoOperacionID, lstAlmacenes, lstCanalesDeVenta, lstListaEstadosComprobantes, lstListaRefContables, lstCanalesDeAtencion));
                                                }

                                                #endregion
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        #region No esta autorizado a acceder a este recurso
                        Request objRequest = new Request();
                        objRequest.Error = new APIImportacionComprobantes.BO.Error();
                        objRequest.Success = false;
                        objRequest.Response = new Response();
                        objRequest.Error.Code = 400;
                        objRequest.Error.Message = "No esta autorizado a acceder a este recurso.";
                        #endregion
                    }

                }
                catch (Exception ex)
                {
                    #region Internal Server Error
                    Request objRequest = new Request();
                    objRequest.Error = new APIImportacionComprobantes.BO.Error();
                    objRequest.Success = false;
                    objRequest.Response = new Response();
                    objRequest.Error.Code = 500;
                    objRequest.Error.Message = "Error interno de la Aplicacion. Descripcion: " + ex.Message + " Contenido JSON: " + contenido;
                    #endregion
                }
                #endregion

                return Ok(lstRequests);
            }

        }

        
    }
}
