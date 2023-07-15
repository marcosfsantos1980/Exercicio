using NSubstitute;
using Questao5.Domain.DTO;
using Questao5.Infrastructure.Sqlite;
using System.Data;

namespace Questao5.Infrastructure.Database
{
    public class DAO
    {
        DatabaseBootstrap databaseBootstrap;
        public DAO(DatabaseConfig db)
        {
            
            databaseBootstrap = new DatabaseBootstrap(db);
            
            
        }
        public string Movimentar(RequestDTO Request)
        {
            return databaseBootstrap.Movimentar(Request);
        }
        public ContaDTO ObterSaldo(int NumeroConta)
        {
            return databaseBootstrap.ObterSaldo(NumeroConta);
        }
        public void RegistrarTransacao(string chaveIdempotencia, string requisicao, string resultado)
        {
            databaseBootstrap.RegistrarTransacao(chaveIdempotencia, requisicao, resultado);
        }
    }
}
