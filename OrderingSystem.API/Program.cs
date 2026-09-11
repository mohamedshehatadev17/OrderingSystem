using OrderingSystem.Infrastructure;
using OrderingSystem.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register infrastructure services
builder.Services.RegisterInfrastructureServices(builder.Configuration);

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
  {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
  }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
