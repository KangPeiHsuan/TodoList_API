using System.ComponentModel.DataAnnotations;

namespace TodoAPI.Models
{
    public abstract class BaseEntity
    {
        public string UserId { get; set; } // 外鍵，指向擁有這筆待辦事項的用戶
        public User User { get; set; } // 對應的 User 實體，表示這個待辦事項屬於的用戶
    }

    public class Todo : BaseEntity
	{
        [Key]
        public Guid Id { get; set; } = new Guid(); // 預設為新生成的 Guid

        [Required]
        public string Content { get; set; }

        public DateTime? CompletedAt { get; set; } = null; // 預設為 null，?表示可為空
    }
}

