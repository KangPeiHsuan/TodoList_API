using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TodoAPI.Dtos;
using TodoAPI.Models;
using TodoAPI.Providers;

namespace TodoAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("users")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly JwtProvider _jwtProvider;
        private readonly TodoContext _todoContext;

        public UsersController(JwtProvider jwtProvider, TodoContext todoContext)
        {
            _jwtProvider = jwtProvider;
            _todoContext = todoContext;
        }

        // POST users/sign_in
        /// <summary>
        /// 使用者登入
        /// </summary>
        /// <returns></returns>
        /// <response code="200">登入成功</response>
        /// <response code="400">登入失敗</response>
        [HttpPost("sign_in")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SignIn([FromBody] LoginDto loginDto)
        {
            if (loginDto.Email == "user@example.com" || loginDto.Email == null)
            {
                return BadRequest(new { message = "請輸入您的電子信箱" });
            }

            var user = _todoContext.Users.SingleOrDefault(u => u.Email == loginDto.Email);
            if (user == null)
            {
                var newUser = new User { Email = loginDto.Email };
                var passwordHasher = new PasswordHasher<User>();
                newUser.PasswordHash = passwordHasher.HashPassword(newUser, loginDto.Password);

                _todoContext.Add(newUser);
                _todoContext.SaveChanges();
                
                var createdUser = _todoContext.Users.SingleOrDefault(u => u.Email == loginDto.Email);
                
                var token = _jwtProvider.GenerateToken(new UserDto { Id = createdUser.Id, Email = createdUser.Email });
                
                // 將 token 放入 response headers 內
                Response.Headers.Add("Authorization", $"Bearer {token}");

                return Ok(new
                {
                    message = "註冊成功",
                    email = loginDto.Email
                });
            }
            else
            {
                var passwordHasher = new PasswordHasher<User>();
                var passwordVerificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);

                if (passwordVerificationResult == PasswordVerificationResult.Success)
                {
                    var existUser = _todoContext.Users.FirstOrDefault(u => u.Email == loginDto.Email);

                    var _userdto = new UserDto
                    {
                        Id = existUser.Id,
                        Email = loginDto.Email,
                        Password = loginDto.Password 
                    };

                    var token = _jwtProvider.GenerateToken(_userdto);

                    // 將 token 放入 response headers 內
                    Response.Headers.Add("Authorization", $"Bearer {token}");

                    return Ok(new
                    {
                        message = "登入成功",
                        email = loginDto.Email
                    });
                }
                else
                {
                    return BadRequest(new { message = "密碼不正確" });
                }

            }
        }

        // DELETE users/sign_in
        /// <summary>
        /// 使用者登出
        /// </summary>
        /// <returns></returns>
        [HttpDelete("sign_out")]
        public IActionResult SignOut()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (_jwtProvider.IsTokenInvalid(token))
            {
                return BadRequest(new { message = "您沒有登入喔" });
            }

            _jwtProvider.SignOut(token);
            Response.Headers.Remove("Authorization");
            return Ok(new { message = "登出成功" });
        }
    }
}

