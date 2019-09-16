using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerificationsController : ControllerBase
    {
        private VerificationService _verificationService;
        private UserService _userService;

        public VerificationsController(VerificationService verificationService, UserService userService)
        {
            _verificationService = verificationService;
            _userService = userService;
        }

        [HttpGet("{id:length(24)}", Name = "GetVerification")]
        public ActionResult<Verification> Get(string id)
        {
            Verification verification = _verificationService.Get(id);

            if (verification == null)
            {
                return BadRequest(new { message = "Email already verified." });
            }

            User user = _userService.Get(verification.UserId);
            user.Active = true;
            _userService.Patch(user, false);

            return verification;
        }
    }
}