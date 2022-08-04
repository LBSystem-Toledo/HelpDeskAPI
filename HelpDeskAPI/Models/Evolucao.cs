using System;

namespace HelpDeskAPI.Models
{
    public class Evolucao
    {
        private decimal? _id_evolucao;
        public decimal? Id_evolucao
        {
            get { return _id_evolucao; }
            set
            {
                if (_id_evolucao != value)
                    _id_evolucao = value;
            }
        }
        private decimal? _id_ticket;
        public decimal? Id_ticket
        {
            get { return _id_ticket; }
            set
            {
                if (_id_ticket != value)
                    _id_ticket = value;
            }
        }
        private string _loginoperador;
        public string Loginoperador
        {
            get { return _loginoperador; }
            set
            {
                if (_loginoperador != value)
                    _loginoperador = value;
            }
        }
        private decimal? _id_etapa;
        public decimal? Id_etapa
        {
            get { return _id_etapa; }
            set
            {
                if (_id_etapa != value)
                    _id_etapa = value;
            }
        }
        private string _ds_etapa;
        public string Ds_etapa
        {
            get { return _ds_etapa; }
            set
            {
                if (_ds_etapa != value)
                    _ds_etapa = value;
            }
        }
        private DateTime? _dt_inietapa;
        public DateTime? Dt_inietapa
        {
            get { return _dt_inietapa; }
            set
            {
                if (_dt_inietapa != value)
                    _dt_inietapa = value;
            }
        }
        private DateTime? _dt_finetapa;
        public DateTime? Dt_finetapa
        {
            get { return _dt_finetapa; }
            set
            {
                if (_dt_finetapa != value)
                    _dt_finetapa = value;
            }
        }
        public string LoginHistorico { get; set; } = string.Empty;
        public string Ds_historico { get; set; } = string.Empty;
        public bool Concluir { get; set; } = false;
        public string ScoreAvaliacao { get; set; } = string.Empty;
        public string ObsAvaliacao { get; set; } = string.Empty;
        public string Logincliente { get; set; } = string.Empty;
        public string Id_cliente { get; set; } = string.Empty;
    }
}
