
using System.ComponentModel.DataAnnotations;

namespace TodoAPI.Models
{
	public class User
	{
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } // 儲存加密後的用戶密碼

        public ICollection<Todo> Todos { get; set; } // 使用者擁有的待辦事項集合，表示一對多的關聯
    }
}

