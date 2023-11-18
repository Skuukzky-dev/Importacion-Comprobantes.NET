using APIImportacionComprobantes.BLL;
using APIImportacionComprobantes.BO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Importacion_Comprobantes.NET.BO;

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
                    if (!HabilitadoPorToken)
                    {
                        #region No esta habilitado por Token
                        Request objRequest = new Request();
                        objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.ePermisosCobros, "No esta autorizado a acceder al servicio. No se encontro el token del usuario. Token Recibido: " + Token, "I",mstrUsuarioID, APIHelper.AltaCobros);
                        objRequest.Success = false;
                        return Unauthorized(objRequest);
                        #endregion
                    }
                    else
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
                                            objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eCantidadDeComprobantesExcedida, "Se supera la cantidad maxima de comprobantes a Importar", "E", MiAPISessionMgr.SessionMgr.UsuarioID, APIHelper.AltaCobros);
                                            objRequest.Success = false;
                                            lstRequests.Add(objRequest);
                                        }
                                        else
                                        {
                                             
                                            VariablesIniciales oVariableInicial = APIHelper.DevolverVariablesIniciales(MiAPISessionMgr);

                                            if (oVariableInicial.SubdiarioID == 0)
                                            {
                                                Request objRequest = new Request();
                                                objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eDatoVacioONull, "No se encontro el SubdiarioID en el ComprobanteID: " + moComprobanteID, "E", MiAPISessionMgr.SessionMgr.UsuarioID, APIHelper.AltaCobros);
                                                objRequest.Success = false;
                                                lstRequests.Add(objRequest);
                                            }
                                            else
                                            {

                                                oVariableInicial.LstReferenciasContables = GESI.GESI.BLL.ReferenciasContablesMgr.GetList(MiAPISessionMgr.SessionMgr.EmpresaID);
                                                foreach (Cobro oCobro in oCobros)
                                                {
                                                    if (oCobro.CobroID > 0 && oCobro.Cliente != null && oCobro.Valores != null)
                                                    {
                                                        CobrosMgr._SessionMgr = MiAPISessionMgr.SessionMgr;
                                                        lstRequests.Add(CobrosMgr.ImportarCobro(oCobro, oVariableInicial));
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
                
                try
                {

                    if (!HabilitadoPorToken)
                    {
                        #region No esta habilitado por Token
                        Request objRequest = new Request();
                        objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.ePermisosCobros, "No esta autorizado a acceder al servicio. No se encontro el token del usuario. Token Recibido: " + Token, "I", mstrUsuarioID, APIHelper.AltaPedidos);
                        objRequest.Success = false;
                        return Unauthorized(objRequest);
                        #endregion
                    }
                    else
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
                                bool blAcceso = GESI.CORE.BLL.Verscom2k.V2KAccesoMgr.ValidarPermisos(MiAPISessionMgr.UsuarioID, (int)APIHelper.PermisosOperaciones.pPedido);

                                if (!blAcceso)
                                {
                                    #region No tiene permisos el usuario para ingresar pedidos
                                    Request objRequest = new Request();
                                    objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.ePermisosCobros, "No tiene permisos para dar de alta pedidos (12100)", "E", mstrUsuarioID, APIHelper.AltaPedidos);
                                    objRequest.Success = false;
                                    lstRequests.Add(objRequest);
                                    return Unauthorized(lstRequests);
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
                                            objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eCantidadDeComprobantesExcedida, "Se supera la cantidad maxima de comprobantes a Importar", "E", mstrUsuarioID, "AltaPedidos");
                                            objRequest.Success = false;
                                            lstRequests.Add(objRequest);
                                            #endregion
                                        }
                                        else
                                        {
                                            VariablesIniciales oVariableInicial = APIHelper.DevolverVariablesIniciales(MiAPISessionMgr);

                                            if (oVariableInicial.SubdiarioID == 0)
                                            {
                                                #region No tiene asignado un SubdiarioID
                                                Request objRequest = new Request();
                                                objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eDatoVacioONull, "El comprobante no tiene asignado un SubdiarioID", "E", mstrUsuarioID, "AltaPedidos");
                                                objRequest.Success = false;
                                                #endregion
                                            }
                                            else
                                            {
                                                #region Importar Pedidos
                                                PedidosMgr._SessionMgr = MiAPISessionMgr.SessionMgr;                                               
                                                LogSucesosAPI.LoguearErrores("Lote enviado. JSON: " + contenido, MiAPISessionMgr.SessionMgr.EmpresaID);

                                                foreach (Pedido oPedido in oPedidos)
                                                {
                                                    if (oPedido.ComprobanteID <= 0)  // SI NO SE LE ENVIA EL ATRIBUTO COMPROBANTEID BUSCA POR DESCRIPCION. EN CASO QUE NO SE ENCUENTRE TOMA EL PREDETERMINADO
                                                    {
                                                        if (oPedido.TipoDeComprobante != null)
                                                        {
                                                            if (oPedido.TipoDeComprobante.Length > 0)
                                                            {
                                                                List<GESI.CORE.BO.Verscom2k.Comprobante> lstComprobantesAux = oVariableInicial.LstComprobantes.Where(x => x.Descripcion.Contains(oPedido.TipoDeComprobante) && x.ClaseDeComprobanteID == 104 && x.EmpresaID == MiAPISessionMgr.SessionMgr.EmpresaID).ToList();

                                                                if (lstComprobantesAux.Count > 0)
                                                                {
                                                                    lstRequests.Add(PedidosMgr.ImportarPedido(oPedido, oVariableInicial, lstComprobantesAux[0]));
                                                                }
                                                                else
                                                                {
                                                                    lstRequests.Add(PedidosMgr.ImportarPedido(oPedido, oVariableInicial,oVariableInicial.Comprobante));
                                                                }
                                                            }
                                                            else
                                                            {
                                                                lstRequests.Add(PedidosMgr.ImportarPedido(oPedido, oVariableInicial,oVariableInicial.Comprobante));
                                                            }
                                                        }
                                                        else
                                                        {
                                                            lstRequests.Add(PedidosMgr.ImportarPedido(oPedido, oVariableInicial, oVariableInicial.Comprobante));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        #region Tiene ComprobanteID 
                                                        //TOMA EL COMPROBANTEID Y LO LEVANTA DE TABLA COMPROBANTES
                                                        List<GESI.CORE.BO.Verscom2k.Comprobante> lstComprobantesAux = oVariableInicial.LstComprobantes.Where(x => x.ClaseDeComprobanteID == 104 && x.EmpresaID == MiAPISessionMgr.SessionMgr.EmpresaID && x.ComprobanteID == oPedido.ComprobanteID).ToList();

                                                        if (lstComprobantesAux.Count > 0)
                                                        {
                                                            lstRequests.Add(PedidosMgr.ImportarPedido(oPedido, oVariableInicial, lstComprobantesAux[0]));
                                                        }
                                                        else
                                                        {
                                                            lstRequests.Add(PedidosMgr.ImportarPedido(oPedido, oVariableInicial, oVariableInicial.Comprobante));
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

                        }
                        else
                        {
                            #region No esta autorizado a acceder a este recurso
                            Request objRequest = new Request();
                            objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.ePermisosPedidos, "No esta autorizado a acceder a este recurso.", "E", mstrUsuarioID, APIHelper.AltaPedidos);
                            objRequest.Success = false;
                            #endregion
                        }
                    }

                }
                catch (Exception ex)
                {
                    #region Internal Server Error
                    Request objRequest = new Request();
                    objRequest.Error = APIHelper.DevolverErrorAPI((int)APIHelper.tErrores.eErrorInternoAplicacion, "Error interno de la Aplicacion. Descripcion: " + ex.Message, "E", mstrUsuarioID, "AltaPedidos");
                    objRequest.Success = false;
                    #endregion
                }
               

                return Ok(lstRequests);
            }

        }

        
    }
}
