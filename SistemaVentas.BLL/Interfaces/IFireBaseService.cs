using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaVentas.BLL.Interfaces
{
    public interface IFireBaseService
    {
        Task<string> SubirStorage(Stream streamArchivo, string carpetaDestino, string nombreArchivo);
        Task<bool> EliminarStorage(string carpetaDestino, string nombreArchivo);
    }
}
