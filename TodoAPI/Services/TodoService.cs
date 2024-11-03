using TodoAPI.Models;

public interface ITodoService
{
    IEnumerable<Todo> GetAll();
    Todo GetById(Guid id);
    Todo Create(Todo todo);
    Todo Update(Todo todo);
    void Delete(Guid id);
    Todo Toggle(Guid id);
}

// 因為有另外設全局篩選，只會出現使用者本人的資料，所以不用另外做篩選
public class TodoService : ITodoService
{
    private readonly TodoContext _todoContext;

    public TodoService(TodoContext todocontext)
    {
        _todoContext = todocontext;
    }

    public IEnumerable<Todo> GetAll()
    {
        return _todoContext.Todos.ToList();
    }

    public Todo GetById(Guid id)
    {
        return _todoContext.Todos.FirstOrDefault(t => t.Id == id);
    }

    public Todo Create(Todo todo)
    {
        _todoContext.Todos.Add(todo);
        _todoContext.SaveChanges();
        return todo;
    }

    public Todo Update(Todo todo)
    {
        var existingTodo = GetById(todo.Id);
        if (existingTodo == null) return null;

        existingTodo.Content = todo.Content;
        _todoContext.SaveChanges();
        return existingTodo;
    }

    public void Delete(Guid id)
    {
        var todo = GetById(id);
        if (todo != null)
        {
            _todoContext.Todos.Remove(todo);
            _todoContext.SaveChanges();
        }
    }

    public Todo Toggle(Guid id)
    {
        var todo = GetById(id);
        if (todo == null) return null;

        todo.CompletedAt = todo.CompletedAt == null ? DateTime.UtcNow : null;
        _todoContext.SaveChanges();
        return todo;
    }
}
