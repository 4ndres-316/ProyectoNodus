using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("presupuesto")]
    public class Presupuesto : BaseModel
    {
        [PrimaryKey("id_presupuesto")]
        public int IdPresupuesto { get; set; }

        [Column("id_evento")]
        public int IdEvento { get; set; }

        [Column("monto_proveedores")]
        public decimal MontoProveedores { get; set; }

        [Column("monto_marketing")]
        public decimal MontoMarketing { get; set; }

        [Column("monto_operativo")]
        public decimal MontoOperativo { get; set; }

        [Column("monto_recinto")]
        public decimal MontoRecinto { get; set; }
    }
}
