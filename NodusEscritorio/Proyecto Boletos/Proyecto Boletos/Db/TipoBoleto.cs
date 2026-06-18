using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("tipo_boleto")]
    public class TipoBoleto : BaseModel
    {
        [PrimaryKey("id")]
        public int IdTipoBoleto { get; set; }

        [Column("id_evento")]
        public int IdEvento { get; set; }

        [Column("nombre_tipo")]
        public string NombreTipoBoleto { get; set; } = string.Empty;

        [Column("precio")]
        public decimal Precio { get; set; }

        [Column("cantidad_total")]
        public long CantidadTotal { get; set; }

        [Column("cantidad_disponible")]
        public long CantidadDisponible { get; set; }

        [Column("imagen_url")]
        public string UrlImagen { get; set; } = string.Empty;

        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
