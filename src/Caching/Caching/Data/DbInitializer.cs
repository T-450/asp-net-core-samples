namespace Caching.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Look for any students.
            if (context.WeatherForecasts.Any())
            {
                return;   // DB has been seeded
            }
            var Summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };
            var weatherForecastsList = new List<WeatherForecast>();
            var rand = new Random();
            foreach (string summary in Summaries)
            {
                var weatherForecast = new WeatherForecast
                { Date = DateTime.Now, Summary = summary, TemperatureC = rand.Next(100) };
                weatherForecastsList.Add(weatherForecast);
            }

            context.WeatherForecasts.AddRange(weatherForecastsList);
            context.SaveChanges();
        }
    }
}