using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVentas.Entidad;

namespace SistemaVentas.BLL.Interfaces
{
    public interface INegocioService
    {
        Task<Negocio> Obtener();
        Task<Negocio> GuardarCambios(Negocio entidad,Stream logo=null, string nombreLogo="");
    }
}
