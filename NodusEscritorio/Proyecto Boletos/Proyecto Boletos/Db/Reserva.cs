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

        [Column("id_recinto")]
        public int IdRecinto { get; set; }

        [Column("fecha_reserva")]
        public int FechaReserva { get; set; }

        [Column("nombre_reservante")]
        public string NombreReservante { get; set; } = string.Empty;

        [Column("estado_reserva")]
        public string EstadoReserva { get; set; } = string.Empty;
    }
}
