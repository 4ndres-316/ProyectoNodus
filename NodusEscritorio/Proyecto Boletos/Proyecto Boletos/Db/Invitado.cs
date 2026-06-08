using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("invitado")]
    public class Invitado : BaseModel
    {
        [PrimaryKey("id_invitado")]
        public int IdInvitado { get; set; }

        [Column("id_evento")]
        public int IdEvento { get; set; }

        [Column("nombre_invitado")]
        public string NombreInvitado { get; set; } = string.Empty;

        [Column("tipo_invitado")]
        public string TipoInvitado { get; set; } = string.Empty;

        [Column("estado_invitado")]
        public string EstadoInvitado { get; set; } = string.Empty;
    }
}
