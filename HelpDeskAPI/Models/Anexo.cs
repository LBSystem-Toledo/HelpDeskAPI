namespace HelpDeskAPI.Models
{
    public class Anexo
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
        private decimal? _id_anexo;
        public decimal? Id_anexo
        {
            get { return _id_anexo; }
            set
            {
                if (_id_anexo != value)
                    _id_anexo = value;
            }
        }
        private string _ds_anexo;
        public string Ds_anexo
        {
            get { return _ds_anexo; }
            set
            {
                if (_ds_anexo != value)
                    _ds_anexo = value;
            }
        }
        private byte[] _imagem;
        public byte[] Imagem
        {
            get { return _imagem; }
            set
            {
                if (_imagem != value)
                    _imagem = value;
            }
        }
        private string _tp_ext;
        public string Tp_ext
        {
            get { return _tp_ext; }
            set
            {
                if (_tp_ext != value)
                    _tp_ext = value;
            }
        }
    }
}
