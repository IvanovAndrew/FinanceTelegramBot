namespace Domain
{
    public class Category : IEquatable<Category>
    {
        public string Code { get; internal init; }
        public string Name { get; internal init; }
        public string? ShortName { get; internal init; }
        public SubCategory[] Subcategories { get; internal set; } = Array.Empty<SubCategory>();
        public bool IsDefaultCategory { get; internal set; } = false;
        public CategoryType Type { get; internal init; } = CategoryType.RegularExpense;
        

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
            return Code == other.Code;
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
            return Code.GetHashCode();
        }
    }

    public class SubCategory : IEquatable<SubCategory>
    {
        public string Code { get; internal init; }
        public string Name { get; internal init; } = String.Empty;
        public string? ShortName { get; internal init; }
        public bool IsRecurringMonthly { get; internal init; }
        

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
            return Code == other.Code;
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