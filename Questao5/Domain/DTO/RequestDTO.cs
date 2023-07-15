namespace Questao5.Domain.DTO
{
    using Domain.Enumerators;
    public class RequestDTO
    {
        public string RequestID { get; set; }
        public int NumeroContaCorrente { get; set; }
        public double Valor { get; set;}
        public string TipoMovimentacao { get; set; }
        
    }
}
