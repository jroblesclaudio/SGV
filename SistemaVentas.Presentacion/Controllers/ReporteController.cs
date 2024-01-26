using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using SistemaVentas.Presentacion.Models.ViewModels;
using SistemaVentas.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace SistemaVentas.Presentacion.Controllers
{
    [Authorize]
    public class ReporteController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IVentaService _ventaService;

        public ReporteController(IMapper mapper, IVentaService ventaService)
        {
            _mapper = mapper;
            _ventaService = ventaService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ReporteVenta(string fechaInicio,string fechaFin)
        {
            List<VMReporteVenta> vmListaReporte =_mapper.Map<List<VMReporteVenta>>(await _ventaService.Reporte(fechaInicio, fechaFin));

            return StatusCode(StatusCodes.Status200OK, new { data = vmListaReporte });
        }
    }
}
