var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// travel overseasapp.UseAuthorization();

app.MapControllers();

app.Run();

/*  In .NET 6 compiler generates the Program class behind the scenes as the internal class, 
 *  thus making it inaccessible in our integration testing project. So to solve this, we can 
 *  create a public partial Program class in the Program.cs file in the main project 
 */
public partial class Program { }