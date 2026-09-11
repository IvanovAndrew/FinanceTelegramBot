namespace Domain;

public static class Categories
{
    public static class Outcome
    {
        public static readonly Category Food = Cat("Food", "Еда", null,
                Sub("products", "Продукты"),
                Sub("snacks", "Перекусы"),
                Sub("alcohol", "Алкоголь"),
                Sub("fruits", "Фрукты/овощи"));

        public static readonly Category Transport = Cat("Transport", "Транспорт", null,
                Sub("subway", "Метро"),
                Sub("bus", "Автобус"),
                Sub("tram", "Трамвай"),
                Sub("trolley", "Троллейбус"),
                Sub("localtrain", "Электричка"),
                Sub("taxi", "Такси"),
                Sub("routetaxi", "Маршрутка"),
                Sub("transportcard", "Транспортная карта"),
                Sub("rent", "Аренда"));

        public static readonly Category Restaurants = Cat(
            "Restaurants",
            "Рестораны",
            null,
            Sub("restaurant", "Ресторан"),
            Sub("bar", "Бар"),
            Sub("diningroom", "Столовая"),
            Sub("cafe", "Кафе"),
            Sub("fooddelivery", "Доставка еды"));

        public static readonly Category Gifts = Cat(
            "Gifts",
            "Подарки",
            null,
                Sub("friends", "Друзьям"),
                Sub("donations", "Пожертвования"),
                Sub("tips", "Чаевые"),
                Sub("flowers", "Цветы"),
                Sub("each other", "Друг другу"));

        public static readonly Category Delivery = Cat("Delivery", "Доставка", null,
                Sub("international", "Международная"),
                Sub("local", "Местная"));

        public static readonly Category Health = Cat(
            "Health",
            "Здоровье, гигиена", null,
                Sub("doctor", "Врач"),
                Sub("analysis", "Анализы"),
                Sub("pills", "Лекарства"),
                Sub("hygiene", "Гигиена"),
                Sub("massauge", "Массаж"),
                Sub("wc", "Туалет"),
                Sub("other", "Прочее"));

        public static readonly Category Beauty = Cat("Beauty", "Красота", null,
                Sub("perfume", "Духи"),
                Sub("manicure", "Маникюр"),
                Sub("haircut", "Стрижка"),
                Sub("cosmetics", "Косметика"),
                Sub("epilation", "Эпиляция"),
                Sub("jewelry", "Украшение"));

        public static readonly Category Pets = Cat("Pets", "Домашние животные", "Коты",
                Sub("food", "Корм"),
                Sub("hygiene", "Гигиена"),
                Sub("toys", "Игрушки"),
                Sub("drugs", "Лекарства"),
                Sub("vetclinic", "Ветклиника"),
                Sub("stray", "Уличные"),
                Sub("nunny", "Котоняня"),
                Sub("other", "Прочее"));

        public static readonly Category ClothesAndShoes = Cat("ClothesAndShoes", "Одежда, обувь", null,
            Sub("clothes", "Одежда"),
            Sub("shoes", "Обувь"),
            Sub("atelier", "Ателье"),
            Sub("laundry", "Прачечная"),
            Sub("drycleaning", "Химчистка"));

        public static readonly Category Leisure = Cat("Leisure", "Досуг", null,
            Sub("cinamatheatre", "Кино/театры"),
            Sub("museum", "Музей"),
            Sub("concert", "Концерт"),
            Sub("stadium", "Стадион"),
            Sub("excursion", "Экскурсия"),
            Sub("zoo", "Зоопарк"),
            Sub("attraction", "Аттракционы"),
            Sub("exhibition", "Выставка"),
            Sub("workshop", "Мастер-класс"));
    
        public static readonly Category Phone = Cat("Phone", "Телефон");

        public static readonly Category Hobby = Cat("Hobby", "Хобби", null,
            Sub("whatwherewhen", "ЧГК"),
            Sub("sport", "Спорт"),
            Sub("improvisation", "Импровизация"),
            Sub("handmade", "Рукоделие"),
            Sub("painting", "Рисование"),
            Sub("chess", "Шахматы"),
            Sub("books", "Книги"),
            Sub("crosswords", "Кроссворды"),
            Sub("boardgame", "Настольные игры"));
    
        public static readonly Category ForHouse = Cat("ForHouse", "Товары в дом", null,
                Sub("fortoilet", "Для туалета"),
                Sub("forwashing", "Для стирки"),
                Sub("forkitchen", "Для кухни"),
                Sub("packages", "Мешки и пакеты"),
                Sub("forcoziness", "Уют"),
                Sub("forcleanup", "Для уборки"),
                Sub("forflowers", "Для цветов"),
                Sub("fordevices", "Для техники"),
                Sub("renovation", "Ремонт"),
                Sub("textile", "Текстиль"),
                Sub("cancellior", "Канцтовары"));

        public static readonly Category OnlineService = Cat("Onlineservice", "Онлайн-сервисы");

        public static readonly Category Documents = Cat("Documents", "Документы", null,
            Sub("customs", "Пошлины"),
            Sub("documents", "Госдокументы"),
            Sub("photo", "Фото"),
            Sub("notariat", "Нотариат"));
            
