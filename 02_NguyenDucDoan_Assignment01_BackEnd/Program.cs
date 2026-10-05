using _02_NguyenDucDoan_Assignment01_BackEnd.Models;
using _02_NguyenDucDoan_Assignment01_BackEnd.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using _02_NguyenDucDoan_Assignment01_BackEnd.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Configure OData Model
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<SystemAccountDTO>("SystemAccounts").EntityType.HasKey(x => x.AccountId);
modelBuilder.EntitySet<CategoryDTO>("Categories").EntityType.HasKey(x => x.CategoryId);
modelBuilder.EntitySet<NewsArticleDTO>("NewsArticles").EntityType.HasKey(x => x.NewsArticleId);

// Add services to the container.
builder.Services.AddControllers().AddOData(options => 
    options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(100)
           .AddRouteComponents("odata", modelBuilder.GetEdmModel()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register DbContext
builder.Services.AddDbContext<FunewsManagementContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// Register application services (Singleton pattern as per assignment)
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
              .AllowAnyMethod();
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
app.UseAuthorization();
app.MapControllers();

app.Run();

