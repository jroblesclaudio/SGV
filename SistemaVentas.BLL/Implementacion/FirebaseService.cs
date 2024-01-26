using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SistemaVentas.BLL.Interfaces;
using Firebase.Auth;
using Firebase.Storage;
using SistemaVentas.Entidad;
using SistemaVentas.DAL.Interfaces;

namespace SistemaVentas.BLL.Implementacion
{
    public class FirebaseService : IFireBaseService
    {
        private readonly IGenericRepository<Configuracion> _repositorio;

        public FirebaseService(IGenericRepository<Configuracion> repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<string> SubirStorage(Stream streamArchivo, string carpetaDestino, string nombreArchivo)
        {
            string urlImagen = "";
            try
            {
                IQueryable<Configuracion> query = await _repositorio.Consultar(c => c.Recurso.Equals("FireBase_Storage"));

                Dictionary<string, string> Config = query.ToDictionary(keySelector: c => c.Propiedad, elementSelector: c => c.Valor);

                var auth = new FirebaseAuthProvider(new FirebaseConfig(Config["api_key"]));
                var a = await auth.SignInWithEmailAndPasswordAsync(Config["email"], Config["clave"]);

                var cancellation = new CancellationTokenSource();

                var task = new FirebaseStorage(
                    Config["ruta"],
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true
                    })
                    .Child(Config[carpetaDestino])
                    .Child(nombreArchivo)
                    .PutAsync(streamArchivo,cancellation.Token);

                urlImagen = await task;
            }
            catch
            {
                urlImagen = "";
            }

            return urlImagen;
        }

        public async Task<bool> EliminarStorage(string carpetaDestino, string nombreArchivo)
        {
            try
            {
                IQueryable<Configuracion> query = await _repositorio.Consultar(c => c.Recurso.Equals("FireBase_Storage"));

                Dictionary<string, string> Config = query.ToDictionary(keySelector: c => c.Propiedad, elementSelector: c => c.Valor);

                var auth = new FirebaseAuthProvider(new FirebaseConfig(Config["api_key"]));
                var a = await auth.SignInWithEmailAndPasswordAsync(Config["email"], Config["clave"]);

                var cancellation = new CancellationTokenSource();

                var task = new FirebaseStorage(
                    Config["ruta"],
                    new FirebaseStorageOptions
                    {
                        AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                        ThrowOnCancel = true
                    })
                    .Child(Config[carpetaDestino])
                    .Child(nombreArchivo)
                    .DeleteAsync();

                await task;
                return true;
            }
            catch
            {
                return false;
            }

        }

        
    }
}
