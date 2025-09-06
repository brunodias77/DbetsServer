using Dbets.Api.Configurations;
using Dbets.Api.Middlewares;
using Dbets.Domain.Services;
using Dbets.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ILoggedUser, LoggedUser>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Adicionar o middleware JWT ANTES do UseAuthorization
app.UseJwtAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();