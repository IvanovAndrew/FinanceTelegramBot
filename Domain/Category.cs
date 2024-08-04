using System.ComponentModel;

namespace Domain
{
    public class Category
    {
        public string Name { get; init; }
        public string? ShortName { get; init; }
        public SubCategory[] Subcategories { get; set; } = Array.Empty<SubCategory>();

        public Category()
        {
        
        }
    }

    public class SubCategory
    {
        public string Name { get; init; } = String.Empty;
        public string? ShortName { get; init; }
    }

    public enum eExpenseCategory
    {
        [Description("Транспорт")]
        Transport = 1,
        [Description("Еда")]
        Food = 2,
        [Description("Рестораны")]
        Restaurants = 3,
        [Description("Подарки")]
        Presents = 4,
        [Description("Услуги")]
        Services = 5,
        [Description("Здоровье, гигиена")]
        Health = 6,
        [Description("Одежда, обувь")]
        Clothes = 7,
        [Description("Культурная жизнь")]
        Culture = 8,
        [Description("Телефон")]
        Phone = 9,
        [Description("Хобби")]
        Hobby = 10,
        [Description("Домашние животные")]
        Pets = 11, 
        [Description("Товары в дом")]
        Home = 12,
        [Description("Онлайн-сервисы")]
        OnlineServices = 13,
        [Description("Сигареты")]
        Cigarettes = 14,
        [Description("Ремонт")]
        Renovation = 15,
        [Description("ЖКХ")]
        Housings = 16,
        [Description("Electricity")]
        Электричество = 17,
        [Description("Оплата квартиры")]
        Rent = 18,
        [Description("Газ")]
        Gas = 19,
        [Description("Обслуживание газа")]
        GasService = 21,
        [Description("Интернет")]
        Internet = 22,
        [Description("Вода")]
        Water = 23,
        [Description("Операция")]
        Operation = 24,
        [Description("Перелёты")]
        Flights = 25,
        [Description("Образование")]
        Education = 26,
        [Description("Отель")]
        Hotel = 27,
        [Description("Техника")]
        Gadgets = 28,
        [Description("Документы")]
        Documents = 29,
        [Description("Обмен валюты")]
        CurrencyExchange = 30,
        [Description("Прочее")]
        Other = 31
    }

    public enum eExpenseSubcategory
    {
        [Description("Метро")] Underground = 1,
        [Description("Автобус")] Bus = 2,
        [Description("Трамвай")] Tram = 3,
        [Description("Электричка")] Train = 4,
        [Description("Такси")] Taxi = 5,
        [Description("Маршрутка")] RouteTaxi = 6,
        [Description("Троллейбус")] Trolleybus = 7,
        [Description("Транспортная карта")] TransportCard = 8,
        [Description("Другой транспорт")] OtherTransport = 9,
        [Description("Продукты")] Products = 10,
        [Description("Фрукты/овощи")] FruitsAndVegetables = 11,
        [Description("Перекусы")] Snacks = 12,
        [Description("Алкоголь")] Alcohol = 13,
        [Description("Ресторан")] Restaurant = 14,
        [Description("Бар")] Bar = 15,
        [Description("Столовая")] Canteen = 16,
        [Description("Кафе")] Caffee = 17,
        [Description("Друзьям")] Friends = 18,
        [Description("Пожертвования")] Donations = 19,
        [Description("Чаевые")] Tips = 20,
        [Description("Цветы")] Flowers = 21,
        [Description("Друг другу")] ToEachOther = 22,
        [Description("Массаж")] Massage = 23,
        [Description("Банк")] Bank = 24,
        [Description("Доставка")] Delivery = 25,
        [Description("Прачечная")] Laundry = 26,
        [Description("Ателье")] Atelier = 27,
        [Description("Госуслуги")] Gosuslugi = 28,
        [Description("Канцелярия")] Сhancellery = 29,
        [Description("Психолог")] Psychologist = 30,
        [Description("Врач")] Doctor = 31,
        [Description("Анализы")] MedicalTests = 32,
        [Description("Красота")] Beauty = 33,
        [Description("Одежда")] Clothes = 34,
        [Description("Обувь")] Shoes = 35,
        [Description("Кино/театры")] CinemaTheater = 36,
        [Description("Музей")] Museum = 37,
        [Description("Концерт")] Concert = 38,
        [Description("Стадион")] Stadium = 39,
        [Description("Экскурсия")] Excursion = 40,
        [Description("Зоопарк")] Zoo = 41,
        [Description("Аттракционы")] Attractions = 42,
        [Description("Выставка")] Exhibition = 43,
        [Description("Мастер-класс")] MasterClass = 44,
        [Description("ЧГК")] Quiz = 45,
        [Description("Плавание")] Swimming = 46,
        [Description("Импровизация")] Improvisation = 47,
        [Description("Бег")] Running = 48,
        [Description("Вышивка")] Embroidery = 49,
        [Description("Велосипед")] Cycling = 50, 
        [Description("Рисование")] Drawing = 51,
        [Description("Шахматы")] Chess = 52,
        [Description("Коньки")] IceSkating = 53,
        [Description("Книги")] Books = 54,
        [Description("Бисер")] Beads = 55,
        [Description("Корм")] Feed = 56,
        [Description("Наполнитель")] Litter = 57,
        [Description("Игрушки")] Toys = 58,
        [Description("Лекарства")] Medicines = 59,
        [Description("Ветклиника")] VetClinic = 60,
        [Description("Уличные коты")] StreetCats = 61,
        [Description("Гигиена")] Hygiene = 62,
        [Description("Котоняня")] CatSitter = 63,
        [Description("Лакомства")] Treats = 64,
        [Description("Прочее")] Other = 65,
        [Description("Для туалета")] ForToilet = 66,
        [Description("Салфетки")] Napkins = 67,
        [Description("Для стирки")] ForLaundry = 68,
        [Description("Для кухни")] ForKitchen = 69,
        [Description("Мусорный мешок")] TrashBag = 70,
        [Description("Лампочка")] LightBulb = 71,
        [Description("Уют")] Comfort = 72,
        [Description("Чистящее средство")] CleaningProduct = 73,
        [Description("Для цветов")] ForFlowers = 74,
        [Description("Для техники")] ForAppliances = 75,
        [Description("Ремонт")] Repair = 76
    }

    public enum eIncomeCategory
    {
        [Description("Зарплата")]
        Salary = 1,
        [Description("Аренда квартиры")]
        Rent = 2,
        [Description("Кэшбек")]
        Cashback = 3,
        [Description("%")]
        Interests = 4,
        [Description("Прочее")]
        Other = 5
    }
}