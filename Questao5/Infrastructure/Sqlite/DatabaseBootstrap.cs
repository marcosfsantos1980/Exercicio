using Dapper;
using Microsoft.Data.Sqlite;
using Questao5.Domain.Enumerators;
using Questao5.Domain.Entities;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using Questao5.Domain.DTO;
using Microsoft.Extensions.Options;
using NSubstitute.Routing.Handlers;

namespace Questao5.Infrastructure.Sqlite
{
    public class DatabaseBootstrap : IDatabaseBootstrap
    {
        private readonly DatabaseConfig databaseConfig;

        public DatabaseBootstrap(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }

        public void Setup()
        {
            using var connection = new SqliteConnection(databaseConfig.Name);

            var table = connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table' AND (name = 'contacorrente' or name = 'movimento' or name = 'idempotencia');");
            var tableName = table.FirstOrDefault();
            if (!string.IsNullOrEmpty(tableName) && (tableName == "contacorrente" || tableName == "movimento" || tableName == "idempotencia"))
                return;

            connection.Execute("CREATE TABLE contacorrente ( " +
                               "idcontacorrente TEXT(37) PRIMARY KEY," +
                               "numero INTEGER(10) NOT NULL UNIQUE," +
                               "nome TEXT(100) NOT NULL," +
                               "ativo INTEGER(1) NOT NULL default 0," +
                               "CHECK(ativo in (0, 1)) " +
                               ");");

            connection.Execute("CREATE TABLE movimento ( " +
                "idmovimento TEXT(37) PRIMARY KEY," +
                "idcontacorrente TEXT(37) NOT NULL," +
                "datamovimento TEXT(25) NOT NULL," +
                "tipomovimento TEXT(1) NOT NULL," +
                "valor REAL NOT NULL," +
                "CHECK(tipomovimento in ('C', 'D')), " +
                "FOREIGN KEY(idcontacorrente) REFERENCES contacorrente(idcontacorrente) " +
                ");");

            connection.Execute("CREATE TABLE idempotencia (" +
                               "chave_idempotencia TEXT(37) PRIMARY KEY," +
                               "requisicao TEXT(1000)," +
                               "resultado TEXT(1000));");

            connection.Execute("INSERT INTO contacorrente(idcontacorrente, numero, nome, ativo) VALUES('B6BAFC09-6967-ED11-A567-055DFA4A16C9', 123, 'Katherine Sanchez', 1);");
            connection.Execute("INSERT INTO contacorrente(idcontacorrente, numero, nome, ativo) VALUES('FA99D033-7067-ED11-96C6-7C5DFA4A16C9', 456, 'Eva Woodward', 1);");
            connection.Execute("INSERT INTO contacorrente(idcontacorrente, numero, nome, ativo) VALUES('382D323D-7067-ED11-8866-7D5DFA4A16C9', 789, 'Tevin Mcconnell', 1);");
            connection.Execute("INSERT INTO contacorrente(idcontacorrente, numero, nome, ativo) VALUES('F475F943-7067-ED11-A06B-7E5DFA4A16C9', 741, 'Ameena Lynn', 0);");
            connection.Execute("INSERT INTO contacorrente(idcontacorrente, numero, nome, ativo) VALUES('BCDACA4A-7067-ED11-AF81-825DFA4A16C9', 852, 'Jarrad Mckee', 0);");
            connection.Execute("INSERT INTO contacorrente(idcontacorrente, numero, nome, ativo) VALUES('D2E02051-7067-ED11-94C0-835DFA4A16C9', 963, 'Elisha Simons', 0);");
        }
        public async Task<string> Movimentar(RequestDTO Request)
        {
            string Tipo = Request.TipoMovimentacao ; int NumeroConta = Request.NumeroContaCorrente; double Valor = Request.Valor;

            if (Tipo.ToUpper() != TipoMovimento.Credito && Tipo.ToUpper() != TipoMovimento.Debito)          
                throw new Exception("INVALID_TYPE");
            
            if (Valor <= 0)
                throw new Exception("INVALID_VALUE");

            ContaCorrente contaCorrente = this.ObterContaCorrente(NumeroConta);

            if (contaCorrente == null)
                throw new Exception("INVALID_ACCOUNT");                                  

            if (contaCorrente.Ativo == 0)
                throw new Exception("INACTIVE_ACCOUNT");           

            if (TransacaoExistente(Request.RequestID))
                throw new Exception("INVALID_REQUEST_ID");

            string IdDoMovimento = Guid.NewGuid().ToString().ToUpper(); //Gera o GUID para o movimento
            string requisicao = System.Text.Json.JsonSerializer.Serialize<RequestDTO>(Request);

            try
            {                
                using var connection = new SqliteConnection(databaseConfig.Name);                
                
                if (connection.State == System.Data.ConnectionState.Closed)
                    connection.Open();

                SqliteParameter[] pars = new SqliteParameter[5];
                pars[0] = new SqliteParameter("IdMovimento", IdDoMovimento);
                pars[1] = new SqliteParameter("IdContacorrente", contaCorrente.IdContaCorrente);
                pars[2] = new SqliteParameter("DataMovimento", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                pars[3] = new SqliteParameter("TipoMovimento", Tipo.ToUpper());
                pars[4] = new SqliteParameter("Valor", Valor);

                //Não estava reconhecendo o parametro( algum problema na minha versão do Dapper X SQLLite)
                //var reg = connection.Execute("Insert Into movimento (idmovimento,idcontacorrente,datamovimento,tipomovimento,valor)Values(@IdMovimento,@IdContacorrente,@DataMovimento,@TipoMovimento,@Valor);", pars);
                // var reg = connection.Execute(sql, pars);

                string sql = $"Insert Into movimento (idmovimento,idcontacorrente,datamovimento,tipomovimento,valor)Values('{IdDoMovimento}','{contaCorrente.IdContaCorrente}','{DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}','{Tipo.ToUpper()}', {FixNumberToRecBD(Valor)})";
                var reg = await connection.ExecuteAsync(sql);
                this.RegistrarTransacao(Request.RequestID ,requisicao ,IdDoMovimento);
            }
            catch (Exception ex)
            {
                this.RegistrarTransacao(Request.RequestID, requisicao, $"ERRO: {ex.Message} ");
                throw new Exception("Erro durante a gravação dos dados ");
            }                       

            return IdDoMovimento;
        }
        public async Task <ContaDTO>ObterSaldo(int NumeroConta)
        {
            ContaCorrente contaCorrente = this.ObterContaCorrente(NumeroConta);

            if (contaCorrente == null)
                throw new Exception("INVALID_ACCOUNT");

            if (contaCorrente.Ativo == 0)
                throw new Exception("INACTIVE_ACCOUNT");

            using var connection = new SqliteConnection(databaseConfig.Name);
            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();

            SqliteParameter parameter = new SqliteParameter("IdContaCorrente", contaCorrente.IdContaCorrente);
            string sql = $"Select tipomovimento,  valor from movimento where  idcontacorrente ='{contaCorrente.IdContaCorrente}' ;";
            var movimento = await  connection.QueryAsync<Movimento>(sql,parameter);
            
            ContaDTO contaDTO = new ContaDTO { DataResposta = DateTime.Now, NomeTitular = contaCorrente.Nome, NumeroConta = contaCorrente.Numero, SaldoAtual = 0 };

            if(movimento!=null)             
            {
                var debitos = movimento.Where(x => x.TipoMovimento.Equals(TipoMovimento.Debito)).Sum(y => y.Valor);
                var creditos = movimento.Where(x => x.TipoMovimento.Equals(TipoMovimento.Credito)).Sum(y=>y.Valor);

                contaDTO.SaldoAtual = (creditos - debitos);
            }

            return contaDTO;

        }
        public async void RegistrarTransacao(string ChaveIdempotencia, string Requisicao, string Resultado)
        {
            using var connection = new SqliteConnection(databaseConfig.Name);
            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();
            Idempotencia idempotencia = new Idempotencia { Chave_Idempotencia= ChaveIdempotencia, Requisicao= Requisicao, Resulado=Resultado};
            SqliteParameter[] pars = new SqliteParameter[3];
            pars[0] = new SqliteParameter("Chave_Idempotencia", ChaveIdempotencia);
            pars[1] = new SqliteParameter("Requisicao", Requisicao);
            pars[2] = new SqliteParameter("Resultado",  Resultado);

            //Não estava reconhecendo o parametro( algum problema na minha versão do Dapper X SQLLite)
            //var reg = connection.Execute("Insert Into idempotencia (chave_idempotencia,requisicao,resultado)Values(@Chave_Idempotencia,@Requisicao,@Resultado);", pars);
            //var reg = connection.Execute(sql, pars);
            string sql = $"Insert Into idempotencia (chave_idempotencia,requisicao,resultado)Values('{ChaveIdempotencia}','{Requisicao}','{Resultado}');";
            var reg  = await connection.ExecuteAsync(sql);

        }
        private ContaCorrente ObterContaCorrente(int NumeroConta)
        {
            using var connection = new SqliteConnection(databaseConfig.Name);
            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();
            SqliteParameter parameter = new SqliteParameter("numero", NumeroConta);

            //Não estava reconhecendo o parametro( algum problema na minha versão do Dapper X SQLLite)
            //string  sql= @"Select idcontacorrente, numero, nome, ativo from contacorrente where numero = @numero;";

            string sql = $"Select idcontacorrente, numero, nome, ativo from contacorrente where numero = {NumeroConta};";
            var conta = connection.QueryFirstOrDefault<ContaCorrente>(sql,parameter);

            return conta;
        }
        private bool TransacaoExistente(string ReqId)
        {
            using var connection = new SqliteConnection(databaseConfig.Name);
            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();
            SqliteParameter parameter = new SqliteParameter("chave", ReqId);

            //Não estava reconhecendo o parametro( algum problema na minha versão do Dapper X SQLLite)
            //string  sql= @"select 1 from idempotencia where chave_idempotencia=@chave;";
            try
            {
                string sql = $"select count(1) from idempotencia where chave_idempotencia ='{ReqId}';";
                var conta = connection.QueryFirstOrDefault<int>(sql, parameter);
                if (conta == null)
                    return false;
                else
                    return (conta >0);
            }
            catch (Exception)
            {

                return false;
            }
          

        }
        private string FixNumberToRecBD(double Valor)
        {
            string ValorBD = Valor.ToString().Replace(".", "");
            ValorBD = ValorBD.Replace(",", ".");

            return ValorBD;
        }
       
    }
}
