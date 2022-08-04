using System;

namespace HelpDeskAPI.Models
{
    public class TokenSicredi
    {
        public string chaveTransacao { get; set; } = string.Empty;
        public DateTime? dataExpiracao { get; set; } = null;
    }

    public class BoletoSicrediPdf
    {
        public string arquivo { get; set; } = string.Empty;
    }
}
