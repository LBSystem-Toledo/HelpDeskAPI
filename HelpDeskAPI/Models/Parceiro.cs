namespace HelpDeskAPI.Models
{
    public class Parceiro
    {
        public int Id_parceiro { get; set; }
        public string Cd_cidade { get; set; } = string.Empty;
        public string Tp_pessoa { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;

    }
}
