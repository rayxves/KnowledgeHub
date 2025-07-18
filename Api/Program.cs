using System.Text;
using Api.Data;
using Api.Interfaces;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Newtonsoft.Json;
using Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "KnowledgeHub API", Version = "v1" });
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

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});


builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseNpgsql(connectionString);
});

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

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
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"]!)),
        ValidateIssuerSigningKey = true
    };
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000", "https://preview--knowledgehub-medium-front.lovable.app", "https://96db0b26-a3ae-4fb4-b7a4-5e773b4761d9.lovableproject.com", "https://id-preview--96db0b26-a3ae-4fb4-b7a4-5e773b4761d9.lovable.app")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
});

var settings = MongoClientSettings.FromConnectionString(
    builder.Configuration["MongoDb:DefaultConnection"]
);

settings.ServerApi = new ServerApi(ServerApiVersion.V1);

var client = new MongoClient(settings);

var database = client.GetDatabase(builder.Configuration["MongoDb:DatabaseName"]);

try
{
    var result = database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
    Console.WriteLine("✅ Conectado com sucesso ao MongoDB Atlas!");
}
catch (Exception ex)
{
    Console.WriteLine("❌ Erro na conexão:");
    Console.WriteLine(ex);
}

BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));


builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration["MongoDb:DefaultConnection"];
    return new MongoClient(connectionString);
});


builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var dbName = builder.Configuration["MongoDb:DatabaseName"];
    return client.GetDatabase(dbName);
});

builder.Services.AddTransient<CategoryDataSeeder>();
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddScoped<IArticleServices, ArticleServices>();
builder.Services.AddScoped<IMediaServices, MediaServices>();
builder.Services.AddScoped<ICommentServices, CommentServices>();
builder.Services.AddScoped<IArticleVersionServices, ArticleVersionServices>();
builder.Services.AddScoped<IFavoriteServices, FavoriteServices>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseCookiePolicy(new CookiePolicyOptions
// {
//     MinimumSameSitePolicy = SameSiteMode.Strict,
//     Secure = CookieSecurePolicy.Always, 
//     HttpOnly = HttpOnlyPolicy.Always 
// });
app.UseCors("AllowSpecificOrigin");


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();



using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<CategoryDataSeeder>();
    seeder.SeedCategory();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    string[] roleNames = { "Admin", "Writer", "Reader" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    var adminConfig = builder.Configuration.GetSection("AdminAccount");
    var adminEmail = adminConfig["Email"];
    var adminPassword = adminConfig["Password"];

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}


app.Run();

