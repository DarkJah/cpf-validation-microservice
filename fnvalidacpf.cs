using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FnValidacpf
{
    public static class FnValidacpf
    {
        [FunctionName("ValidateCpf")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "validate-cpf")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Iniciando a validação do CPF");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            if (data == null)
            {
                return new BadRequestObjectResult("Dados inválidos.");
            }
            
            string cpf = data?.cpf?.ToString();
            if (string.IsNullOrEmpty(cpf))
            {
                return new BadRequestObjectResult("CPF não fornecido.");
            }

            bool isValid = ValidateCpf(cpf);
            return new OkObjectResult(new { isValid = isValid, message = isValid ? "CPF válido." : "CPF inválido." });
        }

        public static bool ValidateCpf(string cpf)
        {
            if (string.IsNullOrEmpty(cpf))
                return false;

            // Remove caracteres especiais
            cpf = cpf.Trim().Replace(".", "").Replace("-", "").Replace(" ", "");

            // Verifica se tem 11 dígitos
            if (cpf.Length != 11)
                return false;

            // Verifica se todos os caracteres são números
            foreach (char c in cpf)
            {
                if (!char.IsDigit(c))
                    return false;
            }

            // Calcula o primeiro dígito verificador
            int soma = 0;
            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(cpf[i].ToString()) * (10 - i);
            }

            int resto = soma % 11;
            int digitoVerificador1 = resto < 2 ? 0 : 11 - resto;

            if (int.Parse(cpf[9].ToString()) != digitoVerificador1)
                return false;

            // Calcula o segundo dígito verificador
            soma = 0;
            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(cpf[i].ToString()) * (11 - i);
            }

            resto = soma % 11;
            int digitoVerificador2 = resto < 2 ? 0 : 11 - resto;

            return int.Parse(cpf[10].ToString()) == digitoVerificador2;
        }
    }
}
