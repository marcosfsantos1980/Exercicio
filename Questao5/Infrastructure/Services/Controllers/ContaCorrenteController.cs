using IdempotentAPI.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Questao5.Domain.DTO;
using Questao5.Application;
using Questao5.Infrastructure.Sqlite;

namespace Questao5.Infrastructure.Services.Controllers
{
    [Route("api/[controller]")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [Idempotent(Enabled = true)]
    [ApiController]
    
    public class ContaCorrenteController : ControllerBase
    { 
        private readonly DatabaseConfig _databaseConfig;
        public ContaCorrenteController(DatabaseConfig databaseConfig) {
            _databaseConfig = databaseConfig;
        }

        [HttpPost, Route("MovimentarConta")]        
        public  async Task<ActionResult<string>> MovimentarConta([FromBody] RequestDTO Requisicao)
        {
            ServicesLayer servicesLayer = new ServicesLayer(_databaseConfig);
            try
            {
                var movimenta = await servicesLayer.Movimentar(Requisicao);
                return Ok(movimenta);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }
            
        }
        [HttpGet, Route("ObterSaldoConta")]
        public async Task<ActionResult<ContaDTO>> ObterSadolConta(int numeroConta )
        {
            try
            {
                ServicesLayer servicesLayer = new ServicesLayer(_databaseConfig);
                var saldo = await servicesLayer.ObterSaldo(numeroConta);
                return Ok(saldo);
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }
    }
}
