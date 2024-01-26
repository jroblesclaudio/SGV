using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using SistemaVentas.DAL.DBContext;
using SistemaVentas.DAL.Interfaces;

namespace SistemaVentas.DAL.Implementacion
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly DbventaContext _dbventaContext;

        public GenericRepository(DbventaContext dbventaContext)
        {
            _dbventaContext= dbventaContext;          
        }
        
        public async Task<TEntity> Obtener(Expression<Func<TEntity, bool>> filtro)
        {
            try
            {
                TEntity entidad = await _dbventaContext.Set<TEntity>().FirstOrDefaultAsync(filtro);
                return entidad;
            }
            catch
            {
                throw;
            }
        }

        public async Task<TEntity> Crear(TEntity entidad)
        {
            try
            {
                _dbventaContext.Set<TEntity>().Add(entidad);
                await _dbventaContext.SaveChangesAsync();
                return entidad;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Editar(TEntity entidad)
        {
            try
            {
                _dbventaContext.Update(entidad);
                await _dbventaContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(TEntity entidad)
        {
            try
            {
                _dbventaContext.Remove(entidad);
                await _dbventaContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<IQueryable<TEntity>> Consultar(Expression<Func<TEntity, bool>> filtro = null)
        {
            IQueryable<TEntity>queryEntidad = filtro == null? _dbventaContext.Set<TEntity>():_dbventaContext.Set<TEntity>().Where(filtro);
            return queryEntidad;
        }


    }
}
