using Api.Services;
using DataAccess.Entity;
using DataAccess.Repository;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebAPI.Models;
using WebAPI.Utils;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("GameStoreDb"));
    //options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

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
    options.AddPolicy("RequireUser", policy =>
    policy.RequireRole("User", "Manager", "Admin"));

    options.AddPolicy("RequireManager", policy =>
    policy.RequireRole("Manager", "Admin"));

    options.AddPolicy("RequireAdmin", policy =>
    policy.RequireRole("Admin"));

    //policyConfig.RequireAuthenticatedUser();
    //policyConfig.RequireClaim("", "");

});
//builder.Services.AddScoped<IGenericCRUDService<EmployeeModel>, EmployeeCRUDService>();

//builder.Services.AddScoped<IGenericCRUDService<GameModel,Game>, GameCRUDService>();
builder.Services.AddScoped<IGameCRUDService, GameCRUDService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//builder.Services.AddScoped<IGenericRepository<Employee>, EmployeeRepository>();
//builder.Services.AddScoped<IGenericRepository<Game>, GameRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();

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
builder.Services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


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

