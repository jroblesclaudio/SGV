using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using Newtonsoft.Json;
using SistemaVentas.Presentacion.Models.ViewModels;
using SistemaVentas.Presentacion.Utilidades.Response;
using SistemaVentas.BLL.Interfaces;
using SistemaVentas.Entidad;
using Microsoft.AspNetCore.Authorization;

namespace SistemaVentas.Presentacion.Controllers
{
    [Authorize]
    public class ProductoController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProductoService _productoService;

        public ProductoController(IMapper mapper, IProductoService productoService)
        {
            _mapper = mapper;
            _productoService = productoService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lista()
        {
            List<VMProducto> vmProductoLista = _mapper.Map<List<VMProducto>>(await _productoService.Lista());

            return StatusCode(StatusCodes.Status200OK, new {data=vmProductoLista});
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromForm] IFormFile imagen, [FromForm]string modelo)
        {
            GenericResponse<VMProducto> gResponse= new GenericResponse<VMProducto>();

            try
            {
                VMProducto vmProducto = JsonConvert.DeserializeObject<VMProducto>(modelo);

                string nombreImagen = "";
                Stream ImagenStream = null;

                if(imagen!=null)
                {
                    string nombreEnCodigo=Guid.NewGuid().ToString("N");
                    string extension=Path.GetExtension(imagen.FileName);
                    nombreImagen=string.Concat(nombreEnCodigo, extension);
                    ImagenStream=imagen.OpenReadStream();
                }

                Producto productoCreado = await _productoService.Crear(_mapper.Map<Producto>(vmProducto), ImagenStream, nombreImagen);

                vmProducto = _mapper.Map<VMProducto>(productoCreado);

                gResponse.Estado = true;
                gResponse.Objeto = vmProducto;


            }catch (Exception ex)
            {
                gResponse.Estado = false; 
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromForm] IFormFile imagen, [FromForm] string modelo)
        {
            GenericResponse<VMProducto> gResponse = new GenericResponse<VMProducto>();

            try
            {
                VMProducto vmProducto = JsonConvert.DeserializeObject<VMProducto>(modelo);

                string nombreImagen = "";
                Stream ImagenStream = null;

                if (imagen != null)
                {
                    string nombreEnCodigo = Guid.NewGuid().ToString("N");
                    string extension = Path.GetExtension(imagen.FileName);
                    nombreImagen = string.Concat(nombreEnCodigo, extension);
                    ImagenStream = imagen.OpenReadStream();
                }

                Producto productoEditado = await _productoService.Editar(_mapper.Map<Producto>(vmProducto), ImagenStream,nombreImagen);

                vmProducto = _mapper.Map<VMProducto>(productoEditado);

                gResponse.Estado = true;
                gResponse.Objeto = vmProducto;


            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int idProducto)
        {
            GenericResponse<string> gResponse = new GenericResponse<string>();

            try
            {
                gResponse.Estado = await _productoService.Eliminar(idProducto);

            }catch(Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje=ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK,gResponse);
        }
    }
}
