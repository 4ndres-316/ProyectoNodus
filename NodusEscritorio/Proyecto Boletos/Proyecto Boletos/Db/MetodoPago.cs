using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("metodo_pago")]
    public class MetodoPago : BaseModel
    {
        [PrimaryKey("id_metodo_pago")]
        public int IdMetodoPago { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("icono_url")]
        public string IconoUrl { get; set; } = string.Empty;

        [Column("comision_porcentaje")]
        public decimal ComisionPorcentaje { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = string.Empty;
    }
}
