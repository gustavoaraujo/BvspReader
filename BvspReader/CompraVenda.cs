using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class CompraVenda
    {
        struct Compra
        {
            public string Sigla;
            public double ValorCompra;
            public int Quantidade;
            public string data;
        }

        public static string EfetuarCompra(string sigla, double preco) {
            if (preco * 2 < RetornaUltimoSaldo())
            {
                Compra c = new Compra();
                c.Sigla = sigla;
                c.ValorCompra = preco;
                c.Quantidade = QuantidadeCompra(preco);
                c.data = DateTime.Now.ToString("yyyyMMddHHmmss");
                if (c.Quantidade > 0)
                {
                    ComandoSql(string.Format("INSERT INTO Transacao (Sigla, ValorAcao, Total, Tipo, Data) VALUES ('{0}', {1}, {2}, '{3}', '{4}')", sigla, c.ValorCompra.ToString().Replace(',', '.'), (c.ValorCompra * c.Quantidade).ToString().Replace(',', '.'), "Compra", c.data));
                    var novoSaldo = (RetornaUltimoSaldo() - (c.ValorCompra * c.Quantidade)).ToString().Replace(',', '.');
                    ComandoSql(string.Format("INSERT INTO Saldo (Saldo, Data) VALUES ({0}, {1})", novoSaldo, DateTime.Now.ToString("yyyyMMddHHmmss")));
                    return string.Format("Compra - {0}: {1} ações. R${2} por ação.", sigla, c.Quantidade, c.ValorCompra);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return string.Empty;
            }
            
        }

        public static int QuantidadeCompra(double preco)
        {
            double saldo = RetornaUltimoSaldo();
            saldo = (saldo / 3);
            string s = string.Format("{0:0.00}", saldo);
            saldo = Double.Parse(s);
            int quantidade = (int) (saldo / preco);
            return quantidade;
        }
        
        private static void RetornaDoubles(string comando, out List<double> listaSaida)
        {
            listaSaida = new List<double>();
            string s = comando;
            string connectionString = @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = C:\Users\Cliente\Documents\Visual Studio 2015\Projects\ConsoleApplication1\ConsoleApplication1\CompraVenda.mdf; Integrated Security = True";
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand command = new SqlCommand(s, con);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.HasRows)
            {
                while (reader.Read())
                {
                    listaSaida.Add(Double.Parse(reader[0].ToString()));
                }
                reader.NextResult();
            }
            con.Close();
        }

        private static void RetornaUltimaCompra(string sigla, out double valorPorAcao, out int quantidade)
        {
            string comando = string.Format("SELECT TOP 1 * FROM Transacao WHERE Sigla = '{0}' AND Tipo = 'Compra' order by Id DESC", sigla);
            string connectionString = GetDBCompraVendaDirectory();
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand command = new SqlCommand(comando, con);
            SqlDataReader reader = command.ExecuteReader();
            valorPorAcao = 0;
            double total = 0;
            while (reader.HasRows)
            {
                while (reader.Read())
                {
                    valorPorAcao = Double.Parse(reader["ValorAcao"].ToString());
                    total = Double.Parse(reader["Total"].ToString());
                }
                reader.NextResult();
            }
            con.Close();
            quantidade = (int)(total / valorPorAcao);
        }

        public static string EfetuarVenda(string sigla, double preco)
        {
            double valorPorAcao = 0;
            int quantidade = 0;
            RetornaUltimaCompra(sigla, out valorPorAcao, out quantidade);
            ComandoSql(string.Format("INSERT INTO Transacao (Sigla, ValorAcao, Total, Tipo, Data) VALUES ('{0}', {1}, {2}, '{3}', '{4}')", 
                sigla, preco.ToString().Replace(',', '.'), (preco * quantidade).ToString().Replace(',','.'), "Venda", DateTime.Now.ToString("yyyyMMddHHmmss")));
            ComandoSql(string.Format("Insert into Saldo(Saldo, Data) values({0}, '{1}'); ",(RetornaUltimoSaldo() + (preco * quantidade)).ToString().Replace(',', '.'), DateTime.Now.ToString("yyyyMMddHHmmss")));
            return string.Format("Venda - {0}: {1} ações. R${2} por ação.", sigla, quantidade, preco);
        }

        public static void ComandoSql(string s)
        {
            string connectionString = GetDBCompraVendaDirectory();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(s, con))
                    command.ExecuteNonQuery();
                con.Close();
            }
        }

        public static void AnalisaPreco(string sigla, double preco, out string compra, out string venda)
        {
            compra = "";
            double valorPorAcao;
            int quantidade;
            if (RetornaUltimoValor(sigla) > preco && preco < Consulta.GetMediaAcao(sigla))
            {
                List<double> acoesCompradas = new List<double>();
                RetornaDoubles(string.Format("SELECT Total FROM Transacao WHERE Tipo = 'Compra' and Sigla = '{0}';", sigla), out acoesCompradas);
                List<double> acoesVendidas = new List<double>();
                RetornaDoubles(string.Format("SELECT Total FROM Transacao WHERE Tipo = 'Venda' and Sigla = '{0}';", sigla), out acoesVendidas);
                if (acoesCompradas.Count == acoesVendidas.Count)
                    compra = EfetuarCompra(sigla, preco);
            }

            var x = RetornaUltimoValor(sigla);
            bool l = RetornaUltimoValor(sigla) < preco;
            var c = Consulta.GetMediaAcao(sigla);

            RetornaUltimaCompra(sigla, out valorPorAcao, out quantidade);

            if (valorPorAcao < preco)
            {
                List<double> acoesCompradas = new List<double>();
                RetornaDoubles(string.Format("SELECT Total FROM Transacao WHERE Tipo = 'Compra' and Sigla = '{0}';", sigla), out acoesCompradas);
                List<double> acoesVendidas = new List<double>();
                RetornaDoubles(string.Format("SELECT Total FROM Transacao WHERE Tipo = 'Venda' and Sigla = '{0}';", sigla), out acoesVendidas);
                if (acoesCompradas.Count > acoesVendidas.Count)
                {
                    venda = EfetuarVenda(sigla, preco);
                }
                else
                    venda = "";
            }
            else
                venda = "";
        }

        public static double RetornaUltimoValor(string sigla)
        {
            string s = string.Format("SELECT Cotacao FROM {0} WHERE Id = (SELECT TOP 1 Id FROM {0} order by Id desc);", sigla);
            string connectionString = Consulta.GetDBDirectory();
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand command = new SqlCommand(s, con);
            SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            var valorMin = Double.Parse(reader["Cotacao"].ToString());
            con.Close();
            return valorMin;
        }

        public static double RetornaUltimoSaldo()
        {
            string s = string.Format("SELECT Saldo FROM Saldo WHERE Id = (SELECT TOP 1 Id FROM Saldo order by Id desc);");
            string connectionString = GetDBCompraVendaDirectory();
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand command = new SqlCommand(s, con);
            SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            var valorMin = Double.Parse(reader["Saldo"].ToString());
            con.Close();
            return valorMin;
        }

        public static double valorMinimo(string sigla) {
            string s = string.Format("SELECT Cotacao FROM {0} WHERE Cotacao = (SELECT TOP 1 Cotacao FROM {0} order by Cotacao desc);", sigla);
            string connectionString = Consulta.GetDBDirectory();
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand command = new SqlCommand(s, con);
            SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            var valorMin = Double.Parse(reader["Cotacao"].ToString());
            con.Close();
            return valorMin;
        }

        public static double TotalSum(string select, string parametro, string sigla)
        {
            string s = string.Format("SELECT {0} as s FROM Transacao WHERE {1} = '{2}';", select, parametro, sigla);
            string connectionString = GetDBCompraVendaDirectory();
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand command = new SqlCommand(s, con);
            SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            var valorMin = Double.Parse(reader["s"].ToString());
            con.Close();
            return valorMin;
        }

        public static double valorMaximo(string sigla)
        {
            string s = string.Format("SELECT Cotacao FROM {0} WHERE Cotacao = (SELECT TOP 1 Cotacao FROM {0} order by Cotacao asc);", sigla);
            string connectionString = Consulta.GetDBDirectory();
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand command = new SqlCommand(s, con);
            SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            var valorMin = Double.Parse(reader["Cotacao"].ToString());
            con.Close();
            return valorMin;
        }

        public static string GetDBCompraVendaDirectory()
        {
            return string.Format(
                @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = {0}CompraVenda.mdf; Integrated Security = True",
                Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\")));
        }
    }
}
