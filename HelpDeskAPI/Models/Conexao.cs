﻿using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace HelpDeskAPI.Models
{
    public class TConexao : IDisposable
    {
        public SqlConnection _conexao { get; private set; }
        public TConexao(string strConexao)
        {
            _conexao = new SqlConnection(strConexao);
        }

        public async Task<bool> OpenConnectionAsync()
        {
            try
            {
                await _conexao.OpenAsync();
                return true;
            }
            catch { return false; }
        }

        public void Dispose()
        {
            if (_conexao != null)
            {
                if (_conexao.State == System.Data.ConnectionState.Open)
                    _conexao.Close();
                _conexao = null;
            }
        }
    }
}
