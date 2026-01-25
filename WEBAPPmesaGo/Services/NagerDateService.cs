using System.Text.Json;

namespace WEBAPPmesaGo.Services
{
    public class Feriado
    {
        public DateTime Date { get; set; }
        public string LocalName { get; set; }
        public string Name { get; set; }
    }

    public class NagerDateService
    {
        private readonly HttpClient _httpClient;

        public NagerDateService()
        {
            _httpClient = new HttpClient();
        }

        // Método para obtener los feriados de Ecuador del año actual
        public async Task<List<Feriado>> ObtenerFeriadosEcuador(int year)
        {
            try
            {
                // Consultamos la API pública de Nager.Date para Ecuador (EC)
                string url = $"https://date.nager.at/api/v3/publicholidays/{year}/EC";

                var response = await _httpClient.GetStringAsync(url);
                var feriados = JsonSerializer.Deserialize<List<Feriado>>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return feriados ?? new List<Feriado>();
            }
            catch
            {
                // Si falla la conexión (ej. sin internet), devolvemos lista vacía para que no explote
                return new List<Feriado>();
            }
        }

        // Método rápido para saber si HOY es feriado
        public async Task<bool> EsFeriadoHoy()
        {
            var feriados = await ObtenerFeriadosEcuador(DateTime.Now.Year);
            var hoy = DateTime.Now.Date;
            return feriados.Any(f => f.Date.Date == hoy);
        }
    }
}