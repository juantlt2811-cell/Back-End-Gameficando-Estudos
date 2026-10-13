using Microsoft.AspNetCore.Mvc;

namespace GameficandoEstudos.Api.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult Proibido(string mensagem) =>
            StatusCode(StatusCodes.Status403Forbidden, new { Erro = mensagem });
    }
}