using EntertainmentGuild.Data; // ✅ 引入你的 DbContext 命名空间
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EntertainmentGuild.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ 注册数据库连接（读取 appsettings.json 中的连接字符串）
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ✅ 注册 Identity 服务（用户和角色）
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ✅ 添加 MVC 支持
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ✅ 中间件配置
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // ⬅️ 加入认证中间件
app.UseAuthorization();  // ⬅️ 加入授权中间件

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentitySeeder.SeedRolesAsync(services); // 🔥 自动创建 Admin / Customer / Employee 角色
}

app.Run();


