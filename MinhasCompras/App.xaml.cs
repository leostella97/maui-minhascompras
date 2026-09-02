namespace MinhasCompras
{
    public partial class App : Application
    {
        // propriedade estática que guarda o caminho completo do banco de dados
        // o set é público porque a tela troca esse caminho quando a pessoa procura outro banco
        public static string CaminhoBancoDados { get; set; } = string.Empty;

        // construtor da aplicação responsável por definir o caminho do banco
        public App()
        {
            // registra no log o início da construção do App para diagnostico
            MauiProgram.RegistrarLog("App.ctor iniciado");

            // inicializa os componentes visuais definidos no App.xaml
            InitializeComponent();

            // caminho da pasta ArquivoDados que fica na raiz do projeto
            // o arquivo Dados.db deve ja estar la, o app nao cria um banco novo
            string pastaDados = @"C:\Users\MarBrasil\source\repos\maui-minhascompras\MinhasCompras\ArquivoDados";

            // monto o caminho padrão do banco dentro da pasta ArquivoDados
            // se esse arquivo nao existir, a tela mostra o aviso vermelho e o botao pra procurar
            CaminhoBancoDados = Path.Combine(pastaDados, "Dados.db");

            // registra no log o caminho do banco para confirmar a inicialização
            MauiProgram.RegistrarLog($"App.CaminhoBancoDados={CaminhoBancoDados}");

            // define a pagina inicial do aplicativo
            MainPage = new AppShell();

            // registra no log o fim da construcao do App para diagnostico
            MauiProgram.RegistrarLog("App.ctor finalizado");
        }
    }
}
