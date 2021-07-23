using System;

namespace MoedaService.Domain
{
    public class DadosMoedaCotacao
    {
        public string ID_MOEDA { get; set; }
        public DateTime DATA_REF { get; set; }

        public float VL_COTACAO { get; set; }
    }
}
