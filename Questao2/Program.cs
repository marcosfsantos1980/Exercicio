using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Questao2;
using System.Reflection.Metadata;

public class Program
{
    public static async Task Main()
    {

        var builder = new HostBuilder()
        .ConfigureServices((hostContext, services) =>
        {               
            services.AddHttpClient();                  
            services.AddTransient<IHelperAPI, HelperAPI>();
        }).UseConsoleLifetime();

        var host = builder.Build();
        var servico = host.Services.GetRequiredService<IHelperAPI>();
        

        string teamName = "Paris Saint-Germain";
        int year = 2013;
        
        int totalGoals = await getTotalScoredGoals(servico, teamName, year);
        Console.WriteLine("Team "+ teamName +" scored "+ totalGoals.ToString() + " goals in "+ year);

        teamName = "Chelsea";
        year = 2014;
        totalGoals = await getTotalScoredGoals(servico, teamName, year);

        Console.WriteLine("Team " + teamName + " scored " + totalGoals.ToString() + " goals in " + year);
        Console.ReadLine();
        // Output expected:
        // Team Paris Saint - Germain scored 109 goals in 2013
        // Team Chelsea scored 92 goals in 2014
    }

    public static async Task<int> getTotalScoredGoals(IHelperAPI servico, string team, int year)
    {
        int goals = 0;        
        try
        {
            // Soma os gols como mandante
            var retorno = await servico.Consultar(team, year);
            if (retorno != null && retorno.Data != null)
            {
                ApiFootball dados = (ApiFootball)retorno.Data;
                for (int i = 1; i <= dados.total_pages; i++)
                {
                    if (i > 1)
                    {
                        retorno = await servico.Consultar(team, year, i);
                        if (retorno != null && retorno.Data != null)
                            dados = (ApiFootball)retorno.Data;
                        else
                            break;
                    }
                    goals += dados.data.Sum(x => int.Parse(x.team1goals));                    

                }
            }
            // Soma os Gols como  Visitante -> passando   Team2
            retorno = await servico.Consultar(team, year,1,true);
            if (retorno != null && retorno.Data != null)
            {
                ApiFootball dados = (ApiFootball)retorno.Data;
                for (int i = 1; i <= dados.total_pages; i++)
                {
                    if (i > 1)
                    {
                        retorno = await servico.Consultar(team, year, i,true);
                        if (retorno != null && retorno.Data != null)
                            dados = (ApiFootball)retorno.Data;
                        else
                            break;
                    }
                    goals += dados.data.Sum(x => int.Parse(x.team2goals));

                }
            }


        }
        catch (Exception)
        {

            throw;
        }      

        return goals;
    }

}