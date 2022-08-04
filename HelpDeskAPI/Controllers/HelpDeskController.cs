using HelpDeskAPI.DAO.Interface;
using HelpDeskAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HelpDeskAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class HelpDeskController : ControllerBase
    {
        private readonly IHelpDesk _helpDeskDAO;
        public HelpDeskController(IHelpDesk helpDeskDAO) { _helpDeskDAO = helpDeskDAO; }

        [HttpGet, Route("ValidarEmpresaMobile")]
        public async Task<IActionResult> ValidarEmpresaMobile()
        {
            try
            {
                var retorno = await _helpDeskDAO.ValidarEmpresaMobileAsync(Request.Headers["nr_doc"].ToString());
                return Ok(retorno);
            }
            catch { return NotFound(); }
        }

        [HttpGet, Route("TerminaisMobile")]
        public async Task<IActionResult> TerminaisMobile()
        {
            try
            {
                var retorno = await _helpDeskDAO.TerminaisMobileAsync(Request.Headers["nr_doc"].ToString());
                return Ok(retorno);
            }
            catch { return NotFound(); }
        }

        [HttpPost, Route("NovoTicket")]
        public async Task<IActionResult> NovoTicket(Ticket ticket)
        {
            try
            {
                await _helpDeskDAO.NovoTicketAsync(ticket);
                return Ok();
            }
            catch { return NotFound(); }
        }

        [HttpPost, Route("NovoHistorico")]
        public async Task<IActionResult> NovoHistorico(HistEvolucao historico)
        {
            try
            {
                await _helpDeskDAO.NovoHistoricoAsync(historico);
                return Ok();
            }
            catch { return NotFound(); }
        }

        [HttpPost, Route("EvoluirTicketCliente")]
        public async Task<IActionResult> EvoluirTicketCliente(HistEvolucao evolucao)
        {
            try
            {
                await _helpDeskDAO.EvoluirTicketClienteAsync(evolucao);
                return Ok();
            }
            catch { return NotFound(); }
        }

        [HttpGet, Route("TicketsERP")]
        public async Task<IActionResult> TicketsERP([FromQuery]string LoginCliente, [FromQuery]string Dt_etapa)
        {
            try
            {
                var lista = await _helpDeskDAO.TicketsERP(LoginCliente, Dt_etapa);
                return Ok(lista);
            }
            catch { return NotFound(); }
        }

        [HttpGet, Route("ValidarLogin")]
        public async Task<IActionResult> ValidarLogin([FromQuery]string login, 
                                                      [FromQuery]string senha, 
                                                      [FromQuery]string Cnpj)
        {
            try
            {
                var ret = await _helpDeskDAO.ValidarLoginAsync(login, senha, Cnpj);
                return Ok(ret);
            }
            catch { return NotFound(); }
        }

        [HttpGet, Route("ValidarOperador")]
        public async Task<IActionResult> ValidarOperador([FromQuery] string login,
                                                         [FromQuery] string senha)
        {
            try
            {
                if (await _helpDeskDAO.ValidarOperador(login, senha))
                    return Ok();
                else return NotFound();
            }
            catch { return NotFound(); }
        }

        [HttpGet, Route("BuscarEvolucao")]
        public async Task<IActionResult> BuscarEvolucao([FromQuery] string id_ticket)
        {
            try
            {
                var lista = await _helpDeskDAO.BuscarEvolucaoAsync(id_ticket);
                return Ok(lista);
            }
            catch { return NotFound(); }
        }

        [HttpGet, Route("BuscarAnexo")]
        public async Task<IActionResult> BuscarAnexo([FromQuery] string id_ticket,
                                                     [FromQuery] string id_evolucao)
        {
            try
            {
                var lista = await _helpDeskDAO.BuscarAnexoAsync(id_ticket, id_evolucao);
                return Ok(lista);
            }
            catch { return NotFound(); }
        }
        [HttpGet, Route("CalcularSerial")]
        public async Task<IActionResult> CalcularSerial([FromQuery]string cnpj_cliente,
                                                        [FromQuery]string dt_servidor,
                                                        [FromQuery]int diasvalidade)
        {
            try
            {
                var ret = await _helpDeskDAO.CalcularSerial(cnpj_cliente, dt_servidor, diasvalidade);
                return Ok(ret);
            }
            catch { return NotFound(); }
        }
        [HttpPost, Route("GravarRDC")]
        public async Task<IActionResult> GravarRDC(TRegistro_Cad_RDC rdc)
        {
            try
            {
                var ret = await _helpDeskDAO.GravarRDC(rdc);
                return Ok(ret);
            }
            catch { return NotFound(); }
        }

        [HttpGet, Route("VerificarVersaoRDC")]
        public async Task<IActionResult> VerificarVersaoRDC([FromQuery] string Id_rdc,
                                                            [FromQuery] string Nm_classe,
                                                            [FromQuery] string Modulo,
                                                            [FromQuery] string Ident,
                                                            [FromQuery] string St_rdc)
        {
            try
            {
                var ret = await _helpDeskDAO.VerificarVersaoRDC(Id_rdc, Nm_classe, Modulo, Ident, St_rdc);
                return Ok(ret);
            }
            catch { return NotFound(); }
        }

        [HttpGet, Route("DownloadRDC")]
        public async Task<IActionResult> DownloadRDC([FromQuery]string Id_rdc,
                                                     [FromQuery]string Nm_classe,
                                                     [FromQuery]string Modulo,
                                                     [FromQuery]string Ident,
                                                     [FromQuery]string St_rdc)
        {
            try
            {
                var lista = await _helpDeskDAO.DownloadRDC(Id_rdc, Nm_classe, Modulo, Ident, St_rdc);
                return Ok(lista);
            }
            catch { return NotFound(); }
        }

        [HttpGet, Route("BuscarRDC")]
        public async Task<IActionResult> BuscarRDC([FromQuery]string Id_rdc,
                                                   [FromQuery]string Ds_rdc,
                                                   [FromQuery]string Modulo,
                                                   [FromQuery]string St_rdc,
                                                   [FromQuery]bool BuscarRelClasse,
                                                   [FromQuery]bool NaoBuscarRelClasse)
        {
            try
            {
                var lista = await _helpDeskDAO.BuscarRDC(Id_rdc, Ds_rdc, Modulo, St_rdc, BuscarRelClasse, NaoBuscarRelClasse);
                return Ok(lista);
            }
            catch { return NotFound(); }
        }

        [HttpGet, Route("BuscarDetalheRDC")]
        public async Task<IActionResult> BuscarDetalheRDC([FromQuery] string Id_rdc)
        {
            try
            {
                var ret = await _helpDeskDAO.BuscarDetalheRDC(Id_rdc);
                return Ok(ret);
            }
            catch { return NotFound(); }
        }

        [HttpPost, Route("GravarParamClasse")]
        public async Task<IActionResult> GravarParamClasse(IEnumerable<TRegistro_Cad_ParamClasse> lParam)
        {
            try
            {
                var ret = await _helpDeskDAO.GravarParamClasseAsync(lParam);
                return Ok(ret);
            }
            catch { return NotFound(); }
        }

        [HttpPost, Route("HomologarRDC")]
        public async Task<IActionResult> HomologarRDC(TRegistro_Cad_RDC rdc)
        {
            try
            {
                var lista = await _helpDeskDAO.HomologarRDCAsync(rdc);
                return Ok(lista);
            }
            catch { return NotFound(); }
        }

        [HttpGet, Route("GetBoletosLBSystemAsync")]
        public async Task<IActionResult> GetBoletosLBSystemAsync([FromQuery] string doc)
        {
            try
            {
                var lista = await _helpDeskDAO.GetBoletosLBSystemAsync(doc);
                return Ok(lista);
            }
            catch { return NotFound(); }
        }
        [HttpGet, Route("GetPDFBoletoLBSystemAsync")]
        public async Task<IActionResult> GetPDFBoletoSicrediAsync([FromQuery] string cd_banco, 
                                                                  [FromQuery] string nosso_numero)
        {
            try
            {
                string pdf = await _helpDeskDAO.GetPDFBoletoSicrediAsync(cd_banco, nosso_numero);
                return Ok(pdf);
            }
            catch { return NotFound(); }
        }
    }
}
