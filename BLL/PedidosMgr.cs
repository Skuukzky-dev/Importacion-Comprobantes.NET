using APIImportacionComprobantes.BO;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace APIImportacionComprobantes.BLL
{
    public class PedidosMgr
    {
        #region Variables
        private static decimal mdecExento;
        private static decimal mdecNeto;
        private static decimal mdecTotal;
        private static SqlConnection myConn;
        public static GESI.CORE.BLL.SessionMgr _SessionMgr;
        public static GESI.ERP.Core.SessionManager ERPSessionMgr;
        private static decimal mdecImpuestos;
        private static String strMotivoNoImportar = "";
        private static String strObservaciones = "";
        private static String moDescuentoPieComoItem = "NO";

        #endregion

        /// <summary>
        /// Realiza la Importacion del Comprobante de Pedido
        /// </summary>
        /// <param name="objPedido"></param>
        /// <param name="moAlicuotasImpuestos"></param>
        /// <param name="moFormaDePago"></param>
        /// <param name="oComprobante"></param>
        /// <param name="lstConfiguracionesBase"></param>
        /// <returns></returns>
        public static  Request ImportarPedido(APIImportacionComprobantes.BO.Pedido objPedido, GESI.CORE.BO.Verscom2k.ListaAlicuotasImpuestos moAlicuotasImpuestos,
                                        GESI.CORE.BO.Verscom2k.ListaFormasDePago moFormaDePago, GESI.CORE.BO.Verscom2k.Comprobante oComprobante,
                                       GESI.CORE.BO.ListaConfiguracionesBase lstConfiguracionesBase, GESI.CORE.BO.Empresa oEmpresa, int intTipoOperacionID,
                                       GESI.CORE.BO.Verscom2k.ListaAlmacenes lstAlmacenes,GESI.GESI.BO.ListaCanalesDeVenta lstCanalesDeVenta,
                                       GESI.GESI.BO.ListaEstadosComprobantesDeVentas lstEstadosComprobantesVenta, GESI.GESI.BO.ListaReferenciasContables lstListaRefContables,
                                       List<GESI.GESI.BO.CanalDeAtencion> lstCanalesDeAtencion)
        {
            Request respuesta = new Request();
            try
            {
                ERPSessionMgr = new GESI.ERP.Core.SessionManager();                              
                GESI.ERP.Core.BLL.PreciosManager.ERPsessionManager = ERPSessionMgr;                

                if (objPedido.PedidoID > 0)
                {
                    String mstrCodigoImportacion = _SessionMgr.UsuarioID + "_" + objPedido.PedidoID;                    
                    GESI.CORE.BO.Verscom2k.MovimientoDeCliente MovimientoABuscar = GESI.CORE.DAL.Verscom2k.MovimientosDeClientesDB.GetItem(mstrCodigoImportacion, _SessionMgr.EmpresaID);

                    if (MovimientoABuscar == null)
                    {
                        if (objPedido.Cliente != null)
                        {
                            if (objPedido.Cliente.ClienteID > 0)
                            {
                                if (objPedido.Items != null)
                                {
                                    if (objPedido.Items.Count > 0)
                                    {
                                        #region Parse Fecha
                                        try
                                        {
                                            DateTime oFecha = DateTime.Parse(objPedido.FechaRegistracion, null, System.Globalization.DateTimeStyles.RoundtripKind);
                                        }
                                        catch (Exception ex) //Parse fecha
                                        {
                                            respuesta = DevolverObjetoRequest(false, 500, "Formato de fecha incorrecto", objPedido.PedidoID);
                                            LogSucesosAPI.LoguearErroresPedidos("Formato de fecha incorrecto "+ objPedido.PedidoID,_SessionMgr.EmpresaID);
                                            return respuesta;
                                        }
                                        #endregion
                                        if (oComprobante != null)
                                        {
                                            if (oEmpresa != null)
                                            {

                                                #region Inicializacion Variables
                                                mdecExento = 0;
                                                mdecNeto = 0;
                                                mdecTotal = 0;
                                                mdecImpuestos = 0;
                                                strObservaciones = "";
                                                String moReleerPrecios = "NO";
                                                moDescuentoPieComoItem = "NO";
                                                //RELEER PRECIOS
                                                List<GESI.CORE.BO.ConfiguracionBase> lstConfiguracion = lstConfiguracionesBase.Where(x => x.GrupoID == "Ventas" && x.SeccionID == "ImportarNPE" && x.ItemID == "ReleerPrecios").ToList();

                                                if(lstConfiguracion.Count > 0)
                                                {
                                                    moReleerPrecios = lstConfiguracion[0].Valor;
                                                }

                                                int AlmacenID = 0;

                                                lstConfiguracion = lstConfiguracionesBase.Where(x => x.GrupoID == "VENTAS" && x.SeccionID == "Comprobante_"+_SessionMgr.EmpresaID+"_"+oComprobante.ComprobanteID && x.ItemID == "AlmacenID").ToList();

                                                if (lstConfiguracion.Count > 0)
                                                {
                                                   AlmacenID = Convert.ToInt32(lstConfiguracion[0].Valor);
                                                }

                                                lstConfiguracion = lstConfiguracionesBase.Where(x => x.GrupoID == "Ventas" && x.SeccionID == "Facturacion" && x.ItemID == "DescuentoPieComoItem").ToList();

                                                if(lstConfiguracion.Count > 0)
                                                {
                                                    moDescuentoPieComoItem = lstConfiguracion[0].Valor;
                                                }

                                                #endregion
                                                if (AlmacenID == 0)
                                                {
                                                    respuesta = DevolverObjetoRequest(false, 404, "No se encontro el AlmacenID", objPedido.PedidoID);
                                                }
                                                else
                                                {
                                                    GESI.CORE.BO.Verscom2k.MovimientoDeCliente oMovimiento = new GESI.CORE.BO.Verscom2k.MovimientoDeCliente();
                                                    GESI.CORE.BLL.Verscom2k.ClientesMgr.SessionManager = _SessionMgr;
                                                    GESI.CORE.BO.Verscom2k.Cliente oCliente = GESI.CORE.BLL.Verscom2k.ClientesMgr.ClientesGetItem(objPedido.Cliente.ClienteID);
                                                        //GESI.CORE.DAL.Verscom2k.TablasGeneralesGESIDB.GetItemCliente(objPedido.Cliente.ClienteID, 0, "", _SessionMgr.EmpresaID);

                                                    if (oCliente != null)
                                                    {
                                                        if (oCliente.CanalDeVentaID == 0) // Canal De venta
                                                        {
                                                            List<GESI.GESI.BO.CanalDeVenta> lstCanalDeVentaABuscar = lstCanalesDeVenta.Where(x => x.Predeterminado == true).ToList();
                                                            if (lstCanalDeVentaABuscar.Count > 0)
                                                            {
                                                                oCliente.CanalDeVentaID = lstCanalDeVentaABuscar[0].CanalDeVentaID;
                                                            }
                                                            else
                                                            {
                                                                oCliente.CanalDeVentaID = lstCanalesDeVenta[0].CanalDeVentaID;
                                                            }
                                                        }
                                                        
                                                        
                                                       /* if(oCliente.Estado != null) // PEDIDO DE CAROLINA 1/8
                                                        {
                                                            if(oCliente.Estado.IndexOf("Inac") != -1)
                                                            {
                                                                return DevolverObjetoRequest(false, 404, "El cliente "+oCliente.ClienteID+" esta bloqueado para las ventas: Estado", objPedido.PedidoID);
                                                            }
                                                        }*/


                                                        #region Existe Cliente
                                                     
                                                            GESI.ERP.Core.BLL.CuentasConClientesManager.SessionManager = _SessionMgr;
                                                            GESI.ERP.Core.BLL.CuentasConClientesManager.ERPsessionManager = new GESI.ERP.Core.SessionManager();
                                                            
                                                            respuesta = AltaPedido(objPedido, oCliente, oComprobante, oEmpresa, moAlicuotasImpuestos, moFormaDePago, moReleerPrecios, intTipoOperacionID, AlmacenID, lstEstadosComprobantesVenta,lstListaRefContables,lstConfiguracionesBase, lstCanalesDeAtencion);
                                                      
                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        List<GESI.CORE.BO.ConfiguracionBase> oConfiguracion = lstConfiguracionesBase.Where(x => x.GrupoID == "Ventas" && x.SeccionID == "Configuracion" && x.ItemID == "AltaCliAlImportarNPE").ToList();

                                                        if (oConfiguracion.Count > 0)
                                                        {
                                                            #region No existe cliente se evalua Configuracion
                                                            if (oConfiguracion[0].Valor.Equals("1"))
                                                            {
                                                                GESI.CORE.BO.Verscom2k.Cliente oClienteABuscar = EvaluarSiExisteClienteEnBD(objPedido.Cliente);
                                                                if (oClienteABuscar == null)
                                                                {
                                                                    oCliente = CrearNuevoClienteBD(objPedido.Cliente, objPedido.SucursalCliente);

                                                                    if (oCliente != null)
                                                                    {
                                                                        respuesta = AltaPedido(objPedido, oCliente, oComprobante, oEmpresa, moAlicuotasImpuestos, moFormaDePago, moReleerPrecios, intTipoOperacionID, AlmacenID, lstEstadosComprobantesVenta, lstListaRefContables,lstConfiguracionesBase, lstCanalesDeAtencion);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    respuesta = AltaPedido(objPedido, oClienteABuscar, oComprobante, oEmpresa, moAlicuotasImpuestos, moFormaDePago, moReleerPrecios, intTipoOperacionID, AlmacenID, lstEstadosComprobantesVenta, lstListaRefContables, lstConfiguracionesBase, lstCanalesDeAtencion);
                                                                }
                                                            }
                                                            else
                                                            {
                                                                respuesta = VerificarClientePredeterminado(objPedido, null, oComprobante, oEmpresa, moAlicuotasImpuestos, moFormaDePago, moReleerPrecios, intTipoOperacionID, AlmacenID, lstEstadosComprobantesVenta,lstListaRefContables,lstConfiguracionesBase, lstCanalesDeAtencion);
                                                            }
                                                            #endregion
                                                        }
                                                        else
                                                        {
                                                            respuesta = VerificarClientePredeterminado(objPedido, null, oComprobante, oEmpresa, moAlicuotasImpuestos, moFormaDePago, moReleerPrecios, intTipoOperacionID, AlmacenID, lstEstadosComprobantesVenta, lstListaRefContables, lstConfiguracionesBase, lstCanalesDeAtencion);
                                                        }

                                                    }
                                                }
                                            }
                                            else
                                            {
                                               
                                               respuesta =  DevolverObjetoRequest(false,404, "No se encontro la empresa", objPedido.PedidoID);
                                               
                                            }

                                        }
                                        else
                                        {
                                           
                                            respuesta = DevolverObjetoRequest(false, 404, "No se encontro un comprobante de Pedido en el sistema", objPedido.PedidoID);
                                          
                                        }

                                    }
                                    else
                                    {
                                        respuesta = DevolverObjetoRequest(false, 404, "El pedido no contiene detalle", objPedido.PedidoID);
                                    }
                                }
                                else
                                {
                                    respuesta = DevolverObjetoRequest(false, 404, "El pedido no contiene detalle", objPedido.PedidoID);
                                    
                                }
                            }
                            else
                            {       

                                respuesta = DevolverObjetoRequest(false, 404, "No se encontro el cliente en la Base de datos", objPedido.PedidoID);                               
                            }

                        }
                        else
                        {
                            respuesta = DevolverObjetoRequest(false, 404, "No se encontro el cliente en la Base de datos", objPedido.PedidoID);
                         }
                    }
                    else
                    {
                        
                        respuesta = DevolverObjetoRequest(false, 404, "Pedido Numero " + objPedido.PedidoID + " ya cargado en sistema con Numero " + MovimientoABuscar.Numero, objPedido.PedidoID);
                    }
                }
                else
                {
                    respuesta = DevolverObjetoRequest(false, 404, "Se requiere un numero de Pedido en la Importacion", objPedido.PedidoID);
                }
            }
            catch(Exception ex)
            {               
                respuesta = DevolverObjetoRequest(false, 500, ex.Message, objPedido.PedidoID);
                LogSucesosAPI.LoguearErroresPedidos("Error interno de la aplicacion. PedidoID:  "+ objPedido.PedidoID+". Descripcion: "+ex.Message,_SessionMgr.EmpresaID);
            }
            return respuesta;
        }

        /// <summary>
        /// Devuelve el objeto Request
        /// </summary>
        /// <param name="Success"></param>
        /// <param name="intCodigoError"></param>
        /// <param name="strMensajeError"></param>
        /// <returns></returns>
        private static Request DevolverObjetoRequest(bool Success, int intCodigoError, String strMensajeError,int PedidoID)
        {
            try
            {
                Request objRequest = new Request();
                objRequest.Error = new Error();
                objRequest.Success = false;
                objRequest.Response = new Response();
                objRequest.Error.Code = intCodigoError;
                objRequest.Error.Message = strMensajeError;
                objRequest.Response.Data = "Codigo de pedido: " + PedidoID;

                return objRequest;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        

        /// <summary>
        /// Crea un nuevo cliente en la BD
        /// </summary>
        /// <param name="oCliente"></param>
        private static GESI.CORE.BO.Verscom2k.Cliente CrearNuevoClienteBD(Cliente oCliente, SucursalCliente oSucursal)
        {

            GESI.CORE.BO.Verscom2k.Cliente oClienteAImportar = null;
            GESI.CORE.BO.Verscom2k.SucursalCliente oSucursalAImportar = null;
            bool CondicionesImportar = false;
            if (oCliente.RazonSocial != null)
            {
                if (oCliente.NumeroDeDocumento != null)
                {
                    if (oCliente.NumeroDeDocumento.Length > 0)
                    {
                        if (oCliente.CondicionIVA > 0)
                        {
                            if (oCliente.TipoDocumentoID > 0)
                            {
                          
                                switch(oCliente.TipoDocumentoID)
                                {
                                    case 1: //CUIT
                                        if(oCliente.NumeroDeDocumento.Length > 10)
                                        {
                                            if(oCliente.NumeroDeDocumento.IndexOf("-") != -1)
                                            {
                                                CondicionesImportar = true;
                                            }
                                            else
                                            {
                                                String cadenaModificada = oCliente.NumeroDeDocumento.Insert(10, "-");
                                                cadenaModificada = cadenaModificada.Insert(2, "-");
                                                oCliente.NumeroDeDocumento = cadenaModificada;
                                                CondicionesImportar = true;
                                            }
                                        }
                                        break;

                                    case 2: // DNI
                                        if(oCliente.NumeroDeDocumento.Length > 7)
                                        {
                                            if(oCliente.NumeroDeDocumento.IndexOf(".") != -1)
                                            {
                                                CondicionesImportar = true;
                                            }
                                            else
                                            {
                                                String cadenaModificada = oCliente.NumeroDeDocumento.Insert(5, ".");
                                                cadenaModificada = cadenaModificada.Insert(2, ".");
                                                oCliente.NumeroDeDocumento = cadenaModificada;
                                                CondicionesImportar = true;
                                            }
                                        }
                                        break;                                                                           

                                }                               

                            }
                        }
                    }
                }
            }


            if (CondicionesImportar)
            {
                oClienteAImportar = new GESI.CORE.BO.Verscom2k.Cliente();
                oClienteAImportar.RazonSocial = oCliente.RazonSocial;
                oClienteAImportar.CondicionIVA = oCliente.CondicionIVA;
                oClienteAImportar.Domicilio = oCliente.Domicilio;
                oClienteAImportar.Localidad = oCliente.Localidad;
                oClienteAImportar.Telefono = oCliente.Telefono;
                oClienteAImportar.TipoDeDocumentoID = oCliente.TipoDocumentoID;
                oClienteAImportar.NumeroDeDocumento = oCliente.NumeroDeDocumento;
                oClienteAImportar.EmpresaID = _SessionMgr.EmpresaID;

                if (oSucursal != null)
                {
                    if (oCliente.ClienteID > 0)
                    {
                        if (oSucursal.SucursalID > 0)
                        {
                            oSucursalAImportar = new GESI.CORE.BO.Verscom2k.SucursalCliente();
                            oSucursalAImportar.EmpresaID = _SessionMgr.EmpresaID ;
                            oSucursalAImportar.ClienteID = oCliente.ClienteID;
                            oSucursalAImportar.SucursalClienteID = oSucursal.SucursalID;
                            oSucursalAImportar.Denominacion = oSucursal.Descripcion;
                            oSucursalAImportar.Domicilio = oSucursal.Domicilio;
                            oSucursalAImportar.DomicilioEntrega = oSucursal.DomicilioEntrega;
                        }
                    }
                }

                try
                {
                    int resultado = GESI.CORE.DAL.Verscom2k.TablasGeneralesGESIDB.SaveClientes(oClienteAImportar);
                    if (oSucursalAImportar != null)
                    {
                        if (resultado > 0)
                        {
                            oSucursalAImportar.ClienteID = resultado;
                            GESI.CORE.DAL.Verscom2k.TablasGeneralesGESIDB.SucursalClienteSave(oSucursalAImportar);
                        }
                    }
                }
                catch (Exception ex)
                {
                    oClienteAImportar = null;
                    Console.WriteLine(ex.Message);
                }

            }
            return oClienteAImportar;
        }

        /// <summary>
        /// Realiza la importacion del Pedido
        /// </summary>
        /// <param name="oPedido"></param>
        /// <param name="oCliente"></param>
        private static  Request AltaPedido(Pedido oPedido, GESI.CORE.BO.Verscom2k.Cliente oCliente, GESI.CORE.BO.Verscom2k.Comprobante oComprobante, 
                                        GESI.CORE.BO.Empresa oEmpresa, GESI.CORE.BO.Verscom2k.ListaAlicuotasImpuestos moAlicuotasImpuestos,
                                        GESI.CORE.BO.Verscom2k.ListaFormasDePago moFormaDePago,String mstrReleePrecios,int intTipoOperacionID,int AlmacenID,
                                        GESI.GESI.BO.ListaEstadosComprobantesDeVentas lstEstadosComprobantesVenta, GESI.GESI.BO.ListaReferenciasContables lstListaRefContables, 
                                        GESI.CORE.BO.ListaConfiguracionesBase lstConfiguracionesBase,List<GESI.GESI.BO.CanalDeAtencion> lstCanalesDeAtencion)
        {
          
            GESI.GESI.BO.MovimientoDeCliente oMovimientoAImportar = LlenarObjetoMovimientoCliente(oPedido, oCliente, oComprobante, oEmpresa, moAlicuotasImpuestos, 
                                                                                                moFormaDePago, mstrReleePrecios, intTipoOperacionID, AlmacenID,lstListaRefContables, moDescuentoPieComoItem,
                                                                                                 lstConfiguracionesBase,lstCanalesDeAtencion);
         
            Request objRequest = new Request();
            bool oValidar = ValidarObjetoMovimientoDeCliente(oMovimientoAImportar,lstEstadosComprobantesVenta);
            int EstadoComprobanteID = 0;

            if (oValidar == true)
            {  
                if(strObservaciones.Length > 0)
                {
                    EstadoComprobanteID = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["EstadoobservadoID"]);
                    oMovimientoAImportar.Notas = oMovimientoAImportar.Notas ;                  
                }
                else
                {
                    EstadoComprobanteID = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["EstadoOKID"]);
                }

                if(lstEstadosComprobantesVenta?.Count > 0)
                {
                    oMovimientoAImportar.EstadoComprobanteDeVentaID = EstadoComprobanteID;
                }

                String mstrCodigoImportacion = _SessionMgr.UsuarioID + "_" + oPedido.PedidoID;
                GESI.CORE.BO.Verscom2k.MovimientoDeCliente MovimientoABuscar = GESI.CORE.DAL.Verscom2k.MovimientosDeClientesDB.GetItem(mstrCodigoImportacion, _SessionMgr.EmpresaID);

                if (MovimientoABuscar == null)
                {

                    SqlTransaction myTran = null;
                    try
                    {
                        #region Grabar Pedido en BD
                        myConn = GESI.CORE.DAL.DBHelper.DevolverConnectionStringCORE();

                        myConn.Open();
                        myTran = myConn.BeginTransaction("TN_ALTANPE");
                        int ProximoNumero = GESI.CORE.DAL.Verscom2k.MovimientosDeClientesDB.UltimoComprobante(_SessionMgr.EmpresaID, oComprobante.ComprobanteID, oComprobante.Serie, oComprobante.PuntoDeVentaID) + 1;
                        oMovimientoAImportar.Numero = ProximoNumero;

                        int oNumeroComprobante = GESI.GESI.DAL.MovimientosDeClientesDB.Save(oMovimientoAImportar, true, myConn, myTran);
                        foreach (var Detalle in oMovimientoAImportar.DetallesDeVentas)
                        {
                            Detalle.Numero = oNumeroComprobante;
                            GESI.GESI.DAL.DetallesDeVentasDB.Save(Detalle, myConn, myTran);
                        }

                        myTran.Commit();

                        LogSucesosAPI.LoguearExitososPedidos("Pedidos Importados exitosamente: " + JsonConvert.SerializeObject(oPedido));

                        objRequest.Error = new Error();
                        objRequest.Success = true;
                        objRequest.Response = new Response();
                        objRequest.Response.Data = JsonConvert.SerializeObject(oPedido);
                        String json = JsonConvert.SerializeObject(objRequest);
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        #region ROLLBACK
                        myTran.Rollback();
                        strMotivoNoImportar = ex.Message;
                        objRequest.Error = new Error();
                        objRequest.Success = false;
                        objRequest.Response = new Response();
                        objRequest.Error.Code = 500;
                        objRequest.Error.Message = strMotivoNoImportar;
                        LogSucesosAPI.LoguearErroresPedidos("Pedido NO importado. Codigo de Pedido: " + oPedido.PedidoID + " Motivo: " + strMotivoNoImportar,_SessionMgr.EmpresaID);
                        objRequest.Response.Data = "Codigo de Pedido: " + oPedido.PedidoID;
                        String json = JsonConvert.SerializeObject(objRequest);
                        #endregion

                    }
                    myConn.Close();
                }
                else
                {
                    objRequest = DevolverObjetoRequest(false, 404, "Pedido Numero " + oPedido.PedidoID + " ya cargado en sistema con Numero " + MovimientoABuscar.Numero, oPedido.PedidoID);

                }

            }
            else
            {  
                objRequest.Error = new Error();
                objRequest.Success = false;
                objRequest.Response = new Response();
                objRequest.Error.Code = 404;
                objRequest.Error.Message = strMotivoNoImportar;
                objRequest.Response.Data = "Codigo de Pedido: " + oPedido.PedidoID;
                LogSucesosAPI.LoguearErroresPedidos("Pedido NO importado. Codigo de Pedido: "+oPedido.PedidoID+" Motivo: " + strMotivoNoImportar,_SessionMgr.EmpresaID);
                String json = JsonConvert.SerializeObject(objRequest);
            }

            return objRequest;
        }

        /// <summary>
        /// Devuelve un objeto MovimientoDeCliente con su Detalle
        /// </summary>
        /// <param name="oPedido"></param>
        /// <param name="oCliente"></param>
        /// <param name="oComprobante"></param>
        /// <param name="oEmpresa"></param>
        /// <param name="moAlicuotasImpuestos"></param>
        /// <param name="moFormaDePago"></param>
        /// <param name="mstrReleePrecios"></param>
        /// <returns></returns>
        private static GESI.GESI.BO.MovimientoDeCliente LlenarObjetoMovimientoCliente(Pedido oPedido, GESI.CORE.BO.Verscom2k.Cliente oCliente, GESI.CORE.BO.Verscom2k.Comprobante oComprobante,
                                                                                    GESI.CORE.BO.Empresa oEmpresa, GESI.CORE.BO.Verscom2k.ListaAlicuotasImpuestos moAlicuotasImpuestos, GESI.CORE.BO.Verscom2k.ListaFormasDePago moFormaDePago, 
                                                                                    String mstrReleePrecios,int intTipoOperacionID,int AlmacenID,GESI.GESI.BO.ListaReferenciasContables lstListaRefContables,String moDescuentoPieComoItem,
                                                                                    GESI.CORE.BO.ListaConfiguracionesBase lstConfiguracionesBase,List<GESI.GESI.BO.CanalDeAtencion> lstCanalesDeAtencion)
        {
            try
            {
                #region Variables
                bool Contado = false;
                GESI.GESI.BO.MovimientoDeCliente oMovimiento = new GESI.GESI.BO.MovimientoDeCliente();
                GESI.ERP.Core.BO.cComprobanteVenta oComprVenta = new GESI.ERP.Core.BO.cComprobanteVenta();
                bool Importar = true;
                GESI.ERP.Core.BLL.ComprobanteDeVentaManager.SessionManager = _SessionMgr;
                GESI.ERP.Core.BLL.ComprobanteDeVentaManager.ERPsessionManager = new GESI.ERP.Core.SessionManager();
                #endregion

                #region Datos Generales Comprobante
                

                oMovimiento.EmpresaID = _SessionMgr.EmpresaID;
                oMovimiento.ComprobanteID = oComprobante.ComprobanteID;
                oMovimiento.Serie = oComprobante.Serie;
                oMovimiento.ClienteID = oCliente.ClienteID;
                oMovimiento.RazonSocial = oCliente.RazonSocial;
                oMovimiento.Pendiente = true;                
                oMovimiento.NumeroDeDocumento = oCliente.NumeroDeDocumento;
                oMovimiento.Fecha = DateTime.Parse(oPedido.FechaRegistracion, null, System.Globalization.DateTimeStyles.RoundtripKind);
                oMovimiento.DivisaID = DefinicionesGenerales.DivisaID; //TODO: Comprobantes MultiDivisa
                oMovimiento.AlmacenID = AlmacenID;
                oMovimiento.VendedorID = oPedido.VendedorID;

                #region CanalDeAtencion
                if (lstCanalesDeAtencion?.Count > 0)
                {
                    List<GESI.GESI.BO.CanalDeAtencion> lstCanalesDeAtencionAux = lstCanalesDeAtencion.Where(x => x.Predeterminado == true).ToList();
                    if(lstCanalesDeAtencionAux.Count > 0)
                    {
                        oMovimiento.CanalDeAtencionID = lstCanalesDeAtencionAux[0].CanalDeAtencionID;
                    }
                    else
                    {
                        oMovimiento.CanalDeAtencionID = lstCanalesDeAtencion[0].CanalDeAtencionID;
                    }
                }
                else
                {
                    oMovimiento.CanalDeAtencionID = 0;
                }
                #endregion

                #region Forma de Pago
                    if (oCliente.FormaDePagoID > 0)
                {
                    oMovimiento.FormaDePagoID = oCliente.FormaDePagoID;
                    List<GESI.CORE.BO.Verscom2k.FormaDePago> lstFormaDePago = moFormaDePago.Where(x => x.FormaDePagoID == oCliente.FormaDePagoID).ToList();
                    if(lstFormaDePago.Count > 0)
                    {
                        Contado = lstFormaDePago[0].Contado;
                    }
                }
                else
                {
                    List<GESI.CORE.BO.Verscom2k.FormaDePago> lstFormaDePago = moFormaDePago.Where(x => x.Predeterminado == true).ToList();

                    if (lstFormaDePago.Count > 0)
                    {
                        oMovimiento.FormaDePagoID = lstFormaDePago[0].FormaDePagoID;
                        Contado = lstFormaDePago[0].Contado;
                    }
                    else
                    {
                        oMovimiento.FormaDePagoID = moFormaDePago[0].FormaDePagoID;
                        Contado = moFormaDePago[0].Contado;
                    }
                }
                #endregion

                oMovimiento.TipoDeDocumentoID = oCliente.TipoDeDocumentoID;
                oMovimiento.PuntoDeVentaID = oComprobante.PuntoDeVentaID;
                oMovimiento.SubTipo = DefinicionesGenerales.SubTipoPedido;
                oMovimiento.TipoOperacionID = intTipoOperacionID;
                oMovimiento.CondicionIVA = oCliente.CondicionIVA;
                oMovimiento.Notas = oCliente.LeyendaFacturas+" "+ oPedido.Notas;
                oMovimiento.Notas2 = "Codigo de Pedido: " + oPedido.PedidoID+ "\r \n" + strObservaciones;
                oMovimiento.DetallesDeVentas = new GESI.GESI.BO.ListaDetallesDeVentas();
                oMovimiento.UsuarioID = _SessionMgr.UsuarioID;
                

                if (oPedido.PedidoID > 0)
                {
                    oMovimiento.CodigoImportacion = _SessionMgr.UsuarioID + "_" + oPedido.PedidoID;
                }
                #endregion

                #region SucursalCliente
                if (oPedido.SucursalCliente != null)
                {
                    if (oPedido.SucursalCliente.SucursalID > 0)
                    {
                        if (oCliente.SucursalesCliente?.Count > 0)
                        {
                            List<GESI.CORE.BO.Verscom2k.SucursalCliente> lstSucursalesABuscar = oCliente.SucursalesCliente.Where(x => x.SucursalClienteID == oPedido.SucursalCliente.SucursalID).ToList();
                            if (lstSucursalesABuscar.Count > 0)
                            {
                                oMovimiento.SucursalID = oPedido.SucursalCliente.SucursalID;
                            }
                            else
                            {
                                Importar = false;
                                strMotivoNoImportar = "No se encontro la sucursal del Cliente. SucursalID: " + oPedido.SucursalCliente.SucursalID;
                            }
                        }
                        else
                        {
                            Importar = false;
                            strMotivoNoImportar = "No se encontro la sucursal del Cliente. SucursalID: " + oPedido.SucursalCliente.SucursalID;
                        }
                    }
                }
                #endregion

                #region Detalle de Ventas
                int intItemID = 1;
                oComprVenta.DetalleDeItems = new List<GESI.ERP.Core.BO.cItemComprobanteDeVenta>();
                int itemGenerales = 1;
                foreach (Item oItem in oPedido.Items)
                {
                    GESI.CORE.BLL.Verscom2k.ProductosMgr.sessionMgr = _SessionMgr;
                    GESI.CORE.BO.Verscom2k.Producto Producto = GESI.CORE.BLL.Verscom2k.ProductosMgr.GetItem(oItem.ProductoID);
                    
                    if (Producto != null)
                    {
                        oComprVenta.DetalleDeItems.Add(DeterminarDetalleDeVenta(Producto, oCliente.CanalDeVentaID, oItem, oCliente, oEmpresa, oComprobante, oPedido.TipoPrecio, moAlicuotasImpuestos, mstrReleePrecios));

                       
                            if (Producto.ProductosEInsumos?.Count > 0) // Si tiene insumos
                            {
                                foreach (GESI.CORE.BO.Verscom2k.ProductoEInsumo oInsumo in Producto.ProductosEInsumos)
                                {
                                    GESI.ERP.Core.BO.cItemComprobanteDeVenta oInsumoDetalle = new GESI.ERP.Core.BO.cItemComprobanteDeVenta();
                                    oInsumoDetalle.CantidadU1 = (oItem.UnidadU1 * oInsumo.CantidadDeInsumo) / oInsumo.CantidadDeProducto;
                                    oInsumoDetalle.PrecioUnitarioFinal = 0;
                                    oInsumoDetalle.PrecioUnitarioNeto = 0;
                                    oInsumoDetalle.ImporteTotalFinal = 0;
                                    oInsumoDetalle.ImporteTotalNeto = 0;
                                    oInsumoDetalle.CodigoDeItem = oInsumo.InsumoID;
                                    oInsumoDetalle.TipoDeItem = "P";
                                    oInsumoDetalle.TipoDeFacturacion = "U";
                                    oInsumoDetalle.CantidadU3 = oInsumoDetalle.CantidadU1 * oInsumo.Unidad2XUnidad1;
                                    oInsumoDetalle.ComponenteDeItem = intItemID;
                                    oComprVenta.DetalleDeItems.Add(oInsumoDetalle);
                                }

                                intItemID = intItemID + Producto.ProductosEInsumos.Count + 1;
                            }
                            else
                            {
                                intItemID++;
                            }
                        itemGenerales++;


                    }
                    else
                    {
                        strMotivoNoImportar = "No se encontro el producto " + oItem.ProductoID + " en la base de datos";
                        oComprVenta.DetalleDeItems.Add(null);
                        Importar = false;
                    }

                   
                }

                

                #endregion

                #region Calcular Totales

                if (Importar) // Si contiene todos los items
                {
                    if(oPedido.DescuentoPorcentajes == null)
                    {
                        oPedido.DescuentoPorcentajes = "";   
                    }
                    oComprVenta.DescuentoPorcentaje = oPedido.DescuentoPorcentajes;
                    GESI.ERP.Core.BO.cComprobanteVenta oComprobanteConTotales = GESI.ERP.Core.BLL.ComprobanteDeVentaManager.CalcularTotales(oComprVenta);
                    
                    if(oComprobante.Impuestos == false)
                    {
                        oComprobanteConTotales.ImporteIVAGeneral = 0;
                        oComprobanteConTotales.ImporteTotal = oComprobanteConTotales.ImporteNeto;
                    }

                    oMovimiento.Descto1_Importe =  oComprobanteConTotales.DescuentoImporte;
                    oMovimiento.Descto1_Porcentaje = oComprVenta.DescuentoPorcentaje;
                    oComprobanteConTotales.DescuentoPorcentaje = oPedido.DescuentoPorcentajes;
                    oMovimiento = PasarAMovimientoDeCliente(oComprobanteConTotales, oMovimiento);
                }
                else
                {
                    oMovimiento = null;
                }
                #endregion

                if (oMovimiento != null)
                {

                    #region Verifica Limite Credito
                    GESI.ERP.Core.BO.dResumenDeudaCliente oResumenDeudaCliente = GESI.ERP.Core.BLL.CuentasConClientesManager.ResumenDeudaCliente(oCliente.ClienteID);

                    decimal decSaldoActual = oResumenDeudaCliente.DebitosPendientesCliente - oResumenDeudaCliente.CreditosPendientesCliente;

                    if (oCliente.LimiteCredito > 0 || oCliente.LimiteCredito_Dias > 0)
                    {

                        if (!Contado && (decSaldoActual + oMovimiento.Total) > Convert.ToDecimal(oCliente.LimiteCredito))
                        {

                            strObservaciones = strObservaciones + "\r\n" + "Se excedio el limite de crédito. Cliente: " + oCliente.ClienteID + ". La operación se excederia por " + ((decSaldoActual + oMovimiento.Total) - Convert.ToDecimal(oCliente.LimiteCredito));

                        }

                        if (oCliente.LimiteCredito_Dias > 0)
                        {

                            TimeSpan diferencia = DateTime.Now.Subtract(oResumenDeudaCliente.FechaDeudaMasViejaCliente);
                            int diferenciaEnDias = diferencia.Days;

                            if (diferenciaEnDias > oCliente.LimiteCredito_Dias)
                            {
                                strObservaciones = strObservaciones + "El cliente Nº " + oCliente.ClienteID + " adeuda " + decSaldoActual + " y la factura mas antigua tiene " + diferenciaEnDias + " dias";

                            }

                        }


                    }

                    #endregion

                    #region Fecha Entrega
                    if (oPedido.FechaEntrega != null)
                    {
                        if (oPedido.FechaEntrega.Length > 0)
                        {
                            try
                            {
                                oMovimiento.FechaEntrega = DateTime.Parse(oPedido.FechaEntrega, null, System.Globalization.DateTimeStyles.RoundtripKind);
                            }
                            catch (Exception ex1)
                            {
                                oMovimiento.FechaEntrega = DateTime.Now;
                            }
                        }
                    }
                    else
                    {
                        oMovimiento.FechaEntrega = DateTime.Now;
                    }
                    #endregion
                }


                return oMovimiento;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Valida el Objeto Movimiento de Cliente
        /// </summary>
        /// <param name="oMovimientoDeCliente"></param>
        /// <returns></returns>
        private static bool ValidarObjetoMovimientoDeCliente(GESI.GESI.BO.MovimientoDeCliente oMovimientoDeCliente,GESI.GESI.BO.ListaEstadosComprobantesDeVentas lstEstadosComprobantesVenta)
        {
            try
            {
                bool EstaOK = true;
                GESI.GESI.BLL.ExistenciasMgr.SessionManager = _SessionMgr;
                if (oMovimientoDeCliente == null)
                {
                    EstaOK = false;
                }
                else
                {

                    #region Revisa detalles de venta
                    foreach (GESI.GESI.BO.DetalleDeVenta oDetalleDeVenta in oMovimientoDeCliente.DetallesDeVentas)
                    {
                        if (oDetalleDeVenta != null)
                        {
                            #region Verifico Existencias
                            if (oDetalleDeVenta.TipoDeItem.Equals("P"))
                            {
                                GESI.GESI.BO.Existencia oExistencia = GESI.GESI.BLL.ExistenciasMgr.GetItem(oMovimientoDeCliente.AlmacenID, oDetalleDeVenta.ProductoID);
                                if (oExistencia == null)
                                {
                                    if (lstEstadosComprobantesVenta.Count > 0)
                                    {
                                        strObservaciones = strObservaciones + "No hay existencias del producto " + oDetalleDeVenta.ProductoID + " en el Almacen " + oMovimientoDeCliente.AlmacenID + "\r\n";
                                    }
                                }
                                else
                                {
                                    if (oExistencia.ExUnidad1 <= 0)
                                    {
                                        if (lstEstadosComprobantesVenta?.Count > 0)
                                        {
                                            strObservaciones = strObservaciones + "No hay existencias del producto " + oDetalleDeVenta.ProductoID + " en el Almacen " + oMovimientoDeCliente.AlmacenID + "\r\n";
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            EstaOK = false;

                        }
                    }
                    #endregion



                    #region Valida Totales
                    decimal Total = oMovimientoDeCliente.Neto + oMovimientoDeCliente.Exento + oMovimientoDeCliente.IVA_General + oMovimientoDeCliente.IVA_Adicional + oMovimientoDeCliente.Impuestos - oMovimientoDeCliente.Descto1_Importe;
                    if (EstaOK)
                    {
                        if (Total == oMovimientoDeCliente.Total)
                        {
                            if (moDescuentoPieComoItem.Equals("SI"))
                                oMovimientoDeCliente.Descto1_Importe = 0;
                            EstaOK = true;
                        }
                        else
                        {
                            decimal Diferencia = Total - oMovimientoDeCliente.Total;

                            if (Diferencia <= Convert.ToDecimal(0.2))
                            {
                                EstaOK = true;
                            }
                            else
                            {
                                strMotivoNoImportar = "Hay diferencias en los totales del comprobante. Total Pedido: " + Total + " , total en Movimiento de Cliente: " + oMovimientoDeCliente.Total;
                                EstaOK = false;
                            }

                        }
                    }
                    #endregion

                 


                }


                return EstaOK;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Verifica si existe Configuracion de Cliente Predeterminado
        /// </summary>
        /// <param name="oPedido"></param>
        /// <param name="oCliente"></param>
        /// <param name="oComprobante"></param>
        /// <param name="oEmpresa"></param>
        /// <returns></returns>
        private static Request VerificarClientePredeterminado(Pedido oPedido, GESI.CORE.BO.Verscom2k.Cliente oCliente, GESI.CORE.BO.Verscom2k.Comprobante oComprobante,
                                                              GESI.CORE.BO.Empresa oEmpresa,GESI.CORE.BO.Verscom2k.ListaAlicuotasImpuestos moAlicuotasImpuestos, 
                                                              GESI.CORE.BO.Verscom2k.ListaFormasDePago moFormaDePago,String strReleerPrecios,int intTipoOperacionID,int AlmacenID,
                                                              GESI.GESI.BO.ListaEstadosComprobantesDeVentas lstEstadosComprobantesVenta,GESI.GESI.BO.ListaReferenciasContables lstListaRefContables,
                                                              GESI.CORE.BO.ListaConfiguracionesBase lstConfiguracionesBase,List<GESI.GESI.BO.CanalDeAtencion> lstCanalesDeAtencion)
        {
            Request respuesta = new Request();
            Request objRequest = new Request();
            GESI.CORE.BO.ConfiguracionBase oConfiguracion = GESI.CORE.DAL.ConfiguracionesBaseDB.GetItem("Ventas", "Configuracion", "CliPredImportacionNPE");

            if (oConfiguracion != null)
            {
                if (!oConfiguracion.Valor.Equals("0"))
                {
                    oCliente = GESI.CORE.DAL.Verscom2k.TablasGeneralesGESIDB.GetItemCliente(Convert.ToInt32(oConfiguracion.Valor), 0, "", _SessionMgr.EmpresaID);
                    if (oCliente != null)
                    {
                        objRequest = AltaPedido(oPedido, oCliente, oComprobante, oEmpresa,moAlicuotasImpuestos,moFormaDePago,strReleerPrecios, intTipoOperacionID,AlmacenID,lstEstadosComprobantesVenta, lstListaRefContables, lstConfiguracionesBase, lstCanalesDeAtencion);
                    }
                    else
                    {
                       
                        objRequest.Error = new Error();
                        objRequest.Success = false;
                        objRequest.Response = new Response();
                        objRequest.Error.Code = 404;
                        objRequest.Error.Message = "No se encontro el Cliente Predeterminado en la BD";
                       
                    }
                }
                else
                {
                   
                    objRequest.Error = new Error();
                    objRequest.Success = false;
                    objRequest.Response = new Response();
                    objRequest.Error.Code = 404;
                    objRequest.Error.Message = "No hay configurado un cliente predeterminado en la Importacion de Pedidos";
                  
                }
            }
            else
            {
               
                objRequest.Error = new Error();
                objRequest.Success = false;
                objRequest.Response = new Response();
                objRequest.Error.Code = 404;
                objRequest.Error.Message = "No hay configurado un cliente predeterminado en la Importacion de Pedidos";
            }

            return objRequest;

        }


        /// <summary>
        /// Verifica si existe el cliente en la BASE
        /// </summary>
        /// <param name="oCliente"></param>
        /// <returns></returns>
        private static GESI.CORE.BO.Verscom2k.Cliente EvaluarSiExisteClienteEnBD(Cliente oCliente)
        {
            try
            {
                bool CondicionesImportar = false;
                List<GESI.CORE.BO.Verscom2k.Cliente> oClienteBD = null;
                GESI.CORE.BO.Verscom2k.Cliente oClienteADevolver = null;
                if (oCliente.RazonSocial != null)
                {
                    if (oCliente.NumeroDeDocumento != null)
                    {
                        if (oCliente.NumeroDeDocumento.Length > 0)
                        {
                            if (oCliente.CondicionIVA > 0)
                            {
                                if (oCliente.TipoDocumentoID > 0)
                                {
                                  
                                    //CondicionesImportar = true;
                                    switch (oCliente.TipoDocumentoID)
                                    {
                                        case 1: //CUIT
                                            if (oCliente.NumeroDeDocumento.Length > 10)
                                            {
                                                if (oCliente.NumeroDeDocumento.IndexOf("-") != -1)
                                                {
                                                    CondicionesImportar = true;
                                                }
                                                else
                                                {
                                                    String cadenaModificada = oCliente.NumeroDeDocumento.Insert(10, "-");
                                                    cadenaModificada = cadenaModificada.Insert(2, "-");
                                                    oCliente.NumeroDeDocumento = cadenaModificada;
                                                    CondicionesImportar = true;
                                                }
                                            }
                                            break;

                                        case 2: // DNI
                                            if (oCliente.NumeroDeDocumento.Length > 7)
                                            {
                                                if (oCliente.NumeroDeDocumento.IndexOf(".") != -1)
                                                {
                                                    CondicionesImportar = true;
                                                }
                                                else
                                                {
                                                    String cadenaModificada = oCliente.NumeroDeDocumento.Insert(5, ".");
                                                    cadenaModificada = cadenaModificada.Insert(2, ".");
                                                    oCliente.NumeroDeDocumento = cadenaModificada;
                                                    CondicionesImportar = true;
                                                }
                                            }
                                            break;

                                    }

                                   /* oClienteBD = GESI.VerscomEcommerce.DAL.V2KClientes.DevolverClientes("usp_GetCustomersEcommerce", "", _SessionMgr.EmpresaID.ToString(), oCliente.NumeroDeDocumento);
                                    
                                    if(oClienteBD == null)
                                    {
                                       
                                    }
                                    else
                                    {
                                        if(oClienteBD.Count > 0)
                                        {
                                            oClienteADevolver = oClienteBD[0];
                                        }
                                        else
                                        {
                                           
                                        }
                                    }*/

                                }
                            }
                        }
                    }
                }

                return oClienteADevolver;

            }
            catch(Exception ex)
            {
                return null;
                throw ex;
            }

        }


        /// <summary>
        /// Realiza el pasaje al movimiento GESI.GESI.BO.MovimientoDeCliente
        /// </summary>
        /// <param name="oCompr"></param>
        /// <param name="oMovimientoDeCliente"></param>
        /// <returns></returns>

        private static GESI.GESI.BO.MovimientoDeCliente PasarAMovimientoDeCliente(GESI.ERP.Core.BO.cComprobanteVenta oCompr,GESI.GESI.BO.MovimientoDeCliente oMovimientoDeCliente)
        {
            try
            {
                if(oCompr != null)
                {
                    if(oCompr.DetalleDeItems != null)
                    {
                        oMovimientoDeCliente.Total = oCompr.ImporteTotal;
                        //oMovimientoDeCliente.Neto = oCompr.ImporteNeto - oCompr.DescuentoImporte;
                       if (moDescuentoPieComoItem.Equals("NO"))
                        {
                            oMovimientoDeCliente.Neto = oCompr.ImporteNeto - oCompr.DescuentoImporte;
                           
                        }
                        else
                        {
                            oMovimientoDeCliente.Neto = oCompr.ImporteNeto;
                        }

                        oMovimientoDeCliente.Exento = oCompr.ImporteExento;
                        oMovimientoDeCliente.Impuestos = oCompr.ImporteImpuestos;
                        oMovimientoDeCliente.Descto1_Importe = oCompr.ImporteDescuento;
                        oMovimientoDeCliente.IVA_General = oCompr.ImporteIVAGeneral;
                        oMovimientoDeCliente.IVA_Adicional = oCompr.ImporteIVAAdicional;
                        oMovimientoDeCliente.Descto1_Importe = oCompr.DescuentoImporte;
                        oMovimientoDeCliente.Descto1_Porcentaje = oCompr.DescuentoPorcentaje;
                        oMovimientoDeCliente.SaldoPendiente = oCompr.ImporteTotal;

                        oMovimientoDeCliente.DetallesDeVentas = new GESI.GESI.BO.ListaDetallesDeVentas();
                        foreach(GESI.ERP.Core.BO.cItemComprobanteDeVenta oItem in oCompr.DetalleDeItems)
                        {
                            GESI.GESI.BO.DetalleDeVenta oDetalle = new GESI.GESI.BO.DetalleDeVenta();
                            oDetalle.EmpresaID = oMovimientoDeCliente.EmpresaID;
                            if (oItem.TipoDeItem.Equals("P"))
                            {
                                oDetalle.ProductoID = oItem.CodigoDeItem;
                            }
                            else
                            {
                                oDetalle.ReferenciaContableID = oItem.CodigoDeItem;
                            }
                            oDetalle.ComprobanteID = oMovimientoDeCliente.ComprobanteID;
                            oDetalle.PuntoDeVentaID = oMovimientoDeCliente.PuntoDeVentaID;
                            oDetalle.Serie = oMovimientoDeCliente.Serie;
                            oDetalle.TipoDeItem = oItem.TipoDeItem;
                            oDetalle.ImporteNeto = oItem.ImporteTotalNeto;
                            if (oMovimientoDeCliente.CondicionIVA == 1 || oMovimientoDeCliente.CondicionIVA == 6)
                            {
                                oDetalle.ImporteTotal = oItem.ImporteTotalNeto;
                            }
                            else
                            {
                                oDetalle.ImporteTotal = oItem.ImporteTotalFinal;
                            }
                            oDetalle.Editable = true;
                            oDetalle.SaldoFactura_U1 = oItem.CantidadU1;
                            oDetalle.Unidad1 = oItem.CantidadU1;
                            oDetalle.TipoDeFacturacion = oItem.TipoDeFacturacion;
                            oDetalle.ItemID = 0;
                            oDetalle.AlicuotaID = oItem.AlicuotaIVA;
                            oDetalle.Descuento = oItem.DescuentosPorcentaje;
                            oDetalle.PrecioUnitario = oItem.PrecioUnitarioFinal;
                            oDetalle.Comentario = oItem.Comentario;
                            oDetalle.ComponenteDeItem = oItem.ComponenteDeItem;
                            // TODO: Aca asignar el valor para el atributo ComponenteDeItem
                            //oDetalle.ComponenteDeItem
                            if (oItem.TipoDeFacturacion.Equals("U"))
                            {
                                if (oItem.CantidadU3 > 0)
                                {
                                    oDetalle.Unidad2 = oItem.CantidadU3;
                                    oDetalle.SaldoFactura_U2 = oItem.CantidadU3;
                                }
                            }
                            else
                            {
                                oDetalle.Unidad2 = oItem.CantidadU2;
                                oDetalle.SaldoFactura_U2 = oItem.CantidadU2;
                            }
                            oMovimientoDeCliente.DetallesDeVentas.Add(oDetalle);

                        }

                    }
                }
                return oMovimientoDeCliente;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
                       

        /// <summary>
        /// Calcula el precio Neto del Producto
        /// </summary>
        /// <param name="decPrecioFinal"></param>
        /// <param name="intAlicuotaID"></param>
        /// <returns></returns>
        private static decimal CalcularPrecioNetoProducto(decimal decPrecioFinal, int intAlicuotaID, GESI.CORE.BO.Verscom2k.ListaAlicuotasImpuestos moAlicuotasImpuestos)
        {
            decimal decPrecioNetoADevolver = decPrecioFinal;
            try
            {
                foreach (var Alicuota in moAlicuotasImpuestos)
                {
                    if (Alicuota.AlicuotaImpuestoID == intAlicuotaID)
                    {
                        decimal Alicuota2 = 1 + (Alicuota.Alicuota / 100);
                        decPrecioNetoADevolver = decPrecioFinal / Alicuota2;
                    }
                }

                return decPrecioNetoADevolver;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Devuelve el Precio Unitario correspondiente al detalle de venta segun C IVA del cliente
        /// </summary>
        /// <param name="decPrecioNeto"></param>
        /// <param name="decPrecioFinal"></param>
        /// <param name="oCliente"></param>
        /// <param name="oEmpresa"></param>
        /// <returns></returns>
        private static decimal CalcularPrecioUnitario(decimal decPrecioNeto, decimal decPrecioFinal, GESI.CORE.BO.Verscom2k.Cliente oCliente, GESI.CORE.BO.Empresa oEmpresa)
        {
            try
            {
                decimal decPrecioUnitario = decPrecioNeto;

                if(oEmpresa.CondicionIVA == 1)
                {
                    if(oCliente.CondicionIVA == 1 || oCliente.CondicionIVA == 6)
                    {
                        decPrecioUnitario = decPrecioNeto;
                    }
                    else
                    {
                        decPrecioUnitario = decPrecioFinal;
                    }
                }
                else
                { 
                    decPrecioUnitario = decPrecioFinal;
                }
                
                return decPrecioUnitario;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Devuelve el detalle de venta para el objeto Movimientos de Clientes
        /// </summary>
        /// <param name="UltimoNumero"></param>
        /// <param name="Producto"></param>
        /// <param name="CanalDeVentaID"></param>
        /// <param name="oItem"></param>
        /// <param name="oCliente"></param>
        /// <param name="oEmpresa"></param>
        /// <param name="oComprobante"></param>
        /// <param name="strTipoPrecio"></param>
        /// <returns></returns>
        private static GESI.ERP.Core.BO.cItemComprobanteDeVenta DeterminarDetalleDeVenta( GESI.CORE.BO.Verscom2k.Producto Producto,int CanalDeVentaID,Item oItem, 
                                                                                          GESI.CORE.BO.Verscom2k.Cliente oCliente, GESI.CORE.BO.Empresa oEmpresa,GESI.CORE.BO.Verscom2k.Comprobante oComprobante,String strTipoPrecio, 
                                                                                          GESI.CORE.BO.Verscom2k.ListaAlicuotasImpuestos moAlicuotasImpuestos,String mstrReleerPrecios)
        {
            GESI.GESI.BO.DetalleDeVenta DetalleVenta = null;
            try
            {
                decimal decPrecioUnitario;
                GESI.ERP.Core.BLL.PreciosManager.SessionManager = _SessionMgr;
                GESI.ERP.Core.BLL.PreciosManager.ERPsessionManager = new GESI.ERP.Core.SessionManager();
                GESI.ERP.Core.BO.dResultadoConsultaPrecio objResultado = new GESI.ERP.Core.BO.dResultadoConsultaPrecio();

                #region Determinar Precios Netos y Finales
                decimal decPrecioNeto = 0;
                decimal decPrecioFinal = 0;
                decimal decImporteNeto = 0;
                decimal decImporteTotal = 0;
                GESI.ERP.Core.BO.cItemComprobanteDeVenta oItemCompr = new GESI.ERP.Core.BO.cItemComprobanteDeVenta();

                if (mstrReleerPrecios.Equals("SI"))
                {

                    #region Relee Precios
                    objResultado = GESI.ERP.Core.BLL.PreciosManager.GetPrecioDeVentaProducto(Producto.ProductoID, (ushort)Convert.ToInt16(CanalDeVentaID),oItem.UnidadU1, (uint?)oCliente.ClienteID);                
                    decPrecioNeto = objResultado.PrecioNeto;                    
                    decPrecioFinal = GESI.ERP.Core.BLL.PreciosManager.PrecioFinal(Producto.ProductoID, decPrecioNeto);
                    if(objResultado.DescuentoPorcentajes?.Length > 0)
                    {
                        if (oItem.DescuentoPorcentajes.Length > 0)
                        {
                            objResultado.DescuentoPorcentajes = objResultado.DescuentoPorcentajes.Replace(".", ",");
                            oItem.DescuentoPorcentajes = objResultado.DescuentoPorcentajes +"+" +oItem.DescuentoPorcentajes.Replace(".",",");
                        }
                        else
                        {
                            oItem.DescuentoPorcentajes = objResultado.DescuentoPorcentajes;
                        }

                    }
                    #endregion
                }
                else
                {
                    #region NO relee Precios
                    if (strTipoPrecio != null)
                    {
                       switch(strTipoPrecio)
                        {
                            case "N": // Tomar los Precios como Netos
                                decPrecioNeto = oItem.Precio;
                                decPrecioFinal = GESI.ERP.Core.BLL.PreciosManager.PrecioFinal(Producto.ProductoID, decPrecioNeto);
                                break;

                            case "F": // Tomar los precios como Finales
                                decPrecioFinal = oItem.Precio;
                                decPrecioNeto = CalcularPrecioNetoProducto(decPrecioFinal, Producto.AlicuotaID,moAlicuotasImpuestos);
                                break;
                        }
                    }
                    else
                    { //Si no viene el dato lo toma como precio neto
                        decPrecioNeto = oItem.Precio;
                        decPrecioFinal = GESI.ERP.Core.BLL.PreciosManager.PrecioFinal(Producto.ProductoID, decPrecioNeto);
                    }
                    #endregion
                }
                #endregion


                decPrecioUnitario = CalcularPrecioUnitario(decPrecioNeto, decPrecioFinal, oCliente, oEmpresa);
                oItemCompr.TipoDeItem = "P";
                oItemCompr.CantidadU1 = oItem.UnidadU1;
                oItemCompr.CantidadU2 = oItem.UnidadU2;
                oItemCompr.PrecioUnitarioNeto = decPrecioNeto;
                oItemCompr.TipoDeFacturacion = Producto.TipoDeFacturacion;
                oItemCompr.CodigoDeItem = Producto.ProductoID;
                oItemCompr.PrecioUnitarioFinal = decPrecioUnitario;
                oItemCompr.AlicuotaIVA = Producto.AlicuotaID;
                oItemCompr.DescuentosPorcentaje = oItem.DescuentoPorcentajes;
                oItemCompr.Comentario = oItem.Comentario;
                
                if (!Producto.ConfirmaAlFacturar)
                {
                    if (Producto.TipoDeFacturacion.Equals("U"))
                    {
                        oItemCompr.CantidadU3 = oItemCompr.CantidadU1 * Producto.Unidad2XUnidad1;
                    }
                }
                else
                {
                    if(oItem.UnidadU2 <= 0)
                    {
                        oItemCompr.CantidadU3 = oItemCompr.CantidadU1 * Producto.Unidad2XUnidad1;
                    }
                    else
                    {
                        oItemCompr.CantidadU3 = oItem.UnidadU1 * oItem.UnidadU2;
                    }
                }
                GESI.ERP.Core.BO.dResultadoImportesItemVenta oResultado = GESI.ERP.Core.BLL.ItemComprobanteDeVentaManager.CalcularImporteVenta(oItemCompr);

                oItemCompr.ImporteTotalNeto = oResultado.ImporteNeto;
                
                if (oComprobante.Impuestos == false)
                {
                    oItemCompr.ImporteTotalFinal = oResultado.ImporteNeto;
                }
                else
                {
                    oItemCompr.ImporteTotalFinal = oResultado.ImporteFinal;
                }

                return oItemCompr;
                

            }
            catch (Exception ex)
            {
                LogSucesosAPI.LoguearErroresPedidos("Error al cargar detalle de venta " + ex.Message,_SessionMgr.EmpresaID);
                throw ex;
            }
        }

    }

   
}