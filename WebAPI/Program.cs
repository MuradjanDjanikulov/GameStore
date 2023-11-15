using Api.Services;
using DataAccess.Entity;
using DataAccess.Repository;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using WebAPI.Utils;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("GameStoreDb")));

builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

/*builder.Services.Configure<IdentityOptions>(opts => {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 5;});
*/
var tokenValidationParameters = new TokenValidationParameters()
{
    ValidateIssuerSigningKey = true,

    ValidateIssuer = true,

    ValidateAudience = true,

    ValidAudience = builder.Configuration["JWT:ValidAudience"],

    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],

    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),

    ValidateLifetime = true,

    ClockSkew = TimeSpan.Zero
};

builder.Services.AddSingleton(tokenValidationParameters);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

})
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = tokenValidationParameters;
    });


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireUserOnly", policy =>
    policy.RequireRole("User", "Manager", "Admin"));

    options.AddPolicy("RequireManagerOnly", policy =>
    policy.RequireRole("Manager", "Admin"));

    options.AddPolicy("RequireAdminOnly", policy =>
    policy.RequireRole("Admin"));

    //policyConfig.RequireAuthenticatedUser();
    //policyConfig.RequireClaim("", "");

});
//builder.Services.AddScoped<IGenericCRUDService<EmployeeModel>, EmployeeCRUDService>();

//builder.Services.AddScoped<IGenericCRUDService<AddressModel>, AddressCRUDService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddScoped<IGenericRepository<Employee>, EmployeeRepository>();
//builder.Services.AddScoped<IGenericRepository<Address>, AddressRepository>();

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
/*builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "api/Auth/login";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30); 

            });
*/
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
    c.ShowCommonExtensions();
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

AppDbInitializer.SeedRolesToDb(app).Wait();

app.Run();

