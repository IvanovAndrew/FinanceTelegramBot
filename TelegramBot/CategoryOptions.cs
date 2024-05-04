using Domain;

namespace TelegramBot
{
    public class CategoryOptions
    {
        public List<Category> Categories { get; private set; }
        public CategoryOptions(IConfiguration configuration)
        {
            Categories = configuration.GetSection("Categories").Get<List<Category>>();
        }
    }
}