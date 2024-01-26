using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVentas.Entidad;

namespace SistemaVentas.BLL.Interfaces
{
    public interface IRolService
    {
        Task<List<Rol>> Lista();
    }
}
