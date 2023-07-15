using System.Globalization;
using System.Runtime.CompilerServices;
using System;

namespace Questao1
{
    class ContaBancaria {
        const double tarifaSaque = 3.5;
        public int Numero { get; set; }
        public string Titular { get; set; }
        public double DepositoInicial { get; set; }
        public double Saldo { get; set; }
        
        public ContaBancaria(int numero, string titular)
        {
            this.Numero = numero;
            this.Titular = titular;
        }
        public ContaBancaria(int numero, string titular, double depositoInicial)
        {
            if (this.Numero != 0 &&  numero!=this.Numero)
                throw new Exception("Você Não pode Mudar o número da conta.");
            
            this.Numero=numero;
            this.Titular=titular;
            this.DepositoInicial = depositoInicial;
            this.Saldo = depositoInicial;
        }
        public  void Deposito(double valor) {

            this.Saldo += valor;
        }
        public  void Saque(double valor) {

            this.Saldo -=(tarifaSaque+ valor);
        }

    }
}
