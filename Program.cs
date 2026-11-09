using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ZeladoriaUrbana.API.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("Configure ConnectionStrings__DefaultConnection por variável de ambiente ou User Secrets.");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32) throw new InvalidOperationException("Configure Jwt__Key com pelo menos 32 bytes aleatórios (não inclua no repositório).");
builder.Services.AddAuthentication(options=> {options.DefaultAuthenticateScheme=JwtBearerDefaults.AuthenticationScheme;options.DefaultChallengeScheme=JwtBearerDefaults.AuthenticationScheme;})
    .AddJwtBearer(options=> {options.RequireHttpsMetadata=!builder.Environment.IsDevelopment();options.TokenValidationParameters=new TokenValidationParameters {
       ValidateIssuerSigningKey=true,IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),ValidateIssuer=true,ValidIssuer=builder.Configuration["Jwt:Issuer"],ValidateAudience=true,ValidAudience=builder.Configuration["Jwt:Audience"],ValidateLifetime=true,ClockSkew=TimeSpan.FromSeconds(30)
    };});
builder.Services.AddAuthentication().AddCookie("PainelCookie", options => {
    options.LoginPath = "/painel-mvc/login";
    options.AccessDeniedPath = "/painel-mvc/login";
    options.Cookie.Name = "Zeladoria.Painel";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
});
builder.Services.AddAuthorization();
var app=builder.Build();
if (!app.Environment.IsDevelopment()) {app.UseExceptionHandler("/erro");app.UseHsts();}
if (app.Environment.IsDevelopment()){app.UseSwagger();app.UseSwaggerUI();}
app.UseHttpsRedirection();
var frontendPath=Path.Combine(app.Environment.ContentRootPath,"frontend");
if (Directory.Exists(frontendPath)) {
    var provider=new PhysicalFileProvider(frontendPath);
    app.UseDefaultFiles(new DefaultFilesOptions {FileProvider=provider});
    app.UseStaticFiles(new StaticFileOptions {FileProvider=provider});
}
app.UseAuthentication();app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(name:"mvc",pattern:"{controller=Painel}/{action=Index}/{id?}");
app.Run();
