namespace Domain
{
    public class Category : IEquatable<Category>
    {
        public string Name { get; init; }
        public string? ShortName { get; init; }
        public SubCategory[] Subcategories { get; set; } = Array.Empty<SubCategory>();
        public bool IsDefaultCategory { get; set; } = false;

        public Category()
        {
        
        }

        public SubCategory? GetSubcategoryByName(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            
            return Subcategories.FirstOrDefault(c => 
                string.Equals(c.ShortName, name, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(c.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }

        public static Category FromString(string name)
        {
            return new Category() { Name = name };
        }
        
        public static bool operator ==(Category one, Category two)
        {
            if (!ReferenceEquals(one, null)) return one.Equals(two);
            if (!ReferenceEquals(two, null)) return two.Equals(one);

            return true;
        }

        public static bool operator !=(Category one, Category two)
        {
            return !(one == two);
        }

        public bool Equals(Category? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Category)obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public class SubCategory : IEquatable<SubCategory>
    {
        public string Name { get; init; } = String.Empty;
        public string? ShortName { get; init; }
        public bool IsRecurringMonthly { get; init; }

        public static bool operator ==(SubCategory one, SubCategory two)
        {
            if (!ReferenceEquals(one, null)) return one.Equals(two);
            if (!ReferenceEquals(two, null)) return two.Equals(one);

            return true;
        }

        public static bool operator !=(SubCategory one, SubCategory two)
        {
            return !(one == two);
        }

        public bool Equals(SubCategory? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SubCategory)obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public static SubCategory? FromString(string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            return new SubCategory() { Name = str };
        }
    }
}