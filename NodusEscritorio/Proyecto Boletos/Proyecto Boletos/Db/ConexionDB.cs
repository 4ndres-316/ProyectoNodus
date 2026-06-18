using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Proyecto_Boletos.Db
{
    public static class ConexionDB
    {
        private const string Url = "https://pjtiuggfsuoghzqtkgda.supabase.co";
        private const string Key = "sb_publishable_cINqokb40-fh2MuSu7OxsQ_z3eFPSbk";
        private static Client _cliente;

        public static Client Client
        {
            get
            {
                if (_cliente == null)
                    throw new InvalidOperationException("Llama a Init() primero");

                return _cliente;
            }
        }

        public static async Task Init()
        {
            if (_cliente != null) return;

            _cliente = new Client(Url, Key, new SupabaseOptions
            {
                AutoConnectRealtime = false,
                AutoRefreshToken = false
            });

            await _cliente.InitializeAsync();
        }
    }
}
