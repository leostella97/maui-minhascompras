using MinhasCompras.Helpers; // importa o helper de acesso ao banco
using MinhasCompras.Models; // importa a classe Produto usada no cadastro

namespace MinhasCompras.Views // agrupa as telas do aplicativo
{
    [QueryProperty(nameof(IdProduto), "id")] // recebe o parametro id vindo da navegacao
    public partial class NovoProduto : ContentPage // tela de cadastro e edicao de produtos
    {
        public int IdProduto { get; set; } // guarda o id do produto em edicao ou zero para novo produto

        readonly SQLiteDatabaseHelper bancoDados; // helper responsavel pelas operacoes no banco

        // construtor da tela de cadastro de produtos
        public NovoProduto()
        {
            InitializeComponent(); // liga o arquivo xaml a esta classe

            bancoDados = new SQLiteDatabaseHelper(App.CaminhoBancoDados); // prepara o helper com o caminho do banco
        }

        // metodo chamado sempre que a tela aparece para carregar os dados em modo edicao
        protected override void OnAppearing()
        {
            base.OnAppearing(); // executa o comportamento padrao da classe base

            _ = PreencherCamposSeEdicao(); // preenche os campos quando esta editando
        }

        // preenche os campos com os dados do produto que esta sendo editado
        private async Task PreencherCamposSeEdicao()
        {
            if (IdProduto == 0) // verifica se nao ha produto em edicao
                return; // encerra o metodo sem preencher os campos

            List<Produto> produtos = await bancoDados.ObterTodosProdutos(); // carrega todos os produtos do banco

            Produto? produto = produtos.FirstOrDefault(p => p.Id == IdProduto); // localiza o produto pelo id

            if (produto == null) // verifica se o produto nao foi encontrado
                return; // encerra o metodo sem preencher os campos

            CampoDescricao.Text = produto.Descricao; // exibe a descricao no campo de texto

            CampoQuantidade.Text = produto.Quantidade.ToString(); // exibe a quantidade no campo de texto

            CampoPreco.Text = produto.Preco.ToString(); // exibe o preco no campo de texto
        }

        // salva o produto no banco e volta para a tela de listagem
        private async void AoSalvarProduto(object sender, EventArgs e)
        {
            int quantidade = int.TryParse(CampoQuantidade.Text, out int valorQuantidade) ? valorQuantidade : 0; // converte a quantidade digitada para inteiro

            double preco = double.TryParse(CampoPreco.Text, out double valorPreco) ? valorPreco : 0; // converte o preco digitado para decimal

            if (string.IsNullOrWhiteSpace(CampoDescricao.Text)) // verifica se a descricao ficou vazia
            {
                await DisplayAlert("Aviso", "Informe a descricao do produto", "OK"); // avisa o usuario sobre a descricao vazia

                return; // encerra o metodo sem salvar
            }

            if (IdProduto == 0) // verifica se esta cadastrando um novo produto
            {
                await bancoDados.InserirProduto(CampoDescricao.Text, quantidade, preco); // insere o novo produto no banco
            }
            else // caso contrario esta atualizando um produto existente
            {
                Produto produto = new Produto // monta o objeto com os dados atualizados
                {
                    Id = IdProduto, // mantem o id do produto em edicao
                    Descricao = CampoDescricao.Text, // preenche a descricao atualizada
                    Quantidade = quantidade, // preenche a quantidade atualizada
                    Preco = preco // preenche o preco atualizado
                };

                await bancoDados.AtualizarProduto(produto); // grava as alteracoes no banco
            }

            await Shell.Current.GoToAsync(".."); // volta para a tela anterior
        }
    }
}
