using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("detalle_orden")]
    public class DetalleOrden : BaseModel
    {
        [PrimaryKey("id_detalle_orden")]
        public int IdDetalleOrden { get; set; }

        [Column("id_orden")]
        public int IdOrden { get; set; }

        [Column("id_boleto")]
        public int IdBoleto { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("precio_unitario")]
        public decimal PrecioUnitario { get; set; }

        [Column("descuento")]
        public decimal Descuento { get; set; }
    }
}
