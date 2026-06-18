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
        [PrimaryKey("nit_proveedor", true)]
        public long NitProveedor { get; set; }

        [Column("id_ciudad")]
        public int? IdCiudad { get; set; }

        [Column("nombre_comercial")]
        public string NombreComercial { get; set; } = string.Empty;

        [Column("telefono_comercial")]
        public string TelefonoComercial { get; set; } = string.Empty;

        [Column("estado_proveedor")]
        public string EstadoProveedor { get; set; } = string.Empty;

        // Ignorar en insert y update
        [Column("nombre_ciudad", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public string NombreCiudad { get; set; } = string.Empty;

        [Column("servicios_con_precio", ignoreOnInsert: true, ignoreOnUpdate: true)]
        public string ServiciosConPrecio { get; set; } = string.Empty;
    }
}