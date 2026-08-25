using System.Collections.ObjectModel; // importa a ObservableCollection que avisa a tela quando a lista muda
using MinhasCompras.Helpers; // importa o helper de acesso ao banco
using MinhasCompras.Models; // importa a classe Produto usada na listagem

namespace MinhasCompras.Views // agrupa as telas do aplicativo
{
    public partial class ListaProduto : ContentPage // tela que lista os produtos cadastrados
    {
        readonly SQLiteDatabaseHelper bancoDados; // helper responsavel pelas operacoes no banco

        // colecao que guarda os produtos mostrados na tela
        // usei ObservableCollection porque ela avisa sozinha a CollectionView quando adiciona ou remove itens
        readonly ObservableCollection<Produto> produtosExibidos = new();

        // construtor da tela de listagem de produtos
        public ListaProduto()
        {
            InitializeComponent(); // liga o arquivo xaml a esta classe

            bancoDados = new SQLiteDatabaseHelper(App.CaminhoBancoDados); // prepara o helper com o caminho do banco

            // amarra a colecao na CollectionView pra atualizar sozinha quando mudar
            ListaProdutos.ItemsSource = produtosExibidos;
        }

        // metodo chamado sempre que a tela aparece para atualizar a lista
        protected override void OnAppearing()
        {
            base.OnAppearing(); // executa o comportamento padrao da classe base

            _ = CarregarProdutos(); // recarrega os produtos do banco
        }

        // busca todos os produtos do banco e preenche a colecao da tela
        private async Task CarregarProdutos()
        {
            List<Produto> produtos = await bancoDados.ObterTodosProdutos(); // obtem a lista completa do banco

            produtosExibidos.Clear(); // limpa o que tinha antes pra nao duplicar

            foreach (Produto p in produtos) // percorre cada produto retornado do banco
            {
                produtosExibidos.Add(p); // adiciona na colecao que esta amarrada na tela
            }
        }

        // metodo chamado toda vez que o usuario digita ou apaga algo na barra de busca
        // fiz async pra nao travar a tela enquanto consulta o banco
        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string termo = e.NewTextValue; // pega o texto atual digitado na busca

            // se o campo estiver vazio ou so espaco, mostra tudo de novo
            if (string.IsNullOrWhiteSpace(termo))
            {
                await CarregarProdutos(); // recarrega a lista completa do banco
                return; // sai do metodo pra nao rodar a busca filtrada
            }

            // consulta o banco filtrando pela descricao com LIKE
            List<Produto> resultado = await bancoDados.BuscarProdutos(termo);

            produtosExibidos.Clear(); // limpa a lista anterior antes de mostrar o resultado

            foreach (Produto p in resultado) // percorre cada produto encontrado na busca
            {
                produtosExibidos.Add(p); // adiciona na colecao e a tela atualiza sozinha
            }

            // se a busca nao retornou nada a lista fica vazia automaticamente
            // porque limpei acima e nao adicionou nenhum item
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
