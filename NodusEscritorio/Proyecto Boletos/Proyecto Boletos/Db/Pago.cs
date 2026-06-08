using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("pago")]
    public class Pago : BaseModel
    {
        [PrimaryKey("id_pago")]
        public int IdPago { get; set; }

        [Column("id_orden")]
        public int IdOrden { get; set; }

        [Column("id_metodo_pago")]
        public int IdMetodoPago { get; set; }

        [Column("monto_pago")]
        public decimal MontoPago { get; set; }

        [Column("moneda")]
        public string Moneda { get; set; } = string.Empty;

        [Column("fecha_pago")]
        public DateTime FechaPago { get; set; }

        [Column("estado_pago")]
        public string EstadoPago { get; set; } = string.Empty;

        [Column("referencia_pago")]
        public string ReferenciaPago { get; set; } = string.Empty;
    }
}
