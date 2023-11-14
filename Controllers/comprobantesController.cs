using APIImportacionComprobantes.BLL;
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
                                        GESI.CORE.BO.Verscom2k.Comprobante oComprobante = GESI.CORE.BLL.Verscom2k.ComprobantesMgr.GetItem(moComprobanteID);
                                        List<GESI.CORE.BO.ConfiguracionBase> oConfiguracionBase = lstListaConfiguraciones.Where(x => x.GrupoID == "VENTAS" && x.SeccionID == "Comprobante_" + _SessionMgr.EmpresaID + "_" + oComprobante.ComprobanteID).ToList();

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

                                            GESI.GESI.BO.ListaReferenciasContables lstListaRefContables = GESI.GESI.BLL.ReferenciasContablesMgr.GetList(moEmpresaID);
                                            foreach (Cobro oCobro in oCobros)
                                            {
                                                if (oCobro.CobroID > 0 && oCobro.Cliente != null && oCobro.Valores != null)
                                                {
                                                    CobrosMgr._SessionMgr = _SessionMgr;
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
        public void AltaPedidos([FromBody] List<Pedido> oPedidos)
        {
            int hola = 0;

        }

        // GET: api/<comprobantesController>
        /*  [HttpGet]
          public IEnumerable<string> Get()
          {
              return new string[] { "value1", "value2" };
          }

          // GET api/<comprobantesController>/5
          [HttpGet("{id}")]
          public string Get(int id)
          {
              return "value";
          }

          // POST api/<comprobantesController>
          [HttpPost]
          public void Post([FromBody] string value)
          {
          }

          // PUT api/<comprobantesController>/5
          [HttpPut("{id}")]
          public void Put(int id, [FromBody] string value)
          {
          }

          // DELETE api/<comprobantesController>/5
          [HttpDelete("{id}")]
          public void Delete(int id)
          {
          }*/
    }
}
