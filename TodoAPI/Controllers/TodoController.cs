
using Microsoft.AspNetCore.Mvc;
using TodoAPI.Dtos;
using TodoAPI.Models;

namespace TodoAPI.Controllers
{
    [ApiController]
    [Route("todos")]
    public class TodoController : ControllerBase
    {
        private readonly TodoContext _todoContext;
        
        public TodoController(TodoContext todoContext)
        {
            _todoContext = todoContext;
        }

        // GET: todos
        [HttpGet]
        public IActionResult GetTodos()
        {
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
        [HttpPost]
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
        [HttpPut("{id}")]
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
        [HttpDelete("{id}")]
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

            return Ok("已刪除");
        }

        //PATCH todos/{id}
        [HttpPatch("{id}")]
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

