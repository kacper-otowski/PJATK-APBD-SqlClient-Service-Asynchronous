using FluentValidation;
using SqlClientExample_Tasks.Endpoints;
using SqlClientExample_Tasks.Services;
using SqlClientExample_Tasks.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register custom services
builder.Services.AddScoped<IDbService, DbService>();
builder.Services.AddScoped<IDbServiceDapper, DbServiceDapperDapper>();

// Register validators
builder.Services.AddValidatorsFromAssemblyContaining<StudentWithGroupNameDTOValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Register endpoints
app.RegisterStudentsEndpoints();
app.RegisterStudentsEndpoints2();


app.Run();
