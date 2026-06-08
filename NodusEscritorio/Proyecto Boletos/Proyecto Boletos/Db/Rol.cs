using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("rol")]
    public class Rol : BaseModel
    {
        [PrimaryKey("id_rol")]
        public int IdRol { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;
    }
}
