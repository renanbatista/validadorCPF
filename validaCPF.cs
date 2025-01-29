using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System;

namespace validaCPF
{
    public static class ValidaCPF
    {
        [FunctionName("validaCPF")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Validação de CPF em progresso.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (requestBody == null)
            {
                return new BadRequestObjectResult(new { message = "Entre com o CPF." });
            }

            // Deserialize the JSON body to extract the CPF
            CpfRequest data = null;
            try
            {
                data = JsonSerializer.Deserialize<CpfRequest>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return new BadRequestObjectResult(new { message = "Erro: CPF inválido." });
            }

            if (data == null || string.IsNullOrWhiteSpace(data.Cpf))
            {
                return new BadRequestObjectResult(new { message = "Entre com o CPF." });
            }

            bool isValid = ValidateCpf(data.Cpf);
            if (isValid)
            {
                return new OkObjectResult("Parabéns, CPF regular.");
            }
            else
            {
                return new BadRequestObjectResult(new { message = "CPF inválido." });
            }
        }

        private static bool ValidateCpf(string cpf)
        {
            cpf = Regex.Replace(cpf, @"\D", "");

            if (cpf.Length != 11 || Regex.IsMatch(cpf, @"^(\d)\1{10}$"))
            {
                return false;
            }

            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += (cpf[i] - '0') * (10 - i);
            }

            int remainder = sum % 11;
            int firstCheckDigit = remainder < 2 ? 0 : 11 - remainder;

            if (cpf[9] - '0' != firstCheckDigit)
            {
                return false;
            }

            sum = 0;
            for (int i = 0; i < 10; i++)
            {
                sum += (cpf[i] - '0') * (11 - i);
            }

            remainder = sum % 11;
            int secondCheckDigit = remainder < 2 ? 0 : 11 - remainder;

            return cpf[10] - '0' == secondCheckDigit;
        }

        public class CpfRequest
        {
            public string Cpf { get; set; }
        }
    }
}
