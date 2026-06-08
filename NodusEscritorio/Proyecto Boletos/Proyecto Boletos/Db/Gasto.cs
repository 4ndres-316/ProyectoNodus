using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("gasto")]
    public class Gasto : BaseModel
    {
        [PrimaryKey("id_gasto")]
        public int IdGasto { get; set; }

        [Column("id_contrato")]
        public int IdContrato { get; set; }

        [Column("descripcion_gasto")]
        public string DescripcionGasto { get; set; } = string.Empty;

        [Column("monto_gasto")]
        public decimal MontoGasto { get; set; }
    }
}
