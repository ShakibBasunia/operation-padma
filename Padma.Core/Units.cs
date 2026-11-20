namespace Padma.Core;

public enum Team { Blue, Red }
public enum Terrain { Grass, Forest, Water }

public abstract class Unit
{
    public Team Team { get; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Hp { get; set; }
    public bool Moved { get; set; }
    public bool Acted { get; set; }

    public abstract string Tag { get; }
    public abstract int MaxHp { get; }
    public abstract int Move { get; }
    public abstract int Range { get; }
    protected abstract int Atk { get; }
    public virtual int MinRange => 1;
    public virtual bool CanCounter => true;
    protected virtual bool IgnoresCover => false;

    protected Unit(Team team, int x, int y) { Team = team; X = x; Y = y; Hp = MaxHp; }

    public int Dist(Unit o) => Math.Abs(X - o.X) + Math.Abs(Y - o.Y);
    public bool CanAttack(Unit o) { var d = Dist(o); return d >= MinRange && d <= Range; }
    public virtual int DamageTo(Unit target, Terrain onTile)
    {
        int cover = onTile == Terrain.Forest && !IgnoresCover ? 2 : 0;
        return Math.Max(1, Atk - cover);
    }
    public void ResetTurn() { Moved = false; Acted = false; }
}

public class Infantry : Unit
{
    public Infantry(Team t, int x, int y) : base(t, x, y) { }
    public override string Tag => "INF";
    public override int MaxHp => 10;
    public override int Move => 2;
    public override int Range => 1;
    protected override int Atk => 5;
}

public class Tank : Unit
{
    public Tank(Team t, int x, int y) : base(t, x, y) { }
    public override string Tag => "TNK";
    public override int MaxHp => 16;
    public override int Move => 3;
    public override int Range => 1;
    protected override int Atk => 6;
    public override int DamageTo(Unit target, Terrain onTile) =>
        base.DamageTo(target, onTile) + (target is Infantry ? 2 : 0);
}

public class Artillery : Unit
{
    public Artillery(Team t, int x, int y) : base(t, x, y) { }
    public override string Tag => "ART";
    public override int MaxHp => 8;
    public override int Move => 1;
    public override int Range => 3;
    public override int MinRange => 2;
    public override bool CanCounter => false;
    protected override int Atk => 7;
    protected override bool IgnoresCover => true;
}
