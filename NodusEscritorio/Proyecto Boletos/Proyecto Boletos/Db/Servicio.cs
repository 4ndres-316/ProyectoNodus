using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("servicio")]
    public class Servicio : BaseModel
    {
        [PrimaryKey("id_servicio")]
        public int IdServicio { get; set; }

        [Column("nombre_servicio")]
        public string NombreServicio { get; set; } = string.Empty;

        [Column("precio")]
        public decimal Precio { get; set; }

        [Column("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [Column("estado")]
        public string Estado { get; set; } = string.Empty;
    }
}
