using Microsoft.EntityFrameworkCore;

namespace TodoAPI.Models
{
    public class TodoContext : DbContext
    {
        // 在 program.cs 會以依賴注入的方式使用這個創建 DbContext 實例
        public TodoContext(DbContextOptions<TodoContext> options) : base(options) { }

        // EFCore 需放一個空的建構函數，才能正確創建一個 DbContext 實例，如 new
        public TodoContext() { }

        // 在資料庫內建立 Todo / User 資料表
        public DbSet<Todo> Todo { get; set; }
        public DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Todo>(entity =>
            {
                entity.HasKey(e => e.Id); // 設置 Id 為主鍵
                entity.Property(e => e.Content).IsRequired(); 
                entity.Property(e => e.CompletedAt).IsRequired(false); 
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id); // 設置 Id 為主鍵
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
            });
        }
    }
}

