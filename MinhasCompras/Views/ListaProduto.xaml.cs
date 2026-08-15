using MinhasCompras.Helpers; // importa o helper de acesso ao banco
using MinhasCompras.Models; // importa a classe Produto usada na listagem

namespace MinhasCompras.Views // agrupa as telas do aplicativo
{
    public partial class ListaProduto : ContentPage // tela que lista os produtos cadastrados
    {
        readonly SQLiteDatabaseHelper bancoDados; // helper responsavel pelas operacoes no banco

        // construtor da tela de listagem de produtos
        public ListaProduto()
        {
            InitializeComponent(); // liga o arquivo xaml a esta classe

            bancoDados = new SQLiteDatabaseHelper(App.CaminhoBancoDados); // prepara o helper com o caminho do banco
        }

        // metodo chamado sempre que a tela aparece para atualizar a lista
        protected override void OnAppearing()
        {
            base.OnAppearing(); // executa o comportamento padrao da classe base

            _ = CarregarProdutos(); // recarrega os produtos do banco
        }

        // busca todos os produtos do banco e preenche a collectionview
        private async Task CarregarProdutos()
        {
            List<Produto> produtos = await bancoDados.ObterTodosProdutos(); // obtem a lista completa do banco

            ListaProdutos.ItemsSource = produtos; // exibe a lista na collectionview
        }

        // abre a tela de cadastro de um novo produto
        private async void AoAdicionarProduto(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(NovoProduto)); // navega para a rota da tela de cadastro
        }

        // abre a tela de cadastro carregando o produto escolhido para edicao
        private async void AoEditarProduto(object sender, EventArgs e)
        {
            Button botao = (Button)sender; // captura o botao que disparou o clique

            Produto produto = (Produto)botao.CommandParameter; // recupera o produto associado ao botao

            await Shell.Current.GoToAsync($"{nameof(NovoProduto)}?id={produto.Id}"); // navega passando o id do produto
        }

        // exclui o produto escolhido apos a confirmacao do usuario
        private async void AoExcluirProduto(object sender, EventArgs e)
        {
            Button botao = (Button)sender; // captura o botao que disparou o clique

            Produto produto = (Produto)botao.CommandParameter; // recupera o produto associado ao botao

            bool confirmado = await DisplayAlert("Excluir", $"Deseja excluir {produto.Descricao}?", "Sim", "Nao"); // pede confirmacao ao usuario

            if (!confirmado) // verifica se o usuario cancelou a exclusao
                return; // encerra o metodo sem excluir

            await bancoDados.DeletarProduto(produto.Id); // remove o produto do banco

            await CarregarProdutos(); // atualiza a lista exibida na tela
        }
    }
}
