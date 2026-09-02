using System.Collections.ObjectModel; // coleção que avisa a tela quando a lista muda
using Microsoft.Maui.Storage; // FilePicker pra pessoa escolher o arquivo do banco
using MinhasCompras.Helpers; // helper que lê o banco de dados
using MinhasCompras.Models; // classe Produto usada na listagem

namespace MinhasCompras.Views // telas do aplicativo
{
    public partial class ListaProduto : ContentPage // tela que lista os produtos cadastrados
    {
        // helper que faz as operações no banco
        // fica nulo quando o arquivo Dados.db nao existe
        SQLiteDatabaseHelper? bancoDados;

        // coleção que guarda os produtos mostrados na tela
        // usei ObservableCollection porque ela avisa sozinha a ListView quando adiciona ou remove itens
        readonly ObservableCollection<Produto> produtosExibidos = new();

        // construtor da tela de listagem de produtos
        public ListaProduto()
        {
            InitializeComponent(); // liga o arquivo xaml a esta classe

            // amarra a coleção na ListView pra atualizar sozinha quando mudar
            ListaProdutos.ItemsSource = produtosExibidos;
        }

        // método chamado sempre que a tela aparece pra conferir o banco e atualizar a lista
        protected override void OnAppearing()
        {
            base.OnAppearing(); // executa o comportamento padrão da classe base

            _ = VerificarBancoEAtualizar(); // verifica o banco e depois carrega os produtos
        }

        // ve se o arquivo do banco existe e mostra a lista quando ele esta disponivel
        private async Task VerificarBancoEAtualizar()
        {
            try
            {
                // o arquivo Dados.db precisa existir, o app não cria um banco novo
                // se o arquivo nao existir esse erro é mostradi pra cair no catch
                if (!File.Exists(App.CaminhoBancoDados))
                    throw new FileNotFoundException("Banco não encontrado", App.CaminhoBancoDados);

                // prepara o helper só com o caminho do banco que ja existe
                bancoDados = new SQLiteDatabaseHelper(App.CaminhoBancoDados);

                EsconderAvisoBancoAusente(); // achou o banco, entao esconde o aviso vermelho

                await CarregarProdutos(); // busca os produtos pra preencher a lista
            }
            catch (FileNotFoundException)
            {
                
                // se o arquivo do banco não é encontrado  mostra a mensagem vermelha e deixo o botão pra procurar o Dados.db
                MostrarAvisoBancoAusente();
            }
            catch (Exception)
            {
                // outro erro ao abrir o banco é avisado com mensagem simples
                await DisplayAlert("Ops", "Não consegui abrir o banco de dados. Tente de novo", "OK");
            }
        }

        // mostra o aviso vermelho e esconde os elementos que dependem do banco
        private void MostrarAvisoBancoAusente()
        {
            AvisoBancoAusente.IsVisible = true; // deixa o aviso e o botao aparecerem
            BuscaProdutos.IsVisible = false; // esconde a busca pra não tentar consultar
            BotaoNovoProduto.IsVisible = false; // esconde o botão de novo produto
            ListaProdutos.IsVisible = false; // esconde a lista que está vazia
        }

        // esconde o aviso e mostra a tela normal quando o banco existe
        private void EsconderAvisoBancoAusente()
        {
            AvisoBancoAusente.IsVisible = false; // tira o aviso vermelho da tela
            BuscaProdutos.IsVisible = true; // volta a mostrar a busca
            BotaoNovoProduto.IsVisible = true; // volta a mostrar o botão de novo produto
            ListaProdutos.IsVisible = true; // volta a mostrar a lista de produtos
        }

        // abre o seletor de arquivos do sistema pra pessoa achar o arquivo Dados.db
        private async void AoProcurarBanco(object sender, EventArgs e)
        {
            try
            {
                // configuro o seletor pra filtrar os arquivos de banco .db
                var opcoes = new PickOptions
                {
                    PickerTitle = "Selecione o arquivo Dados.db",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.WinUI, new[] { ".db" } },
                        { DevicePlatform.Android, new[] { "application/octet-stream" } },
                        { DevicePlatform.iOS, new[] { "public.database" } },
                        { DevicePlatform.MacCatalyst, new[] { "public.database" } }
                    })
                };

                // espera a pessoa escolher um arquivo na janela do sistema
                FileResult? arquivoEscolhido = await FilePicker.Default.PickAsync(opcoes);

                if (arquivoEscolhido == null) // ve se a pessoa cancelou a escolha
                    return; // nao faz nada e mantem o aviso na tela

                // guarda o caminho do banco escolhido pra todas as telas usarem
                App.CaminhoBancoDados = arquivoEscolhido.FullPath;

                // registra no log o caminho escolhido pra ajudar a diagnosticar
                MauiProgram.RegistrarLog($"Banco escolhido pelo usuario: {App.CaminhoBancoDados}");

