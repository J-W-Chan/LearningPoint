using GraphqlUsingHotChocolate.GraphQl.Queries;
using HelloWorldApi.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TimeGraphContext>(context =>
{
    context.UseInMemoryDatabase("TimeGraphServer");
});

builder.Services.AddGraphQLServer().AddQueryType<QueryResolver>();

var app = builder.Build();
app.MapGraphQL();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    //app.UsePlayground(new PlaygroundOptions
    //{
    //    QueryPath = "/api",
    //    Path = "/playground"
    //});
}

//app.UseGraphQL("/api");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();