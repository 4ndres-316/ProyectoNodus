using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Proyecto_Boletos.Db
{
    [Table("recinto")]
    public class Recinto : BaseModel
    {
        [PrimaryKey("id_recinto")]
        public long IdRecinto { get; set; }

        [Column("id_ciudad")]
        public long? IdCiudad { get; set; }

        [Column("nombre_recinto")]
        public string NombreRecinto { get; set; } = string.Empty;

        [Column("direccion_recinto")]
        public string DireccionRecinto { get; set; } = string.Empty;

        [Column("tipo_recinto")]
        public string TipoRecinto { get; set; } = string.Empty;

        [Column("descripcion_recinto")]
        public string DescripcionRecinto { get; set; } = string.Empty;

        [Column("capacidad")]
        public int Capacidad { get; set; }

        [Column("estado_recinto")]
        public string EstadoRecinto { get; set; } = string.Empty;

        [Column("link_ubicacion")]
        public string LinkUbicacion { get; set; } = string.Empty;

        [Column("precio_hora")]
        public long PrecioHora { get; set; }

        [Column("imagen_url")]
        public string ImagenUrl { get; set; } = string.Empty;

        [JsonIgnore]
        public string NombreCiudad { get; set; } = string.Empty;
    }
}
