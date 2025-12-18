using Mango.Services.Auth.Datas;
using Mango.Services.Auth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var conx = builder.Configuration.GetConnectionString("DefaultConnection");
    
    builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("ApiSettings:JwtOptions"));

    options.UseSqlServer(conx,
        sqlServerOptionsAction: sqlOptions =>
        {
            // === ADICIONAR ESTE BLOCO ===
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 1,         // Número máximo de tentativas
                maxRetryDelay: TimeSpan.FromSeconds(20), // Tempo máximo de espera total
                errorNumbersToAdd: null   // Tentar repetir em todos os erros transientes conhecidos
            );
            // ===========================
        });
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapScalarApiReference();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
ApplyMigration();
app.Run();

void ApplyMigration()
{
    using(var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        if(dbContext.Database.GetPendingMigrations().Any())
        {
            dbContext.Database.Migrate();
        }
    }
}
