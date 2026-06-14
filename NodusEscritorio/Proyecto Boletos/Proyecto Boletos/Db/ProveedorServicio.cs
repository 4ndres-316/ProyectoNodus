using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("proveedor_servicio")]
    public class ProveedorServicio : BaseModel
    {
        [PrimaryKey("id_proveedor_servicio")]
        public int IdProveedorServicio { get; set; }

        [Column("id_proveedor")]
        public long IdProveedor { get; set; }

        [Column("id_servicio")]
        public int IdServicio { get; set; }

        [Column("precio")]
        public long Precio { get; set; }
    }
}
