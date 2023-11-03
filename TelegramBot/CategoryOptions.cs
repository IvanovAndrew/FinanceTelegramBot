using System.Collections.Generic;
using Domain;
using Microsoft.Extensions.Configuration;

namespace TelegramBot.Controllers
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