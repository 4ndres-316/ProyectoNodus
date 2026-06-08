using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("reserva")]
    public class Reserva : BaseModel
    {
        [PrimaryKey("id_reserva")]
        public int IdReserva { get; set; }

        [Column("id_evento")]
        public int IdEvento { get; set; }

        [Column("codigo_reserva")]
        public string CodigoReserva { get; set; } = string.Empty;

        [Column("motivo_reserva")]
        public string MotivoReserva { get; set; } = string.Empty;

        [Column("fecha_reserva")]
        public DateTime FechaReserva { get; set; }

        [Column("estado_reserva")]
        public string EstadoReserva { get; set; } = string.Empty;
    }
}
