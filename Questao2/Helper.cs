using System.Text;
using System.Text.Json;

namespace Questao2
{
    public interface IHelperAPI
    {
       Task<ApiRetorno> Consultar(string team, int year, int page =1, bool guest =false);
        
    }
    /// <summary>
    /// Classe para chamada da API
    /// </summary>
    public class HelperAPI: IHelperAPI
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerOptions _options;

        private const string endpointApi = "https://jsonmock.hackerrank.com/api/";
        public HelperAPI(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }
        public async Task<ApiRetorno> Consultar(string team, int year, int page = 1, bool guest = false)
        {
            ApiRetorno retorno = new ApiRetorno { Sucess = true, Mensagem = "OK" };
            try
            {
                int nGuest = (guest) ? 2 : 1;
                string sPars = $"?year={year}&team{nGuest}={team}&page={page}";
                string serviceName = "football_matches";

                var cliente = this._clientFactory.CreateClient("Api");
                cliente.BaseAddress = new System.Uri(endpointApi);                
                string urlServico =  serviceName  + sPars;
                
                using (var response = await cliente.GetAsync(urlServico))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = await response.Content.ReadAsStreamAsync();
                        retorno.Data = await JsonSerializer
                                   .DeserializeAsync<ApiFootball>(apiResponse, _options);
                    }
                    else
                    {
                        return new ApiRetorno { Data = null, Sucess = false, Mensagem = "Erro no retorno da Request da API"};
                    }
                }

            }
            catch (Exception e)
            {
                return new ApiRetorno { Data = null, Sucess = false, Mensagem = e.Message };

            }
            return retorno;

        }

    }
   // Auxiliares para tratamento de retorno da API
    public class ApiRetorno
    {
        public bool Sucess { get; set; }
        public object? Data { get; set; }
        public string? Mensagem { get; set; }
    }
    /// <summary>
    /// Cast do Json de retorno 
    /// </summary>
    public class ApiFootball
    {
        public int page { get; set; }
        public int per_page { get; set; }
        public int total { get; set; }
        public int total_pages { get; set; }
        public Results[] data { get; set; }
    }

    public class Results
    {
        public string competition { get; set; }
        public int year { get; set; }
        public string round { get; set; }
        public string team1 { get; set; }
        public string team2 { get; set; }
        public string team1goals { get; set; }
        public string team2goals { get; set; }
    }

}
