using HelpDeskAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDeskAPI.DAO.Interface
{
    public interface IHelpDesk
    {
        Task<bool> ValidarEmpresaMobileAsync(string Nr_doc);
        Task<int> TerminaisMobileAsync(string Nr_doc);
        Task NovoTicketAsync(Ticket ticket);
        Task NovoHistoricoAsync(HistEvolucao historico);
        Task EvoluirTicketClienteAsync(HistEvolucao evolucao);
        Task<IEnumerable<Ticket>> TicketsERP(string LoginCliente, string Dt_etapa);
        Task<string> ValidarLoginAsync(string login, string senha, string Cnpj);
        Task<bool> ValidarOperador(string login, string senha);
        Task<IEnumerable<Evolucao>> BuscarEvolucaoAsync(string id_ticket);
        Task<IEnumerable<Anexo>> BuscarAnexoAsync(string id_ticket, string id_evolucao);
        Task<TChaveLic> CalcularSerial(string cnpj_cliente,
                                       string dt_servidor,
                                       int diasvalidade);
        Task<string> GravarRDC(TRegistro_Cad_RDC rdc);
        Task<string> VerificarVersaoRDC(string Id_rdc,
                                        string Nm_classe,
                                        string Modulo,
                                        string Ident,
                                        string St_rdc);
        Task<TRegistro_Cad_RDC> DownloadRDC(string Id_rdc,
                                            string Nm_classe,
                                            string Modulo,
                                            string Ident,
                                            string St_rdc);
        Task<IEnumerable<TRegistro_Cad_RDC>> BuscarRDC(string Id_rdc,
                                                       string Ds_rdc,
                                                       string Modulo,
                                                       string St_rdc,
                                                       bool BuscarRelClasse,
                                                       bool NaoBuscarRelClasse);
        Task<TRegistro_Cad_RDC> BuscarDetalheRDC(string Id_rdc);
        Task<bool> GravarParamClasseAsync(IEnumerable<TRegistro_Cad_ParamClasse> lParamClasse);
        Task<string> HomologarRDCAsync(TRegistro_Cad_RDC rdc);
        Task<IEnumerable<Boleto>> GetBoletosLBSystemAsync(string doc);
        Task<string> GetPDFBoletoSicrediAsync(string cd_banco, string nosso_numero);
    }
}
