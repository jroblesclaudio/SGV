using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVentas.DAL.Interfaces;
using SistemaVentas.Entidad;
using SistemaVentas.DAL.DBContext;

namespace SistemaVentas.DAL.Implementacion
{
    public class VentaRepository : GenericRepository<Venta>, IVentaRepository
    {
        private readonly DbventaContext _dbventaContext;

        public VentaRepository(DbventaContext dbventaContext):base(dbventaContext)        {
            _dbventaContext = dbventaContext;    
        }

        public async Task<Venta> Registrar(Venta entidad)
        {
            Venta ventaGenerada=new Venta();

            using (var transaction=_dbventaContext.Database.BeginTransaction())
            {
                try
                {
                    foreach(DetalleVenta dv in entidad.DetalleVenta)
                    {
                        Producto productoEncontrado=_dbventaContext.Productos.Where(p=>p.IdProducto==dv.IdProducto).First();
                        productoEncontrado.Stock = productoEncontrado.Stock - dv.Cantidad;

                        _dbventaContext.Update(productoEncontrado);
                    }
                    await _dbventaContext.SaveChangesAsync();

                    NumeroCorrelativo correlativo=_dbventaContext.NumeroCorrelativos.Where(n=>n.Gestion=="venta").First();

                    correlativo.UltimoNumero = correlativo.UltimoNumero + 1;
                    correlativo.FechaActualizacion = DateTime.Now;

                    _dbventaContext.NumeroCorrelativos.Update(correlativo);
                    await _dbventaContext.SaveChangesAsync();

                    string ceros = string.Concat(Enumerable.Repeat("0", correlativo.CantidadDigitos.Value));
                    string numeroVenta = ceros + correlativo.UltimoNumero.ToString();
                    numeroVenta = numeroVenta.Substring(numeroVenta.Length - correlativo.CantidadDigitos.Value, correlativo.CantidadDigitos.Value);

                    entidad.NumeroVenta = numeroVenta;

                    await _dbventaContext.Venta.AddAsync(entidad);
                    await _dbventaContext.SaveChangesAsync();

                    ventaGenerada = entidad;

                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }

            return ventaGenerada;
        }

        public async Task<List<DetalleVenta>> Reporte(DateTime fechaInicio, DateTime fechaFin)
        {
            List<DetalleVenta> listaResumen=await _dbventaContext.DetalleVenta
                .Include(v=>v.IdVentaNavigation)
                .ThenInclude(u=>u.IdUsuarioNavigation)
                .Include(v=>v.IdVentaNavigation)
                .ThenInclude(tdv=>tdv.IdTipoDocumentoVentaNavigation)
                .Where(dv=>dv.IdVentaNavigation.FechaRegistro.Value.Date>=fechaInicio.Date
                && dv.IdVentaNavigation.FechaRegistro.Value.Date<=fechaFin.Date).ToListAsync();

            return listaResumen;
        }
    }
}
