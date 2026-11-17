using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(o => o.Filters.Add<TCC_Grupo12_III.Filters.EscopoMeta1Filter>());

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<NivelJogoService>();
builder.Services.AddScoped<JornadaService>();
builder.Services.AddScoped<JornadaJogosService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "EcoCanta.Meta2";
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
        options.Events.OnValidatePrincipal = async context => {
            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var raw = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var u = int.TryParse(raw, out var uid) ? await db.Usuarios.FindAsync(uid) : null;
            if (u == null || u.IsBanido) { context.RejectPrincipal(); return; }
            var expected = u.IsAdmin ? "Admin" : "Estudante";
            if (context.Principal?.IsInRole(expected) != true) { context.RejectPrincipal(); }
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.Database.GetPendingMigrations().Any()) {
        var connection = (Microsoft.Data.Sqlite.SqliteConnection)db.Database.GetDbConnection();
        connection.Open();
        var backupDir = Path.Combine(builder.Environment.ContentRootPath, "..", "backups");
        Directory.CreateDirectory(backupDir);
        using var backup = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={Path.Combine(backupDir, $"antes-migracao-{DateTime.UtcNow:yyyyMMdd-HHmmss}.db")}");
        backup.Open();
        connection.BackupDatabase(backup);
        connection.Close();
    }
    db.Database.Migrate();
    if (!db.Usuarios.Any() || !db.Especies.Any()) DataSeeder.Seed(db);
    DataSeederQuizEspecies.Seed(db);
    JornadaSeeder.Seed(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
// Evidências antigas mantêm seus arquivos e caminhos, com acesso administrativo.
app.Use(async (context, next) => {
    if (context.Request.Path.StartsWithSegments("/uploads/avistamentos") && !context.User.IsInRole("Admin")) {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return;
    }
    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
