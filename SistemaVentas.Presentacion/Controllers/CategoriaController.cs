using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using SistemaVentas.Presentacion.Models.ViewModels;
using SistemaVentas.Presentacion.Utilidades.Response;
using SistemaVentas.BLL.Interfaces;
using SistemaVentas.Entidad;
using Microsoft.AspNetCore.Authorization;

namespace SistemaVentas.Presentacion.Controllers
{
    [Authorize]
    public class CategoriaController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ICategoriaService _categoriaService;

        public CategoriaController(IMapper mapper, ICategoriaService categoriaService)
        {
            _mapper = mapper;
            _categoriaService = categoriaService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<VMCategoria> vmCategoriaLista =_mapper.Map<List<VMCategoria>>(await _categoriaService.Lista());

            return StatusCode(StatusCodes.Status200OK, new {data=vmCategoriaLista});
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] VMCategoria modelo)
        {
              GenericResponse<VMCategoria> gResponse = new GenericResponse<VMCategoria>();

            try
            {
                Categoria categoriaCreada=await _categoriaService.Crear(_mapper.Map<Categoria>(modelo));

                modelo = _mapper.Map<VMCategoria>(categoriaCreada);

                gResponse.Estado = true;
                gResponse.Objeto = modelo;
            }
            catch (Exception ex)
            {
                gResponse.Estado = true;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] VMCategoria modelo)
        {
            GenericResponse<VMCategoria> gResponse = new GenericResponse<VMCategoria>();

            try
            {
                Categoria categoriaEditada = await _categoriaService.Editar(_mapper.Map<Categoria>(modelo));

                modelo = _mapper.Map<VMCategoria>(categoriaEditada);

                gResponse.Estado = true;
                gResponse.Objeto = modelo;
            }
            catch (Exception ex)
            {
                gResponse.Estado = true;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int idCategoria)
        {
            GenericResponse<string> gResponse = new GenericResponse<string>();

            try
            {
                gResponse.Estado = await _categoriaService.Eliminar(idCategoria);

            }catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje=ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

    }
}
