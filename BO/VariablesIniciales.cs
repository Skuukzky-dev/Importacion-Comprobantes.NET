using GESI.CORE.BO;
using GESI.GESI.BO;

namespace Importacion_Comprobantes.NET.BO
{
    /// <summary>
    /// Setea las variables Iniciales previo a la importacion de Comprobantes
    /// </summary>
    public class VariablesIniciales
    {
        #region Variables
        private GESI.GESI.BO.ListaCajasYBancos _lstCajasYBancos;
        private GESI.GESI.BO.ListaValores _lstValores;
        private GESI.CORE.BO.ListaConfiguracionesBase _lstConfiguraciones;
        private GESI.CORE.BO.Verscom2k.Comprobante _oComprobante;
        private int _SubdiarioID;
        private int _TipoDeOperacion;
        private GESI.GESI.BO.ListaReferenciasContables _lstReferenciasContables;
        private GESI.GESI.BO.ListaBancos _lstBancos;
        private GESI.CORE.BO.Verscom2k.ListaAlicuotasImpuestos _lstAlicuotasImpuestos;
        private GESI.CORE.BO.Verscom2k.ListaFormasDePago _lstFormaDePago;
        private GESI.CORE.BO.Empresa _oEmpresa;
        private GESI.CORE.BO.Verscom2k.ListaAlmacenes _lstAlmacenes;
        private List<GESI.ERP.Core.BO.cCanalDeVenta> _lstCanalesDeVenta;
        private GESI.GESI.BO.ListaEstadosComprobantesDeVentas _lstEstadosComprobantesVenta;
        private List<GESI.GESI.BO.CanalDeAtencion> _lstCanalesDeAtencion;
        private List<GESI.CORE.BO.Verscom2k.Comprobante> _lstComprobantes;

        public ListaCajasYBancos LstCajasYBancos { get => _lstCajasYBancos; set => _lstCajasYBancos = value; }
        public ListaValores LstValores { get => _lstValores; set => _lstValores = value; }
        public ListaConfiguracionesBase LstConfiguraciones { get => _lstConfiguraciones; set => _lstConfiguraciones = value; }
        public GESI.CORE.BO.Verscom2k.Comprobante Comprobante { get => _oComprobante; set => _oComprobante = value; }
        public int SubdiarioID { get => _SubdiarioID; set => _SubdiarioID = value; }
        public int TipoDeOperacion { get => _TipoDeOperacion; set => _TipoDeOperacion = value; }
        public ListaReferenciasContables LstReferenciasContables { get => _lstReferenciasContables; set => _lstReferenciasContables = value; }
        public ListaBancos LstBancos { get => _lstBancos; set => _lstBancos = value; }
        public GESI.CORE.BO.Verscom2k.ListaAlicuotasImpuestos LstAlicuotasImpuestos { get => _lstAlicuotasImpuestos; set => _lstAlicuotasImpuestos = value; }
        public GESI.CORE.BO.Verscom2k.ListaFormasDePago LstFormaDePago { get => _lstFormaDePago; set => _lstFormaDePago = value; }
        public Empresa OEmpresa { get => _oEmpresa; set => _oEmpresa = value; }
        public GESI.CORE.BO.Verscom2k.ListaAlmacenes LstAlmacenes { get => _lstAlmacenes; set => _lstAlmacenes = value; }
        public List<GESI.ERP.Core.BO.cCanalDeVenta> LstCanalesDeVenta { get => _lstCanalesDeVenta; set => _lstCanalesDeVenta = value; }
        public ListaEstadosComprobantesDeVentas LstEstadosComprobantesVenta { get => _lstEstadosComprobantesVenta; set => _lstEstadosComprobantesVenta = value; }
        public List<CanalDeAtencion> LstCanalesDeAtencion { get => _lstCanalesDeAtencion; set => _lstCanalesDeAtencion = value; }
        public List<GESI.CORE.BO.Verscom2k.Comprobante> LstComprobantes { get => _lstComprobantes; set => _lstComprobantes = value; }
        #endregion


    }
}
