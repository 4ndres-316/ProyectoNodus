using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("notificacion")]
    public class Notificacion : BaseModel
    {
        [PrimaryKey("id_notificacion")]
        public int IdNotificacion { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("mensaje")]
        public string Mensaje { get; set; } = string.Empty;

        [Column("tipo_notificacion")]
        public string TipoNotificacion { get; set; } = string.Empty;

        [Column("fecha_envio")]
        public DateTime FechaEnvio { get; set; }

        [Column("estado_notificacion")]
        public string EstadoNotificacion { get; set; } = string.Empty;
    }
}
