namespace TelegramBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"[COLD START] Process started at {DateTime.UtcNow:O}");
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseDefaultServiceProvider((context, options) =>
                {
                    var isDev = context.HostingEnvironment.IsDevelopment();
                    options.ValidateScopes = isDev;
                    options.ValidateOnBuild = isDev; 
                })
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