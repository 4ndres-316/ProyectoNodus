using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("ciudad")]
    public class Ciudad : BaseModel
    {
        [PrimaryKey("id_ciudad")]
        public int IdCiudad { get; set; }

        [Column("id_departamento")]
        public int IdDepartamento { get; set; }

        [Column("nombre_ciudad")]
        public string NombreCiudad { get; set; } = string.Empty;
    }
}
