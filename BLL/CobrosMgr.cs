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
    /// <summary>
    /// Cobros
    /// </summary>
    public class CobrosMgr
    {
        #region Variables           
        /// <summary>
        /// Session Manager
        /// </summary>
        public static GESI.CORE.BLL.SessionMgr _SessionMgr;           
        private static SqlConnection myConn;
        public static GESI.GESI.BO.ListaEjercicios moEjercicios;
        
        public static String mostrMotivoNoImportar = "";
        public static decimal modecSaldoPendienteCobranza;
        public static String mostrBancosUtilizados = "";
        #endregion

        /// <summary>
        /// Realiza la Importacion de las Cobranzas via API
        /// </summary>
        /// <param name="oCobro"></param>
        /// <param name="lstCajasYBancos"></param>
        /// <param name="lstValores"></param>
        /// <param name="lstConfiguraciones"></param>
        /// <param name="oComprobante"></param>
        /// <returns></returns>
        public static Request ImportarCobro(APIImportacionComprobantes.BO.Cobro oCobro,GESI.GESI.BO.ListaCajasYBancos lstCajasYBancos, GESI.GESI.BO.ListaValores lstValores, GESI.CORE.BO.ListaConfiguracionesBase lstConfiguraciones, GESI.CORE.BO.Verscom2k.Comprobante oComprobante, int mointSubdiarioID,int mointTipoOperacionID,GESI.GESI.BO.ListaReferenciasContables lstReferenciasContables, GESI.GESI.BO.ListaBancos lstBancos)
        {
            Request respuesta = new Request();

            try
            {
               
                    #region Variables Iniciales
                    
                    mostrMotivoNoImportar = "";
                    #endregion

                    if (oCobro.CobroID > 0)
                    {
                        if (oCobro.FechaRegistracion != null) // No tiene fecha
                        {
                            if (oCobro.FechaRegistracion.Length > 0)
                            {
                                #region Parse Fecha
                                try
                                {
                                    DateTime oFecha = DateTime.Parse(oCobro.FechaRegistracion, null, System.Globalization.DateTimeStyles.RoundtripKind);
                                    SetearVariablesGlobalesAUtilizar(oFecha,oComprobante,lstConfiguraciones);
                                }
                                catch (Exception ex) //Parse fecha
                                {
                                    throw ex;
                                }
                                #endregion


                                if (moEjercicios != null)
                                {
                                    if (moEjercicios.Count == 1) //Contiene un periodo contable
                                    {

                                            if (oCobro.TotalCobrado > 0)
                                            {
                                                modecSaldoPendienteCobranza = oCobro.TotalCobrado;
                                                String mstrCodigoImportacion = _SessionMgr.UsuarioID + "_" + oCobro.CobroID;
                                                GESI.CORE.BO.Verscom2k.MovimientoDeCliente MovimientoABuscar = GESI.CORE.DAL.Verscom2k.MovimientosDeClientesDB.GetItem(mstrCodigoImportacion, _SessionMgr.EmpresaID);

                                                if (MovimientoABuscar == null)
                                                {
                                                    if (oCobro.Cliente != null)
                                                    {
                                                        if (oCobro.Cliente.ClienteID > 0)
                                                        {
                                                            
                                                            if (oComprobante != null)
                                                            {
                                                                if (oComprobante.ClaseDeComprobanteID == 120)
                                                                {
                                                                    GESI.CORE.BO.Verscom2k.Cliente oCliente = GESI.CORE.DAL.Verscom2k.TablasGeneralesGESIDB.GetItemCliente(oCobro.Cliente.ClienteID, 0, "", _SessionMgr.EmpresaID);
                                                                    if (oCliente != null)
                                                                    {
                                                                        respuesta = AltaCobranza(oCliente, oCobro, oComprobante, lstCajasYBancos, lstValores, lstConfiguraciones, mointSubdiarioID, mointTipoOperacionID, lstReferenciasContables, lstBancos);
                                                                    }
                                                                    else
                                                                    {
                                                                        respuesta = DevolverObjetoRequest(false, (int)DefinicionesErrores.eNoEncontrado, "No se encontro el cliente en la Base de datos", oCobro.CobroID);
                                                                        LogSucesosAPI.LoguearErrores("No se encontro el cliente en la Base de datos. CobranzaID: " + oCobro.CobroID,_SessionMgr.EmpresaID);
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    respuesta = DevolverObjetoRequest(false, (int)DefinicionesErrores.eNoEncontrado, "El comprobante no es de tipo recibo (120)", oCobro.CobroID);
                                                                     LogSucesosAPI.LoguearErrores("El comprobante no es de tipo recibo (120)" + oCobro.CobroID,_SessionMgr.EmpresaID);
                                                                }

                                                            }
                                                            else
                                                            {
                                                                respuesta = DevolverObjetoRequest(false, (int)DefinicionesErrores.eNoEncontrado, "No se encontro el comprobante a utilizar", oCobro.CobroID);
                                                                LogSucesosAPI.LoguearErrores("No se encontro el comprobanteID a utilizar en la Base de datos. CobranzaID: " + oCobro.CobroID,_SessionMgr.EmpresaID);

                                                            }
                                                        }
                                                        else
                                                        {
                                                           respuesta = DevolverObjetoRequest(false, (int)DefinicionesErrores.eNoEncontrado, "No se encontro el cliente la peticion recibida", oCobro.CobroID);
                                                           LogSucesosAPI.LoguearErrores("No se encontro el cliente en la Base de datos. CobranzaID: " + oCobro.CobroID,_SessionMgr.EmpresaID);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        respuesta = DevolverObjetoRequest(false, (int)DefinicionesErrores.eNoEncontrado, "No se encontro el cliente la peticion recibida", oCobro.CobroID);
                                                        LogSucesosAPI.LoguearErrores("No se encontro el cliente en la Base de datos. CobranzaID: " + oCobro.CobroID,_SessionMgr.EmpresaID);
                                                    }

                                                }
                                                else
                                                {
                                                    respuesta = DevolverObjetoRequest(false, (int)DefinicionesErrores.eYaCargadoEnSistema, "Cobro Numero " + oCobro.CobroID + " ya cargado en sistema con Numero " + MovimientoABuscar.Numero, oCobro.CobroID);
                                                     LogSucesosAPI.LoguearErrores("Cobro Numero " + oCobro.CobroID + " ya cargado en sistema con Numero " + MovimientoABuscar.Numero,_SessionMgr.EmpresaID);
                                                }
                                            }
                                            else
                                            {
                                                respuesta = DevolverObjetoRequest(false, (int)DefinicionesErrores.eDatoVacioONull, "El total de la cobranza debe ser mayor a cero", oCobro.CobroID);
                                                 LogSucesosAPI.LoguearErrores("El total de la cobranza debe ser mayor a cero",_SessionMgr.EmpresaID);
                                            }
                                       
                                    }
                                    else
                                    {
                                        respuesta = DevolverObjetoRequest(false, (int)DefinicionesErrores.eNoEncontrado, "No se encontro el periodo contable de los datos enviados", oCobro.CobroID); 
                                        LogSucesosAPI.LoguearErrores("No se encontro el periodo contable de los datos enviados. Fecha: " + oCobro.FechaRegistracion + ". CobranzaID: " + oCobro.CobroID,_SessionMgr.EmpresaID);
                                    }

                                }
                                else
                                {
                                    respuesta = DevolverObjetoRequest(false, (int)DefinicionesErrores.eNoEncontrado, "No se encontro el periodo contable de los datos enviados", oCobro.CobroID);   
                                   LogSucesosAPI.LoguearErrores("No se encontro el periodo contable de los datos enviados. CobranzaID: " + oCobro.CobroID,_SessionMgr.EmpresaID);
                                }

                            }

                        }
                    }
               
                return respuesta;
               
            }
            catch (Exception ex)
            {
                respuesta = DevolverObjetoRequest(false, (int)DefinicionesErrores.eInternoAplicacion, ex.Message, oCobro.CobroID);
                LogSucesosAPI.LoguearErrores(ex.Message+" CobranzaID: " + oCobro.CobroID,_SessionMgr.EmpresaID);
                return respuesta;
            }
        }

        /// <summary>
        /// Realiza la Importacion de la cobranza con sus Movimientos contables , Valores y Comprobantes Aplicados
        /// </summary>
        /// <param name="oCliente"></param>
        /// <param name="oCobro"></param>
        /// <param name="oComprobante"></param>
        /// <param name="oEmpresa"></param>
        /// <returns></returns>
        private static Request AltaCobranza(GESI.CORE.BO.Verscom2k.Cliente oCliente, Cobro oCobro, GESI.CORE.BO.Verscom2k.Comprobante oComprobante, GESI.GESI.BO.ListaCajasYBancos lstCajasYBancos, GESI.GESI.BO.ListaValores lstValores, GESI.CORE.BO.ListaConfiguracionesBase lstConfiguraciones, int mointSubdiarioID,int mointTipoOperacioID, GESI.GESI.BO.ListaReferenciasContables lstReferenciasContables, GESI.GESI.BO.ListaBancos lstBancos)
        {
            try
            {
                String strQueEstaHaciendo = "";
                 Request respuesta = new Request();
                 GESI.GESI.BLL.MovimientosDeClientesMgr.SessionManager = _SessionMgr;
                 GESI.CORE.BO.Verscom2k.MovimientoDeCliente oMovimientoDeCliente = new GESI.CORE.BO.Verscom2k.MovimientoDeCliente();
                 String strMotivoNoImportar = "";
                 GESI.GESI.BO.ListaStockDeValores lstStockValores = new GESI.GESI.BO.ListaStockDeValores();
                 oMovimientoDeCliente = LlenarObjetoMovimientoCliente(oCliente, oCobro, oComprobante,lstCajasYBancos,lstValores,lstConfiguraciones,mointSubdiarioID,mointTipoOperacioID,lstReferenciasContables,lstBancos);
                
                    if (ValidarObjetoMovimientoDeCliente(oMovimientoDeCliente))
                    {
                        oMovimientoDeCliente = SetearProximoNumeroComprobante(oMovimientoDeCliente,oComprobante);
                        
                        GESI.GESI.BO.MovimientoDeCliente objMovimientoDeClienteAGrabar = ConvertirAGesiMovimientoDeCliente(oMovimientoDeCliente);
                        lstStockValores = VerificarStockDeValores(objMovimientoDeClienteAGrabar, lstValores);
                        
                        SqlTransaction myTran = null;
                        try
                        {

                        #region Grabar Cobro en BD
                            myConn = GESI.CORE.DAL.DBHelper.DevolverConnectionStringCORE();                        
                            myConn.Open();
                            myTran = myConn.BeginTransaction("TN_ALTANCB");
                            strQueEstaHaciendo = "Grabando Movimiento de Cliente";
                            GESI.GESI.DAL.MovimientosDeClientesDB.Save(objMovimientoDeClienteAGrabar, true);

                            strQueEstaHaciendo = "Grabando Detalle de Movimiento de Cliente";

                            foreach (var Detalle in objMovimientoDeClienteAGrabar.MovimientosDeValores)
                             {
                                GESI.GESI.DAL.MovimientosDeValoresDB.Save(Detalle, myConn, myTran);
                             }

                            #region Movimiento Contable
                            strQueEstaHaciendo = "Grabando Movimiento Contable";

                            int intNumeroContable = GESI.GESI.DAL.MovimientosContablesDB.Save(objMovimientoDeClienteAGrabar.MovimientoContable,myConn,myTran);

                            strQueEstaHaciendo = "Grabando Detalle de Movimiento Contable";

                            foreach (var Detalle in objMovimientoDeClienteAGrabar.MovimientoContable.Detalles)
                            {
                                Detalle.MovimientoContableID = intNumeroContable;
                                GESI.GESI.DAL.DetallesMovimientosContablesDB.Save(Detalle, myConn, myTran);
                            }

                            #endregion



                            #region Stock de Valores
                            if (lstStockValores.Count > 0)
                            {
                                foreach (GESI.GESI.BO.StockDeValor oStock in lstStockValores)
                                {
                                    GESI.GESI.DAL.StockDeValoresDB.Save(oStock, true, myConn, myTran);
                                }
                            }
                            #endregion

                            #region Relaciones Cobros Clientes
                            
                            if(objMovimientoDeClienteAGrabar.RelacionesCobrosClientes.Count > 0)
                            {
                                foreach(GESI.GESI.BO.RelacionCobroCliente oRelacionCobroCliente in objMovimientoDeClienteAGrabar.RelacionesCobrosClientes)
                                {
                                    if (oRelacionCobroCliente != null)
                                    {
                                        GESI.GESI.DAL.RelacionesCobrosClientesDB.Save(oRelacionCobroCliente, true, myConn, myTran);
                                        GESI.GESI.BO.VencimientoDeCliente oVencimiento = GESI.GESI.DAL.VencimientosDeClientesDB.GetItem(oRelacionCobroCliente.EmpresaID, oRelacionCobroCliente.Vto_ComprobanteID, oRelacionCobroCliente.Vto_Serie, oRelacionCobroCliente.Vto_PuntoDeVentaID, oRelacionCobroCliente.Vto_Numero, oRelacionCobroCliente.VencimientoID,myConn,myTran);
                                        oVencimiento.SaldoPendiente = oVencimiento.SaldoPendiente - oRelacionCobroCliente.Importe;
                                        GESI.GESI.DAL.VencimientosDeClientesDB.Save(oVencimiento, myConn, myTran);

                                        GESI.GESI.BO.MovimientoDeCliente oMovimientoClienteAActualizar = new GESI.GESI.BO.MovimientoDeCliente();
                                        oMovimientoClienteAActualizar.EmpresaID = oMovimientoDeCliente.EmpresaID;
                                        oMovimientoClienteAActualizar.PuntoDeVentaID = oRelacionCobroCliente.Vto_PuntoDeVentaID;
                                        oMovimientoClienteAActualizar.Serie = oRelacionCobroCliente.Vto_Serie;
                                        oMovimientoClienteAActualizar.Numero = oRelacionCobroCliente.Vto_Numero;
                                        oMovimientoClienteAActualizar.ComprobanteID = oRelacionCobroCliente.Vto_ComprobanteID;
                                        oMovimientoClienteAActualizar = GESI.GESI.BLL.MovimientosDeClientesMgr.GetItem(oMovimientoClienteAActualizar, myConn, myTran);
                                        oMovimientoClienteAActualizar.FechaEntrega = oMovimientoClienteAActualizar.Fecha;

                                        if (oMovimientoClienteAActualizar != null)
                                        {
                                            oMovimientoClienteAActualizar.SaldoPendiente = oVencimiento.SaldoPendiente;
                                            if (oMovimientoClienteAActualizar.SaldoPendiente <= 0)
                                            {
                                                oMovimientoClienteAActualizar.Pendiente = false;
                                            }
                                        }
                                        GESI.GESI.DAL.MovimientosDeClientesDB.Save(oMovimientoClienteAActualizar, false, myConn, myTran);
                                    }
                                }                                                               
                            }

                            #endregion

                            myTran.Commit();
                            Request objRequest = new Request();
                            objRequest.Error = new Error();
                            objRequest.Success = true;
                            objRequest.Response = new Response();
                            objRequest.Response.Data = JsonConvert.SerializeObject(oCobro);
                            respuesta = objRequest;
                            #endregion

                            #region Registrar Exitoso
                            LogSucesosAPI.LoguearExitosos(JsonConvert.SerializeObject(respuesta));
                            #endregion

                        }
                        catch (Exception ex)
                        {

                            #region ROLLBACK
                            myTran.Rollback();
                            strMotivoNoImportar = ex.Message;
                            Request objRequest = new Request();
                            objRequest.Error = new Error();
                            objRequest.Success = false;
                            objRequest.Response = new Response();
                            objRequest.Error.Code = (int)DefinicionesErrores.eInternoAplicacion;
                            objRequest.Error.Message = strMotivoNoImportar;
                            respuesta = objRequest;
                            LogSucesosAPI.LoguearErrores(mostrMotivoNoImportar + " Response: " + JsonConvert.SerializeObject(oCobro),_SessionMgr.EmpresaID);
                            #endregion

                        }
                            myConn.Close();
                    }
                    else
                    {
                        Request objRequest = new Request();
                        objRequest.Error = new Error();
                        objRequest.Success = false;
                        objRequest.Response = new Response();
                        objRequest.Error.Code = (int)DefinicionesErrores.eFaltaAtributo;
                        objRequest.Error.Message = mostrMotivoNoImportar;
                        objRequest.Response.Data = "Codigo de Pago: " + oCobro.CobroID;
                        respuesta = objRequest;

                         LogSucesosAPI.LoguearErrores(mostrMotivoNoImportar+" Response: "+JsonConvert.SerializeObject(oCobro),_SessionMgr.EmpresaID);
                    }
                
                

                return respuesta;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Genera un objeto MovimientoDeCliente con los datos de Empresa, Comprobante y JSON
        /// </summary>
        /// <param name="oCliente"></param>
        /// <param name="oCobro"></param>
        /// <param name="oComprobante"></param>
        /// <param name="lstCajasYBancos"></param>
        /// <param name="lstValores"></param>
        /// <param name="lstConfiguraciones"></param>
        /// <returns></returns>
        private static GESI.CORE.BO.Verscom2k.MovimientoDeCliente LlenarObjetoMovimientoCliente(GESI.CORE.BO.Verscom2k.Cliente oCliente, Cobro oCobro, GESI.CORE.BO.Verscom2k.Comprobante oComprobante, GESI.GESI.BO.ListaCajasYBancos lstCajasYBancos, GESI.GESI.BO.ListaValores lstValores, GESI.CORE.BO.ListaConfiguracionesBase lstConfiguraciones, int mointSubdiarioID, int mointTipoOperacionID, GESI.GESI.BO.ListaReferenciasContables lstReferenciasContables,GESI.GESI.BO.ListaBancos lstBancos)
        {
            try
            {
                bool blImportarloACuentaPorIncoherencia = false;
                GESI.GESI.BLL.VencimientosDeClientesMgr.SessionManager = _SessionMgr;
                GESI.CORE.BO.Verscom2k.MovimientoDeCliente oMovimientoDeCliente = new GESI.CORE.BO.Verscom2k.MovimientoDeCliente();
                GESI.GESI.BLL.TablasGeneralesGESIMgr.SessionManager = _SessionMgr;
                #region Asignar Atributos Objeto
                oMovimientoDeCliente.ComprobanteID = oComprobante.ComprobanteID;
                oMovimientoDeCliente.EmpresaID = _SessionMgr.EmpresaID;
                oMovimientoDeCliente.PuntoDeVentaID = oComprobante.PuntoDeVentaID;
                oMovimientoDeCliente.Serie = oComprobante.Serie;
                oMovimientoDeCliente.SubTipo = "R";
                oMovimientoDeCliente.TipoOperacionID = mointTipoOperacionID;
                oMovimientoDeCliente.Total = oCobro.TotalCobrado;                              
                oMovimientoDeCliente.DivisaID = 1; //TODO: Ver cobro MultiDivisa
                oMovimientoDeCliente.Notas = oCobro.Notas;
                oMovimientoDeCliente.Notas2 = "Comprobante Original de la Aplicacion Importada: " + oCobro.CobroID;
                oMovimientoDeCliente.Fecha = DateTime.Parse(oCobro.FechaRegistracion, null, System.Globalization.DateTimeStyles.RoundtripKind);
                oMovimientoDeCliente.ClienteID = oCliente.ClienteID;
                oMovimientoDeCliente.NumeroDeDocumento = oCliente.NumeroDeDocumento;
                oMovimientoDeCliente.RazonSocial = oCliente.RazonSocial;
                oMovimientoDeCliente.UsuarioID = _SessionMgr.UsuarioID;
                oMovimientoDeCliente.CodigoImportacion = _SessionMgr.UsuarioID + "_" + oCobro.CobroID;                
                oMovimientoDeCliente.MovimientosDeValores = new GESI.CORE.BO.Verscom2k.ListaMovimientosDeValores();
                oMovimientoDeCliente.RelacionesCobrosClientes = new GESI.CORE.BO.Verscom2k.ListaRelacionesCobrosClientes();
                #endregion

                #region Comprobantes Aplicados
                if(oCobro.ComprobantesAplicados != null)
                {
                    GESI.CORE.BO.Verscom2k.RelacionCobroCliente oRelacionCobroCliente = null;
                    foreach(ComprobanteAplicado oComprobanteAplicado in oCobro.ComprobantesAplicados)
                    {
                        if (oComprobanteAplicado != null)
                        {
                            oRelacionCobroCliente = DevolverRelacionCobroCliente(oComprobanteAplicado, oCobro, oComprobante, oMovimientoDeCliente);
                            if (oRelacionCobroCliente != null)
                            {
                                oMovimientoDeCliente.RelacionesCobrosClientes.Add(oRelacionCobroCliente);
                            }
                            else
                            {
                                blImportarloACuentaPorIncoherencia = true;
                            }
                        }
                    }
                }                               

                if(blImportarloACuentaPorIncoherencia)
                {
                    oMovimientoDeCliente.RelacionesCobrosClientes = new GESI.CORE.BO.Verscom2k.ListaRelacionesCobrosClientes();
                    modecSaldoPendienteCobranza = oMovimientoDeCliente.Total;
                    oMovimientoDeCliente.Pendiente = true;
                }

                #endregion

                #region Valores y Cajas
                if (oCobro.Valores != null)
                {
                    int i = 1;
                    if (oCobro.Valores.Count > 0)
                    {
                        foreach (Valor oValor in oCobro.Valores)
                        {
                            List<GESI.GESI.BO.CajaYBanco> lstCajaYBancoEncontrado = lstCajasYBancos.Where(x => x.CajaBancoID == oValor.CajaBancoID).ToList();

                            if(lstCajaYBancoEncontrado.Count == 0)
                            {
                                oMovimientoDeCliente.Numero = -1;
                                mostrMotivoNoImportar = mostrMotivoNoImportar + "No se encontro la Caja " + oValor.CajaBancoID + " en la base de datos\n";
                            }
                            else
                            {
                                List<GESI.GESI.BO.Valor> lstValoresEncontrados = lstValores.Where(x => x.ValorID == oValor.ValorID).ToList();
                                if (lstValoresEncontrados.Count == 0)
                                {
                                    oMovimientoDeCliente.Numero = -1;
                                    mostrMotivoNoImportar = mostrMotivoNoImportar + "No se encontro el Valor " + oValor.ValorID + " en la base de datos\n";
                                }
                                else
                                {
                                    #region Llenar Movimiento de Valores

                                    #region BANCO DEL VALOR
                                    int BancoID = 0;
                                    String strDescripcionBanco = "";
                                    List<GESI.GESI.BO.Banco> oBancoABuscar = lstBancos.Where(x => x.BancoID == oValor.BancoID).ToList();

                                    if(oBancoABuscar.Count > 0)
                                    {
                                        BancoID = oBancoABuscar[0].BancoID;
                                        strDescripcionBanco = oBancoABuscar[0].Descripcion;
                                    }
                                    else
                                    {
                                        if(oValor.BancoID > 0)
                                        mostrBancosUtilizados = "Banco: " + oValor.BancoID + " utilizado para el valor " + oValor.NumeroDeValor;
                                    }
                                    #endregion

                                    foreach (GESI.GESI.BO.Valor ValorVerscom in lstValoresEncontrados)
                                    {
                                        GESI.CORE.BO.Verscom2k.MovimientoDeValor Valor = new GESI.CORE.BO.Verscom2k.MovimientoDeValor();
                                        Valor.EmpresaID = _SessionMgr.EmpresaID;
                                        Valor.ValorID = oValor.ValorID;
                                        Valor.CajaBancoID = oValor.CajaBancoID;
                                        Valor.ComprobanteID = oComprobante.ComprobanteID;
                                        Valor.DivisaID = 1;
                                        Valor.Serie = oMovimientoDeCliente.Serie;
                                        Valor.PuntoDeVentaID = oMovimientoDeCliente.PuntoDeVentaID;
                                        Valor.Fecha = oMovimientoDeCliente.Fecha;
                                        Valor.ItemID = 0;
                                        Valor.Importe = oValor.Importe;
                                        Valor.NumeroDeValor = oValor.NumeroDeValor;
                                        Valor.PoC = "C";
                                        Valor.PoCID = oCliente.ClienteID;
                                        Valor.TipoMovimiento = "E";
                                        Valor.Estado = "C";
                                        Valor.BancoID = BancoID;
                                        Valor.DescripcionBanco = strDescripcionBanco;

                                        if (oValor.Emisor != null)
                                            Valor.EmisorOBeneficiario = oValor.Emisor;
                                        try
                                        {
                                            if (oValor.FechaDC != null)
                                            {
                                                Valor.FechaDC = DateTime.Parse(oValor.FechaDC, null, System.Globalization.DateTimeStyles.RoundtripKind);
                                            }
                                        }
                                        catch(Exception ex)
                                        {
                                            mostrMotivoNoImportar = "Formato Incorrecto de Fecha DC \n";
                                        }
                                        oMovimientoDeCliente.MovimientosDeValores.Add(Valor);
                                    }
                                    #endregion
                                }
                            }
                          
                        }
                    }
                    else
                    {
                        oMovimientoDeCliente.Numero = -1;
                        mostrMotivoNoImportar = mostrMotivoNoImportar + "No se encontraron valores en los datos enviados\n";
                    }
                }
                else
                {
                    oMovimientoDeCliente.Numero = -1;
                    mostrMotivoNoImportar = mostrMotivoNoImportar + "No se encontraron valores en los datos enviados\n";
                }
                #endregion

                oMovimientoDeCliente.MovimientoContable = DevolverMovimientoContable(oMovimientoDeCliente, oComprobante, oCliente,lstConfiguraciones,lstCajasYBancos,lstValores,mointSubdiarioID,mointTipoOperacionID,lstReferenciasContables);
                oMovimientoDeCliente.SaldoPendiente = modecSaldoPendienteCobranza;

                if(oMovimientoDeCliente.SaldoPendiente <= 0)
                {
                    oMovimientoDeCliente.Pendiente = false;
                }
                else
                {
                    oMovimientoDeCliente.Pendiente = true;
                }

                if(mostrMotivoNoImportar.Length > 0)
                {
                    oMovimientoDeCliente.Notas = oMovimientoDeCliente.Notas + "\r\n" + mostrMotivoNoImportar;
                }

                if (mostrBancosUtilizados.Length > 0)
                    oMovimientoDeCliente.Notas = oMovimientoDeCliente.Notas + "\r\n" + mostrBancosUtilizados;

                
               
                return oMovimientoDeCliente;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Devuelve el Movimiento Contable a Grabar en la BD
        /// </summary>
        /// <param name="oMovimientoDeCliente"></param>
        /// <param name="oComprobante"></param>
        /// <param name="oCliente"></param>
        /// <param name="lstConfiguraciones"></param>
        /// <param name="lstCajasYBancos"></param>
        /// <param name="lstValores"></param>
        /// <returns></returns>
        private static GESI.CORE.BO.Verscom2k.MovimientoContable DevolverMovimientoContable(GESI.CORE.BO.Verscom2k.MovimientoDeCliente oMovimientoDeCliente, GESI.CORE.BO.Verscom2k.Comprobante oComprobante, GESI.CORE.BO.Verscom2k.Cliente oCliente, GESI.CORE.BO.ListaConfiguracionesBase lstConfiguraciones, GESI.GESI.BO.ListaCajasYBancos lstCajasYBancos, GESI.GESI.BO.ListaValores lstValores, int mointSubdiarioID, int mointTipoOperacionID, GESI.GESI.BO.ListaReferenciasContables lstReferenciasContables)
        {
            try
            {
                
                String strImputacionesAReasignar = "";
                String strCobroRegistrarAplicaciones = "";
                String strDeudoresXVentas = "";

                #region Configuraciones Iniciales

                List<GESI.CORE.BO.ConfiguracionBase> olstConfiguracionBase = lstConfiguraciones.Where(x => x.GrupoID == "Contabilidad" && x.SeccionID == "Configuracion" && x.ItemID == "ImputacionesAReapropiar_000" + _SessionMgr.EmpresaID + "_000" + _SessionMgr.SucursalID).ToList();

                if (olstConfiguracionBase.Count > 0)
                {
                    strImputacionesAReasignar = olstConfiguracionBase[0].Valor;
                                      
                }
                else
                {
                    olstConfiguracionBase = lstConfiguraciones.Where(x => x.GrupoID == "Contabilidad" && x.SeccionID == "Configuracion" && x.ItemID == "ImputacionesAReapropiar_000" + _SessionMgr.EmpresaID + "_0001").ToList();
                    if (olstConfiguracionBase.Count > 0)
                    {
                        strImputacionesAReasignar = olstConfiguracionBase[0].Valor;
                    }
                }

                olstConfiguracionBase = lstConfiguraciones.Where(x => x.GrupoID == "Contabilidad" && x.SeccionID == "Configuracion" && x.ItemID == "CobrosRegistrarAplicaciones").ToList();
                if (olstConfiguracionBase.Count > 0)
                {
                    strCobroRegistrarAplicaciones = olstConfiguracionBase[0].Valor;
                }
                olstConfiguracionBase = lstConfiguraciones.Where(x => x.GrupoID == "Contabilidad" && x.SeccionID == "Configuracion" && x.ItemID == "DeudoresPorVentas_000" + _SessionMgr.EmpresaID + "_000"+_SessionMgr.SucursalID).ToList();
                if (olstConfiguracionBase.Count > 0)
                {
                    strDeudoresXVentas = olstConfiguracionBase[0].Valor;
                }
                #endregion



                oMovimientoDeCliente.MovimientoContable = new GESI.CORE.BO.Verscom2k.MovimientoContable();
                oMovimientoDeCliente.MovimientoContable.ComprobanteID = oComprobante.ComprobanteID;
                oMovimientoDeCliente.MovimientoContable.Descripcion = "C: " + oMovimientoDeCliente.RazonSocial;
                oMovimientoDeCliente.MovimientoContable.EjercicioID = moEjercicios[0].EjercicioID;
                oMovimientoDeCliente.MovimientoContable.EmpresaID = _SessionMgr.EmpresaID;
                oMovimientoDeCliente.MovimientoContable.Fecha = oMovimientoDeCliente.Fecha;
                oMovimientoDeCliente.MovimientoContable.Numero = oMovimientoDeCliente.Numero;
                oMovimientoDeCliente.MovimientoContable.PoC = "C";
                oMovimientoDeCliente.MovimientoContable.PoCID = oCliente.ClienteID;
                oMovimientoDeCliente.MovimientoContable.Serie = oMovimientoDeCliente.Serie;
                oMovimientoDeCliente.MovimientoContable.PuntoDeVentaID = oMovimientoDeCliente.PuntoDeVentaID;
                oMovimientoDeCliente.MovimientoContable.SubDiarioID = mointSubdiarioID;                
                oMovimientoDeCliente.MovimientoContable.Detalles = new GESI.CORE.BO.Verscom2k.ListaDetallesMovimientosContables();
                GESI.GESI.BO.ListaDetallesMovimientosContables lstListaDetallesMovimientosContables = new GESI.GESI.BO.ListaDetallesMovimientosContables();
                oMovimientoDeCliente.MovimientoContable.Detalles = new GESI.CORE.BO.Verscom2k.ListaDetallesMovimientosContables();
                #region Detalles Contables Debe
                foreach (GESI.CORE.BO.Verscom2k.MovimientoDeValor oValorRecibo in oMovimientoDeCliente.MovimientosDeValores)
                {
                    List<GESI.GESI.BO.Valor> lstValorEncontrado = lstValores.Where(x => x.ValorID == oValorRecibo.ValorID).ToList();
                    List<GESI.GESI.BO.CajaYBanco> lstCajaEncontrada = lstCajasYBancos.Where(x => x.CajaBancoID == oValorRecibo.CajaBancoID).ToList();
                    oMovimientoDeCliente.MovimientoContable.Detalles.Add(DevolverDetalleMovimientoContable(oValorRecibo, lstValorEncontrado[0], lstCajaEncontrada[0], strImputacionesAReasignar, mointSubdiarioID, mointTipoOperacionID,lstReferenciasContables));                  
                }
                #endregion

                #region Haber
                if (oCliente.ReferenciaContableID != null)
                {
                    if(oCliente.ReferenciaContableID.Length > 0)
                    {
                        List<GESI.GESI.BO.ReferenciaContable> lstRefContableAux = lstReferenciasContables.Where(x => x.ReferenciaContableID == oCliente.ReferenciaContableID).ToList();
                        if (lstRefContableAux.Count > 0)
                        {
                            oMovimientoDeCliente.MovimientoContable.Detalles.Add(CargarDetalleMovimientoContable("H", oCliente.ReferenciaContableID, oMovimientoDeCliente.Total, mointSubdiarioID, mointTipoOperacionID));
                        }
                        else
                        {
                            lstRefContableAux = lstReferenciasContables.Where(x => x.ReferenciaContableID == strImputacionesAReasignar).ToList();
                            if(lstRefContableAux.Count > 0)
                            {
                                oMovimientoDeCliente.MovimientoContable.Detalles.Add(CargarDetalleMovimientoContable("H", strImputacionesAReasignar, oMovimientoDeCliente.Total, mointSubdiarioID, mointTipoOperacionID));
                            }
                        }
                     }
                    else
                    {
                        if (strCobroRegistrarAplicaciones.Length > 0)
                        {
                            if (strCobroRegistrarAplicaciones.Equals("NO"))
                            {
                              
                                if (strDeudoresXVentas.Length > 0)
                                {
                                    List<GESI.GESI.BO.ReferenciaContable> lstRefContableAux = lstReferenciasContables.Where(x => x.ReferenciaContableID == strDeudoresXVentas).ToList();
                                    if (lstRefContableAux.Count > 0)
                                    {
                                        oMovimientoDeCliente.MovimientoContable.Detalles.Add(CargarDetalleMovimientoContable("H", strDeudoresXVentas, oMovimientoDeCliente.Total, mointSubdiarioID, mointTipoOperacionID));
                                    }
                                    else
                                    {
                                        lstRefContableAux = lstReferenciasContables.Where(x => x.ReferenciaContableID == strImputacionesAReasignar).ToList();
                                        if (lstRefContableAux.Count > 0)
                                        {
                                            oMovimientoDeCliente.MovimientoContable.Detalles.Add(CargarDetalleMovimientoContable("H", strImputacionesAReasignar, oMovimientoDeCliente.Total, mointSubdiarioID, mointTipoOperacionID));

                                        }
                                    }
                                }
                                else
                                {
                                    oMovimientoDeCliente.MovimientoContable.Detalles.Add(CargarDetalleMovimientoContable("H", strImputacionesAReasignar, oMovimientoDeCliente.Total, mointSubdiarioID, mointTipoOperacionID));
                                 }

                            }
                            else
                            {
                                //TODO: Registrar Aplicaciones en cobranzas
                            }
                        }
                        else
                        {
                            if (strDeudoresXVentas.Length > 0)
                            {
                                List<GESI.GESI.BO.ReferenciaContable> lstRefContableAux = lstReferenciasContables.Where(x => x.ReferenciaContableID == strDeudoresXVentas).ToList();
                                if (lstRefContableAux.Count > 0)
                                {
                                    oMovimientoDeCliente.MovimientoContable.Detalles.Add(CargarDetalleMovimientoContable("H", strDeudoresXVentas, oMovimientoDeCliente.Total, mointSubdiarioID, mointTipoOperacionID));
                                }
                                else
                                {
                                    lstRefContableAux = lstReferenciasContables.Where(x => x.ReferenciaContableID == strImputacionesAReasignar).ToList();
                                    if (lstRefContableAux.Count > 0)
                                    {
                                        oMovimientoDeCliente.MovimientoContable.Detalles.Add(CargarDetalleMovimientoContable("H", strImputacionesAReasignar, oMovimientoDeCliente.Total, mointSubdiarioID, mointTipoOperacionID));

                                    }
                                }
                            }
                            else
                            {
                                oMovimientoDeCliente.MovimientoContable.Detalles.Add(CargarDetalleMovimientoContable("H", strImputacionesAReasignar, oMovimientoDeCliente.Total, mointSubdiarioID, mointTipoOperacionID));
                            }
                        }
                    }
                }
                else
                {
                   if(strCobroRegistrarAplicaciones.Length > 0)
                    {
                        if(strCobroRegistrarAplicaciones.Equals("NO"))
                        {
                         
                            if(strDeudoresXVentas.Length > 0)
                            {
                                List<GESI.GESI.BO.ReferenciaContable> lstRefContableAux = lstReferenciasContables.Where(x => x.ReferenciaContableID == strDeudoresXVentas).ToList();
                                if (lstRefContableAux.Count > 0)
                                {
                                    oMovimientoDeCliente.MovimientoContable.Detalles.Add(CargarDetalleMovimientoContable("H", strDeudoresXVentas, oMovimientoDeCliente.Total, mointSubdiarioID, mointTipoOperacionID));

                                }
                                else
                                {
                                    lstRefContableAux = lstReferenciasContables.Where(x => x.ReferenciaContableID == strImputacionesAReasignar).ToList();
                                    if (lstRefContableAux.Count > 0)
                                    {
                                        oMovimientoDeCliente.MovimientoContable.Detalles.Add(CargarDetalleMovimientoContable("H", strImputacionesAReasignar, oMovimientoDeCliente.Total, mointSubdiarioID, mointTipoOperacionID));
                                    }
                                }

                            }
                            else
                            {
                                oMovimientoDeCliente.MovimientoContable.Detalles.Add(CargarDetalleMovimientoContable("H", strImputacionesAReasignar, oMovimientoDeCliente.Total, mointSubdiarioID, mointTipoOperacionID));
                             }

                        }
                        else
                        {
                            //TODO: Registrar Aplicaciones en cobranzas
                        }
                    }

                }
                #endregion



                return oMovimientoDeCliente.MovimientoContable;

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Determina el detalle del movimiento contable a grabar y evalua que coincida el Debe con el Haber
        /// </summary>
        /// <param name="oValor"></param>
        /// <param name="oValorVerscom"></param>
        /// <param name="oCajaYBanco"></param>
        /// <param name="strReimputacionesAReasignar"></param>
        /// <returns></returns>
        private static GESI.CORE.BO.Verscom2k.DetalleMovimientoContable DevolverDetalleMovimientoContable(GESI.CORE.BO.Verscom2k.MovimientoDeValor oValor, GESI.GESI.BO.Valor oValorVerscom,GESI.GESI.BO.CajaYBanco oCajaYBanco,String strReimputacionesAReasignar, int mointSubdiarioID,int mointTipoOperacionID,GESI.GESI.BO.ListaReferenciasContables lstReferenciasContables)
        {
            try
            {

                GESI.CORE.BO.Verscom2k.DetalleMovimientoContable oDetalleMovimientoContable = new GESI.CORE.BO.Verscom2k.DetalleMovimientoContable();

                if(oValor != null)
                {
                    if(oValorVerscom != null)
                    {
                        if(oValorVerscom.ReferenciaContableID != null)
                        {
                            if(oValorVerscom.ReferenciaContableID.Length > 0) // Chequear si existe la ref contable
                            {
                                List<GESI.GESI.BO.ReferenciaContable> lstReferenciaContableABuscar = lstReferenciasContables.Where(x => x.ReferenciaContableID == oValorVerscom.ReferenciaContableID).ToList();
                                if (lstReferenciaContableABuscar.Count > 0)
                                {
                                    oDetalleMovimientoContable = CargarDetalleMovimientoContable("D", oValorVerscom.ReferenciaContableID, oValor.Importe, mointSubdiarioID, mointTipoOperacionID);
                                }
                                else
                                {
                                    if (oCajaYBanco != null)
                                    {
                                        if (oCajaYBanco.ReferenciaContableID != null)
                                        {
                                            if (oCajaYBanco.ReferenciaContableID.Length > 0)
                                            {
                                                lstReferenciaContableABuscar = lstReferenciasContables.Where(x => x.ReferenciaContableID == oCajaYBanco.ReferenciaContableID).ToList();
                                                if (lstReferenciaContableABuscar.Count > 0)
                                                {
                                                    oDetalleMovimientoContable = CargarDetalleMovimientoContable("D", oCajaYBanco.ReferenciaContableID, oValor.Importe, mointSubdiarioID, mointTipoOperacionID);
                                                }
                                                else
                                                {
                                                    oDetalleMovimientoContable = CargarDetalleMovimientoContable("D", strReimputacionesAReasignar, oValor.Importe, mointSubdiarioID, mointTipoOperacionID);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            oDetalleMovimientoContable = CargarDetalleMovimientoContable("D", strReimputacionesAReasignar, oValor.Importe, mointSubdiarioID, mointTipoOperacionID);
                                        }
                                    }
                                    else
                                    {
                                        oDetalleMovimientoContable = CargarDetalleMovimientoContable("D", strReimputacionesAReasignar, oValor.Importe, mointSubdiarioID, mointTipoOperacionID);
                                    }
                                }
                             }
                            else
                            {
                                if(oCajaYBanco != null)
                                {
                                    if(oCajaYBanco.ReferenciaContableID != null)
                                    {
                                        if (oCajaYBanco.ReferenciaContableID.Length > 0)
                                        {
                                            oDetalleMovimientoContable = CargarDetalleMovimientoContable("D", oCajaYBanco.ReferenciaContableID, oValor.Importe, mointSubdiarioID, mointTipoOperacionID);
                                       }
                                    }
                                    else
                                    {
                                        oDetalleMovimientoContable = CargarDetalleMovimientoContable("D", strReimputacionesAReasignar, oValor.Importe, mointSubdiarioID, mointTipoOperacionID);
                                     }
                                    }
                                else
                                {
                                    oDetalleMovimientoContable = CargarDetalleMovimientoContable("D", strReimputacionesAReasignar, oValor.Importe, mointSubdiarioID, mointTipoOperacionID);
                                }
                            }
                        }
                        else
                        {
                            if(oCajaYBanco != null)
                            {
                                if(oCajaYBanco.ReferenciaContableID != null)
                                {
                                    if (oCajaYBanco.ReferenciaContableID.Length > 0)
                                    {
                                        List<GESI.GESI.BO.ReferenciaContable> lstRefContableAEncontrar = lstReferenciasContables.Where(x => x.ReferenciaContableID == oCajaYBanco.ReferenciaContableID).ToList();
                                        if(lstRefContableAEncontrar.Count > 0)
                                        {
                                            oDetalleMovimientoContable = CargarDetalleMovimientoContable("D", oCajaYBanco.ReferenciaContableID, oValor.Importe, mointSubdiarioID, mointTipoOperacionID);
                                        }
                                        else
                                        {
                                            oDetalleMovimientoContable = CargarDetalleMovimientoContable("D", strReimputacionesAReasignar, oValor.Importe, mointSubdiarioID, mointTipoOperacionID);
                                        }
                                    }
                                    else
                                    {
                                        oDetalleMovimientoContable = CargarDetalleMovimientoContable("D", strReimputacionesAReasignar, oValor.Importe, mointSubdiarioID, mointTipoOperacionID);
                                    }
                                }
                                else
                                {
                                    oDetalleMovimientoContable = CargarDetalleMovimientoContable("D", strReimputacionesAReasignar, oValor.Importe, mointSubdiarioID, mointTipoOperacionID);
                                  }
                            }
                        }
                    }
                }
                return oDetalleMovimientoContable;

            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Carga el detalle del movimiento contable
        /// </summary>
        /// <param name="DoH"></param>
        /// <param name="strReferenciaContableID"></param>
        /// <param name="Importe"></param>
        /// <returns></returns>
        private static GESI.CORE.BO.Verscom2k.DetalleMovimientoContable CargarDetalleMovimientoContable(String DoH,String strReferenciaContableID,decimal Importe, int mointSubdiarioID, int mointTipoOperacionID)
        {
            try
            {
                GESI.CORE.BO.Verscom2k.DetalleMovimientoContable oDetallMovimientoContable = new GESI.CORE.BO.Verscom2k.DetalleMovimientoContable();
                oDetallMovimientoContable.EmpresaID = _SessionMgr.EmpresaID;
                oDetallMovimientoContable.SubDiarioID = mointSubdiarioID;
                oDetallMovimientoContable.ReferenciaContableID = strReferenciaContableID;
                oDetallMovimientoContable.Importe = Importe;
                oDetallMovimientoContable.OperacionID = mointTipoOperacionID;
                oDetallMovimientoContable.EjercicioID = moEjercicios[0].EjercicioID;
                oDetallMovimientoContable.DoH = DoH;

                return oDetallMovimientoContable;

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Devuelve la relacion de cobro del cliente
        /// </summary>
        /// <param name="oComprobanteAAplicar"></param>
        /// <param name="oCobro"></param>
        /// <param name="oComprobante"></param>
        /// <param name="oMovimientoDeCliente"></param>
        /// <returns></returns>
        private static GESI.CORE.BO.Verscom2k.RelacionCobroCliente DevolverRelacionCobroCliente(ComprobanteAplicado oComprobanteAAplicar, Cobro oCobro, GESI.CORE.BO.Verscom2k.Comprobante oComprobante, GESI.CORE.BO.Verscom2k.MovimientoDeCliente oMovimientoDeCliente)
        {
            try
            {
                GESI.CORE.BO.Verscom2k.RelacionCobroCliente oRelacionCobroCliente = null;
                decimal decImporteAAplicar = oComprobanteAAplicar.ImporteAplicado;
                if(oComprobanteAAplicar.ImporteAplicado <= oCobro.TotalCobrado)
                {
                    GESI.GESI.BO.VencimientoDeCliente oVencimiento = GESI.GESI.BLL.VencimientosDeClientesMgr.GetItem(_SessionMgr.EmpresaID, oComprobanteAAplicar.ComprobanteID, oComprobanteAAplicar.Serie, oComprobanteAAplicar.PuntoDeVentaID, oComprobanteAAplicar.Numero, oComprobanteAAplicar.Vto_ComprobanteID);

                    if (oVencimiento != null)
                    {
                        if (oVencimiento.SaldoPendiente > 0)
                        {
                            if (oComprobanteAAplicar.ImporteAplicado > oVencimiento.SaldoPendiente)
                            {
                                   mostrMotivoNoImportar = mostrMotivoNoImportar + "Incoherencia en el importe aplicado con el total del vencimiento Importe a Aplicar: "+oComprobanteAAplicar.ImporteAplicado+" Saldo Pendiente:"+oVencimiento.SaldoPendiente+"\n";                               
                            }
                            else
                            {
                                oRelacionCobroCliente = new GESI.CORE.BO.Verscom2k.RelacionCobroCliente();
                                oRelacionCobroCliente.EmpresaID = _SessionMgr.EmpresaID;
                                oRelacionCobroCliente.ComprobanteID = oComprobante.ComprobanteID;
                                oRelacionCobroCliente.Importe = decImporteAAplicar;
                                oRelacionCobroCliente.PuntoDeVentaID = oMovimientoDeCliente.PuntoDeVentaID;
                                oRelacionCobroCliente.Serie = oMovimientoDeCliente.Serie;
                                oRelacionCobroCliente.Fecha = oMovimientoDeCliente.Fecha;
                                oRelacionCobroCliente.VencimientoID = oComprobanteAAplicar.Vto_ComprobanteID;
                                oRelacionCobroCliente.Vto_Numero = oVencimiento.Numero;
                                oRelacionCobroCliente.Vto_PuntoDeVentaID = oVencimiento.PuntoDeVentaID;
                                oRelacionCobroCliente.Vto_ComprobanteID = oVencimiento.ComprobanteID;
                                oRelacionCobroCliente.Vto_Serie = oVencimiento.Serie;
                                modecSaldoPendienteCobranza = modecSaldoPendienteCobranza - decImporteAAplicar;
                            }
                        }
                        return oRelacionCobroCliente;
                    }
                    else 
                    { 
                        mostrMotivoNoImportar = mostrMotivoNoImportar + "No se encontro el vencimiento del comprobante: "+oComprobanteAAplicar.Numero+" en la base de datos \n";
                        return oRelacionCobroCliente;
                    }
                }
                else
                {
                    mostrMotivoNoImportar = mostrMotivoNoImportar + "El importe a Aplicar debe ser menor al total de la Cobranza. ComprobanteID:" + oComprobanteAAplicar.ComprobanteID + "|" + oComprobanteAAplicar.Numero + " \n";
                    return oRelacionCobroCliente;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Setea las variables globales a utilizar ya sea Ejercicio , Subdiario, TipoOperacionID
        /// </summary>
        /// <param name="oFecha"></param>
        /// <param name="oComprobante"></param>
        private static void SetearVariablesGlobalesAUtilizar(DateTime oFecha,GESI.CORE.BO.Verscom2k.Comprobante oComprobante, GESI.CORE.BO.ListaConfiguracionesBase lstConfiguraciones)
        {
            #region Ejercicio
            moEjercicios = GESI.GESI.DAL.TablasGeneralesGESIDB.GetListEjercicios(oFecha, _SessionMgr.EmpresaID);
            
            #endregion

            #region Subdiario y TipoOperacion
            GESI.CORE.BLL.ConfiguracionesBaseMgr.SessionManager = _SessionMgr;            
            #endregion

        }

        /// <summary>
        /// Devuelve el objeto del request
        /// </summary>
        /// <param name="Success"></param>
        /// <param name="intCodigoError"></param>
        /// <param name="strMensajeError"></param>
        /// <returns></returns>
        private static Request DevolverObjetoRequest(bool Success, int intCodigoError, String strMensajeError,int CobroID)
        {
            try
            {
                Request objRequest = new Request();
                objRequest.Error = new Error();
                objRequest.Success = false;
                objRequest.Response = new Response();
                objRequest.Error.Code = intCodigoError;
                objRequest.Error.Message = strMensajeError;
                objRequest.Response.Data = "Codigo de Pago: " + CobroID;
                    

                return objRequest;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

      
        /// <summary>
        /// Setea el proximo numero de comprobante 
        /// </summary>
        /// <param name="oMovimientoDeCliente"></param>
        /// <param name="oComprobante"></param>
        /// <returns></returns>
        private static GESI.CORE.BO.Verscom2k.MovimientoDeCliente SetearProximoNumeroComprobante(GESI.CORE.BO.Verscom2k.MovimientoDeCliente oMovimientoDeCliente,GESI.CORE.BO.Verscom2k.Comprobante oComprobante)
        {
            try
            {
                int intProximoNumero = GESI.CORE.DAL.Verscom2k.MovimientosDeClientesDB.UltimoComprobante(_SessionMgr.EmpresaID, oComprobante.ComprobanteID, oComprobante.Serie, oComprobante.PuntoDeVentaID) + 1;

                oMovimientoDeCliente.Numero = intProximoNumero;

                if(oMovimientoDeCliente.MovimientosDeValores.Count > 0)
                {
                    foreach(GESI.CORE.BO.Verscom2k.MovimientoDeValor oMovimientoDeValor in oMovimientoDeCliente.MovimientosDeValores)
                    {
                        oMovimientoDeValor.Numero = intProximoNumero;
                    }
                }

                if(oMovimientoDeCliente.MovimientoContable != null)
                {
                    oMovimientoDeCliente.MovimientoContable.Numero = intProximoNumero;                   
                }

                if(oMovimientoDeCliente.RelacionesCobrosClientes.Count > 0)
                {
                    foreach(GESI.CORE.BO.Verscom2k.RelacionCobroCliente oRelacionCobroCliente in oMovimientoDeCliente.RelacionesCobrosClientes)
                    {
                        oRelacionCobroCliente.Numero = intProximoNumero;
                    }
                }

                return oMovimientoDeCliente;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Valida los movimientos de valores , Debe y Haber en el movimiento Contable
        /// </summary>
        /// <param name="oMovimientoDeCliente"></param>
        /// <returns></returns>
        private static bool ValidarObjetoMovimientoDeCliente(GESI.CORE.BO.Verscom2k.MovimientoDeCliente oMovimientoDeCliente)
        {
            try
            {
                bool blCondicionesImportar = true;
                decimal decImporteTotalMP = 0;
                decimal decDebe = 0;
                decimal decHaber = 0;
                GESI.GESI.BLL.MovimientosDeValoresMgr.SessionManager = _SessionMgr;

                if(oMovimientoDeCliente.MovimientosDeValores.Count > 0)
                {
                    foreach(GESI.CORE.BO.Verscom2k.MovimientoDeValor oMovimientoDeValor in oMovimientoDeCliente.MovimientosDeValores)
                    {
                        decImporteTotalMP = decImporteTotalMP + oMovimientoDeValor.Importe;
                        if (oMovimientoDeValor.NumeroDeValor != null)
                            if (oMovimientoDeValor.NumeroDeValor.Length > 0)
                            {
                                GESI.GESI.BO.MovimientoDeValor oMovimientoEnCartera = GESI.GESI.BLL.MovimientosDeValoresMgr.GetItem(oMovimientoDeValor.EmpresaID, oMovimientoDeValor.CajaBancoID, oMovimientoDeValor.ValorID, oMovimientoDeValor.NumeroDeValor);
                                if (oMovimientoEnCartera != null)
                                {
                                    blCondicionesImportar = false;
                                    mostrMotivoNoImportar = mostrMotivoNoImportar + "El valor " + oMovimientoEnCartera.NumeroDeValor + " ya esta en cartera \n";
                                }
                            }
                    }
                }

                if(decImporteTotalMP != oMovimientoDeCliente.Total)
                {
                    blCondicionesImportar = false;
                    mostrMotivoNoImportar = mostrMotivoNoImportar + " No coincide el total de Medios de Pago con el total del Comprobante \n";
                }
                else
                {
                    if(oMovimientoDeCliente.MovimientoContable != null)
                    {
                        if(oMovimientoDeCliente.MovimientoContable.Detalles.Count == 0)
                        {
                            blCondicionesImportar = false;
                            mostrMotivoNoImportar = mostrMotivoNoImportar + "No contiene detalle de movimientos contables \n";
                        }
                        else
                        {
                            foreach(GESI.CORE.BO.Verscom2k.DetalleMovimientoContable oDetalleMovimientoContable in oMovimientoDeCliente.MovimientoContable.Detalles)
                            {
                                if(oDetalleMovimientoContable.DoH.Equals("D"))
                                {
                                    decDebe = decDebe + oDetalleMovimientoContable.Importe;
                                }
                                else
                                {
                                    decHaber = decHaber + oDetalleMovimientoContable.Importe;
                                }
                            }

                            if(decDebe != decHaber)
                            {
                                blCondicionesImportar = false;
                                mostrMotivoNoImportar = mostrMotivoNoImportar + "No coincide la suma del debe y haber \n";
                            }
                        }
                    }
                    else
                    {
                        blCondicionesImportar = false;
                        mostrMotivoNoImportar = mostrMotivoNoImportar + "No contiene movimientos contables \n";

                    }
                }

                return blCondicionesImportar;


            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Convierte el objeto GESI.CORE.Verscom2k a GESI.GESI
        /// </summary>
        /// <param name="oMovimientoCliente"></param>
        /// <returns></returns>
        private static GESI.GESI.BO.MovimientoDeCliente ConvertirAGesiMovimientoDeCliente(GESI.CORE.BO.Verscom2k.MovimientoDeCliente oMovimientoCliente)
        {
            try
            {
                GESI.GESI.BO.MovimientoDeCliente objMovimientoCliente = new GESI.GESI.BO.MovimientoDeCliente();

                #region Datos Generales
                objMovimientoCliente.EmpresaID = oMovimientoCliente.EmpresaID;
                objMovimientoCliente.ClienteID = oMovimientoCliente.ClienteID;
                objMovimientoCliente.RazonSocial = oMovimientoCliente.RazonSocial;
                objMovimientoCliente.Fecha = oMovimientoCliente.Fecha;
                objMovimientoCliente.TipoOperacionID = oMovimientoCliente.TipoOperacionID;
                objMovimientoCliente.Total = oMovimientoCliente.Total;
                objMovimientoCliente.UsuarioID = oMovimientoCliente.UsuarioID;
                objMovimientoCliente.ComprobanteID = oMovimientoCliente.ComprobanteID;
                objMovimientoCliente.Serie = oMovimientoCliente.Serie;
                objMovimientoCliente.PuntoDeVentaID = oMovimientoCliente.PuntoDeVentaID;
                objMovimientoCliente.Numero = oMovimientoCliente.Numero;
                objMovimientoCliente.DivisaID = oMovimientoCliente.DivisaID;
                objMovimientoCliente.Notas = oMovimientoCliente.Notas;
                objMovimientoCliente.TipoDeDocumentoID = oMovimientoCliente.TipoDeDocumentoID;
                objMovimientoCliente.NumeroDeDocumento = oMovimientoCliente.NumeroDeDocumento;
                objMovimientoCliente.Notas2 = oMovimientoCliente.Notas2;
                objMovimientoCliente.SubTipo = oMovimientoCliente.SubTipo;
                objMovimientoCliente.SaldoPendiente = oMovimientoCliente.SaldoPendiente;
                objMovimientoCliente.Pendiente = oMovimientoCliente.Pendiente;
                objMovimientoCliente.CodigoImportacion = oMovimientoCliente.CodigoImportacion;
                objMovimientoCliente.MovimientosDeValores = new GESI.GESI.BO.ListaMovimientosDeValores();
                objMovimientoCliente.MovimientoContable = new GESI.GESI.BO.MovimientoContable();
                objMovimientoCliente.MovimientoContable.Detalles = new GESI.GESI.BO.ListaDetallesMovimientosContables();
                objMovimientoCliente.RelacionesCobrosClientes = new GESI.GESI.BO.ListaRelacionesCobrosClientes();
                #endregion

                #region Movimiento de Valores
                foreach (GESI.CORE.BO.Verscom2k.MovimientoDeValor oMovimientoDeValor in oMovimientoCliente.MovimientosDeValores)
                {
                    objMovimientoCliente.MovimientosDeValores.Add(
                        new GESI.GESI.BO.MovimientoDeValor
                        {
                            EmpresaID = oMovimientoDeValor.EmpresaID,
                            ComprobanteID = oMovimientoDeValor.ComprobanteID,
                            Serie = oMovimientoDeValor.Serie,
                            PuntoDeVentaID = oMovimientoDeValor.PuntoDeVentaID,
                            PoC = oMovimientoDeValor.PoC,
                            PoCID = oMovimientoDeValor.PoCID,
                            Numero = oMovimientoDeValor.Numero,
                            NumeroDeValor = oMovimientoDeValor.NumeroDeValor,
                            CajaBancoID = oMovimientoDeValor.CajaBancoID,
                            EmisorOBeneficiario = oMovimientoDeValor.EmisorOBeneficiario,
                            FechaDC = oMovimientoDeValor.FechaDC,
                            Fecha = oMovimientoDeValor.Fecha,
                            TipoMovimiento = oMovimientoDeValor.TipoMovimiento,
                            Estado = oMovimientoDeValor.Estado,
                            ValorID = oMovimientoDeValor.ValorID,
                            Importe = oMovimientoDeValor.Importe,
                            ItemID = 0,
                            BancoID = oMovimientoDeValor.BancoID,
                            DescripcionBanco = oMovimientoDeValor.DescripcionBanco

                        }
                        );
                }
                #endregion

                #region Movimiento Contable
                objMovimientoCliente.MovimientoContable.EmpresaID = oMovimientoCliente.MovimientoContable.EmpresaID;
                objMovimientoCliente.MovimientoContable.EjercicioID = oMovimientoCliente.MovimientoContable.EjercicioID;
                objMovimientoCliente.MovimientoContable.SubDiarioID = oMovimientoCliente.MovimientoContable.SubDiarioID;
                objMovimientoCliente.MovimientoContable.ComprobanteID = oMovimientoCliente.MovimientoContable.ComprobanteID;
                objMovimientoCliente.MovimientoContable.Fecha = oMovimientoCliente.MovimientoContable.Fecha;
                objMovimientoCliente.MovimientoContable.Numero = oMovimientoCliente.MovimientoContable.Numero;
                objMovimientoCliente.MovimientoContable.PoC = oMovimientoCliente.MovimientoContable.PoC;
                objMovimientoCliente.MovimientoContable.PoCID = oMovimientoCliente.MovimientoContable.PoCID;
                objMovimientoCliente.MovimientoContable.PuntoDeVentaID = oMovimientoCliente.MovimientoContable.PuntoDeVentaID;
                objMovimientoCliente.MovimientoContable.Serie = oMovimientoCliente.MovimientoContable.Serie;
                objMovimientoCliente.MovimientoContable.AsientoID = oMovimientoCliente.MovimientoContable.AsientoID;
                objMovimientoCliente.MovimientoContable.Descripcion = oMovimientoCliente.MovimientoContable.Descripcion;

                foreach(GESI.CORE.BO.Verscom2k.DetalleMovimientoContable oDetallMovimientoContable in oMovimientoCliente.MovimientoContable.Detalles)
                {
                    objMovimientoCliente.MovimientoContable.Detalles.Add(
                        new GESI.GESI.BO.DetalleMovimientoContable
                        {
                        EmpresaID = oDetallMovimientoContable.EmpresaID,
                        SubDiarioID = oDetallMovimientoContable.SubDiarioID,
                        ReferenciaContableID = oDetallMovimientoContable.ReferenciaContableID,
                        Importe = oDetallMovimientoContable.Importe,                       
                        EjercicioID = oDetallMovimientoContable.EjercicioID,
                        DoH = oDetallMovimientoContable.DoH,
                        Comentario = ""
                        });
                        }

                #endregion

                #region Relaciones Cobros Clientes

                if(oMovimientoCliente.RelacionesCobrosClientes.Count > 0)
                {
                    foreach(GESI.CORE.BO.Verscom2k.RelacionCobroCliente oRelacion in oMovimientoCliente.RelacionesCobrosClientes)
                    {
                        objMovimientoCliente.RelacionesCobrosClientes.Add(
                            new GESI.GESI.BO.RelacionCobroCliente
                            {
                                EmpresaID = oRelacion.EmpresaID,
                                ComprobanteID = oRelacion.ComprobanteID,
                                PuntoDeVentaID = oRelacion.PuntoDeVentaID,
                                Serie = oRelacion.Serie,
                                Numero = oRelacion.Numero,
                                Vto_ComprobanteID = oRelacion.Vto_ComprobanteID,
                                Vto_Numero = oRelacion.Vto_Numero,
                                Vto_PuntoDeVentaID = oRelacion.Vto_PuntoDeVentaID,
                                Vto_Serie = oRelacion.Vto_Serie,
                                FechaVto = oRelacion.FechaVto,
                                Importe = oRelacion.Importe,
                                Fecha = oRelacion.Fecha,
                                VencimientoID = oRelacion.VencimientoID
                                
                                
                            }
                            );
                    }
                }

                #endregion


                return objMovimientoCliente;

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// Crea la coleccion de Stock de Valores
        /// </summary>
        /// <param name="oMovimientoDeCliente"></param>
        /// <param name="lstValores"></param>
        /// <returns></returns>
        private static GESI.GESI.BO.ListaStockDeValores VerificarStockDeValores(GESI.GESI.BO.MovimientoDeCliente oMovimientoDeCliente, GESI.GESI.BO.ListaValores lstValores)
        {
            try
            {
                GESI.GESI.BO.ListaStockDeValores lstListaStockValores = new GESI.GESI.BO.ListaStockDeValores();
                if(oMovimientoDeCliente.MovimientosDeValores.Count > 0)
                {
                    foreach(GESI.GESI.BO.MovimientoDeValor oMovimientoDeValor in oMovimientoDeCliente.MovimientosDeValores)
                    {
                        List<GESI.GESI.BO.Valor> lstValorEncontrado = lstValores.Where(x => x.ValorID == oMovimientoDeValor.ValorID).ToList();

                        if(lstValorEncontrado.Count > 0)
                        {
                            if(oMovimientoDeValor.EmisorOBeneficiario == null)
                            {
                                oMovimientoDeValor.EmisorOBeneficiario = "";
                            }
                           
                            if(oMovimientoDeValor.FechaDC == null)
                            {
                                oMovimientoDeValor.FechaDC = oMovimientoDeCliente.Fecha;
                            }

                            if (lstValorEncontrado[0].Tipo.Equals("T") || lstValorEncontrado[0].Tipo.Equals("C") || lstValorEncontrado[0].Tipo.Equals("B"))
                            {
                                lstListaStockValores.Add(
                                    new GESI.GESI.BO.StockDeValor
                                    {
                                        EmpresaID = oMovimientoDeValor.EmpresaID,
                                        CajaBancoID = oMovimientoDeValor.CajaBancoID,
                                        Importe = oMovimientoDeValor.Importe,
                                        NumeroDeValor = oMovimientoDeValor.NumeroDeValor,
                                        ValorID = oMovimientoDeValor.ValorID,
                                        EmisorOBeneficiario = oMovimientoDeValor.EmisorOBeneficiario,
                                        FechaDC = (DateTime)oMovimientoDeValor.FechaDC,
                                        DescripcionBanco = oMovimientoDeValor.DescripcionBanco,                                   
                                        
                                        
                                    }
                               );
                            }
                        }

                    }
                }
                return lstListaStockValores;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }


    }
}