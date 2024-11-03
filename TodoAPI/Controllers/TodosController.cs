using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoAPI.Dtos;
using TodoAPI.Models;
using TodoAPI.Providers;

namespace TodoAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("todos")]
    [Produces("application/json")]
    public class TodosController : ControllerBase
    {
        private readonly JwtProvider _jwtProvider;
        private readonly ITodoService _todoService;

        public TodosController(JwtProvider jwtProvider, ITodoService todoService)
        {
            _jwtProvider = jwtProvider;
            _todoService = todoService;
        }

        // 把 token 驗證抽出來
        private bool ValidateToken()
        {
            var auth = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(auth)) return false;

            var token = auth.StartsWith("Bearer ") ? auth.Substring("Bearer ".Length).Trim() : auth;
            return !string.IsNullOrEmpty(token) && !_jwtProvider.IsTokenInvalid(token);
        }

        // GET: todos
        /// <summary>
        /// 取得 TODO 列表
        /// </summary>
        /// <returns></returns>
        /// <response code="200">自己的 TODO List</response>
        /// <response code="401">未授權</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetTodos()
        {
            if (!ValidateToken())
            {
                return Unauthorized(new { message = "未授權" });
            }

            var todos = _todoService.GetAll();
            var result = todos.Select(todo => new TodoDto
            {
                Id = todo.Id,
                Content = todo.Content,
                CompletedAt = todo.CompletedAt
            });

            return Ok(new { todos = result });
        }

        // POST todos
        /// <summary>
        /// 新增 TODO
        /// </summary>
        /// <returns></returns>
        /// <response code="201">該筆 TODO 資料</response>
        /// <response code="401">未授權</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CreateTodo([FromBody] ContentDto contentDto)
        {
            if (!ValidateToken())
            {
                return Unauthorized(new { message = "未授權" });
            }

            var todo = new Todo
            {
                Content = contentDto.Content
            };

            var createdTodo = _todoService.Create(todo);

            var created = new IdRequiredDto
            {
                Id = createdTodo.Id,
                Content = createdTodo.Content
            };

            return Created("", created);
        }

        // PUT todos/{id}
        /// <summary>
        /// 修改 TODO
        /// </summary>
        /// <returns></returns>
        /// <response code="200">修改過的 TODO</response>
        /// <response code="401">未授權</response>
        /// <response code="404">該筆 TODO 不存在</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateTodo(Guid id, [FromBody] ContentDto contentDto)
        {
            if (!ValidateToken())
            {
                return Unauthorized(new { message = "未授權" });
            }

            var todo = _todoService.GetById(id);
            if (todo == null)
            {
                return NotFound();
            }

            todo.Content = contentDto.Content;
            var updatedTodo = _todoService.Update(todo);

            var updated = new IdRequiredDto
            {
                Id = updatedTodo.Id,
                Content = updatedTodo.Content
            };

            return Ok(updated);
        }

        // DELETE todos/{id}
        /// <summary>
        /// 刪除 TODO
        /// </summary>
        /// <returns></returns>
        /// <response code="200">已刪除</response>
        /// <response code="401">未授權</response>
        /// <response code="404">該筆 TODO 不存在</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteTodo(Guid id)
        {
            if (!ValidateToken())
            {
                return Unauthorized(new { message = "未授權" });
            }

            var todo = _todoService.GetById(id);
            if (todo == null)
            {
                return NotFound();
            }

            _todoService.Delete(id);
            return Ok(new { message = "已刪除" });
        }

        //PATCH todos/{id}/toggle
        /// <summary>
        /// TODO 完成 / 已完成切換
        /// </summary>
        /// <returns></returns>
        /// <response code="200">已完成 TODO</response>
        /// <response code="401">未授權</response>
        /// <response code="404">該筆 TODO 不存在</response>
        [HttpPatch("{id}/toggle")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ToggleTodo(Guid id)
        {
            if (!ValidateToken())
            {
                return Unauthorized(new { message = "未授權" });
            }

            var todo = _todoService.GetById(id);
            if (todo == null)
            {
                return NotFound();
            }

            var updatedTodo = _todoService.Toggle(id);

            var result = new IdRequiredDto
            {
                Id = updatedTodo.Id,
                Content = updatedTodo.Content
            };

            return Ok(result);
        }
    }
}

