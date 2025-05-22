using System;

namespace HelpDeskAPI.Models
{
    public class Boleto
    {
        public DateTime Dt_emissao { get; set; }
        public DateTime Dt_vencto { get; set; }
        public decimal Vl_Atual { get; set; } = decimal.Zero;
        public string NossoNumero { get; set; } = string.Empty;
        public string Cd_banco { get; set; } = string.Empty;
        public string Pdf_boleto { get; set; } = string.Empty;
    }
}
