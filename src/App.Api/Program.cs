using App.Api.Extensions;
using App.Api.Hubs;
using App.Api.Middleware;
using App.Api.Routes;
using App.DataContext;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwagger();
builder.Services.AddCorsPolicies();
builder.Services.ConfigureMediatR();
builder.Services.AddSignalR();
builder.Logging.AddConsole();

builder.Services.AddIdentity();
builder.Services.AddJwtSettings(builder.Configuration);
builder.Services.AddServices(builder.Configuration);

builder.Services.AddDbContext(builder.Configuration);

var app = builder.Build();

app.ConfigureIdentityRoles();

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseMiddleware<UnauthorizedResponseMiddleware>();
app.UseMiddleware<ForbiddenResponseMiddleware>();

app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.MapAuthRoutes();
app.MapProfileRoutes();
app.MapUserRoutes();


app.MapHub<ChatHub>("chat-hub");

app.Run();