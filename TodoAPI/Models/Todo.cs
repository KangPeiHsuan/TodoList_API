using System.ComponentModel.DataAnnotations;

namespace TodoAPI.Models
{
	public class Todo
	{
        [Key]
        public Guid Id { get; set; } = new Guid(); // 預設為新生成的 Guid

        [Required]
        public string Content { get; set; }

        public DateTime? CompletedAt { get; set; } = null; // 預設為 null，?表示可為空
    }
}

