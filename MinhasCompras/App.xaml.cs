namespace MinhasCompras
{
    public partial class App : Application
    {
        // propriedade estatica que guarda o caminho completo do banco de dados
        public static string CaminhoBancoDados { get; private set; } = string.Empty;

        // construtor da aplicacao responsavel por preparar o banco de dados
        public App()
        {
            // inicializa os componentes visuais definidos no App.xaml
            InitializeComponent();

            // monta o caminho do banco unindo a pasta local de dados com o nome do arquivo
            CaminhoBancoDados = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Dados.db");

            // define a pagina inicial do aplicativo
            MainPage = new AppShell();
        }
    }
}
