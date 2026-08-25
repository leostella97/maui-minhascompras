using Microsoft.Extensions.Logging;

namespace MinhasCompras
{
    public static class MauiProgram
    {
        // CAminho do arquivo de log usado para diagnosticar falhas silenciosas
        public static string CaminhoLog { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "minhascompras.log");

        // Método estatico que registra uma linha no arquivo de log com horario
        public static void RegistrarLog(string mensagem)
        {
            try
            {
                File.AppendAllText(CaminhoLog, $"{DateTime.Now:HH:mm:ss.fff} - {mensagem}{Environment.NewLine}");
            }
            catch
            {
                // Ignora falhas de escrita do log para nao quebrar o app
            }
        }

        public static MauiApp CreateMauiApp()
        {
            // registra os tratadores de excecoes nao tratadas para capturar falhas silenciosas
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                RegistrarLog($"AppDomain UnhandledException: {e.ExceptionObject}");

            TaskScheduler.UnobservedTaskException += (s, e) =>
                RegistrarLog($"TaskScheduler UnobservedTaskException: {e.Exception}");

            RegistrarLog("MauiProgram.CreateMauiApp iniciado");

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            RegistrarLog("MauiProgram.CreateMauiApp finalizado");
            return builder.Build();
        }
    }
}
