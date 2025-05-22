using System;

namespace HelpDeskAPI.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string NrDoc { get; set; } = string.Empty;
        public string DocParceiro { get; set; } = string.Empty;
        public int Nr_seqlic { get; set; }
        public decimal Qt_diasvalidadelic { get; set; }
        public DateTime? Dt_licenca { get; set; } = null;
    }
}
