namespace Questao5.Application
{
    using Questao5.Domain.DTO;
    using Questao5.Domain.Entities;
    using Questao5.Infrastructure.Database;
    using Questao5.Infrastructure.Sqlite;

    public class ServicesLayer
    {
        private DAO camadaDAO;
        public ServicesLayer(DatabaseConfig db) {
            camadaDAO = new DAO(db);

        }   
        public async Task<string> Movimentar(RequestDTO Request)
        {
            return await camadaDAO.Movimentar(Request);
        }
        public async Task<ContaDTO> ObterSaldo(int NumeroConta)
        {
            return await camadaDAO.ObterSaldo(NumeroConta);
        }
        public void RegistrarTransacao(string chaveIdempotencia, string requisicao, string resultado)
        {
            camadaDAO.RegistrarTransacao(chaveIdempotencia, requisicao, resultado);
        }

    }
}
