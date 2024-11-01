using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TodoAPI.Dtos;

namespace TodoAPI.Providers
{
    public class JwtProvider
    {
        private readonly IConfiguration _configuration;
        private readonly string _jwtIssuer;
        private readonly string _jwtSignKey;
        private readonly int _jwtExpireSec;

        public JwtProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtIssuer = _configuration.GetValue<string>("JwtSettings:Issuer");
            _jwtSignKey = _configuration.GetValue<string>("JwtSettings:SignKey");
            _jwtExpireSec = _configuration.GetValue<int>("JwtSettings:ExpireSec");
        }

        public string GenerateToken(UserDto userDto)
        {
            // 設定要加入到 JWT Token 中的聲明資訊(Claims)
            var claims = GenerateClaims(userDto);

            var userClaimsIdentity = new ClaimsIdentity(claims);

            // 建立一組對稱式加密的金鑰，主要用於 JWT 簽章之用
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSignKey));

            // https:\//stackoverflow.com/questions/47279947/idx10603-the-algorithm-hs256-requires-the-securitykey-keysize-to-be-greater
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);


            // 建立 SecurityTokenDescriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // 一般 API 通常沒有區分對象，所以 Audience 不需額外設定驗證
                Issuer = _jwtIssuer,
                Subject = userClaimsIdentity,
                Expires = DateTime.Now.AddSeconds(_jwtExpireSec),
                SigningCredentials = signingCredentials
            };

            // 產出所需要的 JWT securityToken 物件，並取得序列化後的 Token 結果(字串格式)
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var serializeToken = tokenHandler.WriteToken(securityToken);

            return serializeToken;
        }

        internal object GenerateToken(LoginDto loginDto)
        {
            var user = new UserDto
            {
                Email = loginDto.Email,
                Password = loginDto.Password
            };

            return GenerateToken(user);
        }

        private List<Claim> GenerateClaims(UserDto _userDto)
        {
            var claims = new List<Claim>
            {
                // 在 RFC 7519 規格中(Section#4)，總共定義了 7 個預設的 Claims，可以看需要哪些
                new Claim(JwtRegisteredClaimNames.Sub, _userDto.Id.ToString()), // User.Identity.Id
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
            };

            return claims;
        }
    }
}

