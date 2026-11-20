namespace Padma.Core;

public class MatchResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Player { get; set; } = "Player";
    public string Winner { get; set; } = "";
    public int Rounds { get; set; }
    public int BlueLeft { get; set; }
    public int RedLeft { get; set; }
    public DateTime PlayedAt { get; set; } = DateTime.UtcNow;
}

public class GameEngine
{
    public const int W = 10, H = 8;
    static readonly (int, int)[] Dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    public Terrain[,] Map { get; } = BuildMap();
    public List<Unit> Units { get; private set; } = new();
    public List<string> Log { get; } = new();
    public Team Turn { get; private set; }
    public int Round { get; private set; }
    public Team? Winner { get; private set; }

    public GameEngine() => Reset();

    static Terrain[,] BuildMap()
    {
        string[] rows = { "..........", ".f..~...f.", "..f.~..f..", "..........",
                          "..........", "..f.~..f..", ".f..~...f.", ".........." };
        var m = new Terrain[H, W];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
                m[y, x] = rows[y][x] switch { 'f' => Terrain.Forest, '~' => Terrain.Water, _ => Terrain.Grass };
        return m;
    }

    public void Reset()
    {
        Units = new List<Unit>
        {
            new Infantry(Team.Blue, 0, 2), new Infantry(Team.Blue, 0, 5),
            new Tank(Team.Blue, 1, 3), new Tank(Team.Blue, 1, 4),
            new Artillery(Team.Blue, 0, 3), new Artillery(Team.Blue, 0, 4),
            new Infantry(Team.Red, 9, 2), new Infantry(Team.Red, 9, 5),
            new Tank(Team.Red, 8, 3), new Tank(Team.Red, 8, 4),
            new Artillery(Team.Red, 9, 3), new Artillery(Team.Red, 9, 4)
        };
        Turn = Team.Blue; Round = 1; Winner = null; Log.Clear();
    }

    public Unit? UnitAt(int x, int y) => Units.FirstOrDefault(u => u.X == x && u.Y == y);

    public List<(int X, int Y)> Reach(Unit u)
    {
        var best = new Dictionary<(int, int), int> { [(u.X, u.Y)] = 0 };
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var (k, c) in best.ToList())
                foreach (var (dx, dy) in Dirs)
                {
                    int nx = k.Item1 + dx, ny = k.Item2 + dy;
                    if (nx < 0 || ny < 0 || nx >= W || ny >= H) continue;
                    var t = Map[ny, nx];
                    if (t == Terrain.Water || UnitAt(nx, ny) != null) continue;
                    int nc = c + (t == Terrain.Forest ? 2 : 1);
                    if (nc > u.Move) continue;
                    if (best.TryGetValue((nx, ny), out var old) && old <= nc) continue;
                    best[(nx, ny)] = nc; changed = true;
                }
        }
        best.Remove((u.X, u.Y));
        return best.Keys.Select(k => (k.Item1, k.Item2)).ToList();
    }

    public List<Unit> Targets(Unit u) =>
        u.Acted ? new() : Units.Where(o => o.Team != u.Team && u.CanAttack(o)).ToList();

    public void Move(Unit u, int x, int y) { u.X = x; u.Y = y; u.Moved = true; }

    public void Attack(Unit a, Unit d)
    {
        int dmg = a.DamageTo(d, Map[d.Y, d.X]);
        d.Hp -= dmg;
        Log.Add($"{a.Team} {a.Tag} hit {d.Team} {d.Tag} for {dmg}");
        if (d.Hp > 0 && d.CanCounter && d.CanAttack(a))
        {
            int c = Math.Max(1, d.DamageTo(a, Map[a.Y, a.X]) / 2);
            a.Hp -= c;
            Log.Add($"{d.Team} {d.Tag} counter for {c}");
        }
        Units.RemoveAll(u => u.Hp <= 0);
        a.Acted = true; a.Moved = true;
        bool blue = Units.Any(u => u.Team == Team.Blue), red = Units.Any(u => u.Team == Team.Red);
        if (!blue || !red) Winner = blue ? Team.Blue : Team.Red;
    }

    public void BeginEnemyTurn()
    {
        Turn = Team.Red;
        foreach (var u in Units.Where(x => x.Team == Team.Red)) u.ResetTurn();
    }

    public void EndEnemyTurn()
    {
        if (Winner != null) return;
        foreach (var u in Units) u.ResetTurn();
        Turn = Team.Blue; Round++;
    }

    public MatchResult Result(string player) => new()
    {
        Player = string.IsNullOrWhiteSpace(player) ? "Player" : player.Trim(),
        Winner = Winner?.ToString() ?? "",
        Rounds = Round,
        BlueLeft = Units.Count(u => u.Team == Team.Blue),
        RedLeft = Units.Count(u => u.Team == Team.Red)
    };
}

public static class EnemyAI
{
    public static void Act(GameEngine e, Unit u)
    {
        var foes = e.Units.Where(x => x.Team == Team.Blue).ToList();
        if (foes.Count == 0) return;

        Unit? Pick() => e.Targets(u)
            .OrderBy(t => t.Hp - u.DamageTo(t, e.Map[t.Y, t.X])).FirstOrDefault();

        var target = Pick();
        if (target == null)
        {
            var near = foes.OrderBy(f => u.Dist(f)).First();
            (int X, int Y)? best = null;
            int bestScore = int.MaxValue;
            foreach (var (x, y) in e.Reach(u))
            {
                int d = Math.Abs(x - near.X) + Math.Abs(y - near.Y);
                bool inRange = d >= u.MinRange && d <= u.Range;
                int score = inRange ? -100 - (u is Artillery ? d : 0) : d;
                if (score < bestScore) { bestScore = score; best = (x, y); }
            }
            if (best.HasValue) e.Move(u, best.Value.X, best.Value.Y);
            target = Pick();
        }
        if (target != null) e.Attack(u, target);
    }
}
