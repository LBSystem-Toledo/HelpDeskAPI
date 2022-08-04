namespace HelpDeskAPI.Models
{
    public class TChaveLic
    {
        private string _status = string.Empty;
        public string Status
        {
            get { return _status; }
            set
            {
                if (_status != value)
                    _status = value;
            }
        }
        private string _chave = string.Empty;
        public string Chave
        {
            get { return _chave; }
            set
            {
                if (_chave != value)
                    _chave = value;
            }
        }
        private double _qt_diasvalidade = double.MinValue;
        public double Qt_diasvalidade
        {
            get { return _qt_diasvalidade; }
            set
            {
                if (_qt_diasvalidade != value)
                    _qt_diasvalidade = value;
            }
        }
        private string _dt_licenca = string.Empty;
        public string Dt_licenca
        {
            get { return _dt_licenca; }
            set
            {
                if (_dt_licenca != value)
                    _dt_licenca = value;
            }
        }
        private int _nr_seqlic = int.MinValue;
        public int Nr_seqlic
        {
            get { return _nr_seqlic; }
            set
            {
                if (_nr_seqlic != value)
                    _nr_seqlic = value;
            }
        }
    }
}
