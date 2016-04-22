using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            //Calculo para total de Compras/Vendas e Valor gasto/ganho
            /*var x = CompraVenda.TotalSum("Sum(Total)","Tipo", "Compra");
            var y = CompraVenda.TotalSum("Sum(Total)","Tipo", "Venda");

            var numeroCompras = CompraVenda.TotalSum("Count(Total)", "Tipo", "Compra");
            var numeroVendas = CompraVenda.TotalSum("Count(Total)", "Tipo", "Venda");

            var r = numeroCompras - numeroVendas;

            var z = y - x;
            Console.WriteLine(z);*/

            //CompraVenda.ComandoSql(string.Format("Insert into Saldo(Saldo, Data) values (300.00 , '{0}');", DateTime.Now.ToString("yyyyMMddHHmmss")));
            var continua = true;
            while (continua)
            {
                //Console.Clear();
                if (DateTime.Now.Hour < 8 || (DateTime.Now.Hour >= 18 && DateTime.Now.Minute >= 20))
                {
                    continua = false;
                    break;
                }

                if (DateTime.Now.Hour >= 11)
                {
                    Thread t = new Thread(new ThreadStart(ThreadMethod));
                    t.Start();
                    t.Join();
                }
                else
                {
                    Console.WriteLine("Pregão fechado no momento. Aguarde.");
                }

                Thread.Sleep(600000);
            }
            
            /*var html = Consulta.EfetuarNavegacao("http://pregao-online.bmfbovespa.com.br/Cotacoes.aspx?idioma=pt-BR");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodeCodigos = doc.DocumentNode.SelectNodes("//table[tr/th[contains(text(),'Ativo')]]/tr/td[1]");
            string connectionString = @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = C:\Users\Cliente\Documents\Visual Studio 2015\Projects\ConsoleApplication1\ConsoleApplication1\Acoes.mdf; Integrated Security = True";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                int g = 0;
                con.Open();
                foreach (var node in nodeCodigos)
                {
                    string s = string.Format("INSERT INTO SiglasAcoes VALUES ('{0}');", node.InnerText.Trim());
                    using (SqlCommand command = new SqlCommand(s, con))
                        command.ExecuteNonQuery();
                    g++;
                }

            }*/

            /*var x = Consulta.EfetuarNavegacao("http://pregao-online.bmfbovespa.com.br/Cotacoes.aspx?idioma=pt-BR");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(x);
            var dic = Consulta.GetMedia(Consulta.GetListaSiglas(doc));
            Console.WriteLine("Médias: ");
            foreach (var kvp in dic)
            {
                Console.WriteLine(string.Format("{0} - {1}", kvp.Key, kvp.Value));
            }*/

            //Consulta.ComandoSql("CREATE TABLE DataConsulta(DataSite VARCHAR(30), DataConsulta VARCHAR(30), Id INT IDENTITY(1, 1) PRIMARY KEY);");

            /*var html = Consulta.EfetuarNavegacao("http://pregao-online.bmfbovespa.com.br/Cotacoes.aspx?idioma=pt-BR");
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var nodeCodigos = doc.DocumentNode.SelectNodes("//table[tr/th[contains(text(),'Ativo')]]/tr/td[1]");

            string connectionString = @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = C:\Users\Cliente\Documents\Visual Studio 2015\Projects\ConsoleApplication1\ConsoleApplication1\Acoes.mdf; Integrated Security = True";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                foreach (var node in nodeCodigos)
                {
                    string s = string.Format("CREATE TABLE {0}(Cotacao DECIMAL(6,2),Data_Consulta varchar(30), Id INT IDENTITY(1, 1) PRIMARY KEY);", node.InnerText.Trim());
                    using (SqlCommand command = new SqlCommand(s, con))
                        command.ExecuteNonQuery();
                }
            }*/

            Console.WriteLine("Concluído com sucesso.");
            Console.ReadKey();
        }

        static void ThreadMethod()
        {
            List<string> dic = Consulta.RetornaValoresConsultados();
            Console.WriteLine(string.Format("Hora da consulta: {0}", DateTime.Now));
            if (dic.Count > 0)
            {
                /*foreach (var par in dic)
                {
                    Console.WriteLine(string.Format("{0} - {1}", par.Key, par.Value));
                }*/
                foreach (var par in dic)
                {
                    Console.WriteLine(par);
                }

                Console.WriteLine("A consulta foi executada com sucesso.");
            }
            else
                Console.WriteLine("A consulta não foi executada. Os dados atuais já estão atualizados.");
        }
    }
}
