using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Proyecto_Boletos.Db
{
    [Table("usuario")]
    public class Usuario : BaseModel
    {
        [PrimaryKey("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_rol")]
        public int IdRol { get; set; }

        [Column("nombre_usuario")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Column("correo")]
        public string EmailUsuario { get; set; } = string.Empty;

        [Column("telefono")]
        public string Telefono { get; set; } = string.Empty;

        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Column("estado_usuario")]
        public string EstadoUsuario { get; set; } = string.Empty;

        [Column("foto_rostro")]
        public string FotoRostro { get; set; } = string.Empty;

        public Rol Rol { get; set; }
    }
}
