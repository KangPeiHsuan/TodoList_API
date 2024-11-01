using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoAPI.Dtos;
using TodoAPI.Models;

namespace TodoAPI.Controllers
{
    [ApiController]
    [Route("todos")]
    [Authorize]
    [Produces("application/json")]
    public class TodosController : ControllerBase
    {
        private readonly TodoContext _todoContext;

        public TodosController(TodoContext todoContext)
        {
            _todoContext = todoContext;
        }

        // GET: todos
        /// <summary>
        /// 取得 TODO 列表
        /// </summary>
        /// <param name="authorization">JWT Token</param>
        /// <returns></returns>
        /// <response code="200">自己的 TODO List</response>
        /// <response code="401">未授權</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetTodos([FromHeader(Name = "Authorization")] string authorization)
        {
            if (string.IsNullOrEmpty(authorization))
            {
                return BadRequest("未授權");
            }

            // 處理 authorization 格式
            var token = authorization.StartsWith("Bearer ") ? authorization.Substring("Bearer ".Length).Trim() : authorization;

            List<Todo> todos = _todoContext.Todo.ToList();

            var result = todos.Select(todo => new TodoDto
            {
                Id = todo.Id,
                Content = todo.Content,
                CompletedAt = todo.CompletedAt
            }).ToList();

            return Ok(result);
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

            var todo = new Todo
            {
                Content = contentDto.Content,
            };

            _todoContext.Todo.Add(todo);
            _todoContext.SaveChanges();

            var _created = new IdRequiredDto
            {
                Id = todo.Id,
                Content = todo.Content,
            };

            // 返回 201，及新建立的 Todo Dto
            return Created("", _created);
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

            var todo = _todoContext.Todo.Find(id);
            if (todo == null)
            {
                // 返回 404
                return NotFound();
            }

            todo.Content = contentDto.Content;
            _todoContext.SaveChanges();

            var _updated = new IdRequiredDto
            {
                Id = todo.Id,
                Content = todo.Content
            };

            return Ok(_updated);
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
            var todo = _todoContext.Todo.Find(id);
            if (todo == null)
            {
                // 返回 404 
                return NotFound();
            }

            _todoContext.Todo.Remove(todo);
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
            var todo = _todoContext.Todo.Find(id);
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

            return Ok(todo);

        }
    }
}

