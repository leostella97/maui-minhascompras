namespace MinhasCompras
{
    public partial class App : Application
    {
        // propriedade estatica que guarda o caminho completo do banco de dados
        public static string CaminhoBancoDados { get; private set; } = string.Empty;

        // construtor da aplicacao responsavel por preparar o banco de dados
        public App()
        {
            // registra no log o inicio da construcao do App para diagnostico
            MauiProgram.RegistrarLog("App.ctor iniciado");

            // inicializa os componentes visuais definidos no App.xaml
            InitializeComponent();

            // caminho fixo da pasta ArquivoDados que fica na raiz do projeto
            // assim o banco fica visivel dentro da pasta do projeto e nao escondido no pacote MSIX
            string pastaDados = @"C:\Users\MarBrasil\source\repos\maui-minhascompras\MinhasCompras\ArquivoDados";

            // cria a pasta ArquivoDados se ela ainda nao existir
            if (!Directory.Exists(pastaDados))
            {
                Directory.CreateDirectory(pastaDados);
            }

            // monta o caminho do banco dentro da pasta ArquivoDados do projeto
            CaminhoBancoDados = Path.Combine(pastaDados, "Dados.db");

            // registra no log o caminho do banco para confirmar a inicializacao
            MauiProgram.RegistrarLog($"App.CaminhoBancoDados={CaminhoBancoDados}");

            // define a pagina inicial do aplicativo
            MainPage = new AppShell();

            // registra no log o fim da construcao do App para diagnostico
            MauiProgram.RegistrarLog("App.ctor finalizado");
        }
    }
}
