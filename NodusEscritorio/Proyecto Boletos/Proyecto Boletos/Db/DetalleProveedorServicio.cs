using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Proyecto_Boletos.Db
{
    [Table("detalle_proveedor_servicio")]
    public class DetalleProveedorServicio : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("id_proveedor")]
        public long IdProveedor { get; set; }

        [Column("id_servicio")]
        public int IdServicio { get; set; }

        [Column("nombre_item")]
        public string NombreItem { get; set; } = string.Empty;

        [Column("precio_item")]
        public long PrecioItem { get; set; }
    }
}
