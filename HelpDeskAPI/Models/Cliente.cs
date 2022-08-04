using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDeskAPI.Models
{
    public class Cliente
    {
        public int Id_cliente { get; set; }
        public int Id_parceiro { get; set; }
        public string Cd_cidade { get; set; } = string.Empty;
        public int Id_categoria { get; set; }
        public string Tp_pessoa { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string Nm_razaosocial { get; set; } = string.Empty;
        public string Nm_fantasia { get; set; } = string.Empty;
        public string Ds_endereco { get; set; } = string.Empty;
        public string Ds_complemento { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public string Insc_estadual { get; set; } = string.Empty;
        public string Nm_contato { get; set; } = string.Empty;
        public string Fone { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Obs { get; set; } = string.Empty;
        public int Nr_seqlic { get; set; }
        public decimal Qt_diasvalidadelic { get; set; }
        public DateTime? Dt_licenca { get; set; }
        public string St_registro { get; set; } = string.Empty;
    }
}
