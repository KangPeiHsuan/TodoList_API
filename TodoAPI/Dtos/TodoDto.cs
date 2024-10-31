
namespace TodoAPI.Dtos
{
    public class TodoDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class ContentDto
    {
        public string Content { get; set; }
    }

    public class IdRequiredDto
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
    }
}