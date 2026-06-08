using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public int? IdOrganizador { get; set; }

        [Column("id_recinto")]
        public int IdRecinto { get; set; }

        [Column("nombre_evento")]
        public string NombreEvento { get; set; } = string.Empty;

        [Column("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [Column("estado_evento")]
        public string EstadoEvento { get; set; } = string.Empty;

        [Column("nombre_reservante")]
        public string NombreReservante { get; set; } = string.Empty;

        [Column("id_servicio")]
        public int? IdServicio { get; set; }

        [Column("esPublico")]
        public string EsPublico { get; set; } = string.Empty;

        [Column("fecha")]
        public int IdFechaEvento { get; set; }

        [Column("imagen_url")]
        public string ImagenUrl { get; set; } = string.Empty;
    }
}
