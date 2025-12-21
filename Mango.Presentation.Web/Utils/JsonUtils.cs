using System.Text.Json;

namespace Mango.Presentation.Web.Utils
{

    public static class JsonUtils
    {
        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            // Ignora a diferença de maiúsculas/minúsculas (ex: couponCode vs CouponCode)
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Desserializa uma string JSON para o tipo de destino especificado (TResult), 
        /// que pode ser um objeto único (T) ou uma lista (List<T>).
        /// </summary>
        /// <typeparam name="TResult">O tipo de destino completo (ex: CouponsDto ou List<CouponsDto>).</typeparam>
        /// <param name="jsonString">A string JSON bruta a ser desserializada.</param>
        /// <returns>O objeto desserializado do tipo TResult, ou o valor default (geralmente null) em caso de erro ou JSON inválido.</returns>
        public static TResult? Deserialize<TResult>(string? jsonString) where TResult : class
        {
            // 1. Verificação de Nulidade e Vazio
            if (string.IsNullOrEmpty(jsonString))
            {
                Console.WriteLine($"JsonUtils: String JSON nula ou vazia para o tipo de retorno {typeof(TResult).Name}.");
                return default; // Retorna null
            }

            try
            {
                // 2. Desserialização
                // O tipo TResult é usado diretamente, seja ele T ou List<T>
                TResult? resultado = JsonSerializer.Deserialize<TResult>(jsonString, DefaultOptions);

                return resultado;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JsonUtils: Erro ao desserializar JSON para o tipo {typeof(TResult).Name}. Erro: {ex.Message}");
                return default; // Retorna null em caso de falha de formato
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JsonUtils: Erro inesperado. {ex.Message}");
                return default;
            }
        }
    }


}
