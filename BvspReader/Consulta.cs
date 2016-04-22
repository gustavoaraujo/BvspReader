using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Consulta
    {
        public static List<string> RetornaValoresConsultados()
        {
            var html = EfetuarNavegacao(string.Format("http://pregao-online.bmfbovespa.com.br/Cotacoes.aspx?idioma=pt-BR"));
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            string compra, venda;
            List<string> listaTransacoes = new List<string>();
            //Dictionary<string, string> dic = new Dictionary<string, string>();
            //string x = RetornaUltimaData();
            //string y = ConsultaDataConsulta(doc);
            if (RetornaUltimaData() != ConsultaDataConsulta(doc))
            {
                //List<string> nodeTabelaValores = RetornaTodasAcoes();
                var nodeTabelaValores = doc.DocumentNode.SelectNodes("//table[@id='ctl00_DefaultContent_GrdCarteiraIndice']/tr[td]");
                var comando = "";
                foreach (var node in nodeTabelaValores)
                {
                    var nodeAcoes = node.SelectNodes("./td");
                    CompraVenda.AnalisaPreco(nodeAcoes[0].InnerText.Trim(), Double.Parse(nodeAcoes[2].InnerText.Trim()), out compra, out venda);

                    if (compra != string.Empty)
                        listaTransacoes.Add(compra);

                    if (venda != string.Empty)
                        listaTransacoes.Add(venda);
                    
                    //dic.Add(nodeNudes[0].InnerText.Trim(), nodeNudes[2].InnerText.Trim());
                    comando = string.Format("INSERT INTO {0}(Cotacao, Data_Consulta) VALUES ({1} , {2});",
                        nodeAcoes[0].InnerText.Trim(), nodeAcoes[2].InnerText.Trim().Replace(',', '.'),
                        DateTime.Now.ToString("yyyyMMddHHmmss"));
                    ComandoSql(comando);
                }
                InsertDataConsulta(doc);
            }
            //return dic;
            return listaTransacoes;
        }
        public static string EfetuarNavegacao(string url)
        {
            WebRequest request = WebRequest.Create(url);
            WebResponse response = request.GetResponse();
            StreamReader responseStream = new StreamReader(response.GetResponseStream());
            string responseText = responseStream.ReadToEnd();
            response.Close();

            return responseText;
        }

        public static void ComandoSql(string s)
        {
            using (SqlConnection con = new SqlConnection(GetDBDirectory()))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand(s, con))
                    command.ExecuteNonQuery();
                con.Close();
            }
        }

        private static void InsertDataConsulta(HtmlDocument doc)
        {
            string s = string.Format("INSERT INTO DataConsulta(DataConsulta, DataSite) VALUES ('{0}' , '{1}');", 
                DateTime.Now.ToString("yyyyMMddHHmmss"), ConsultaDataConsulta(doc));
            ComandoSql(s);
        }

        public static string RetornaUltimaData()
        {
            string s = "SELECT * FROM DataConsulta WHERE Id = (SELECT TOP 1 Id FROM DataConsulta order by ID desc);";
            SqlConnection con = new SqlConnection(GetDBDirectory());
            con.Open();
            SqlCommand command = new SqlCommand(s, con);
            SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            var x = reader["DataSite"].ToString();
            con.Close();
            return x;
        }
        
        public static List<string> RetornaTodasAcoes()
        {
            List<string> lista = new List<string>();
            string s = "SELECT Sigla FROM SiglasAcoes;";
            SqlConnection con = new SqlConnection(GetDBDirectory());
            con.Open();
            SqlCommand command = new SqlCommand(s, con);
            SqlDataReader reader = command.ExecuteReader();
            while (reader.HasRows)
            {
                while (reader.Read())
                {
                    lista.Add(reader[0].ToString());
                }
                reader.NextResult();
            }
            con.Close();
            return lista;
        }

        private static string ConsultaDataConsulta(HtmlDocument doc)
        {
            var nodeDataConsultaSite = doc.DocumentNode.SelectSingleNode("//table[@id='ctl00_DefaultContent_GrdCarteiraIndice']/tr[2]/td[5]");
            return nodeDataConsultaSite.InnerText.Trim().Replace("/", string.Empty).Replace(" ", string.Empty);
        }

        public static List<string> GetListaSiglas(HtmlDocument doc)
        {
            List<string> listaSiglas = new List<string>();
            var nodeTabelaValores = doc.DocumentNode.SelectNodes("//table[tr/th[contains(text(),'Ativo')]]/tr/td[1]");
            foreach (var item in nodeTabelaValores)
            {
                listaSiglas.Add(item.InnerText.Trim());
            }
            return listaSiglas;
        }

        public static Dictionary<string, double> GetMedia(List<string> lista)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            var comando = "";
            foreach (var item in lista)
            {
                double d = GetMediaAcao(item, comando, result);
            }

            return result;
        }

        public static double GetMediaAcao(string item,
            string comando = "", 
            Dictionary<string, double> result = null)
        {
            comando = string.Format("SELECT AVG(Cotacao) as a FROM {0}", item);
            SqlConnection con = new SqlConnection(GetDBDirectory());
            con.Open();
            SqlCommand command = new SqlCommand(comando, con);
            SqlDataReader reader = command.ExecuteReader();
            reader.Read();
            string mediaStr = reader["a"].ToString();
            var match = Regex.Match(mediaStr, @"^[+-]?[0-9]{1,3}(?:,?[0-9]{3})*\,[0-9]{2}");
            var mediaCotacao = Double.Parse(match.ToString());
            if(result != null)
                result.Add(item, mediaCotacao);
            con.Close();
            return mediaCotacao;
        }

        public static string GetDBDirectory()
        {
            return string.Format(
                @"Data Source = (LocalDB)\MSSQLLocalDB; AttachDbFilename = {0}Acoes.mdf; Integrated Security = True",
                Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\")));
        }
    }
}
