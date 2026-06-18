using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("contrato")]
    public class Contrato : BaseModel
    {
        [PrimaryKey("id_contrato")]
        public int IdContrato { get; set; }

        [Column("id_proveedor")]
        public long? IdProveedor { get; set; }

        [Column("id_evento")]
        public int? IdEvento { get; set; }

        [Column("tipo_contrato")]
        public string TipoContrato { get; set; } = string.Empty;

        [Column("monto_contrato")]
        public decimal? MontoContrato { get; set; }

        [Column("fecha_contrato")]
        public DateTime? FechaContrato { get; set; }

        [Column("estado_contrato")]
        public string EstadoContrato { get; set; } = string.Empty;
    }
}
