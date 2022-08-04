using System;
using System.Collections.Generic;

namespace HelpDeskAPI.Models
{
    public class HistEvolucao
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
        private decimal? _id_historico;
        public decimal? Id_historico
        {
            get { return _id_historico; }
            set
            {
                if (_id_historico != value)
                    _id_historico = value;
            }
        }
        private string _logincliente;
        public string Logincliente
        {
            get { return _logincliente; }
            set
            {
                if (_logincliente != value)
                    _logincliente = value;
            }
        }
        private decimal? _id_cliente;
        public decimal? Id_cliente
        {
            get { return _id_cliente; }
            set
            {
                if (_id_cliente != value)
                    _id_cliente = value;
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
        private string _ds_historico;
        public string Ds_historico
        {
            get { return _ds_historico; }
            set
            {
                if (_ds_historico != value)
                    _ds_historico = value;
            }
        }
        private string _st_exibiratualizacao;
        public string St_exibiratualizacao
        {
            get { return _st_exibiratualizacao; }
            set
            {
                if (_st_exibiratualizacao != value)
                {
                    _st_exibiratualizacao = value;
                    _st_exibiratualizacaobool = value.Trim().ToUpper().Equals("S");
                }
            }
        }
        private bool _st_exibiratualizacaobool;
        public bool St_exibiratualizacaobool
        {
            get { return _st_exibiratualizacaobool; }
            set
            {
                if (_st_exibiratualizacaobool != value)
                {
                    _st_exibiratualizacaobool = value;
                    _st_exibiratualizacao = value ? "S" : "N";
                }
            }
        }
        private DateTime? _dt_historico;
        public DateTime? Dt_historico
        {
            get { return _dt_historico; }
            set
            {
                if (_dt_historico != value)
                    _dt_historico = value;
            }
        }
        private List<Anexo> _lAnexo;
        public List<Anexo> lAnexo
        {
            get { return _lAnexo; }
            set
            {
                if (_lAnexo != value)
                    _lAnexo = value;
            }
        }
        public bool Concluir { get; set; } = false;
        public string ScoreAvaliacao { get; set; } = string.Empty;
        public string ObsAvaliacao { get; set; } = string.Empty;
    }
}
