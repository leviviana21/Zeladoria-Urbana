using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZeladoriaUrbana.API.Models;

[Table("ocorrencias")]
public class Ocorrencia
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Required, Column("categoria_id")]
    public int CategoriaId { get; set; }

    [Required, MaxLength(150), Column("contato_nome")]
    public string ContatoNome { get; set; } = string.Empty;

    [MaxLength(150), EmailAddress, Column("contato_email")]
    public string? ContatoEmail { get; set; }

    [MaxLength(30), Column("contato_telefone")]
    public string? ContatoTelefone { get; set; }

    [Required, MinLength(10), Column("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [Required, MaxLength(255), Column("endereco")]
    public string Endereco { get; set; } = string.Empty;

    [Required, MaxLength(50), Column("situacao")]
    public string Situacao { get; set; } = "Pendente";

    [Column("data_criacao")]
    public DateTime DataCriacao { get; set; } = DateTime.Now;

    [ForeignKey(nameof(CategoriaId))]
    public Categoria? Categoria { get; set; }
}
