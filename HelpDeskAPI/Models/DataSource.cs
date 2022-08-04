using System;
using System.Collections.Generic;

namespace HelpDeskAPI.Models
{
    public class TRegistro_Cad_DataSource
    {
        public string ID_DataSource { get; set; } = string.Empty;
        public string DS_DataSource { get; set; } = string.Empty;
        public string DS_SQL { get; set; } = string.Empty;
        private DateTime? _DT_DataSource = null;
        public DateTime? DT_DataSource
        {
            get { return _DT_DataSource; }
            set
            {
                _DT_DataSource = value;
                _DT_DataSourceString = value.ToString();
            }
        }
        private string _DT_DataSourceString = string.Empty;
        public string DT_DataSourceString
        {
            get
            {
                try
                {
                    return Convert.ToDateTime(_DT_DataSourceString).ToString("dd/MM/yyyy");
                }
                catch
                { return string.Empty; }
            }
            set
            {
                _DT_DataSourceString = value;
                try
                { _DT_DataSource = Convert.ToDateTime(value); }
                catch
                { _DT_DataSource = null; }
            }
        }

        public List<TRegistro_Cad_ParamClasse> lCad_ParamClasse { get; set; } = new List<TRegistro_Cad_ParamClasse>();
    }
}
