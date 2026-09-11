namespace Domain
{
    public sealed class Category : IEquatable<Category>
    {
        private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
        
        public string Code { get; internal init; }
        public string Name { get; internal init; }
        public string? ShortName { get; internal init; }
        public IReadOnlyList<SubCategory> Subcategories { get; internal init; } = [];
        

        internal Category()
        {
        
        }
        
        public SubCategory? GetSubcategoryByName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            
            return Subcategories.FirstOrDefault(c => 
                string.Equals(c.Code, name, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(c.ShortName, name, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(c.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool operator ==(Category? one, Category? two) => EqualityComparer<Category>.Default.Equals(one, two);

        public static bool operator !=(Category? one, Category? two) => !(one == two);

        public bool Equals(Category? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Comparer.Equals(Code, other.Code);
        }

        public override bool Equals(object? obj)
        {
            return obj is Category other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(Code);
        }

        public override string ToString()
        {
            return $"{Name} ({Code})";
        }
    }

    public class SubCategory : IEquatable<SubCategory>
    {
        private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;
        
        public string Code { get; internal init; }
        public string Name { get; internal init; } = String.Empty;
        public string? ShortName { get; internal init; }

        public static bool operator ==(SubCategory? one, SubCategory? two) => EqualityComparer<SubCategory>.Default.Equals(one, two);

        public static bool operator !=(SubCategory? one, SubCategory? two) => !(one == two);

        public bool Equals(SubCategory? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            
            return Comparer.Equals(Code, other.Code);
        }

        public override bool Equals(object? obj)
        {
            return obj is SubCategory other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(Code);
        }

        public override string ToString()
        {
            return $"{Name} ({Code})";
        }
    }
}