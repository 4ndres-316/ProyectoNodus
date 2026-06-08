using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("departamento")]
    public class Departamento : BaseModel
    {
        [PrimaryKey("id_departamento")]
        public int IdDepartamento { get; set; }

        [Column("nombre_departamento")]
        public string NombreDepartamento { get; set; } = string.Empty;
    }
}
