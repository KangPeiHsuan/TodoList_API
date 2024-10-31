
using System.ComponentModel.DataAnnotations;

namespace TodoAPI.Models
{
	public class User
	{
        [Key]
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } // 儲存加密後的用戶密碼
    }
}

