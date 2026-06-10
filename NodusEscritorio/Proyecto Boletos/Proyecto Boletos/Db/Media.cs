using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Proyecto_Boletos.Db
{
    [Table("media")]
    public class Media : BaseModel
    {
        [PrimaryKey("id_media")]
        public int IdMedia { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; }

        [Column("url")]
        public string Url { get; set; } = string.Empty;

        [Column("orden")]
        public int Orden { get; set; }

        [Column("id_evento")]
        public int IdEvento { get; set; }
    }
}
