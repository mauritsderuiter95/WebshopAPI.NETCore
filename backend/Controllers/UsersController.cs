using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public ActionResult Authenticate(User userParam)
        {
            var user = _userService.Authenticate(userParam.Username, userParam.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });
            if (user.Id == "-2")
                return BadRequest(new { message = "Creating JWT token failed. Contact the software distributor." });

            return Ok(user);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Role.Admin)]
        [HttpGet]
        public ActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id:length(24)}", Name = "GetUser")]
        public ActionResult<User> Get(string id)
        {
            var user = _userService.Get(id);

            string currentUserId = User.Identity.Name;

            if (user.Id != currentUserId && !User.IsInRole(Role.Admin))
            {
                return Forbid();
            }


            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("currentuser", Name = "GetUserByUserName")]
        public ActionResult<User> GetCurrent()
        {
            string currentUserId = User.Identity.Name;

            var user = _userService.GetByUserName(currentUserId);

            if (user.Id != currentUserId && !User.IsInRole(Role.Admin))
            {
                return Forbid();
            }


            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [AllowAnonymous]
        [HttpPost]
        public ActionResult<Product> Create(User user)
        {
            if (!User.IsInRole(Role.Admin))
            {
                if (user.Role != Role.User)
                    return BadRequest(new { message = "Disallowed" });
            }

            user = _userService.Create(user);

            if (user == null)
                return BadRequest(new { message = "Username is taken." });
            if (user.Id == "-2")
                return BadRequest(new { message = "Creating JWT token failed. Contact the software distributor." });

            return CreatedAtRoute("GetUser", new { id = user.Id.ToString() }, user);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("{id:length(24)}")]
        public ActionResult<Product> Edit(string id, User user)
        {
            string currentUserId = User.Identity.Name;
            if (user.Id != currentUserId && !User.IsInRole(Role.Admin))
            {
                return Forbid();
            }

            user = _userService.Update(id,user);

            return CreatedAtRoute("GetUser", new { id = user.Id.ToString() }, user);
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPatch("{id:length(24)}")]
        public ActionResult<Product> Patch(string id, JsonPatchDocument patchDoc)
        {
            if (patchDoc != null)
            {
                User user = _userService.Get(id);

                string oldPassword = user.Password;

                patchDoc.ApplyTo(user);

                string newPassword = user.Password;

                if (oldPassword == newPassword)
                    user = _userService.Patch(user, false);
                else
                    user = _userService.Patch(user, true);

                return CreatedAtRoute("GetUser", new { id = user.Id.ToString() }, user);
            }
            else
            {
                return null;
            }
        }
    }
}
