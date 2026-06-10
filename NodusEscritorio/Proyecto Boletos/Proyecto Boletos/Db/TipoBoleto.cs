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
    internal class TipoBoleto: BaseModel
    {
        [PrimaryKey("id")]
        public int IdTipoBoleto { get; set; }

        [Column("id_evento")]
        public int IdEvento { get; set; }

        [Column("nombre_tipo_boleto")]
        public string NombreTipoBoleto { get; set; } = string.Empty;

        [Column("precio")]
        public decimal Precio { get; set; }

        [Column("cantidad_total")]
        public int CantidadTotal { get; set; }

        [Column("cantidad_disponible")]
        public int CantidadDisponible { get; set; }

        [Column("url_imagen")]
        public string UrlImagen { get; set; } = string.Empty;

        [Column("descripcion")]
        public string Descripcion { get; set; } = string.Empty;
    }
}
