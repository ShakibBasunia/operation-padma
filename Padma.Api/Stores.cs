using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Padma.Core;

public interface IMatchStore
{
    Task Add(MatchResult m);
    Task<List<MatchResult>> Top(int n);
}

// SQL: EF Core + SQLite (swap UseSqlite for UseSqlServer / UseMySql for MS SQL or MySQL)
public class Db : DbContext
{
    public Db(DbContextOptions<Db> o) : base(o) { }
    public DbSet<MatchResult> Matches => Set<MatchResult>();
}

public class SqlStore : IMatchStore
{
    readonly Db db;
    public SqlStore(Db db) => this.db = db;
    public async Task Add(MatchResult m) { db.Matches.Add(m); await db.SaveChangesAsync(); }
    public Task<List<MatchResult>> Top(int n) => db.Matches
        .Where(m => m.Winner == "Blue").OrderBy(m => m.Rounds).ThenByDescending(m => m.BlueLeft)
        .Take(n).ToListAsync();
}

// NoSQL: MongoDB document store
public class MongoStore : IMatchStore
{
    readonly IMongoCollection<MatchResult> col;
    public MongoStore(string url) =>
        col = new MongoClient(url).GetDatabase("padma").GetCollection<MatchResult>("matches");
    public Task Add(MatchResult m) => col.InsertOneAsync(m);
    public Task<List<MatchResult>> Top(int n) => col.Find(m => m.Winner == "Blue")
        .SortBy(m => m.Rounds).Limit(n).ToListAsync();
}
