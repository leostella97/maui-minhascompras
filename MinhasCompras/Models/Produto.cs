using SQLite; // importa os atributos usados para mapear a tabela no banco

namespace MinhasCompras.Models // agrupa as classes de modelo do aplicativo
{
    public class Produto // classe que representa um produto na lista de compras
    {
        [PrimaryKey, AutoIncrement] // define o campo como chave primaria com valor automatico
        public int Id { get; set; } // identificador unico do produto

        public string Descricao { get; set; } = string.Empty; // descricao do produto

        public int Quantidade { get; set; } // quantidade de itens do produto

        public double Preco { get; set; } // preco unitario do produto
    }
}
