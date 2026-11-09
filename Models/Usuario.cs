using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZeladoriaUrbana.API.Models;

[Table("usuarios")]
public class Usuario
{
    [Key, Column("id")]
    public int Id { get; set; }

    [Required, MaxLength(100), Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(150), Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(255), Column("senha_hash")]
    public string SenhaHash { get; set; } = string.Empty;

    [Required, MaxLength(50), Column("perfil")]
    public string Perfil { get; set; } = "Operador";
}
