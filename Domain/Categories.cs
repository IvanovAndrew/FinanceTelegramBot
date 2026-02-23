namespace Domain;

public static class Categories
{
    public static class Outcome
    {
        public static readonly Category Food = new Category()
        {
            Code = "Food",
            Name = "Еда",
            Subcategories =
            [
                new SubCategory() { Code = "products", Name = "Продукты" },
                new SubCategory() { Code = "alcohol", Name = "Алкоголь" },
                new SubCategory() { Code = "snacks", Name = "Перекусы" },
                new SubCategory() { Code = "fruits", Name = "Фрукты/овощи" },
            ]
        };

        public static readonly Category Transport = new Category()
        {
            Code = "Transport",
            Name = "Транспорт",
            Subcategories =
            [
                new SubCategory() { Code = "subway", Name = "Метро" },
                new SubCategory() { Code = "bus", Name = "Автобус" },
                new SubCategory() { Code = "tram", Name = "Трамвай" },
                new SubCategory() { Code = "trolley", Name = "Троллейбус" },
                new SubCategory() { Code = "localtrain", Name = "Электричка" },
                new SubCategory() { Code = "taxi", Name = "Такси" },
                new SubCategory() { Code = "transportcard", Name = "Транспортная карта" },
            ],
        };

        public static readonly Category Restaurants = new Category()
        {
            Code = "Restaurants",
            Name = "Рестораны",
            Subcategories =
            [
                new SubCategory() { Code = "restaurant", Name = "Ресторан" },
                new SubCategory() { Code = "bar", Name = "Бар" },
                new SubCategory() { Code = "diningroom", Name = "Столовая" },
                new SubCategory() { Code = "cafe", Name = "Кафе" },
            ]
        };

        public static readonly Category Gifts = new Category()
        {
            Code = "Gifts",
            Name = "Подарки",
            Subcategories =
            [
                new SubCategory() { Code = "friends", Name = "Друзьям" },
                new SubCategory() { Code = "donations", Name = "Пожертвования" },
                new SubCategory() { Code = "tips", Name = "Чаевые" },
                new SubCategory() { Code = "flowers", Name = "Цветы" },
                new SubCategory() { Code = "each other", Name = "Друг другу" },
            ]

        };

        public static readonly Category Delivery = new Category() { Code = "Delivery", Name = "Доставка" };

        public static readonly Category Health = new Category()
        {
            Code = "Health",
            Name = "Здоровье, гигиена",
            Subcategories =
            [
                new SubCategory() { Code = "doctor", Name = "Врач" },
                new SubCategory() { Code = "analysis", Name = "Анализы" },
                new SubCategory() { Code = "pills", Name = "Лекарства" },
                new SubCategory() { Code = "hygiene", Name = "Гигиена" },
                new SubCategory() { Code = "massauge", Name = "Массаж" },
                new SubCategory() { Code = "wc", Name = "Туалет" },
                new SubCategory() { Code = "other", Name = "Прочее" }
            ]
        };

        public static readonly Category Beauty = new Category()
        {
            Code = "Beauty",
            Name = "Красота",
            Subcategories = new[]
            {
                new SubCategory() { Code = "perfume", Name = "Духи" },
                new SubCategory() { Code = "manicure", Name = "Маникюр" },
                new SubCategory() { Code = "haircut", Name = "Стрижка" },
                new SubCategory() { Code = "cosmetics", Name = "Косметика" },
                new SubCategory() { Code = "epilation", Name = "Эпиляция" },
                new SubCategory() { Code = "jewelry", Name = "Украшения" },
            }
        };

        public static readonly Category Pets = new Category()
        {
            Code = "Pets",
            Name = "Домашние животные",
            ShortName = "Коты",
            Subcategories =
            [
                new SubCategory() { Code = "food", Name = "Корм" },
                new SubCategory() { Code = "hygiene", Name = "Гигиена" },
                new SubCategory() { Code = "toys", Name = "Игрушки" },
                new SubCategory() { Code = "drugs", Name = "Лекарства" },
                new SubCategory() { Code = "vetclinic", Name = "Ветклиника" },
                new SubCategory() { Code = "stray", Name = "Уличные" },
                new SubCategory() { Code = "nunny", Name = "Котоняня" },
                new SubCategory() { Code = "other", Name = "Прочее" },
            ],
        };

        public static readonly Category ClothesAndShoes = new Category()
        {
            Code = "ClothesAndShoes",
            Name = "Одежда, обувь",
            Subcategories =
            [
                new SubCategory() { Code = "clothes", Name = "Одежда" },
                new SubCategory() { Code = "shoes", Name = "Обувь" },
                new SubCategory() { Code = "atelier", Name = "Ателье" },
                new SubCategory() { Code = "laundry", Name = "Прачечная" },
                new SubCategory() { Code = "drycleaning", Name = "Химчистка" },
            ]
        };

        public static readonly Category CulturalLife = new Category()
        {
            Code = "CulturalLife",
            Name = "Культурная жизнь",
            Subcategories = new []
            {
                new SubCategory() {Code = "cinamatheatre", Name = "Кино/театры"},
                new SubCategory() {Code = "museum", Name = "Музей"},
                new SubCategory() {Code = "concert", Name = "Концерт"},
                new SubCategory() {Code = "stadium", Name = "Стадион"},
                new SubCategory() {Code = "excursion", Name = "Экскурсия"},
                new SubCategory() {Code = "zoo", Name = "Зоопарк"},
                new SubCategory() {Code = "attraction", Name = "Аттракционы"},
                new SubCategory() {Code = "exhibition", Name = "Выставка"},
                new SubCategory() {Code = "workshop", Name = "Мастер-класс"},
            }
        };

