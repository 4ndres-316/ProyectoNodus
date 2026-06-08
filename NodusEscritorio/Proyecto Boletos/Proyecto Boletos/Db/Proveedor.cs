using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_Boletos.Db
{
    [Table("proveedor")]
    public class Proveedor : BaseModel
    {
        [PrimaryKey("nit_proveedor", shouldInsert: true)]
        public string NitProveedor { get; set; } = string.Empty;

        [Column("id_ciudad")]
        public int IdCiudad { get; set; }

        [Column("nombre_comercial")]
        public string NombreComercial { get; set; } = string.Empty;

        [Column("telefono_comercial")]
        public string TelefonoComercial { get; set; } = string.Empty;

        [Column("estado_proveedor")]
        public string EstadoProveedor { get; set; } = string.Empty;
    }
}