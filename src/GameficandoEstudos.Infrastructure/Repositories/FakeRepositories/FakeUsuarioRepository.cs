using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameficandoEstudos.Infrastructure.Repositories.FakeRepositories
{
    public class FakeUsuarioRepository : IUsuarioRepository
    {
        private readonly List<Usuario> _usuarios = new();
        private int _contadorId = 1;

        public Task<Usuario> AdicionarAsync(Usuario usuario)
        {
            // Simula o banco de dados gerando o ID
            typeof(Usuario).GetProperty("Id")?.SetValue(usuario, _contadorId++);

            _usuarios.Add(usuario);

            // Agora retornamos o usuário criado (com o Id preenchido), respeitando a interface
            return Task.FromResult(usuario);
        }

        public Task<bool> ExisteEmailAsync(string email)
        {
            var existe = _usuarios.Any(u => u.Email == email);
            return Task.FromResult(existe);
        }

        public Task<Usuario?> ObterPorEmailAsync(string email)
        {
            var usuario = _usuarios.FirstOrDefault(u => u.Email == email);
            return Task.FromResult(usuario);
        }

        public Task<Usuario?> ObterPorIdAsync(int id)
        {
            var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(usuario);
        }

        public Task<IEnumerable<Usuario?>> ObterUsuariosPorTipoAsync(TipoUsuario tipoUsuario)
        {
            var usuarios = _usuarios.Where(u => u.Tipo == tipoUsuario).AsEnumerable();
            // Permite nulos conforme a assinatura da sua interface
            return Task.FromResult<IEnumerable<Usuario?>>(usuarios);
        }

        public Task<Usuario> AtualizarAsync(Usuario usuario)
        {
            var index = _usuarios.FindIndex(u => u.Id == usuario.Id);
            if (index != -1)
            {
                _usuarios[index] = usuario;
            }
            return Task.FromResult(usuario);
        }
    }
}