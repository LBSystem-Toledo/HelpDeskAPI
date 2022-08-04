using System;
using System.Collections.Generic;

namespace HelpDeskAPI.Models
{
    public class Ticket
    {
        private decimal? _id_ticket = null;
        public decimal? Id_ticket
        {
            get { return _id_ticket; }
            set
            {
                if (_id_ticket != value)
                    _id_ticket = value;
            }
        }
        private string _logincliente = string.Empty;
        public string Logincliente
        {
            get { return _logincliente; }
            set
            {
                if (_logincliente != value)
                    _logincliente = value;
            }
        }
        private decimal? _id_cliente = null;
        public decimal? Id_cliente
        {
            get { return _id_cliente; }
            set
            {
                if (_id_cliente != value)
                    _id_cliente = value;
            }
        }
        private string _nm_cliente = string.Empty;
        public string Nm_cliente
        {
            get { return _nm_cliente; }
            set
            {
                if (_nm_cliente != value)
                    _nm_cliente = value;
            }
        }
        private string _st_prioridade = string.Empty;
        public string St_prioridade
        {
            get { return _st_prioridade; }
            set
            {
                if (_st_prioridade != value)
                    _st_prioridade = value;
            }
        }
        public string Prioridade
        {
            get
            {
                if (_st_prioridade.Trim().Equals("0"))
                    return "BAIXA";
                else if (_st_prioridade.Trim().Equals("1"))
                    return "MÉDIA";
                else if (_st_prioridade.Trim().Equals("2"))
                    return "ALTA";
                else return string.Empty;
            }
        }
        private string _ds_assunto = string.Empty;
        public string Ds_assunto
        {
            get { return _ds_assunto; }
            set
            {
                if (_ds_assunto != value)
                    _ds_assunto = value;
            }
        }
        private string _ds_historico = string.Empty;
        public string Ds_historico
        {
            get { return _ds_historico; }
            set
            {
                if (_ds_historico != value)
                    _ds_historico = value;
            }
        }
        private DateTime? _dt_abertura = null;
        public DateTime? Dt_abertura
        {
            get { return _dt_abertura; }
            set
            {
                if (_dt_abertura != value)
                    _dt_abertura = value;
            }
        }
        private DateTime? _dt_concluido = null;
        public DateTime? Dt_concluido
        {
            get { return _dt_concluido; }
            set
            {
                if (_dt_concluido != value)
                    _dt_concluido = value;
            }
        }
        private DateTime? _dt_encerrado = null;
        public DateTime? Dt_encerrado
        {
            get { return _dt_encerrado; }
            set
            {
                if (_dt_encerrado != value)
                    _dt_encerrado = value;
            }
        }
        private DateTime? _dt_etapaatual = null;
        public DateTime? Dt_etapaatual
        {
            get { return _dt_etapaatual; }
            set
            {
                if (_dt_etapaatual != value)
                    _dt_etapaatual = value;
            }
        }
        private string _st_registro = string.Empty;
        public string St_registro
        {
            get { return _st_registro; }
            set
            {
                if (_st_registro != value)
                    _st_registro = value;
            }
        }
        public string Status
        {
            get
            {
                if (_st_registro.Trim().ToUpper().Equals("A"))
                    return "ABERTO";
                else if (_st_registro.Trim().ToUpper().Equals("L"))
                    return "CONCLUIDO";
                else if (_st_registro.Trim().ToUpper().Equals("E"))
                    return "ENCERRADO";
                else if (_st_registro.Trim().ToUpper().Equals("C"))
                    return "CANCELADO";
                else return string.Empty;
            }
        }
        private string _ds_etapaatual;
        public string Ds_etapaatual
        {
            get { return _ds_etapaatual; }
            set
            {
                if (_ds_etapaatual != value)
                    _ds_etapaatual = value;
            }
        }
        private string _st_expirado = "N";
        public string St_expirado
        {
            get { return _st_expirado; }
            set
            {
                if (_st_expirado != value)
                {
                    _st_expirado = value;
                    _st_expiradobool = value.Trim().ToUpper().Equals("S");
                }
            }
        }
        private bool _st_expiradobool = false;
        public bool St_expiradobool
        {
            get { return _st_expiradobool; }
            set
            {
                if (_st_expiradobool != value)
                {
                    _st_expiradobool = value;
                    _st_expirado = value ? "S" : "N";
                }
            }
        }
        public bool Encerrado { get; set; } = false;
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
        private List<HistEvolucao> _lhist;
        public List<HistEvolucao> lHist
        {
            get { return _lhist; }
            set
            {
                if (_lhist != value)
                    _lhist = value;
            }
        }
    }
}
