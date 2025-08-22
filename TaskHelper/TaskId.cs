namespace SharpBoxesCore.TaskHelper;
public class RegularTaskId : IEquatable<RegularTaskId>
{
    public string Name { get; }

    public RegularTaskId(string name)
    {
        Name = name;
    }

    public static RegularTaskId Parse(string name)
    {
        return new RegularTaskId(name);
    }

    public override bool Equals(object obj)
    {
        if (obj is RegularTaskId other)
        {
            return Equals(other);
        }
        return false;
    }

    public bool Equals(RegularTaskId other)
    {
        if (other is null)
            return false;
        return Name == other.Name;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return Name?.GetHashCode() ?? 0;
        }
    }

    public override string ToString()
    {
        return $"[{Name}]";
    }
}
