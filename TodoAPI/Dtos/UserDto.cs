using System.ComponentModel.DataAnnotations;

namespace TodoAPI.Dtos
{
    public class UserDto
    {
        public Guid Id { get; set; }

        //[EmailAddress(ErrorMessage = "信箱格式錯誤")]
        public string Email { get; set; }

        public string Password { get; set; }
    }

    public class LoginDto
    {
        // 在 Dto 放入 emailaddress 屬性後，request 內文也變成信箱格式
        //[EmailAddress(ErrorMessage = "信箱格式錯誤")]
        public string Email { get; set; }

        public string Password { get; set; }
    }
}

