using System.Collections.Generic;

namespace HelpDeskAPI.Models
{
    public class TRegistro_Cad_RDC
    {
        public string ID_RDC { get; set; } = string.Empty;
        public string DS_RDC { get; set; } = string.Empty;
        public decimal Versao { get; set; } = decimal.Zero;
        public string ST_RDC { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public string Ident { get; set; } = string.Empty;
        public string NM_Classe { get; set; } = string.Empty;
        private byte[] code_report = null;
        public byte[] Code_Report
        {
            get { return code_report; }
            set { code_report = value; }
        }
        public List<TRegistro_Cad_DataSource> lCad_DataSource { get; set; } = new List<TRegistro_Cad_DataSource>();
    }
}
