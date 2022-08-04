using Dapper;
using HelpDeskAPI.DAO.Interface;
using HelpDeskAPI.Models;
using HelpDeskAPI.Utils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace HelpDeskAPI.DAO.Repository
{
    class BoletoPdfInter
    {
        public string pdf { get; set; } = string.Empty;
    }

    public class HelpDeskDAO: IHelpDesk
    {
        readonly IConfiguration _config;
        public HelpDeskDAO(IConfiguration config) { _config = config; }

        public async Task<bool> ValidarEmpresaMobileAsync(string Nr_doc)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select 1 from TB_CRM_Cliente a ")
                    .AppendLine("where isnull(a.st_registro, 'A') <> 'I' ")
                    .AppendLine("and isnull(a.mobile, 0) = 1 ")
                    .AppendLine("and dbo.FVALIDA_NUMEROS(a.cnpj) = '" + Nr_doc.SoNumero() + "'");
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.ExecuteScalarAsync<bool>(sql.ToString());
                    else return false;
                }
            }
            catch { return false; }
        }

        public async Task<int> TerminaisMobileAsync(string Nr_doc)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select qt_mobile from TB_CRM_Cliente a ")
                    .AppendLine("where isnull(a.st_registro, 'A') <> 'I' ")
                    .AppendLine("and isnull(a.mobile, 0) = 1 ")
                    .AppendLine("and dbo.FVALIDA_NUMEROS(a.cnpj) = '" + Nr_doc.SoNumero() + "'");
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.ExecuteScalarAsync<int>(sql.ToString());
                    else return 0;
                }
            }
            catch { return 0; }
        }

        public async Task NovoTicketAsync(Ticket ticket)
        {
            SqlTransaction t = null;
            try
            {
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        t = conexao._conexao.BeginTransaction(IsolationLevel.ReadCommitted);
                        decimal etapa = await conexao._conexao.ExecuteScalarAsync<decimal>("select top 1 a.id_etapa from tb_crm_etapa a where isnull(a.st_abertura, 'N') = 'S'", transaction: t);
                        //Gravar Ticket
                        DynamicParameters p = new DynamicParameters();
                        p.Add("@P_ID_TICKET", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                        p.Add("@P_LOGINCLIENTE", ticket.Logincliente, dbType: DbType.String, size: 20);
                        p.Add("@P_ID_CLIENTE", ticket.Id_cliente, dbType: DbType.Decimal);
                        p.Add("@P_ST_PRIORIDADE", ticket.St_prioridade, dbType: DbType.String, size: 1);
                        p.Add("@P_DS_ASSUNTO", ticket.Ds_assunto, dbType: DbType.String, size: 50);
                        if (ticket.Dt_abertura.HasValue)
                            p.Add("@P_DT_ABERTURA", ticket.Dt_abertura.Value, dbType: DbType.DateTime);
                        else p.Add("@P_DT_ABERTURA", null);
                        if (ticket.Dt_concluido.HasValue)
                            p.Add("@P_DT_CONCLUIDO", ticket.Dt_concluido.Value, dbType: DbType.DateTime);
                        else p.Add("@P_DT_CONCLUIDO", null);
                        if (ticket.Dt_encerrado.HasValue)
                            p.Add("@P_DT_ENCERRAMENTO", ticket.Dt_encerrado.Value, dbType: DbType.DateTime);
                        else p.Add("@P_DT_ENCERRAMENTO", null);
                        p.Add("@P_ST_REGISTRO", ticket.St_registro, dbType: DbType.String, size: 1);
                        p.Add("@P_SCOREAVALIACAO", null);
                        p.Add("@P_OBSAVALIACAO", null);
                        await conexao._conexao.ExecuteAsync("IA_CRM_TICKET", p, transaction: t, commandType: CommandType.StoredProcedure);
                        ticket.Id_ticket = p.Get<decimal>("@P_ID_TICKET");
                        //Gravar evolucao
                        p = new DynamicParameters();
                        p.Add("@P_ID_EVOLUCAO", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                        p.Add("@P_ID_TICKET", ticket.Id_ticket, dbType: DbType.Decimal);
                        p.Add("@P_LOGINOPERADOR", null);
                        p.Add("@P_ID_ETAPA", etapa, dbType: DbType.Decimal);
                        p.Add("@P_DT_INIETAPA", DateTime.Now, dbType: DbType.DateTime);
                        p.Add("@P_DT_FINETAPA", null);
                        await conexao._conexao.ExecuteAsync("IA_CRM_EVOLUCAOTICKET", p, transaction: t, commandType: CommandType.StoredProcedure);
                        decimal id_evolucao = p.Get<decimal>("@P_ID_EVOLUCAO");
                        //Gravar Historico Evolucao
                        p = new DynamicParameters();
                        p.Add("@P_ID_EVOLUCAO", id_evolucao, dbType: DbType.Decimal);
                        p.Add("@P_ID_TICKET", ticket.Id_ticket, dbType: DbType.Decimal);
                        p.Add("@P_ID_HISTORICO", null);
                        p.Add("@P_LOGINCLIENTE", ticket.Logincliente, dbType: DbType.String, size: 20);
                        p.Add("@P_ID_CLIENTE", ticket.Id_cliente, dbType: DbType.Decimal);
                        p.Add("@P_LOGINOPERADOR", null);
                        p.Add("@P_DS_HISTORICO", ticket.Ds_historico, dbType: DbType.String, size: 2048);
                        await conexao._conexao.ExecuteAsync("IA_CRM_HISTEVOLUCAO", p, transaction: t, commandType: CommandType.StoredProcedure);
                        //Anexos
                        if(ticket.lAnexo != null)
                            foreach(var v in ticket.lAnexo)
                            {
                                p = new DynamicParameters();
                                p.Add("@P_ID_EVOLUCAO", id_evolucao, dbType: DbType.Decimal);
                                p.Add("@P_ID_TICKET", ticket.Id_ticket, dbType: DbType.Decimal);
                                p.Add("@P_ID_ANEXO", null);
                                p.Add("@P_DS_ANEXO", v.Ds_anexo, dbType: DbType.String, size: 255);
                                p.Add("@P_IMAGEM", v.Imagem, dbType: DbType.Binary);
                                p.Add("@P_TP_EXT", v.Tp_ext, dbType: DbType.String, size: 10);
                                await conexao._conexao.ExecuteAsync("IA_CRM_ANEXOEVOLUCAO", p, transaction: t, commandType: CommandType.StoredProcedure);
                            };
                        t.Commit();
                    }
                }
            }
            catch { t.Rollback(); }
        }

        public async Task NovoHistoricoAsync(HistEvolucao historico)
        {
            SqlTransaction t = null;
            try
            {
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        t = conexao._conexao.BeginTransaction(IsolationLevel.ReadCommitted);
                        //Gravar historico
                        DynamicParameters p = new DynamicParameters();
                        p.Add("@P_ID_HISTORICO", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                        p.Add("@P_ID_EVOLUCAO", historico.Id_evolucao, dbType: DbType.Decimal);
                        p.Add("@P_ID_TICKET", historico.Id_ticket, dbType: DbType.Decimal);
                        p.Add("@P_LOGINCLIENTE", historico.Logincliente, dbType: DbType.String, size: 20);
                        p.Add("@P_ID_CLIENTE", Convert.ToInt32(historico.Id_cliente), dbType: DbType.Decimal);
                        p.Add("@P_LOGINOPERADOR", null);
                        p.Add("@P_DS_HISTORICO", historico.Ds_historico, dbType: DbType.String, size: 2048);
                        await conexao._conexao.ExecuteAsync("IA_CRM_HISTEVOLUCAO", p, transaction: t, commandType: CommandType.StoredProcedure);
                        //Anexos
                        if (historico.lAnexo != null)
                            foreach (var v in historico.lAnexo)
                            {
                                p = new DynamicParameters();
                                p.Add("@P_ID_EVOLUCAO", historico.Id_evolucao, dbType: DbType.Decimal);
                                p.Add("@P_ID_TICKET", historico.Id_ticket, dbType: DbType.Decimal);
                                p.Add("@P_ID_ANEXO", null);
                                p.Add("@P_DS_ANEXO", v.Ds_anexo, dbType: DbType.String, size: 255);
                                p.Add("@P_IMAGEM", v.Imagem, dbType: DbType.Binary);
                                p.Add("@P_TP_EXT", v.Tp_ext, dbType: DbType.String, size: 10);
                                await conexao._conexao.ExecuteAsync("IA_CRM_ANEXOEVOLUCAO", p, transaction: t, commandType: CommandType.StoredProcedure);
                            };
                        t.Commit();
                    }
                }
            }
            catch { t.Rollback(); }
        }

        public async Task EvoluirTicketClienteAsync(HistEvolucao evolucao)
        {
            SqlTransaction t = null;
            try
            {
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        t = conexao._conexao.BeginTransaction(IsolationLevel.ReadCommitted);
                        decimal etapa = await conexao._conexao.ExecuteScalarAsync<decimal>("select top 1 id_etapa from tb_crm_etapa where " + (evolucao.Concluir ? "st_concluir = 'S'" : "st_repcliente = 1"), transaction: t);
                        //Gravar Ticket
                        DynamicParameters p = new DynamicParameters();
                        p.Add("@P_ID_EVOLUCAO", dbType: DbType.Decimal, direction: ParameterDirection.Output);
                        p.Add("@P_ID_TICKET", evolucao.Id_ticket, dbType: DbType.Decimal);
                        p.Add("@P_LOGINOPERADOR", null);
                        p.Add("@P_ID_ETAPA", etapa);
                        p.Add("@P_DT_INIETAPA", DateTime.Now, dbType: DbType.DateTime);
                        if (evolucao.Concluir)
                            p.Add("@P_DT_FINETAPA", DateTime.Now, dbType: DbType.Date);
                        else p.Add("@P_DT_FINETAPA", null);
                        await conexao._conexao.ExecuteAsync("IA_CRM_EVOLUCAOTICKET", p, transaction: t, commandType: CommandType.StoredProcedure);
                        decimal id_evolucao = p.Get<decimal>("@P_ID_EVOLUCAO");
                        //Gravar Historico Evolucao
                        p = new DynamicParameters();
                        p.Add("@P_ID_EVOLUCAO", id_evolucao, dbType: DbType.Decimal);
                        p.Add("@P_ID_TICKET", evolucao.Id_ticket, dbType: DbType.Decimal);
                        p.Add("@P_ID_HISTORICO", null);
                        p.Add("@P_LOGINCLIENTE", evolucao.Logincliente, dbType: DbType.String, size: 20);
                        p.Add("@P_ID_CLIENTE", evolucao.Id_cliente, dbType: DbType.Decimal);
                        p.Add("@P_LOGINOPERADOR", null);
                        p.Add("@P_DS_HISTORICO", evolucao.Ds_historico, dbType: DbType.String, size: 2048);
                        await conexao._conexao.ExecuteAsync("IA_CRM_HISTEVOLUCAO", p, transaction: t, commandType: CommandType.StoredProcedure);
                        //Alterar status do ticket
                        if (evolucao.Concluir &&
                            !string.IsNullOrEmpty(evolucao.ScoreAvaliacao))
                        {
                            //Gravar avaliação
                            StringBuilder sql = new StringBuilder();
                            sql.AppendLine("update TB_CRM_Ticket set ScoreAvaliacao = @ScoreAvaliacao, ")
                                .AppendLine("ObsAvaliacao = @ObsAvaliacao, st_registro = 'L', ")
                                .AppendLine("dt_alt = getdate() where ID_Ticket = @ID_Ticket");
                            p = new DynamicParameters();
                            p.Add("@ScoreAvaliacao", evolucao.ScoreAvaliacao, dbType: DbType.String, size: 1);
                            p.Add("@ObsAvaliacao", evolucao.ObsAvaliacao, dbType: DbType.String, size: 1024);
                            p.Add("@ID_Ticket", evolucao.Id_ticket);
                            await conexao._conexao.ExecuteAsync(sql.ToString(), p, transaction: t, commandType: CommandType.Text);
                        }
                        else
                        {
                            StringBuilder sql = new StringBuilder();
                            sql.AppendLine("update TB_CRM_Ticket set st_registro = 'A', dt_alt = getdate() where ID_Ticket = @ID_Ticket");
                            p = new DynamicParameters();
                            p.Add("@ID_Ticket", evolucao.Id_ticket, dbType: DbType.Decimal);
                            await conexao._conexao.ExecuteAsync(sql.ToString(), p, transaction: t, commandType: CommandType.Text);
                        }
                        t.Commit();
                    }
                }
            }
            catch { t.Rollback(); }
        }

        public async Task<IEnumerable<Ticket>> TicketsERP(string LoginCliente, string Dt_etapa)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.id_ticket, b.DS_ETAPA as DS_EtapaAtual, case when b.ST_Encerrar = 'S' then 1 else 0 end as Encerrado, a.ds_assunto, a.dt_abertura, a.dt_etapaatual")
                    .AppendLine("from VTB_CRM_TICKET a ")
                    .AppendLine("left join TB_CRM_ETAPA b ")
                    .AppendLine("on a.Id_etapaatual = b.ID_ETAPA")
                    .AppendLine("where a.logincliente = '" + LoginCliente.Trim() + "'")
                    .AppendLine("and isnull(a.st_registro, 'A') IN('A', 'E')")
                    .AppendLine("and isnull(b.st_interna, 'N') <> 'S'");
                DateTime data;
                if (DateTime.TryParse(Dt_etapa, out data))
                    sql.AppendLine("and DATEADD(ms, -DATEPART(ms, a.dt_etapaatual), a.dt_etapaatual) > '" + data.ToString("yyyyMMdd HH:mm:ss") + "'");
                sql.AppendLine("order by a.id_ticket desc ");
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<Ticket>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }

        public async Task<string> ValidarLoginAsync(string login, string senha, string Cnpj)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.id_cliente from tb_crm_usercliente a join tb_crm_cliente b on a.id_cliente = b.id_cliente ")
                    .AppendLine("and dbo.FVALIDA_NUMEROS(b.cnpj) in(" + Cnpj + ") where logincliente = '" + login.Trim() + "' and senha = '" + senha.Trim() + "'");
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.ExecuteScalarAsync<string>(sql.ToString());
                    else return string.Empty;
                }
            }
            catch { return string.Empty; }
        }
        public async Task<bool> ValidarOperador(string login, string senha)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select 1 from tb_crm_operador ")
                    .AppendLine("where loginoperador = '" + login.Trim() + "' and senha = '" + senha.Trim() + "'");
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.ExecuteScalarAsync<bool>(sql.ToString());
                    else return false;
                }
            }
            catch { return false; }
        }

        public async Task<IEnumerable<Evolucao>> BuscarEvolucaoAsync(string id_ticket)
        {
            try
            {
                StringBuilder sql = new StringBuilder()
                .AppendLine("select a.id_evolucao, a.id_ticket, a.loginoperador, ")
                .AppendLine("a.id_etapa, b.ds_etapa, a.dt_inietapa, a.dt_finetapa, ")
                .AppendLine("Hist.LoginHistorico, Hist.DS_HISTORICO ")
                .AppendLine("from tb_crm_evolucaoticket a ")
                .AppendLine("inner join tb_crm_etapa b ")
                .AppendLine("on a.id_etapa = b.id_etapa ")
                .AppendLine("outer apply")
                .AppendLine("(")
                .AppendLine("	select top 1 isnull(x.LOGINCLIENTE, x.LOGINOPERADOR) as LoginHistorico, x.DS_HISTORICO ")
                .AppendLine("	from TB_CRM_HISTEVOLUCAO x ")
                .AppendLine("	where x.ID_TICKET = a.ID_TICKET ")
                .AppendLine("	and x.ID_EVOLUCAO = a.ID_EVOLUCAO ")
                .AppendLine("	order by x.DT_CAD asc ")
                .AppendLine(") as Hist ")
                .AppendLine("where a.id_ticket = " + id_ticket)
                .AppendLine("order by a.dt_inietapa desc ");
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<Evolucao>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }

        public async Task<IEnumerable<Anexo>> BuscarAnexoAsync(string id_ticket, string id_evolucao)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.id_evolucao, a.id_ticket, a.id_anexo, ");
                sql.AppendLine("a.ds_anexo, a.imagem, a.tp_ext ");
                sql.AppendLine("from tb_crm_AnexoEvolucao a ");
                sql.AppendLine("where a.id_ticket = " + id_ticket);
                sql.AppendLine("and a.id_evolucao = " + id_evolucao);
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<Anexo>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }

        public async Task<TChaveLic> CalcularSerial(string cnpj_cliente,
                                                    string dt_servidor,
                                                    int diasvalidade)
        {
            TChaveLic retorno = new TChaveLic();
            string msg = string.Empty;
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select id_cliente, isnull(qt_diasvalidadelic, 0) as qt_diasvalidadelic, dt_licenca, cnpj, st_registro ");
                sql.AppendLine("from tb_crm_cliente ");
                sql.AppendLine("where REPLACE(REPLACE(REPLACE(REPLACE(cnpj, ',',''), '.', ''), '/', ''), '-', '') in(" + cnpj_cliente.Trim() + ")");
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        var cliente = await conexao._conexao.QueryFirstOrDefaultAsync<Cliente>(sql.ToString());
                        if(cliente != null)
                        {
                            //Verificar financeiro em aberto
                            sql.Clear();
                            sql.AppendLine("select 1 from vtb_fin_parcela a ");
                            sql.AppendLine("inner join vtb_fin_clifor b ");
                            sql.AppendLine("on a.cd_clifor = b.cd_clifor ");
                            sql.AppendLine("inner join tb_fin_duplicata c ");
                            sql.AppendLine("on a.cd_empresa = c.cd_empresa ");
                            sql.AppendLine("and a.nr_lancto = c.nr_lancto ");
                            sql.AppendLine("where isnull(c.st_registro, 'A') <> 'C' ");
                            sql.AppendLine("and isnull(a.st_registro, 'A') in('A', 'P') ");
                            sql.AppendLine("and convert(datetime, floor(convert(decimal(30,10), dateadd(day, 5, a.dt_vencto)))) < convert(datetime, floor(convert(decimal(30,10), getdate()))) ");
                            sql.AppendLine("and REPLACE(REPLACE(REPLACE(REPLACE(case when b.tp_pessoa = 'F' then b.nr_cpf else b.nr_cgc end, ',',''), '.', ''), '/', ''), '-', '') = '" + cliente.Cnpj.SoNumero() + "'");
                            using (TConexao conexaoLB = new TConexao(_config.GetConnectionString("conexaoLB")))
                            {
                                if (await conexaoLB.OpenConnectionAsync())
                                    retorno.Status = await conexaoLB._conexao.ExecuteScalarAsync<string>(sql.ToString());
                                else retorno.Status = "8";
                            }
                            //Calcular chave de acesso
                            if (string.IsNullOrEmpty(retorno.Status))
                            {
                                if (diasvalidade > 0)
                                    retorno.Qt_diasvalidade = Convert.ToDouble(diasvalidade);
                                else
                                {
                                    retorno.Qt_diasvalidade = Convert.ToDouble(cliente.Qt_diasvalidadelic);
                                    if (retorno.Qt_diasvalidade.Equals(0))
                                        retorno.Qt_diasvalidade = 30;
                                }
                                if (!cliente.Dt_licenca.HasValue)
                                    retorno.Dt_licenca = dt_servidor;
                                else
                                {
                                    DateTime dt_lic = cliente.Dt_licenca.Value;
                                    if (dt_lic.AddDays(retorno.Qt_diasvalidade - 5).Date < DateTime.Now.Date)
                                        retorno.Dt_licenca = cliente.Dt_licenca.Value.AddDays(retorno.Qt_diasvalidade).ToString("dd/MM/yyyy");
                                    else retorno.Dt_licenca = cliente.Dt_licenca.Value.ToString("dd/MM/yyyy");
                                }
                                retorno.Nr_seqlic = new Random().Next(9999);
                                retorno.Chave = new Cryptography.Cryptography().GerarChaveAliance(cliente.Cnpj.SoNumero(),
                                                                                                  Convert.ToDouble(retorno.Nr_seqlic),
                                                                                                  new DateTime(int.Parse(retorno.Dt_licenca.Substring(6,4)), int.Parse(retorno.Dt_licenca.Substring(3, 2)), int.Parse(retorno.Dt_licenca.Substring(0, 2))),
                                                                                                  retorno.Qt_diasvalidade);
                                retorno.Status = "0";
                                DynamicParameters p = new DynamicParameters();
                                p.Add("@dt_licenca", new DateTime(int.Parse(retorno.Dt_licenca.Substring(6, 4)), int.Parse(retorno.Dt_licenca.Substring(3, 2)), int.Parse(retorno.Dt_licenca.Substring(0, 2))),
                                    dbType: DbType.DateTime);
                                p.Add("@p_nr_seqlic", retorno.Nr_seqlic, dbType: DbType.Int32);
                                p.Add("@id_cliente", cliente.Id_cliente, dbType: DbType.Decimal);
                                await conexao._conexao.ExecuteAsync("update tb_crm_cliente set dt_licenca = @dt_licenca, nr_seqlic = @p_nr_seqlic where id_cliente = @id_cliente", p);
                            }
                        }
                        else retorno.Status = "2";//Cliente inativo
                    }
                    else retorno.Status = "3";//Cliente não encontrado
                }
            }
            catch (Exception ex) { retorno.Status = "9|" + ex.Message.Trim(); }
            return retorno;
        }

        public async Task<string> GravarRDC(TRegistro_Cad_RDC rdc)
        {
            SqlTransaction t = null;
            try
            {
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        t = conexao._conexao.BeginTransaction(IsolationLevel.ReadCommitted);
                        //Gravar RDC
                        DynamicParameters p = new DynamicParameters();
                        p.Add("@P_ID_RDC", string.IsNullOrWhiteSpace(rdc.ID_RDC) ? null : new Guid(rdc.ID_RDC), 
                            dbType: DbType.Guid, direction: ParameterDirection.InputOutput);
                        p.Add("@P_ST_RDC", rdc.ST_RDC, dbType: DbType.String, size: 1);
                        p.Add("@P_DS_RDC", rdc.DS_RDC, dbType: DbType.String, size: 255);
                        p.Add("@P_VERSAO", rdc.Versao, dbType: DbType.Decimal);
                        p.Add("@P_CODE_REPORT", rdc.Code_Report, dbType: DbType.Binary);
                        p.Add("@P_MODULO", rdc.Modulo, dbType: DbType.String, size: 3);
                        p.Add("@P_IDENT", rdc.Ident, dbType: DbType.String, size: 255);
                        p.Add("@P_NM_CLASSE", rdc.NM_Classe, dbType: DbType.String, size: 255);
                        await conexao._conexao.ExecuteAsync("IA_BIN_RDC", p, transaction: t, commandType: CommandType.StoredProcedure);
                        rdc.ID_RDC = p.Get<Guid>("@P_ID_RDC").ToString();
                        //Excluir Data Source do RDC
                        p = new DynamicParameters();
                        p.Add("@P_ID_RDC", rdc.ID_RDC);
                        await conexao._conexao.ExecuteAsync("EXCLUI_BIN_RDCPORRDC", p, transaction: t, commandType: CommandType.StoredProcedure);
                        foreach(var v in rdc.lCad_DataSource)
                        {
                            p = new DynamicParameters();
                            p.Add("@P_ID_DATASOURCE", string.IsNullOrWhiteSpace(v.ID_DataSource) ? null : new Guid(v.ID_DataSource), dbType: DbType.Guid, direction: ParameterDirection.InputOutput);
                            p.Add("@P_DS_DATASOURCE", v.DS_DataSource, dbType: DbType.String, size: 255);
                            p.Add("@P_DS_SQL", v.DS_SQL, dbType: DbType.AnsiString);
                            await conexao._conexao.ExecuteAsync("IA_BIN_DATASOURCE", p, transaction: t, commandType: CommandType.StoredProcedure);
                            v.ID_DataSource = p.Get<Guid>("@P_ID_DATASOURCE").ToString();
                            //Gravar RDC X Data Source
                            p = new DynamicParameters();
                            p.Add("@P_ID_RDC", new Guid(rdc.ID_RDC), dbType: DbType.Guid);
                            p.Add("@P_ST_RDC", rdc.ST_RDC, dbType: DbType.String, size: 1);
                            p.Add("@P_ID_DATASOURCE", new Guid(v.ID_DataSource), dbType: DbType.Guid);
                            await conexao._conexao.ExecuteAsync("IA_BIN_RDC_X_DATASOURCE", p, transaction: t, commandType: CommandType.StoredProcedure);
                            foreach(var param in v.lCad_ParamClasse)
                            {
                                //Gravar Parametros
                                p = new DynamicParameters();
                                p.Add("@P_NM_PARAM", param.NM_Param, dbType: DbType.String, size: 255);
                                p.Add("@P_NM_CAMPOFORMAT", param.NM_CampoFormat, dbType: DbType.String, size: 255);
                                p.Add("@P_NM_CLASSE", param.NM_Classe, dbType: DbType.String, size: 255);
                                p.Add("@P_NM_DLL", param.NM_DLL, dbType: DbType.String, size: 255);
                                p.Add("@P_CONDICAO_BUSCA", param.CondicaoBusca, dbType: DbType.String, size: 255);
                                p.Add("@P_CODIGOCMP", param.CodigoCMP, dbType: DbType.String, size: 30);
                                p.Add("@P_NOMECMP", param.NomeCMP, dbType: DbType.String, size: 30);
                                p.Add("@P_TP_DADO", param.TP_Dado, dbType: DbType.String, size: 20);
                                p.Add("@P_RADIOCHECKGROUP", param.RadioCheckGroup, dbType: DbType.AnsiString);
                                p.Add("@P_ST_OBRIGATORIO", param.St_Obrigatorio, dbType: DbType.String, size: 1);
                                p.Add("@P_ST_NULL", param.St_Null, dbType: DbType.String, size: 1);
                                await conexao._conexao.ExecuteAsync("IA_BIN_PARAMCLASSE", p, transaction: t, commandType: CommandType.StoredProcedure);
                            }
                        }
                        t.Commit();
                        return "0|" + rdc.ID_RDC;
                    }
                    else return "1|Erro abrir conexão banco dados.";
                }
            }
            catch(Exception ex) { t.Rollback(); return "1|Erro: " + ex.Message.Trim(); }
        }

        public async Task<string> VerificarVersaoRDC(string Id_rdc,
                                                     string Nm_classe,
                                                     string Modulo,
                                                     string Ident,
                                                     string St_rdc)
        {
            if (!string.IsNullOrWhiteSpace(Id_rdc))
                Id_rdc = Id_rdc.Replace("IFEM", "-");
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select versao from tb_bin_rdc");
                string cond = "where";
                if (!string.IsNullOrWhiteSpace(Id_rdc))
                {
                    sql.AppendLine(cond + " id_rdc = '" + Id_rdc.Trim() + "'");
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(Nm_classe))
                {
                    sql.AppendLine(cond + " nm_classe = '" + Nm_classe.Trim() + "'");
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(Modulo))
                {
                    sql.AppendLine(cond + " modulo = '" + Modulo.Trim() + "'");
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(Ident))
                {
                    sql.AppendLine(cond + " ident = '" + Ident.Trim() + "'");
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(St_rdc))
                    sql.AppendLine(cond + " st_rdc = '" + St_rdc.Trim() + "'");
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        string ret = await conexao._conexao.ExecuteScalarAsync<string>(sql.ToString());
                        return string.IsNullOrWhiteSpace(ret) ? "0" : ret;
                    }
                    else return "0";
                }
            }
            catch { return "0"; }
        }

        public async Task<TRegistro_Cad_RDC> DownloadRDC(string Id_rdc,
                                                         string Nm_classe,
                                                         string Modulo,
                                                         string Ident,
                                                         string St_rdc)
        {
            if (!string.IsNullOrWhiteSpace(Id_rdc))
                Id_rdc = Id_rdc.Replace("IFEM", "-");
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select convert(varchar(255), ID_RDC) as ID_RDC, DS_RDC, ")
                    .AppendLine("Modulo, Ident, NM_Classe, Versao, Code_Report,  ST_RDC ")
                    .AppendLine("FROM TB_BIN_RDC ");
                string cond = "where";
                if (!string.IsNullOrWhiteSpace(Id_rdc))
                {
                    sql.AppendLine(cond + " id_rdc = '" + Id_rdc.Trim() + "'");
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(Nm_classe))
                {
                    sql.AppendLine(cond + " nm_classe = '" + Nm_classe.Trim() + "'");
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(Modulo))
                {
                    sql.AppendLine(cond + " modulo = '" + Modulo.Trim() + "'");
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(Ident))
                {
                    sql.AppendLine(cond + " ident = '" + Ident.Trim() + "'");
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(St_rdc))
                    sql.AppendLine(cond + " st_rdc = '" + St_rdc.Trim() + "'");
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        var retorno = await conexao._conexao.QueryFirstOrDefaultAsync<TRegistro_Cad_RDC>(sql.ToString());
                        if(retorno != null)
                        {
                            sql.Clear();
                            sql.AppendLine("select convert(varchar(255), a.id_dts) as ID_DataSource, a.ds_dts as DS_DataSource, a.ds_sql, a.dt_dts as DT_DataSource")
                                .AppendLine("from tb_bin_datasource a ")
                                .AppendLine("where exists(select 1 from tb_bin_rdc_x_datasource x where x.id_dts = a.id_dts and x.id_rdc = '" + retorno.ID_RDC + "')")
                                .AppendLine("order by a.ds_dts asc");
                            var lista = await conexao._conexao.QueryAsync<TRegistro_Cad_DataSource>(sql.ToString());
                            foreach(var v in lista)
                            {
                                List<TRegistro_Cad_ParamClasse> lParamRetorno = new List<TRegistro_Cad_ParamClasse>();
                                const char chaveInicio = '{';
                                const char chaveFim = '}';
                                char[] delimitadores = new char[] { chaveInicio, chaveFim };
                                string[] resultadoArray = v.DS_SQL.Split(delimitadores);
                                for (int i = 0; i < resultadoArray.Length; i++)
                                {
                                    if (resultadoArray[i].IndexOf("@") == 0)
                                    {
                                        sql.Clear();
                                        sql.AppendLine("select a.NM_ParamCaption as NM_Param, a.NM_CampoFormat, a.NM_Classe, a.TP_Dado, ")
                                            .AppendLine("a.CondicaoBusca, a.CodigoCMP, a.NomeCmp, a.NM_DLL, a.RadioCheckGroup, ")
                                            .AppendLine("isnull(a.ST_Obrigatorio,'N') as ST_Obrigatorio, isnull(a.ST_Null,'N') as  ST_Null ")
                                            .AppendLine("from TB_BIN_ParamClasse a ")
                                            .AppendLine("where a.NM_CampoFormat like '%{" + resultadoArray[i] + "}%'")
                                            .AppendLine("ORDER BY a.NM_ParamCaption ASC ");
                                        var listaParam = await conexao._conexao.QueryAsync<TRegistro_Cad_ParamClasse>(sql.ToString());
                                        for (int x = 0; x < listaParam.ToList().Count; x++)
                                            if (!lParamRetorno.Exists(y => y.NM_CampoFormat == (listaParam.ToList()[x] as TRegistro_Cad_ParamClasse).NM_CampoFormat))
                                                lParamRetorno.Add(listaParam.ToList()[x] as TRegistro_Cad_ParamClasse);
                                    }
                                }
                                v.lCad_ParamClasse = lParamRetorno;
                            }
                            retorno.lCad_DataSource = lista.ToList();
                        }
                        return retorno;
                    }
                    else return null;
                }
            }
            catch { return null; }
        }

        public async Task<IEnumerable<TRegistro_Cad_RDC>> BuscarRDC(string Id_rdc,
                                                                    string Ds_rdc,
                                                                    string Modulo,
                                                                    string St_rdc,
                                                                    bool BuscarRelClasse,
                                                                    bool NaoBuscarRelClasse)
        {
            if (!string.IsNullOrWhiteSpace(Id_rdc))
                Id_rdc = Id_rdc.Replace("IFEM", "-");
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select convert(varchar(255), ID_RDC) as ID_RDC, DS_RDC, Modulo, Ident, ")
                    .AppendLine("NM_Classe, Versao, Code_Report,  ST_RDC ")
                    .AppendLine("FROM TB_BIN_RDC ");
                string cond = "where";
                if (!string.IsNullOrWhiteSpace(Id_rdc))
                {
                    sql.AppendLine(cond + " id_rdc = '" + Id_rdc.Trim() + "'");
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(Ds_rdc))
                {
                    sql.AppendLine(cond + " ds_rdc like '%" + Ds_rdc.Trim() + "%'");
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(Modulo))
                {
                    sql.AppendLine(cond + " modulo = '" + Modulo.Trim() + "'");
                    cond = "and";
                }
                if (!string.IsNullOrWhiteSpace(St_rdc))
                {
                    sql.AppendLine(cond + " st_rdc = '" + St_rdc.Trim() + "'");
                    cond = "and";
                }
                if (BuscarRelClasse)
                {
                    sql.AppendLine(cond + " isnull(nm_classe, '') <> ''");
                    cond = "and";
                }
                if (NaoBuscarRelClasse)
                    sql.AppendLine(cond + " isnull(nm_classe, '') = ''");
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                        return await conexao._conexao.QueryAsync<TRegistro_Cad_RDC>(sql.ToString());
                    else return null;
                }
            }
            catch { return null; }
        }

        public async Task<TRegistro_Cad_RDC> BuscarDetalheRDC(string Id_rdc)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select convert(varchar(255), ID_RDC) as ID_RDC, DS_RDC, Modulo, Ident, ")
                    .AppendLine("NM_Classe, Versao, Code_Report,  ST_RDC ")
                    .AppendLine("FROM TB_BIN_RDC ")
                    .AppendLine("where id_rdc = '" + Id_rdc.Trim() + "'");
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        var retorno = await conexao._conexao.QueryFirstOrDefaultAsync<TRegistro_Cad_RDC>(sql.ToString());
                        if (retorno != null)
                        {
                            sql.Clear();
                            sql.AppendLine("select convert(varchar(255), a.id_dts) as ID_DataSource, a.ds_dts as DS_DataSource, a.ds_sql, a.dt_dts as DT_DataSource ")
                                .AppendLine("from tb_bin_datasource a ")
                                .AppendLine("where exists(select 1 from tb_bin_rdc_x_datasource x where x.id_dts = a.id_dts and x.id_rdc = '" + retorno.ID_RDC + "')")
                                .AppendLine("order by a.ds_dts asc");
                            var lista = await conexao._conexao.QueryAsync<TRegistro_Cad_DataSource>(sql.ToString());
                            foreach (var v in lista)
                            {
                                List<TRegistro_Cad_ParamClasse> lParamRetorno = new List<TRegistro_Cad_ParamClasse>();
                                const char chaveInicio = '{';
                                const char chaveFim = '}';
                                char[] delimitadores = new char[] { chaveInicio, chaveFim };
                                string[] resultadoArray = v.DS_SQL.Split(delimitadores);
                                for (int i = 0; i < resultadoArray.Length; i++)
                                {
                                    if (resultadoArray[i].IndexOf("@") == 0)
                                    {
                                        sql.Clear();
                                        sql.AppendLine("select a.NM_ParamCaption as NM_Param, a.NM_CampoFormat, a.NM_Classe, a.TP_Dado, ")
                                            .AppendLine("a.CondicaoBusca, a.CodigoCMP, a.NomeCmp, a.NM_DLL, a.RadioCheckGroup, ")
                                            .AppendLine("isnull(a.ST_Obrigatorio,'N') as ST_Obrigatorio, isnull(a.ST_Null,'N') as  ST_Null ")
                                            .AppendLine("from TB_BIN_ParamClasse a ")
                                            .AppendLine("where a.NM_CampoFormat like '%{" + resultadoArray[i] + "}%'")
                                            .AppendLine("ORDER BY a.NM_ParamCaption ASC ");
                                        var listaParam = await conexao._conexao.QueryAsync<TRegistro_Cad_ParamClasse>(sql.ToString());
                                        for (int x = 0; x < listaParam.ToList().Count; x++)
                                            if (!lParamRetorno.Exists(y => y.NM_CampoFormat == (listaParam.ToList()[x] as TRegistro_Cad_ParamClasse).NM_CampoFormat))
                                                lParamRetorno.Add(listaParam.ToList()[x] as TRegistro_Cad_ParamClasse);
                                    }
                                }
                                v.lCad_ParamClasse = lParamRetorno;
                            }
                            retorno.lCad_DataSource = lista.ToList();
                            return retorno;
                        }
                        else return null;
                    }
                    else return null;
                }
            }
            catch { return null; }
        }

        public async Task<bool> GravarParamClasseAsync(IEnumerable<TRegistro_Cad_ParamClasse> lParamClasse)
        {
            SqlTransaction t = null;
            try
            {
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        t = conexao._conexao.BeginTransaction(IsolationLevel.ReadCommitted);
                        foreach (var p in lParamClasse)
                        {
                            DynamicParameters param = new DynamicParameters();
                            param.Add("@P_NM_PARAM", p.NM_Param);
                            param.Add("@P_NM_CAMPOFORMAT", p.NM_CampoFormat);
                            param.Add("@P_NM_CLASSE", p.NM_Classe);
                            param.Add("@P_NM_DLL", p.NM_DLL);
                            param.Add("@P_CONDICAO_BUSCA", p.CondicaoBusca);
                            param.Add("@P_CODIGOCMP", p.CodigoCMP);
                            param.Add("@P_NOMECMP", p.NomeCMP);
                            param.Add("@P_TP_DADO", p.TP_Dado);
                            param.Add("@P_RADIOCHECKGROUP", p.RadioCheckGroup);
                            param.Add("@P_ST_OBRIGATORIO", p.St_Obrigatorio);
                            param.Add("@P_ST_NULL", p.St_Null);
                            await conexao._conexao.ExecuteAsync("IA_BIN_PARAMCLASSE", param, transaction: t, commandType: CommandType.StoredProcedure);
                        }
                        t.Commit();
                        return true;
                    }
                    else return false;
                }
            }
            catch { t.Rollback();return false; }
        }

        public async Task<string> HomologarRDCAsync(TRegistro_Cad_RDC rdc)
        {
            SqlTransaction t = null;
            try
            {
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoHelp")))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        t = conexao._conexao.BeginTransaction(IsolationLevel.ReadCommitted);
                        //Gravar RDC
                        DynamicParameters p = new DynamicParameters();
                        p.Add("@P_ID_RDC", direction: ParameterDirection.Output);
                        p.Add("@P_ST_RDC", rdc.ST_RDC);
                        p.Add("@P_DS_RDC", rdc.DS_RDC);
                        p.Add("@P_VERSAO", rdc.Versao);
                        p.Add("@P_CODE_REPORT", rdc.Code_Report);
                        p.Add("@P_MODULO", rdc.Modulo);
                        p.Add("@P_IDENT", rdc.Ident);
                        p.Add("@P_NM_CLASSE", rdc.NM_Classe);
                        await conexao._conexao.ExecuteAsync("IA_BIN_RDC", p, transaction: t, commandType: CommandType.StoredProcedure);
                        rdc.ID_RDC = p.Get<string>("@P_ID_RDC");
                        //Excluir Data Source do RDC
                        p = new DynamicParameters();
                        p.Add("@P_ID_RDC", rdc.ID_RDC);
                        await conexao._conexao.ExecuteAsync("EXCLUI_BIN_RDCPORRDC", p, transaction: t, commandType: CommandType.StoredProcedure);
                        foreach (var v in rdc.lCad_DataSource)
                        {
                            //Gravar Data Source
                            p = new DynamicParameters();
                            p.Add("@P_ID_DATASOURCE", direction: ParameterDirection.Output);
                            p.Add("@P_DS_DATASOURCE", v.DS_DataSource);
                            p.Add("@P_DS_SQL", v.DS_SQL);
                            await conexao._conexao.ExecuteAsync("IA_BIN_DATASOURCE", p, transaction: t, commandType: CommandType.StoredProcedure);
                            v.ID_DataSource = p.Get<string>("@P_ID_DATASOURCE");
                            //Gravar RDC X Data Source
                            p = new DynamicParameters();
                            p.Add("@P_ID_RDC", new Guid(rdc.ID_RDC));
                            p.Add("@P_ST_RDC", rdc.ST_RDC);
                            p.Add("@P_ID_DATASOURCE", new Guid(v.ID_DataSource));
                            await conexao._conexao.ExecuteAsync("IA_BIN_RDC_X_DATASOURCE", p, transaction: t, commandType: CommandType.StoredProcedure);
                            foreach (var c in v.lCad_ParamClasse)
                            {
                                //Gravar Parametros
                                p = new DynamicParameters();
                                p.Add("@P_NM_PARAM", c.NM_Param);
                                p.Add("@P_NM_CAMPOFORMAT", c.NM_CampoFormat);
                                p.Add("@P_NM_CLASSE", c.NM_Classe);
                                p.Add("@P_NM_DLL", c.NM_DLL);
                                p.Add("@P_CONDICAO_BUSCA", c.CondicaoBusca);
                                p.Add("@P_CODIGOCMP", c.CodigoCMP);
                                p.Add("@P_NOMECMP", c.NomeCMP);
                                p.Add("@P_TP_DADO", c.TP_Dado);
                                p.Add("@P_RADIOCHECKGROUP", c.RadioCheckGroup);
                                p.Add("@P_ST_OBRIGATORIO", c.St_Obrigatorio);
                                p.Add("@P_ST_NULL", c.St_Null);
                                await conexao._conexao.ExecuteAsync("IA_BIN_PARAMCLASSE", p, transaction: t, commandType: CommandType.StoredProcedure);
                            }
                        }
                        t.Commit();
                        return "0|" + rdc.ID_RDC;
                    }
                    else return "1|Erro abrir conexão banco dados.";
                }
            }
            catch(Exception ex) { t.Rollback(); return "1|Erro: " + ex.Message.Trim(); }
        }
        public async Task<IEnumerable<Boleto>> GetBoletosLBSystemAsync(string doc)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.ID_Config, a.CodigoCedente, a.PostoCedente, b.Nr_Agencia,")
                    .AppendLine("a.ChaveTransacaoSicredi, a.DT_ExpiracaoChave, c.Url_Autenticacao,")
                    .AppendLine("c.Url_API, a.Token")
                    .AppendLine("from TB_COB_CfgBanco a")
                    .AppendLine("inner join TB_FIN_ContaGer b")
                    .AppendLine("on a.CD_ContaGer = b.CD_ContaGer")
                    .AppendLine("inner join TB_FIN_Banco c")
                    .AppendLine("on b.CD_Banco = c.CD_Banco")
                    .AppendLine("where b.CD_Banco = '748'")
                    .AppendLine("and ISNULL(a.ST_Registro, 'A') <> 'C'");
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoLB")))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        ConfigBanco config = await conexao._conexao.QueryFirstOrDefaultAsync<ConfigBanco>(sql.ToString());
                        if (config != null)
                        {
                            sql = new StringBuilder();
                            sql.AppendLine("select a.DT_Emissao, b.DT_Vencto, b.Vl_Atual, c.NossoNumero, g.cd_banco")
                                .AppendLine("from TB_FIN_Duplicata a")
                                .AppendLine("inner join VTB_FIN_PARCELA b")
                                .AppendLine("on a.CD_Empresa = b.CD_Empresa")
                                .AppendLine("and a.Nr_Lancto = b.Nr_Lancto")
                                .AppendLine("and ISNULL(a.ST_Registro, 'A') <> 'C'")
                                .AppendLine("and ISNULL(b.ST_Registro, 'A') in('A', 'P')")
                                .AppendLine("left join TB_COB_Titulo c")
                                .AppendLine("on b.CD_Empresa = c.CD_Empresa")
                                .AppendLine("and b.Nr_Lancto = c.Nr_Lancto")
                                .AppendLine("and b.CD_Parcela = c.CD_Parcela")
                                .AppendLine("inner join VTB_FIN_CLIFOR d")
                                .AppendLine("on a.CD_Clifor = d.CD_Clifor")
                                .AppendLine("inner join TB_FIN_TPDuplicata e")
                                .AppendLine("on a.TP_Duplicata = e.TP_Duplicata")
                                .AppendLine("and e.TP_MOV = 'R'")
                                .AppendLine("inner join TB_COB_CfgBanco f")
                                .AppendLine("on c.id_config = f.id_config")
                                .AppendLine("inner join TB_FIN_ContaGer g")
                                .AppendLine("on f.cd_contager = g.cd_contager")
                                .AppendLine("where dbo.FVALIDA_NUMEROS(case when d.TP_Pessoa = 'F' then d.NR_CPF else d.NR_CGC end) in(" + doc + ")");
                            IEnumerable<Boleto> boletos = await conexao._conexao.QueryAsync<Boleto>(sql.ToString());
                            boletos.ToList()
                                .ForEach(p => p.NossoNumero += Utilitario.Mod11(config.Nr_Agencia.Trim().PadLeft(4, '0') +
                                                                                config.PostoCedente.Trim().PadLeft(2, '0') +
                                                                                config.CodigoCedente.Trim().PadLeft(5, '0') +
                                                                                p.NossoNumero.SoNumero(), 9, false, 0).ToString());
                            return boletos;
                        }
                        else return null;
                    }
                    else return null;
                }
            }
            catch { return null; }
        }
        public async Task<string> GetPDFBoletoSicrediAsync(string cd_banco, string nosso_numero)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("select a.ID_Config, a.CodigoCedente, a.PostoCedente, b.Nr_Agencia,")
                    .AppendLine("a.ChaveTransacaoSicredi, a.DT_ExpiracaoChave, c.Url_Autenticacao,")
                    .AppendLine("c.Url_API, a.Token, b.CertificadoPfx, b.SenhaPfx,")
                    .AppendLine("b.Client_secret, b.Client_id")
                    .AppendLine("from TB_COB_CfgBanco a")
                    .AppendLine("inner join TB_FIN_ContaGer b")
                    .AppendLine("on a.CD_ContaGer = b.CD_ContaGer")
                    .AppendLine("inner join TB_FIN_Banco c")
                    .AppendLine("on b.CD_Banco = c.CD_Banco")
                    .AppendLine("where b.CD_Banco = '" + cd_banco + "'")
                    .AppendLine("and ISNULL(a.ST_Registro, 'A') <> 'C'");
                using (TConexao conexao = new TConexao(_config.GetConnectionString("conexaoLB")))
                {
                    if (await conexao.OpenConnectionAsync())
                    {
                        ConfigBanco config = await conexao._conexao.QueryFirstOrDefaultAsync<ConfigBanco>(sql.ToString());
                        if (config != null)
                        {
                            if (cd_banco.Trim().Equals("748"))
                            {
                                HttpClient client = new HttpClient();
                                HttpResponseMessage response;
                                if (!config.DT_ExpiracaoChave.HasValue ? true : config.DT_ExpiracaoChave.Value < DateTime.Now)
                                {
                                    client = new HttpClient();
                                    client.BaseAddress = new Uri(config.Url_Autenticacao);
                                    client.DefaultRequestHeaders.Add("token", config.Token);
                                    response = await client.PostAsync("/sicredi-cobranca-ws-ecomm-api/ecomm/v1/boleto/autenticacao", null);
                                    if (response.IsSuccessStatusCode)
                                    {
                                        var res = await response.Content.ReadAsStringAsync();
                                        TokenSicredi tk = JsonConvert.DeserializeObject<TokenSicredi>(res);
                                        config.ChaveTransacaoSicredi = tk.chaveTransacao;
                                        config.DT_ExpiracaoChave = tk.dataExpiracao;
                                        DynamicParameters param = new DynamicParameters();
                                        param.Add("@CHAVE", config.ChaveTransacaoSicredi);
                                        param.Add("@DATA", config.DT_ExpiracaoChave);
                                        param.Add("@ID", config.Id_config);
                                        await conexao._conexao.ExecuteAsync("update TB_COB_CfgBanco set ChaveTransacaoSicredi = @CHAVE, " +
                                                                            "DT_ExpiracaoChave = @DATA, dt_alt = getdate() " +
                                                                            "where ID_Config = @ID", param);
                                    }
                                }
                                client = new HttpClient();
                                client.BaseAddress = new Uri(config.Url_API);
                                client.DefaultRequestHeaders.Add("token", config.ChaveTransacaoSicredi);
                                string consulta = "agencia=" + config.Nr_Agencia.SoNumero() +
                                                  "&cedente=" + config.CodigoCedente.SoNumero() +
                                                  "&nossoNumero=" + nosso_numero.SoNumero() +
                                                  "&posto=" + config.PostoCedente.SoNumero();
                                response = await client.GetAsync("/sicredi-cobranca-ws-ecomm-api/ecomm/v1/boleto/impressao?" + consulta);
                                if (response.IsSuccessStatusCode)
                                {
                                    BoletoSicrediPdf pdf = JsonConvert.DeserializeObject<BoletoSicrediPdf>(await response.Content.ReadAsStringAsync());
                                    return pdf.arquivo;
                                }
                                else return string.Empty;
                            }
                            else if (cd_banco.Trim().Equals("077"))
                            {
                                HttpClientHandler handler = new HttpClientHandler();
                                handler.ClientCertificates.Add(new X509Certificate2(config.CertificadoPfx, config.SenhaPfx));
                                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                                handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                                using (var client = new HttpClient(handler))
                                {
                                    var response = await client.PostAsync(config.Url_API + "/oauth/v2/token",
                                        new FormUrlEncodedContent(
                                            new List<KeyValuePair<string, string>>
                                            {
                                                new KeyValuePair<string, string>("client_id", config.Client_id),
                                                new KeyValuePair<string, string>("client_secret", config.Client_secret),
                                                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                                                new KeyValuePair<string, string>("scope", "extrato.read boleto-cobranca.read boleto-cobranca.write")
                                            }));
                                    if (response.IsSuccessStatusCode)
                                    {
                                        TokenInter tokenInter = JsonConvert.DeserializeObject<TokenInter>(await response.Content.ReadAsStringAsync());
                                        if (tokenInter != null)
                                        {
                                            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(tokenInter.token_type, tokenInter.access_token);
                                            var url = string.Concat(config.Url_API, String.Format("/cobranca/v2/boletos/{0}/pdf", nosso_numero));
                                            response = await client.GetAsync(url);
                                            if (response.IsSuccessStatusCode)
                                            {
                                                var pdf = JsonConvert.DeserializeObject<BoletoPdfInter>(await response.Content.ReadAsStringAsync());
                                                if (pdf != null)
                                                    return pdf.pdf;
                                                else return string.Empty;
                                            }
                                            else return string.Empty;
                                        }
                                        else return string.Empty;
                                    }
                                    else return string.Empty;
                                }
                            }
                            else return string.Empty;
                        }
                        else return string.Empty;
                    }
                    else return string.Empty;
                }
            }
            catch { return string.Empty; }
        }
    }
}
