using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("boleto")]
    public class Boleto : BaseModel
    {
        [PrimaryKey("id_boleto")]
        public int IdBoleto { get; set; }

        [Column("id_evento")]
        public int IdEvento { get; set; }

        [Column("id_orden")]
        public int IdOrden { get; set; }

        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Column("fila")]
        public string Fila { get; set; } = string.Empty;

        [Column("asiento")]
        public string Asiento { get; set; } = string.Empty;

        [Column("tipo_boleto")]
        public string TipoBoleto { get; set; } = string.Empty;

        [Column("precio_boleto")]
        public decimal PrecioBoleto { get; set; }

        [Column("estado_boleto")]
        public string EstadoBoleto { get; set; } = string.Empty;
    }
}
