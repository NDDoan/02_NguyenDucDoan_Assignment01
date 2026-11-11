using _02_NguyenDucDoan_Assignment01_BackEnd.Models;
using _02_NguyenDucDoan_Assignment01_BackEnd.Services;
using _02_NguyenDucDoan_Assignment01_BackEnd.Filters;
using _02_NguyenDucDoan_Assignment01_BackEnd.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using _02_NguyenDucDoan_Assignment01_BackEnd.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure OData Model
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<SystemAccountDTO>("SystemAccounts").EntityType.HasKey(x => x.AccountId);
modelBuilder.EntitySet<CategoryDTO>("Categories").EntityType.HasKey(x => x.CategoryId);
modelBuilder.EntitySet<NewsArticleDTO>("NewsArticles").EntityType.HasKey(x => x.NewsArticleId);

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidateModelFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        ValidateModelFilter.CreateValidationErrorResult(context.ModelState, context.HttpContext.Response);
})
.AddOData(options =>
    options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100)
           .AddRouteComponents("odata", modelBuilder.GetEdmModel()));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FUNewsManagement API",
        Version = "v1",
        Description = "API cho hệ thống FUNewsManagement - hỗ trợ OData và JWT Authentication"
    });

    // Cấu hình JWT Bearer Authentication cho Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập mã JWT token trực tiếp vào ô bên dưới (Swagger sẽ tự động thêm 'Bearer ' vào header):"
    });

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
            Array.Empty<string>()
        }
    });
});

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "this_is_a_fallback_secret_key_that_is_long_enough";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
        ValidateIssuer = false,
        ValidateAudience = false
    };

    // --- CẤU HÌNH HEADERS & RESPONSE ---
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            context.Response.Headers["X-Auth-Error"] = "Unauthorized - Invalid or missing token";

            var responseBody = new
            {
                statusCode = 401,
                error = "Unauthorized",
                message = "Bạn chưa đăng nhập hoặc token đã hết hạn / không hợp lệ."
            };

            await context.Response.WriteAsJsonAsync(responseBody);
        },

        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            context.Response.Headers["X-Auth-Error"] = "Forbidden - Insufficient permissions";

            var responseBody = new
            {
                statusCode = 403,
                error = "Forbidden",
                message = "Tài khoản của bạn không có đủ quyền (Role) để thực hiện thao tác này."
            };

            await context.Response.WriteAsJsonAsync(responseBody);
        }
    };
});

// Register DbContext
builder.Services.AddDbContext<FunewsManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// Register repositories (Singleton/Scoped Repository pattern as per assignment)
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ISystemAccountRepository, SystemAccountRepository>();
builder.Services.AddScoped<INewsArticleRepository, NewsArticleRepository>();

// Register application services
builder.Services.AddScoped<ISystemAccountService, SystemAccountService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<INewsArticleService, NewsArticleService>();

// Allow CORS for FrontEnd project
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontEnd", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Location", "X-Application-Name", "X-API-Version", "X-Auth-Error", "X-Error-Type", "X-Validation-Error", "X-Created-Status");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontEnd");

// Thêm các Response Headers dùng chung cho toàn hệ thống
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Application-Name"] = "FUNewsManagement";
        context.Response.Headers["X-API-Version"] = "1.0.0";
        return Task.CompletedTask;
    });
    await next();
});

// Phải đặt UseExceptionHandler() trước Authentication/Authorization/MapControllers để bắt toàn bộ lỗi
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

