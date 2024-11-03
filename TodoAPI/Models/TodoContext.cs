using Microsoft.EntityFrameworkCore;

namespace TodoAPI.Models
{
    public class TodoContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;


        // 在 program.cs 會以依賴注入的方式使用這個創建 DbContext 實例
        public TodoContext(DbContextOptions<TodoContext> options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        // EFCore 需放一個空的建構函數，才能正確創建一個 DbContext 實例，如 new
        public TodoContext() { }

        // 在資料庫內建立 Todo / User 資料表
        public DbSet<Todo> Todos { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 設置全局查詢過濾器，實現數據隔離
            modelBuilder.Entity<Todo>()
                .HasQueryFilter(t => t.UserId == _currentUserService.GetCurrentUserId());

            modelBuilder.Entity<Todo>(entity =>
            {
                entity.HasKey(e => e.Id); // 設置 Id 為主鍵
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.CompletedAt).IsRequired(false);

                // 外鍵關聯
                entity.HasOne(e => e.User)
                  .WithMany(u => u.Todos)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade); // 當使用者被刪除時，對應的待辦事項也會被刪除
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id); // 設置 Id 為主鍵
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
            });
        }

        public override int SaveChanges()
        {
            // 會自動在保存到資料庫之前去為 BaseEntity 設定 UserId
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.UserId = _currentUserService.GetCurrentUserId();
                }
            }
            
            return base.SaveChanges();
        }
    }
}


