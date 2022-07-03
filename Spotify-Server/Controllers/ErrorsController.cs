using Application.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Spotify_Server.Controllers
{
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : ControllerBase
    {
        private readonly IMailService _mailService;

        public ErrorsController(IMailService mailService)
        {
            _mailService = mailService;
        }

        [Route("error")]
        public ActionResult Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context.Error;

            _mailService.SendMail("huy27297@gmail.com",
                $"Type: {exception.GetType().FullName}" + $"<br/>" +
                $"Message: {exception.Message}" + $"<br/>" +
                $"{exception.StackTrace}", 
                "Internal Server Error");

            return StatusCode(500, exception.Message);
        }
    }
}
