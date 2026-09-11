namespace Domain;

public class Shop : IEquatable<Shop>
{
    public static readonly Shop UnknownShop = new Shop("Unknown");
    
    public string Name { get; }
    private readonly string _cleanedName;

    private Shop(string name)
    {
        _cleanedName = name.Replace(" ", string.Empty).Replace("-", string.Empty).ToUpperInvariant();
        Name = name;
    }
    
    public static Shop? Create(string? name)
    {
        var trimmedName = name?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(trimmedName) ? null : new Shop(trimmedName);
    }

    public bool Equals(Shop? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(_cleanedName, other._cleanedName);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Shop)obj);
    }
    
    public static bool operator ==(Shop? left, Shop? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Shop? left, Shop? right)
    {
        return !Equals(left, right);
    }

    public override int GetHashCode()
    {
        return string.GetHashCode(_cleanedName);
    }

    public override string ToString()
    {
        return Name; 
    }
}