using System.Text;
using System.Text.Json;

namespace WEBAPPmesaGo.Services
{
    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public GeminiService(IConfiguration configuration)
        {
            _apiKey = configuration["GoogleGemini:ApiKey"];
            _httpClient = new HttpClient();
        }

        // 1. GENERAR DESCRIPCIÓN CON IA (Usando Gemini Pro)
        public async Task<string> GenerarDescripcion(string nombrePlato, string ingredientes)
        {
            try
            {
                // Prompt mejorado para forzar el uso de ingredientes
                string ingredientesTexto = string.IsNullOrWhiteSpace(ingredientes) ? "los ingredientes tradicionales" : ingredientes;
                string prompt = $"Actúa como un chef experto en marketing. Escribe una descripción deliciosa y persuasiva (máximo 25 palabras) para el plato: '{nombrePlato}'. \n\nREQUISITO OBLIGATORIO: Debes mencionar explícitamente y resaltar estos ingredientes clave: {ingredientesTexto}. \n\nEstilo: Apetitoso, elegante y directo. Sin saludos, solo la descripción.";
                return await CallGeminiApi(prompt);
            }
            catch (Exception ex)
            {
                // LOGGING COMPLETO DEL ERROR
                Console.WriteLine($"[GEMINI EXCEPTION] {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"[INNER] {ex.InnerException.Message}");
                
                // Retornar el error visiblemente para depurar
                return $"[ERROR IA] No se pudo generar. Detalle: {ex.Message}";
            }
        }

        // 2. GENERAR IMAGEN (Mantenemos Pollinations.ai o similar si se desea, o solo para texto)
        // El usuario solo pidió cambiar la descripción ("para que funcione la descripción con ia")
        // Pero el servicio anterior tenía GenerarImagenUrl. Lo mantenemos igual para no romper nada.
        public Task<string> GenerarImagenUrl(string nombrePlato)
        {
            try 
            {
                string promptVisual = $"{nombrePlato} delicious food photorealistic 4k high quality";
                string promptCodificado = Uri.EscapeDataString(promptVisual);
                int seed = new Random().Next(1, 99999);
                return Task.FromResult($"https://image.pollinations.ai/prompt/{promptCodificado}?nologo=true&seed={seed}&width=800&height=600");
            }
             catch
            {
                 return Task.FromResult("/img/default_dish.png");
            }
        }

        // --- CONEXIÓN NUEVA: POLLINATIONS.AI (Gratis, Sin Key) ---
        private async Task<string> CallGeminiApi(string textPrompt)
        {
            // Pollinations.ai para texto funciona con un simple GET: https://text.pollinations.ai/TU_TEXTO
            // No requiere JSON body ni API Key.
            
            try 
            {
                // Limpiamos el prompt para URL
                string promptCodificado = Uri.EscapeDataString(textPrompt);
                var url = $"https://text.pollinations.ai/{promptCodificado}";

                // Hacemos la llamada simple
                var responseString = await _httpClient.GetStringAsync(url);
                
                // Pollinations devuelve el texto directo, sin JSON complejo
                if (string.IsNullOrWhiteSpace(responseString))
                {
                    return "La IA no devolvió ninguna descripción.";
                }

                return responseString.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[POLLINATIONS ERROR] {ex.Message}");
                throw new Exception($"Error consultando IA gratuita: {ex.Message}");
            }
        }
    }
}
