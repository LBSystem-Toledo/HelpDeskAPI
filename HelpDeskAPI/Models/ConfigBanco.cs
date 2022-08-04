using System;

namespace HelpDeskAPI.Models
{
    public class ConfigBanco
    {
        public decimal Id_config { get; set; } = decimal.Zero;
        public string CodigoCedente { get; set; } = string.Empty;
        public string PostoCedente { get; set; } = string.Empty;
        public string Nr_Agencia { get; set; } = string.Empty;
        public string ChaveTransacaoSicredi { get; set; } = string.Empty;
        public DateTime? DT_ExpiracaoChave { get; set; } = null;
        public string Url_Autenticacao { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Url_API { get; set; } = string.Empty;
        public byte[] CertificadoPfx { get; set; }
        public string SenhaPfx { get; set; } = string.Empty;
        public string Client_id { get; set; } = string.Empty;
        public string Client_secret { get; set; } = string.Empty;
    }
}
