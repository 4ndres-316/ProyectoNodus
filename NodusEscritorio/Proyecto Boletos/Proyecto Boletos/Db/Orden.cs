using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("orden")]
    public class Orden : BaseModel
    {
        [PrimaryKey("id_orden")]
        public int IdOrden { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("fecha_orden")]
        public DateTime FechaOrden { get; set; }

        [Column("estado_orden")]
        public string EstadoOrden { get; set; } = string.Empty;

        [Column("descuento_orden")]
        public decimal DescuentoOrden { get; set; }
    }
}
