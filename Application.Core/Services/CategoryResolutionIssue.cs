namespace Application.Core.Services;

public enum CategoryIssueKind { CategoryNotFound, SubcategoryNotFound }

public class CategoryResolutionIssueBase
{
    public DateOnly Date { get; init; }
    public string? RawCategory { get; init; }
    public string? Description { get; init; }
}

public class CategoryResolutionIssue : CategoryResolutionIssueBase
{
    public override string ToString()
    {
        return $"Couldn't find a category for {RawCategory?? "<null>"} ({Date} {Description})";
    }
}

public class SubCategoryResolutionIssue : CategoryResolutionIssueBase
{
    public string? RawSubcategory { get; init; }
    
    public override string ToString()
    {
        return $"Couldn't find a subcategory {RawCategory} {RawSubcategory} ({Date} {Description})";
    }
}