using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Proyecto_Boletos.Db
{
    [Table("evento")]
    public class Evento : BaseModel
    {
        [PrimaryKey("id_evento")]
        public int IdEvento { get; set; }

        [Column("id_organizador")]
        public int IdOrganizador { get; set; }

        [Column("id_recinto")]
        public int IdRecinto { get; set; }

        [Column("fecha")]
        public int IdFechaEvento { get; set; }

        [Column("nombre_evento")]
        public string NombreEvento { get; set; } = string.Empty;

        [Column("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [Column("estado_evento")]
        public string EstadoEvento { get; set; } = string.Empty;

        [Column("es_publico")]
        public bool EsPublico { get; set; }

        [Column("imagen_url")]
        public string ImagenUrl { get; set; } = string.Empty;

        [Column("nombre_reservante")]
        public string NombreReservante { get; set; } = string.Empty;

        [Column("id_reserva")]
        public long? IdReserva { get; set; }

        [Column("descuento")]
        public long Descuento { get; set; }

        [Column("tope_reserva")]
        public long TopeReserva { get; set; }
    }
}
