using Microsoft.AspNetCore.Mvc;

namespace TelegramBot.Controllers;

[ApiController]
[Route("api")]
public class CategoryController : ControllerBase
{
    [HttpGet("categories")]
    public List<Contracts.CategoryDTO> GetCategories(bool isOutcome, bool includeOutdated = false)
    {
        IReadOnlyList<Domain.Category> categories;
        
        if (isOutcome)
        {
            categories = includeOutdated? Domain.Categories.Outcome.All : Domain.Categories.Outcome.Actual; 
        }
        else
        {
            categories = Domain.Categories.Income.All;
        }
        
        return 
        [
            .. categories.Select(c => new Contracts.CategoryDTO()
            {
                Code = c.Code,
                Name = c.ShortName?? c.Name,
                SubCategories =
                [
                    .. c.Subcategories.Select(sc => new Contracts.SubCategoryDTO()
                    {
                        Code = sc.Code,
                        Name = sc.ShortName ?? sc.Name,
                    })
                ],
                IsPopular = Domain.Categories.Outcome.Popular.Contains(c)
            })
        ];
    }
}