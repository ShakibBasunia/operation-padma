using Microsoft.EntityFrameworkCore;
using Padma.Core;

var b = WebApplication.CreateBuilder(args);
bool mongo = b.Configuration["Store"] == "Mongo";

if (mongo)
    b.Services.AddSingleton<IMatchStore>(new MongoStore(b.Configuration["MongoUrl"]!));
else
{
    b.Services.AddDbContext<Db>(o => o.UseSqlite("Data Source=padma.db"));
    b.Services.AddScoped<IMatchStore, SqlStore>();
}
b.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = b.Build();
app.UseCors();
if (!mongo)
{
    using var s = app.Services.CreateScope();
    s.ServiceProvider.GetRequiredService<Db>().Database.EnsureCreated();
}

app.MapGet("/", () => "Operation Padma API");
app.MapPost("/matches", async (IMatchStore st, MatchResult m) => { m.PlayedAt = DateTime.UtcNow; await st.Add(m); return Results.Ok(); });
app.MapGet("/leaderboard", async (IMatchStore st) => await st.Top(10));
app.Run();
