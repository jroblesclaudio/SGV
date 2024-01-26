using SistemaVentas.Entidad;

namespace SistemaVentas.Presentacion.Models.ViewModels
{
    public class VMCategoria
    {
        public int IdCategoria { get; set; }

        public string? Descripcion { get; set; }

        public int? EsActivo { get; set; }

    }
}
