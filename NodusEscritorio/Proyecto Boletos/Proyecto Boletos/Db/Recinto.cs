using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Proyecto_Boletos.Db
{
    [Table("recinto")]
    public class Recinto : BaseModel
    {
        [PrimaryKey("id_recinto")]
        public int IdRecinto { get; set; }

        [Column("id_ciudad")]
        public int? IdCiudad { get; set; }

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

        public string NombreCiudad { get; set; } = string.Empty;
    }
}
