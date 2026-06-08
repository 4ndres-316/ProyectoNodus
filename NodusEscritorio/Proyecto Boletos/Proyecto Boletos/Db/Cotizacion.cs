using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("cotizacion")]
    public class Cotizacion : BaseModel
    {
        [PrimaryKey("id_cotizacion")]
        public int IdCotizacion { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_evento")]
        public int IdEvento { get; set; }

        [Column("detalle_cotizacion")]
        public string DetalleCotizacion { get; set; } = string.Empty;

        [Column("monto_cotizacion")]
        public decimal MontoCotizacion { get; set; }

        [Column("vigencia_hora")]
        public TimeSpan VigenciaHora { get; set; }

        [Column("vigencia_dia")]
        public DateTime VigenciaDia { get; set; }
    }
}
