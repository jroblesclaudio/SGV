using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using SistemaVentas.BLL.Interfaces;
using SistemaVentas.DAL.Interfaces;
using SistemaVentas.Entidad;
using System.Globalization;

namespace SistemaVentas.BLL.Implementacion
{
    public class DashBoadService : IDashBoardService
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IGenericRepository<DetalleVenta> _detalleRepository;
        private readonly IGenericRepository<Categoria> _categoriaRepository;
        private readonly IGenericRepository<Producto> _productoRepository;
        private DateTime FechaInicio=DateTime.Now;

        public DashBoadService(IVentaRepository ventaRepository, IGenericRepository<DetalleVenta> detalleRepository, IGenericRepository<Categoria> categoriaRepository, IGenericRepository<Producto> productoRepository)
        {
            _ventaRepository = ventaRepository;
            _detalleRepository = detalleRepository;
            _categoriaRepository = categoriaRepository;
            _productoRepository = productoRepository;

            FechaInicio = FechaInicio.AddDays(-7);
        }

        public async Task<int> TotalVentasUltimaSemana()
        {
            try
            {
                IQueryable<Venta> query = await _ventaRepository.Consultar(v => v.FechaRegistro.Value.Date >= FechaInicio.Date);

                int total=query.Count();

                return total;

            }
            catch
            {
                throw;
            }
        }

        public async Task<string> TotalIngresosUltimaSemana()
        {
            try
            {
                IQueryable<Venta> query = await _ventaRepository.Consultar(v => v.FechaRegistro.Value.Date >= FechaInicio.Date);

                decimal resultado = query.Select(v => v.Total).Sum(v => v.Value);

                return Convert.ToString(resultado,new CultureInfo("es-PE"));

            }
            catch
            {
                throw;
            }
        }

        public async Task<int> TotalProductos()
        {
            try
            {
                IQueryable<Producto> query = await _productoRepository.Consultar();

                int total = query.Count();

                return total;

            }
            catch
            {
                throw;
            }
        }

        public async Task<int> TotalCategorias()
        {
            try
            {
                IQueryable<Categoria> query = await _categoriaRepository.Consultar();

                int total = query.Count();

                return total;

            }
            catch
            {
                throw;
            }
        }

        public async Task<Dictionary<string, int>> VentasUltimaSemana()
        {
            try
            {
                IQueryable<Venta> query = await _ventaRepository.Consultar(v=>v.FechaRegistro.Value.Date>=FechaInicio.Date);

                Dictionary<string, int> resultado = query
                    .GroupBy(v => v.FechaRegistro.Value.Date)
                    .OrderByDescending(g => g.Key).Select(dv=>new {fecha=dv.Key.ToString("dd/MM/yyyy"),total=dv.Count()})
                    .ToDictionary(keySelector: r=>r.fecha, elementSelector:r=>r.total);

                return resultado;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Dictionary<string, int>> ProductosTopUltimaSemana()
        {
            try
            {
                IQueryable<DetalleVenta> query = await _detalleRepository.Consultar();

                Dictionary<string, int> resultado = query
                    .Include(v=>v.IdVentaNavigation)
                    .Where(dv=>dv.IdVentaNavigation.FechaRegistro.Value.Date>=FechaInicio.Date)
                    .GroupBy(dv => dv.DescripcionProducto)
                    .OrderByDescending(g => g.Count())
                    .Select(dv => new { producto = dv.Key, total = dv.Count() })
                    .ToDictionary(keySelector: r => r.producto, elementSelector: r => r.total);

                return resultado;
            }
            catch
            {
                throw;
            }
        }

        
    }
}
