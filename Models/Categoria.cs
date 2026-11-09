using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZeladoriaUrbana.API.Models;

[Table("categorias")]
public class Categoria
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(100), Column("nome")]
    public string Nome { get; set; } = string.Empty;
}
