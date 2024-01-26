using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SistemaVentas.BLL.Implementacion;
using SistemaVentas.BLL.Interfaces;
using SistemaVentas.DAL.DBContext;
using SistemaVentas.DAL.Implementacion;
using SistemaVentas.DAL.Interfaces;

namespace SistemaVentas.IOC
{
    public static class Dependencia
    {
        public static void InyectarDependencia(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DbventaContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("CadenaConexion"));
            });

            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IVentaRepository, VentaRepository>();
            services.AddScoped<ICorreoService,CorreoService>();
            services.AddScoped<IFireBaseService, FirebaseService>();
            services.AddScoped<IUtilidadesService, UtilidadesService>();
            services.AddScoped<IRolService, RolService>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<INegocioService, NegocioService>();
            services.AddScoped<ICategoriaService, CategoriaService>();
            services.AddScoped<IProductoService, ProductoService>();
            services.AddScoped<ITipoDocumentoVentaService,TipoDocumentoVentaService>();
            services.AddScoped<IVentaService, VentaService>();
            services.AddScoped<IDashBoardService,DashBoadService>();
            services.AddScoped<IMenuService,MenuService>();

        }
    }
}