        public static readonly Category Phone = new Category() { Code = "Phone", Name = "Телефон" };
        
        public static readonly Category Hobby = new Category()
        {
            Code = "Hobby", Name = "Хобби",
            Subcategories = [
                new SubCategory() { Code = "whatwherewhen", Name = "ЧГК" },
                new SubCategory() { Code = "sport", Name = "Спорт" },
                new SubCategory() { Code = "improvisation", Name = "Импровизация" },
                new SubCategory() { Code = "handmade", Name = "Рукоделие" },
                new SubCategory() { Code = "painting", Name = "Рисование" },
                new SubCategory() { Code = "chess", Name = "Шахматы" },
                new SubCategory() { Code = "books", Name = "Книги" },
                new SubCategory() { Code = "crosswords", Name = "Кроссворды" },
                new SubCategory() { Code = "boardgame", Name = "Настольные игры" },
            ]
        };

        public static readonly Category ForHouse = new Category()
        {
            Code = "ForHouse",
            Name = "Товары в дом",
            Subcategories = [
                new SubCategory(){Code = "fortoilet", Name = "Для туалета"},
                new SubCategory(){Code = "forwashing", Name = "Для стирки"},
                new SubCategory(){Code = "forkitchen", Name = "Для кухни"},
                new SubCategory(){Code = "packages", Name = "Мешки и пакеты"},
                new SubCategory(){Code = "forcoziness", Name = "Уют"},
                new SubCategory(){Code = "forcleanup", Name = "Для уборки"},
                new SubCategory(){Code = "forflowers", Name = "Для цветов"},
                new SubCategory(){Code = "fordevices", Name = "Для техники"},
                new SubCategory(){Code = "renovation", Name = "Ремонт"},
                new SubCategory(){Code = "textile", Name = "Текстиль"},
                new SubCategory(){Code = "cancellior", Name = "Канцтовары"},
            ]
        };

        public static readonly Category OnlineService = new Category()
            { Code = "Onlineservice", Name = "Онлайн-сервисы" };

        public static readonly Category Documents = new Category() { Code = "Documents", Name = "Документы и пошлины" };
        public static readonly Category Bank = new Category() { Code = "Bank", Name = "Банк" };
        public static readonly Category Psycologist = new Category() { Code = "Psycologist", Name = "Психолог" };
        public static readonly Category Cigarettes = new Category() { Code = "Cigarettes", Name = "Сигареты" };
        
        public static readonly Category Education = new Category()
        {
            Code = "education", Name = "Образование",
            Subcategories = [
                new SubCategory() { Code = "languages", Name = "Языки", },
                new SubCategory() { Code = "driving", Name = "Вождение", },
                new SubCategory() { Code = "lecture", Name = "Лекция", },
                new SubCategory() { Code = "design", Name = "Дизайн", },
            ]
        };
        
        public static readonly Category Flat = new Category()
        {
            Code = "Flat", Name = "Квартира",
            Subcategories = [
                new SubCategory() { Code = "renovation", Name = "Ремонт", },
                new SubCategory() { Code = "supplies", Name = "ЖКХ", },
                new SubCategory() { Code = "electricity", Name = "Электричество", },
                new SubCategory() { Code = "rent", Name = "Оплата квартиры", },
                new SubCategory() { Code = "gaz", Name = "Газ", },
                new SubCategory() { Code = "gazservice", Name = "Обслуживание газа", },
                new SubCategory() { Code = "internet", Name = "Интернет", },
                new SubCategory() { Code = "water", Name = "Вода", },
                new SubCategory() { Code = "other", Name = "Прочее", },
            ]
        };

        public static readonly Category CurrencyExchange = new Category()
        {
            Code = "CurrencyExchange",
            Name = "Обмен валюты"
        };

        public static readonly Category Other = new Category()
        {
            Code = "Others",
            Name = "Прочее"
        };

        public static Category DefaultCategory => Food;

        public static IReadOnlyList<Category> All =
        [
            Food, Pets, Bank, Beauty, Cigarettes, ClothesAndShoes, CulturalLife,
            CurrencyExchange, Delivery, Documents, Education, Flat, ForHouse, 
            Gifts, Health, Hobby, OnlineService, Phone, Psycologist, Restaurants, 
            Transport, Other
        ];

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
        public static readonly Category Salary = new Category()
        {
            Code = "Salary",
            Name = "Зарплата",
        };
        
        public static readonly Category Bonus = new Category()
        {
            Code = "Bonus",
            Name = "Бонус",
        };
        
        public static readonly Category VacationBonuses = new Category()
        {
            Code = "VacationBonuses",
            Name = "Отпускные",
        };
        
        public static readonly Category Cashback = new Category()
        {
            Code = "Cashback",
            Name = "Кэшбек",
        };

        public static readonly Category Interests = new Category()
        {
            Code = "Interests",
            Name = "% на остаток",
        };
        
        public static readonly Category Rent = new Category()
        {
            Code = "Rent",
            Name = "Аренда квартиры",
        };
        
        public static readonly Category Improvisation = new Category()
        {
            Code = "Improvisation",
            Name = "Импровизация",
        };
        
        public static readonly Category Others = new Category()
        {
            Code = "Others",
            Name = "Прочее",
        };

        public static IReadOnlyList<Category> Salaries = [Salary, VacationBonuses, Bonus];

        public static IReadOnlyList<Category> All =>
            [Salary, VacationBonuses, Bonus, Cashback, Interests, Rent, Improvisation, Others];

        public static Category? GetCategory(string input)
        {
            return All.FirstOrDefault(c => string.Equals(c.Name, input, StringComparison.InvariantCultureIgnoreCase)|| string.Equals(c.Code, input, StringComparison.InvariantCultureIgnoreCase));
        }
    }
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