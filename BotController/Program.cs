namespace TelegramBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var app = webBuilder.UseStartup<Startup>();
                    var port = Environment.GetEnvironmentVariable("PORT");
                    if (port != null)
                    {
                        app.UseUrls($"http://*:{port}");
                    }
                });
        }
    }
}