        public static readonly Category Bank = Cat("Bank", "Банк");
        public static readonly Category Psycologist = Cat("Psycologist", "Психолог");
        public static readonly Category Devices = Cat("Devices", "Техника");
        public static readonly Category Cigarettes = Cat("Cigarettes", "Сигареты");

        public static readonly Category Education = Cat("Education", "Образование", null,
            Sub("languages", "Языки"),
            Sub("driving", "Вождение"),
            Sub("lecture", "Лекция"),
            Sub("design", "Дизайн"));

        public static readonly Category Flat = Cat("Flat", "Квартира", null,
            Sub("renovation", "Ремонт"),
            Sub("supplies", "ЖКХ"),
            Sub("electricity", "Электричество"),
            Sub("rent", "Оплата квартиры"),
            Sub("gaz", "Газ"),
            Sub("gazservice", "Обслуживание газа"),
            Sub("internet", "Интернет"),
            Sub("water", "Вода"),
            Sub("agent", "Риелтор"),
            Sub("zalog", "Залог"),
            Sub("other", "Прочее"));

        public static readonly Category CurrencyExchange = Cat("CurrencyExchange", "Обмен валюты");

        public static readonly Category Travel = Cat("Travel", "Путешествия", null,
            Sub("tickets", "Билеты"),
            Sub("buggage", "Багаж"),
            Sub("hotel", "Отель"),
            Sub("insurance", "Страховка"), 
            Sub("visa", "Виза"), 
            Sub("rouming", "Роуминг"), 
            Sub("accessories", "Аксессуары")
            );

        public static readonly Category BigDeal = Cat("BigDeal", "Крупные", null,
            Sub("technique", "Техника"),
            Sub("operation", "Операция"));
        
        public static readonly Category Savings = Cat("Savings","Сбережения");
        public static readonly Category Other = Cat("Others","Прочее");
    
        public static Category DefaultCategory => Other;
    
        public static readonly IReadOnlyList<Category> All =
        [
            Food, Pets, Bank, BigDeal, Beauty, Cigarettes, 
            ClothesAndShoes, Devices, Leisure, CurrencyExchange, Delivery, Documents, 
            Education, Flat, ForHouse, Gifts, Health, Hobby, 
            OnlineService, Phone, Psycologist, Restaurants, Savings, Transport, 
            Travel, Other
        ];
        
        public static readonly IReadOnlyList<Category> Popular =
        [
            Food, Pets, Beauty, 
            Leisure, Education, ForHouse, 
            Health, Hobby, OnlineService, 
            Psycologist, Restaurants, Transport
        ];

        public static readonly IReadOnlyList<Category> OutDated = [Cigarettes];
        
        public static IReadOnlyList<Category> Actual => All.ToList().Except(OutDated).ToList();
        
        public static Category? GetCategory(string input)
        {
            return All.FirstOrDefault(c => string.Equals(c.Name, input, StringComparison.InvariantCultureIgnoreCase) ||
                                           string.Equals(c.ShortName, input, StringComparison.InvariantCultureIgnoreCase) || 
                                           string.Equals(c.Code, input, StringComparison.InvariantCultureIgnoreCase)
                                           );
        }
    }
    
    public static class Income
    {
        public static readonly Category Salary = Cat("Salary", "Зарплата");
        
        public static readonly Category Bonus = Cat("Bonus", "Бонус");
        
        public static readonly Category VacationBonuses = Cat("VacationBonuses", "Отпускные");
        
        public static readonly Category Cashback = Cat("Cashback", "Кэшбек");
    
        public static readonly Category Interests = Cat("Interests", "% на остаток");
        
        public static readonly Category Rent = Cat("Rent", "Аренда квартиры");
        public static readonly Category CurrencyExchange = Cat("CurrencyExchange", "Обмен валюты");
        
        public static readonly Category Improvisation = Cat("Improvisation", "Импровизация");
        public static readonly Category Savings = Cat("Savings", "Из сбережений");
        
        public static readonly Category Others = Cat("Others", "Прочее");
    
        public static IReadOnlyList<Category> Salaries = [Salary, VacationBonuses, Bonus];
    
        public static IReadOnlyList<Category> All =>
            [Salary, VacationBonuses, Bonus, Cashback, Interests, Rent, Improvisation, Savings, Others];
    
        public static Category? GetCategory(string input)
        {
            return All.FirstOrDefault(c => string.Equals(c.Name, input, StringComparison.InvariantCultureIgnoreCase)|| string.Equals(c.Code, input, StringComparison.InvariantCultureIgnoreCase));
        }
    }
    
    static Category Cat(string code, string name, string? shortName = null, params SubCategory[] subs)
        => new() { 
            Code = code,
            Name = name,
            ShortName = shortName,
            Subcategories = subs
        };

    static SubCategory Sub(string code, string name, string? shortName = null)
        => new()
        {
            Code = code,
            Name = name,
            ShortName = shortName,
        };
}

public static class CategoryExtensions
{
    public static SubCategory? Sub(this Category category, string codeOrName)
    {
        if (string.IsNullOrEmpty(codeOrName))
            return null;
        
        var subcategory = codeOrName.Trim();
        
        return
            category?.Subcategories?.FirstOrDefault(s =>
                string.Equals(s.Code, subcategory, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(s.Name, subcategory, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(s.ShortName, subcategory, StringComparison.InvariantCultureIgnoreCase));
    }
}