using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using SistemaVentas.BLL.Interfaces;
using SistemaVentas.DAL.Interfaces;
using SistemaVentas.Entidad;

namespace SistemaVentas.BLL.Implementacion
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IGenericRepository<Usuario> _repositorio;
        private readonly IFireBaseService _firebaseService;
        private readonly IUtilidadesService _utilidadesService;
        private readonly ICorreoService _correoService;

        public UsuarioService(
            IGenericRepository<Usuario> repositorio,
            IFireBaseService firebaseService,
            IUtilidadesService utilidadesService,
            ICorreoService correoService)
        {
            _repositorio = repositorio;
            _firebaseService = firebaseService;
            _utilidadesService = utilidadesService;
            _correoService = correoService;
        }

        public async Task<List<Usuario>> Lista()
        {
            IQueryable<Usuario> query = await _repositorio.Consultar();
            return query.Include(r=>r.IdRolNavigation).ToList();
        }

        public async Task<Usuario> Crear(Usuario entidad, Stream foto = null, string nombreFoto = "", string urlPlantillaCorreo = "")
        {
            Usuario usuarioExiste = await _repositorio.Obtener(u => u.Correo == entidad.Correo);

            if(usuarioExiste!=null)
            {
                throw new TaskCanceledException("El correo ya existe");
            }

            try
            {
                string claveGenerada = _utilidadesService.GenerarClave();
                entidad.Clave = _utilidadesService.ConvertirSHA256(claveGenerada);
                entidad.NombreFoto = nombreFoto;

                if(foto!=null)
                {
                    string urlFoto = await _firebaseService.SubirStorage(foto, "carpeta_usuario", nombreFoto);
                    entidad.UrlFoto = urlFoto;
                }

                Usuario usuarioCreado = await _repositorio.Crear(entidad);

                if (usuarioCreado.IdUsuario == 0)
                {
                    throw new TaskCanceledException("No se pudo crear el usuario");
                }

                if (urlPlantillaCorreo != "")
                {
                    urlPlantillaCorreo = urlPlantillaCorreo.Replace("[correo]", usuarioCreado.Correo).Replace("[clave]", claveGenerada);

                    string htlmCorreo = "";

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlPlantillaCorreo);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            StreamReader readerStream = null;

                            if (response.CharacterSet == null)
                            {
                                readerStream = new StreamReader(dataStream);
                            }
                            else
                            {
                                readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));
                            }

                            htlmCorreo = readerStream.ReadToEnd();
                            response.Close();
                            readerStream.Close();
                        }
                    }

                    if (htlmCorreo != "")
                    {
                        await _correoService.EnviarCorreo(usuarioCreado.Correo, "Cuenta Creada", htlmCorreo);
                    }

                }

                IQueryable<Usuario> query = await _repositorio.Consultar(u=>u.IdUsuario==usuarioCreado.IdUsuario);
                
                usuarioCreado=query.Include(r=>r.IdRolNavigation).First();

                return usuarioCreado;
            }
            catch(Exception ex) 
            {
                throw;
            }
        }

        public async Task<Usuario> Editar(Usuario entidad, Stream foto = null, string nombreFoto = "")
        {
            Usuario usuarioExiste = await _repositorio.Obtener(u => u.Correo == entidad.Correo && u.IdUsuario!=entidad.IdUsuario);

            if (usuarioExiste != null)
            {
                throw new TaskCanceledException("El correo ya existe");
            }

            try
            {
                IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == entidad.IdUsuario);

                Usuario usuarioEditar=query.First();
                usuarioEditar.Nombre = entidad.Nombre;
                usuarioEditar.Correo= entidad.Correo;
                usuarioEditar.Telefono= entidad.Telefono;
                usuarioEditar.IdRol= entidad.IdRol;
                usuarioEditar.EsActivo= entidad.EsActivo;

                if (usuarioEditar.Nombre == "")
                {
                    usuarioEditar.NombreFoto = nombreFoto;
                }

                if(foto!=null)
                {
                    string urlFoto = await _firebaseService.SubirStorage(foto, "carpeta_usuario", usuarioEditar.NombreFoto);
                    usuarioEditar.UrlFoto=urlFoto;
                }

                bool respuesta=await _repositorio.Editar(usuarioEditar);

                if(!respuesta)
                {
                    throw new TaskCanceledException("No se pudo modificar el usuario");
                }

                Usuario usuarioEditado=query.Include(r=>r.IdRolNavigation).First();

                return usuarioEditado;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> Eliminar(int idUsuario)
        {
            try
            {
                Usuario usuarioEncontrado = await _repositorio.Obtener(u => u.IdUsuario == idUsuario);

                if (usuarioEncontrado == null)
                {
                    throw new TaskCanceledException("El usuario no existe");
                }

                string nombreFoto=usuarioEncontrado.NombreFoto;
                bool respuesta = await _repositorio.Eliminar(usuarioEncontrado);

                if(respuesta)
                {
                    await _firebaseService.EliminarStorage("carpeta_usuario", nombreFoto);
                }

                return true;

            }
            catch
            {
                throw;
            }
        }
        public async Task<Usuario> ObtenerPorCredenciales(string correo, string clave)
        {
            string claveEncriptada = _utilidadesService.ConvertirSHA256(clave);

            Usuario usuarioEcontrado = await _repositorio.Obtener(u => u.Correo.Equals(correo) && u.Clave.Equals(claveEncriptada));

            return usuarioEcontrado;
        }

        public async Task<Usuario> ObtenerPorId(int idUsuario)
        {
            IQueryable<Usuario> query = await _repositorio.Consultar(u => u.IdUsuario == idUsuario);

            Usuario usuarioEncontrado=query.Include(r=>r.IdRolNavigation).FirstOrDefault();
            return usuarioEncontrado;
        }
        public async Task<bool> GuardarPerfil(Usuario entidad)
        {
            try
            {
                Usuario usuarioEncontrado = await _repositorio.Obtener(u => u.IdUsuario == entidad.IdUsuario);

                if(usuarioEncontrado==null)
                {
                    throw new TaskCanceledException("El usuario no existe");
                }

                usuarioEncontrado.Correo=entidad.Correo;
                usuarioEncontrado.Telefono=entidad.Telefono;

                bool respuesta = await _repositorio.Editar(usuarioEncontrado);
                return respuesta;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> CambiarClave(int idUsuario, string claveActual, string claveNueva)
        {
            try
            {
                Usuario usuarioEncontrado = await _repositorio.Obtener(u => u.IdUsuario == idUsuario);

                if (usuarioEncontrado == null)
                {
                    throw new TaskCanceledException("El usuario no existe");
                }

                if (usuarioEncontrado.Clave != _utilidadesService.ConvertirSHA256(claveActual))
                {
                    throw new TaskCanceledException("Las contraseñas no coinciden");
                }

                usuarioEncontrado.Clave = _utilidadesService.ConvertirSHA256(claveNueva);

                bool respuesta=await _repositorio.Editar(usuarioEncontrado);

                return respuesta;

            }
            catch(Exception ex) 
            {
                throw;
            }
        } 

        public async Task<bool> RestablecerClave(string correo, string urlPlantillaCorreo)
        {
            try
            {
                Usuario usuarioEncontrado = await _repositorio.Obtener(u => u.Correo == correo);

                if(usuarioEncontrado==null) 
                { 
                    throw new TaskCanceledException("No se encontro ningun usuario con este correo"); 
                }

                string claveGenerada = _utilidadesService.GenerarClave();
                usuarioEncontrado.Clave = _utilidadesService.ConvertirSHA256(claveGenerada);


                urlPlantillaCorreo = urlPlantillaCorreo.Replace("[clave]", claveGenerada);

                string htlmCorreo = "";

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlPlantillaCorreo);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        StreamReader readerStream = null;

                        if (response.CharacterSet == null)
                        {
                            readerStream = new StreamReader(dataStream);
                        }
                        else
                        {
                            readerStream = new StreamReader(dataStream, Encoding.GetEncoding(response.CharacterSet));
                        }

                        htlmCorreo = readerStream.ReadToEnd();
                        response.Close();
                        readerStream.Close();
                    }
                }

                bool correoEnviado = false;

                if (htlmCorreo != "")
                {
                    correoEnviado=await _correoService.EnviarCorreo(correo, "Contraseña Restablecida", htlmCorreo);
                }

                if (!correoEnviado)
                {
                    throw new TaskCanceledException("Tenemos problemas, Intentalo de nuevo mas tarde");
                }

                bool respuesta = await _repositorio.Editar(usuarioEncontrado);
                return respuesta;

            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
