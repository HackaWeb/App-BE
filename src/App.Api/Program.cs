using App.Api.Extensions;
using App.Api.Middleware;
using App.Api.Routes;
using App.DataContext;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwagger();
builder.Services.AddCorsPolicies();
builder.Services.ConfigureMediatR();

builder.Services.AddJwt(builder.Configuration);
builder.Services.AddServices(builder.Configuration);

builder.Services.AddDbContext(builder.Configuration);
builder.Services.AddIdentity();

var app = builder.Build();

app.ConfigureIdentityRoles();

app.UseMiddleware<ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthRoutes();
app.MapControllers();

app.Run();
