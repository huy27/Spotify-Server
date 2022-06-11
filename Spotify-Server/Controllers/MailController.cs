using Application.IService;
using Data.Models.Mail;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Spotify_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;

        public MailController(IMailService mailService)
        {
            _mailService = mailService;
        }

        [HttpPost]
        public ActionResult SendMail(SendMailModel sendMail)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            _mailService.SendMail(sendMail.ToEmail, sendMail.Message, sendMail.Subject);
            return Ok();
        }

    }
}
