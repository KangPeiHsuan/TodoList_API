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
        private readonly TodoContext _todoContext;
        private readonly JwtProvider _jwtprovider;

        public TodosController(TodoContext todoContext, JwtProvider jwtProvider)
        {
            _todoContext = todoContext;
            _jwtprovider = jwtProvider;
        }

        private string? GetAuthFromHeader()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return null; // 如果未找到 Authorization 標頭，返回 null
            }

            var auth = Request.Headers["Authorization"].ToString();
            return auth;
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
            var auth = GetAuthFromHeader(); // 獲取 token
            if (string.IsNullOrEmpty(auth))
            {
                return Unauthorized(new { message = "未授權" }); // 如果 auth 為 null 或空，返回未授權
            }
            var token = auth.StartsWith("Bearer ") ? auth.Substring("Bearer ".Length).Trim() : auth;

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "未授權" });// 如果 token 為空，返回未授權
            }

            // 驗證 token 的有效性
            if (_jwtprovider.IsTokenInvalid(token))
            {
                return Unauthorized(new { message = "Token 無效" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // 透過 JWT Token 的 claim 獲取當前用戶的 ID

            List<Todo> todos = _todoContext.Todos.Where(u => u.UserId == userId ).ToList();

            var result = todos.Select(todo => new TodoDto
            {
                Id = todo.Id,
                Content = todo.Content,
                CompletedAt = todo.CompletedAt
            }).ToList();

            var response = new { todos = result };

            return Ok(response);
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
            var auth = GetAuthFromHeader(); // 獲取 token

            var token = auth.StartsWith("Bearer ") ? auth.Substring("Bearer ".Length).Trim() : auth;

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "未授權" }); // 如果 token 為空，返回未授權
            }

            // 驗證 token 的有效性
            if (_jwtprovider.IsTokenInvalid(token))
            {
                return Unauthorized(new { message = "Token 無效" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // 獲取當前用戶 ID

            // 創建新的 TODO 項目
            var todo = new Todo
            {
                Content = contentDto.Content,
                UserId =  userId // 將當前用戶 ID 設置為 TODO 的 UserId
            };

            _todoContext.Todos.Add(todo);
            _todoContext.SaveChanges();

            var created = new IdRequiredDto
            {
                Id = todo.Id,
                Content = todo.Content,
            };

            // 返回 201，及新建立的 Todo Dto
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
            // 確認這筆 todoitem 是否屬於當前使用者 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // 透過 JWT Token 的 claim 獲取當前用戶的 ID
            var foundtodo = _todoContext.Todos.FirstOrDefault(t => t.Id == id && t.UserId == userId);
            if (foundtodo == null)
            {
                return Unauthorized(new { message = "未授權" });
            }

            var auth = GetAuthFromHeader(); // 獲取 token

            var token = auth.StartsWith("Bearer ") ? auth.Substring("Bearer ".Length).Trim() : auth;

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "未授權" }); // 如果 token 為空，返回未授權
            }

            // 驗證 token 的有效性
            if (_jwtprovider.IsTokenInvalid(token))
            {
                return Unauthorized(new { message = "Token 無效" });
            }

            var todo = _todoContext.Todos.Find(id);
            if (todo == null)
            {
                // 返回 404
                return NotFound();
            }

            todo.Content = contentDto.Content;
            _todoContext.SaveChanges();

            var updated = new IdRequiredDto
            {
                Id = todo.Id,
                Content = todo.Content
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
            // 確認這筆 todoitem 是否屬於當前使用者 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // 透過 JWT Token 的 claim 獲取當前用戶的 ID
            var foundtodo = _todoContext.Todos.FirstOrDefault(t => t.Id == id && t.UserId == userId);
            if (foundtodo == null)
            {
                return Unauthorized(new { message = "未授權" });
            }

            var auth = GetAuthFromHeader(); // 獲取 token

            var token = auth.StartsWith("Bearer ") ? auth.Substring("Bearer ".Length).Trim() : auth;

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "未授權" }); // 如果 token 為空，返回未授權
            }

            // 驗證 token 的有效性
            if (_jwtprovider.IsTokenInvalid(token))
            {
                return Unauthorized(new { message = "Token 無效" });
            }

            var todo = _todoContext.Todos.Find(id);
            if (todo == null)
            {
                // 返回 404 
                return NotFound();
            }

            _todoContext.Todos.Remove(todo);
            _todoContext.SaveChanges();

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
            // 確認這筆 todoitem 是否屬於當前使用者 
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // 透過 JWT Token 的 claim 獲取當前用戶的 ID
            var foundtodo = _todoContext.Todos.FirstOrDefault(t => t.Id == id && t.UserId == userId);
            if (foundtodo == null)
            {
                return Unauthorized(new { message = "未授權" });
            }

            var auth = GetAuthFromHeader(); // 獲取 token

            var token = auth.StartsWith("Bearer ") ? auth.Substring("Bearer ".Length).Trim() : auth;

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "未授權" }); // 如果 token 為空，返回未授權
            }

            // 驗證 token 的有效性
            if (_jwtprovider.IsTokenInvalid(token))
            {
                return Unauthorized(new { message = "Token 無效" });
            }

            var todo = _todoContext.Todos.Find(id);
            if (todo == null)
            {
                // 返回 404 
                return NotFound();
            }

            if (todo.CompletedAt == null)
            {
                todo.CompletedAt = DateTime.UtcNow;
            }

            _todoContext.SaveChanges();

            var updated = new IdRequiredDto
            {
                Id = todo.Id,
                Content = todo.Content
            };

            return Ok(updated);

        }
    }
}

