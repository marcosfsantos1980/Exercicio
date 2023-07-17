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
        public async Task <string> Movimentar(RequestDTO Request)
        {
            return await databaseBootstrap.Movimentar(Request);
        }
        public async Task<ContaDTO> ObterSaldo(int NumeroConta)
        {
            return await databaseBootstrap.ObterSaldo(NumeroConta);
        }
        public  async void RegistrarTransacao(string chaveIdempotencia, string requisicao, string resultado)
        {
            databaseBootstrap.RegistrarTransacao(chaveIdempotencia, requisicao, resultado);
        }
    }
}
