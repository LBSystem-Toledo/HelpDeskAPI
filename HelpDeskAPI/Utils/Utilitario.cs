using System;

namespace HelpDeskAPI.Utils
{
    public static class Utilitario
    {
        public static int Mod11(string vValor, int vBase, bool vResto, int vVl_complento)
        {
            int soma = 0;
            int peso = 2;
            for (int i = (vValor.Trim().Length - 1); i >= 0; i--)
            {
                soma += Convert.ToInt32(vValor.Trim()[i].ToString()) * peso;
                if (peso < vBase)
                    peso += 1;
                else
                    peso = 2;
            }
            if (vResto)
                return (soma + vVl_complento) % 11;
            else
            {
                int ret = ((soma + vVl_complento) % 11);
                int digito = 11 - ret;
                if (digito > 9)
                    return 0;
                else
                    return digito;
            }
        }
    }
}
