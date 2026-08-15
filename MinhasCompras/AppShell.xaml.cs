using MinhasCompras.Views; // importa as telas que serao registradas nas rotas

namespace MinhasCompras // namespace principal do aplicativo
{
    public partial class AppShell : Shell // shell que organiza a navegacao do aplicativo
    {
        // construtor do shell que registra as rotas de navegacao
        public AppShell()
        {
            InitializeComponent(); // inicializa os componentes visuais definidos no AppShell.xaml

            Routing.RegisterRoute(nameof(NovoProduto), typeof(NovoProduto)); // registra a rota da tela de cadastro de produtos
        }
    }
}
