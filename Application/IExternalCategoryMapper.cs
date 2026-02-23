using Domain;

namespace Application;

public class Shops
{
    public const string YerevanCity = "Yerevan City";
}

public record ExternalCategory
{
    public string Source { get; init; }
    public string RawName { get; init; }
}

public interface IExternalCategoryMapper
{
    public (Category, SubCategory?) Map(ExternalCategory external, Category fallback);
}

public class ExternalCategoryMapper() : IExternalCategoryMapper
{
    private static readonly Dictionary<(string, string), (Category, SubCategory?)> _map =
        new()
        {
            { (Shops.YerevanCity, "Armenian beer"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Алкоголь"))},
            { (Shops.YerevanCity, "Ayran"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы"))},
            
            { (Shops.YerevanCity, "Baklava, dry cake and muffine"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Bath towel"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для туалета")) },
            { (Shops.YerevanCity, "Batteries"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для техники")) },
            { (Shops.YerevanCity, "Buckwheat"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            
            { (Shops.YerevanCity, "Cakes"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Candy dragee"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Caramel"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Carbonated drinks"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Casseroles"), (Categories.Outcome.ForHouse, Categories.Outcome.Food.Sub("Для кухни")) },
            { (Shops.YerevanCity, "Chocolate bars"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Chocolate candies"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Cocoa powder"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Condensed milk"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Corn chips"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Cottage cheese"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Crab meat and sticks"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Cracker"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Croissants and buns"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Cupcake, croissants, donuts"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Cutting boards"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для кухни")) },
            
            { (Shops.YerevanCity, "Daily pads"), (Categories.Outcome.Health, Categories.Outcome.Health.Sub("Гигиена")) },
            { (Shops.YerevanCity, "Diet bread"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Dried fruits"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Dry cat food"), (Categories.Outcome.Pets, Categories.Outcome.Pets.Sub("Уличные")) },
            { (Shops.YerevanCity, "Dry cookies"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Dry dog food"), (Categories.Outcome.Pets, Categories.Outcome.Pets.Sub("Уличные")) },
            { (Shops.YerevanCity, "Dumplings, khinkali"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            
            { (Shops.YerevanCity, "Eggs 10pcs"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Eggs 20pcs"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Eggs 30pcs"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Exotic fruits"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Фрукты/овощи")) },
            
            { (Shops.YerevanCity, "Feta,brined, bryndza, mozzarella cheeses"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты"))},
            { (Shops.YerevanCity, "Flakes"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы"))},
            { (Shops.YerevanCity, "Forks"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для кухни"))},
            { (Shops.YerevanCity, "French bread"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Fresh chicken"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Frying pans"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для кухни")) },
                
            { (Shops.YerevanCity, "Gingerbread, cookies"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Glass cleansers"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для уборки")) },
            { (Shops.YerevanCity, "Glasses and cups"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для кухни")) },
            { (Shops.YerevanCity, "Grain"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Graters"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для кухни")) },
            { (Shops.YerevanCity, "Grilled, mix vegetable"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Gums"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },

            { (Shops.YerevanCity, "Hand towel"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для кухни")) },
            { (Shops.YerevanCity, "Hangers"), (Categories.Outcome.ClothesAndShoes, Categories.Outcome.ClothesAndShoes.Sub("clothes")) },
            { (Shops.YerevanCity, "Hot sauces"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            
            { (Shops.YerevanCity, "Ice cream Cup"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Ice cream Eskimo"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Imported beer"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Алкоголь")) },
            { (Shops.YerevanCity, "Industrial bread"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            
            { (Shops.YerevanCity, "Jelly cake"), (Categories.Outcome.Food, Categories.Outcome.ForHouse.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Jugs"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для кухни")) },
            
            { (Shops.YerevanCity, "Kefir"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для кухни")) },
            { (Shops.YerevanCity, "Kitchenwares"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для кухни")) },
            { (Shops.YerevanCity, "Kvass"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            
            
            { (Shops.YerevanCity, "Linens"), (Categories.Outcome.ClothesAndShoes, Categories.Outcome.ClothesAndShoes.Sub("clothes")) },
            { (Shops.YerevanCity, "Local bread"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Local crackers"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            
            { (Shops.YerevanCity, "Marmalade"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Mascarpone, ricotta, burrata cheeses"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Meals with meat and seafood"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Meals with vegetables"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Milk"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Mineral water"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Moldy cheeses"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Mouthwash"), (Categories.Outcome.Health, Categories.Outcome.Health.Sub("Гигиена")) },

            { (Shops.YerevanCity, "Nail Care"), (Categories.Outcome.Beauty, Categories.Outcome.Beauty.Sub("Косметика")) },
            { (Shops.YerevanCity, "Nuggets, snack"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Nuts"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Фрукты/овощи")) },
            
            { (Shops.YerevanCity, "Oat flakes, Hercules"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Okroshka"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Olive oil"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Oven utensils"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для кухни")) },
            
            { (Shops.YerevanCity, "Pasta"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Pens"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Канцтовары")) },
            { (Shops.YerevanCity, "Pillows"), (Categories.Outcome.ClothesAndShoes, Categories.Outcome.ClothesAndShoes.Sub("clothes")) },
            { (Shops.YerevanCity, "Plastic packets"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Мешки и пакеты")) },
            { (Shops.YerevanCity, "Plates"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для кухни")) },
            { (Shops.YerevanCity, "Pocket napkins"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для уборки")) },
            { (Shops.YerevanCity, "Porridge"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Potato chips"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
             
            { (Shops.YerevanCity, "Rice"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },  

            { (Shops.YerevanCity, "Salad and sauce bowls"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для кухни")) },
            { (Shops.YerevanCity, "Salted sticks"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Sandviches"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Sauces for salads"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Sauces for sushi and Asian cuisine"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Simple napkins"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для уборки")) },
            { (Shops.YerevanCity, "Snacks"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Soups"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Sour cream"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Продукты")) },
            { (Shops.YerevanCity, "Spoons"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для кухни")) },
            
            { (Shops.YerevanCity, "Toilet paper"), (Categories.Outcome.ForHouse, Categories.Outcome.ForHouse.Sub("Для туалета")) },
            { (Shops.YerevanCity, "Toothpaste"), (Categories.Outcome.Health, Categories.Outcome.Health.Sub("Гигиена")) },
            
            { (Shops.YerevanCity, "Vegetables"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Фрукты/овощи")) },
            
            { (Shops.YerevanCity, "Waffles"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            
            { (Shops.YerevanCity, "Yogurt"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
            { (Shops.YerevanCity, "Yogurt product, dessert, pudding"), (Categories.Outcome.Food, Categories.Outcome.Food.Sub("Перекусы")) },
        };
    
    public (Category, SubCategory?) Map(ExternalCategory external, Category fallback)
    {
        if (_map.TryGetValue((external.Source, external.RawName), out var mapped))
        {
            return mapped;
        }

        return (fallback, null);
    }
}