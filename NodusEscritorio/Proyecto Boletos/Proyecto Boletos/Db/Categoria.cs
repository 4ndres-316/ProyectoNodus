using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("categoria")]
    public class Categoria : BaseModel
    {
        [PrimaryKey("id_categoria")]
        public int IdCategoria { get; set; }

        [Column("nombre_categoria")]
        public string NombreCategoria { get; set; } = string.Empty;
    }
}