                await VerificarBancoEAtualizar(); // tenta abrir o banco escolhido e carregar a lista
            }
            catch (Exception)
            {
                // se der erro ao abrir o seletor ou ler o arquivo, aviso com mensagem simples
                await DisplayAlert("Ops", "Não consegui abrir o arquivo do banco. Tente de novo", "OK");
            }
        }

        // busca todos os produtos do banco e preenche a colecao da tela
        private async Task CarregarProdutos()
        {
            if (bancoDados == null) // ve se o banco ainda nao foi aberto
                return; // nao tem o que carregar

            try
            {
                List<Produto> produtos = await bancoDados.ObterTodosProdutos(); // obtem a lista completa do banco

                produtosExibidos.Clear(); // limpa o que tinha antes pra nao duplicar

                foreach (Produto p in produtos) // percorre cada produto retornado do banco
                {
                    produtosExibidos.Add(p); // adiciona na colecao que esta amarrada na tela
                }
            }
            catch (Exception)
            {
                // se o banco falhar na hora de buscar, avisa o usuario com uma mensagem simples
                // assim o app nao quebra e a pessoa sabe o que fazer
                await DisplayAlert("Ops", "Não consegui buscar os produtos. Tente de novo", "OK");
            }
        }

        // metodo chamado toda vez que o usuario digita ou apaga algo na barra de busca
        private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (bancoDados == null) // ve se o banco nao esta disponivel
                return; // nao faz a busca porque o aviso esta na tela

            string termo = e.NewTextValue; // pega o texto atual digitado na busca

            // se o campo estiver vazio ou so espaco, mostra tudo de novo
            if (string.IsNullOrWhiteSpace(termo))
            {
                await CarregarProdutos(); // recarrega a lista completa do banco
                return; // sai do metodo pra nao rodar a busca filtrada
            }

            try
            {
                // consulta o banco filtrando pela descricao com LIKE
                List<Produto> resultado = await bancoDados.BuscarProdutos(termo);

                produtosExibidos.Clear(); // limpa a lista anterior antes de mostrar o resultado

                foreach (Produto p in resultado) // percorre cada produto encontrado na busca
                {
                    produtosExibidos.Add(p); // adiciona na colecao e a tela atualiza sozinha
                }
            }
            catch (Exception)
            {
                // se der problema na busca, avisa o usuario com uma mensagem simples
                await DisplayAlert("Ops", "Não consegui buscar os produtos. Tente de novo", "OK");
            }

            // se a busca nao retornou nada a lista fica vazia automaticamente
            // porque limpei acima e nao adicionou nenhum item
        }

        // abre a tela de cadastro de um novo produto
        private async void AoAdicionarProduto(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(NovoProduto)); // navega para a rota da tela de cadastro
        }

        // metodo chamado quando o usuario toca em um item da ListView
        private async void AoSelecionarProduto(object sender, ItemTappedEventArgs e)
        {
            // verifica se o item tocado e mesmo um produto
            if (e.Item is Produto produto)
            {
                // navega para a tela de cadastro levando o id pra editar esse produto
                await Shell.Current.GoToAsync($"{nameof(NovoProduto)}?id={produto.Id}");
            }
        }

        // abre a tela de cadastro carregando o produto escolhido no menu de contexto
        private async void AoEditarProduto(object sender, EventArgs e)
        {
            MenuItem menu = (MenuItem)sender; // captura o item do menu que disparou o clique

            Produto produto = (Produto)menu.CommandParameter; // recupera o produto associado ao menu

            await Shell.Current.GoToAsync($"{nameof(NovoProduto)}?id={produto.Id}"); // navega passando o id do produto
        }

        // exclui o produto escolhido no menu de contexto apos a confirmacao do usuario
        private async void AoExcluirProduto(object sender, EventArgs e)
        {
            MenuItem menu = (MenuItem)sender; // captura o item do menu que disparou o clique

            Produto produto = (Produto)menu.CommandParameter; // recupera o produto associado ao menu

            bool confirmado = await DisplayAlert("Excluir", $"Deseja excluir {produto.Descricao}?", "Sim", "Nao"); // pede confirmacao ao usuario

            if (!confirmado) // verifica se o usuario cancelou a exclusao
                return; // encerra o metodo sem excluir

            try
            {
                if (bancoDados == null) // ve se o banco nao esta disponivel
                    return; // nao tem como excluir

                await bancoDados.DeletarProduto(produto.Id); // remove o produto do banco

                await CarregarProdutos(); // atualiza a lista exibida na tela
            }
            catch (Exception)
            {
                // se der problema ao excluir, avisa o usuario com uma mensagem simples
                await DisplayAlert("Ops", "Não consegui excluir o produto. Tente de novo", "OK");
            }
        }
    }
}
