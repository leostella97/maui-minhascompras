using MinhasCompras.Models; // importa a classe Produto usada nas operacoes
using SQLite; // importa as classes de acesso ao banco sqlite

namespace MinhasCompras.Helpers // agrupa as classes auxiliares do aplicativo
{
    // classe que centraliza o acesso ao banco de dados
    // os metodos de banco podem falhar por varios motivos, por isso as telas usam try/catch
    // e mostram uma mensagem simples pro usuario quando algum problema acontece
    public class SQLiteDatabaseHelper // classe que centraliza o acesso ao banco de dados
    {
        readonly SQLiteAsyncConnection conexao; // conexao assincrona com o banco sqlite

        // construtor que recebe o caminho do banco e apenas le o arquivo que ja existe
        // nao crio um arquivo novo do banco, o Dados.db precisa ter sido informado antes
        public SQLiteDatabaseHelper(string caminhoBancoDados)
        {
            // abro a conexão sem a flag de Create, assim nunca gera um arquivo novo no disco
            // se o arquivo nao existir, a primeira operação falha e a tela trata o erro
            conexao = new SQLiteAsyncConnection(caminhoBancoDados, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache);
        }

        // insere um novo produto no banco de dados
        public Task<int> InserirProduto(string descricao, int quantidade, double preco)
        {
            Produto produto = new Produto // monta o objeto com os dados recebidos
            {
                Descricao = descricao, // preenche a descricao do produto
                Quantidade = quantidade, // preenche a quantidade do produto
                Preco = preco // preenche o preco do produto
            };

            return conexao.InsertAsync(produto); // grava o produto e retorna o numero de linhas afetadas
        }

        // atualiza os dados de um produto ja cadastrado
        public Task<int> AtualizarProduto(Produto produto)
        {
            return conexao.UpdateAsync(produto); // aplica as alterações do produto no banco
        }

        // remove um produto do banco a partir do id informado
        public Task<int> DeletarProduto(int id)
        {
            return conexao.DeleteAsync<Produto>(id); // exclui o registro com o id recebido
        }

        // retorna todos os produtos cadastrados no banco
        public Task<List<Produto>> ObterTodosProdutos()
        {
            return conexao.Table<Produto>().ToListAsync(); // consulta a tabela inteira e devolve a lista
        }

        // busca produtos pela descrição usando a instrução sql com like
        public Task<List<Produto>> BuscarProdutos(string termo)
        {
            string sql = "SELECT * FROM Produto WHERE Descricao LIKE ?"; // monta a consulta com o operador like

            return conexao.QueryAsync<Produto>(sql, $"%{termo}%"); // executa a consulta com o termo informado
        }
    }
}
