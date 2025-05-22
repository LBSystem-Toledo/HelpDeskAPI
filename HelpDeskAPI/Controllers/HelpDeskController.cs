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

        [HttpGet, Route("GetBoletosAsync")]
        public async Task<IActionResult> GetBoletosLBSystemAsync([FromQuery] string doc)
        {
            try
            {
                var lista = await _helpDeskDAO.GetBoletosAsync(doc);
                return Ok(lista);
            }
            catch { return NotFound(); }
        }
    }
}
