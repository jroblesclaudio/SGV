﻿using Microsoft.AspNetCore.Mvc;

using AutoMapper;
using Newtonsoft.Json;
using SistemaVentas.BLL.Interfaces;
using SistemaVentas.Presentacion.Models.ViewModels;
using SistemaVentas.Presentacion.Utilidades.Response;
using SistemaVentas.Entidad;
using Microsoft.AspNetCore.Authorization;

namespace SistemaVentas.Presentacion.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;
        private readonly IMapper _mapper;

        public UsuarioController(IUsuarioService usuarioService, IRolService rolService,IMapper mapper)
        {
            _usuarioService= usuarioService;
            _rolService= rolService;
            _mapper=mapper;

        }
        
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ListaRoles()
        {
            var listaRol = await _rolService.Lista();
            List<VMRol> vmListaRoles=_mapper.Map<List<VMRol>>(listaRol);

            return StatusCode(StatusCodes.Status200OK,vmListaRoles);
        }

        [HttpGet]
        public async Task<IActionResult> ListaUsuarios()
        {
            var listaUsuario = await _usuarioService.Lista();
            List<VMUsuario> vmListaUsuario = _mapper.Map<List<VMUsuario>>(listaUsuario);

            return StatusCode(StatusCodes.Status200OK, new { data= vmListaUsuario });
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromForm] IFormFile foto, [FromForm] string modelo)
        {
            GenericResponse <VMUsuario> gResponse=new GenericResponse<VMUsuario>();

            try
            {
                VMUsuario vmUsuario = JsonConvert.DeserializeObject<VMUsuario>(modelo);

                string nombreFoto = "";
                Stream fotoStream = null;

                if (foto != null)
                {
                    string nombreEnCodigo=Guid.NewGuid().ToString("N");
                    string extension = Path.GetExtension(foto.FileName);
                    nombreFoto=string.Concat(nombreEnCodigo, extension);
                    fotoStream=foto.OpenReadStream();
                }

                string urlPlantillaCorreo = $"{this.Request.Scheme}://{this.Request.Host}/Plantilla/EnviarClave?correo=[correo]&clave=[clave]";

                Usuario usuarioCreado = await _usuarioService.Crear(_mapper.Map<Usuario>(vmUsuario), fotoStream, nombreFoto, urlPlantillaCorreo);

                vmUsuario = _mapper.Map<VMUsuario>(usuarioCreado);

                gResponse.Estado = true;
                gResponse.Objeto= vmUsuario;


            }catch(Exception ex)
            {
                gResponse.Estado= false;
                gResponse.Mensaje = ex.Message;
            }


            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromForm] IFormFile foto, [FromForm] string modelo)
        {
            GenericResponse<VMUsuario> gResponse = new GenericResponse<VMUsuario>();

            try
            {
                VMUsuario vmUsuario = JsonConvert.DeserializeObject<VMUsuario>(modelo);

                string nombreFoto = "";
                Stream fotoStream = null;

                if (foto != null)
                {
                    string nombreEnCodigo = Guid.NewGuid().ToString("N");
                    string extension = Path.GetExtension(foto.FileName);
                    nombreFoto = string.Concat(nombreEnCodigo, extension);
                    fotoStream = foto.OpenReadStream();
                }
                
                Usuario usuarioEditado = await _usuarioService.Editar(_mapper.Map<Usuario>(vmUsuario), fotoStream, nombreFoto);

                vmUsuario = _mapper.Map<VMUsuario>(usuarioEditado);

                gResponse.Estado = true;
                gResponse.Objeto = vmUsuario;
            }
            catch (Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int idUsuario)
        {
            GenericResponse<string> gResponse= new GenericResponse<string>();

            try
            {
                gResponse.Estado = await _usuarioService.Eliminar(idUsuario);

            }catch(Exception ex)
            {
                gResponse.Estado = false;
                gResponse.Mensaje=ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK,gResponse);
        }
    }
}
