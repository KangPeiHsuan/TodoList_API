using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using TodoAPI.Models;
using TodoAPI.Providers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// 建立資料庫連線
builder.Services.AddDbContext<TodoContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// 註冊 swagger 文件
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 自訂 API 資訊描述
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Todos API",
        Description = "簡單的 API 用來管理待辦事項列表",
    });

    // 自訂排序，將 Users 區塊移到上面
    options.OrderActionsBy(apiDesc =>
    {
        if (apiDesc.ActionDescriptor.RouteValues["controller"] == "Users")
        {
            return "0";
        }
        else return "1";
    });

    // 設定以使用 xml 註解檔案
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    // 設定 JWT 限制 + 右上方 Authorize 按鈕
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "請輸入您的 Token (包含 Bearer)",
    });

    // 在調用 API 時會自動去 Bearer 做驗證
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

});

// 設定 JWT 驗證
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SignKey"]))
    };
});

// 註冊 JwtProvider
// AddSingleton vs AddScoped 生命週期差異
builder.Services.AddSingleton<JwtProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication(); // 認證，一定要放在 UseAuthorization 前
app.UseAuthorization();  // 授權

app.MapControllers();

app.Run();

