using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Proyecto_Boletos.Db
{
    [Table("evento_servicio")]
    internal class EventoServicio : BaseModel 
    {
        [PrimaryKey("id_evento_servicio")]
        public int IdEventoServicio { get; set; }

        [Column("id_evento")]
        public int IdEvento { get; set; }

        [Column("id_servicio")]
        public int IdServicio { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; } = 1;

        [Column("precio_acordado")]
        public int PrecioAcordado { get; set; }

        [Column("estado_evento_servicio")]
        public string EstadoEventoServicio { get; set; } = string.Empty;
    }
}
