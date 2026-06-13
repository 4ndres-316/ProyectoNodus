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

        [Column("categoria")]
        public int IdCategoria { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = string.Empty;

        public string NombreCategoria { get; set; } = string.Empty;
    }
}
