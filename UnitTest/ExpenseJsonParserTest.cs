using Domain;
using Infrastructure.Fns;
using NUnit.Framework;

namespace UnitTest;

public class ExpenseJsonParserTest
{
    [Test]
    public void Parse()
    {
        var text = @"
{
""code"":3,
""user"":""ИП Спесивцева Анфиса Вячеславовна"",
""items"":[
    {
    ""nds"":6,
    ""sum"":33400,
    ""name"":""Лисбрю Кукис мид 0,45 (банка)"",
    ""price"":33400,
    ""ndsSum"":0,""quantity"":1,""paymentType"":4,""productType"":2},
    {""nds"":6,""sum"":58000,""name"":""Лисбрю Пайнэппл коконат мид 0,5 (стекло)"",""price"":29000,""ndsSum"":0,""quantity"":2,""paymentType"":4,""productType"":2},
    {""nds"":6,""sum"":31200,""name"":""ЛаБИРинт Эль с малиной лимоном и вербеной 0.5 (банка)"",""price"":31200,""ndsSum"":0,""quantity"":1,""paymentType"":4,""productType"":2},
    {""nds"":6,""sum"":31900,""name"":""4 Пивовара Кукинг гид Блуберри пай 0,5 (банка)"",""price"":31900,""ndsSum"":0,""quantity"":1,""paymentType"":4,""productType"":2}],
""ndsNo"":154500,""fnsUrl"":""www.nalog.gov.ru"",""region"":""47"",""userInn"":""262901602791"",""dateTime"":""2023-07-04T19:15:00"",""kktRegId"":""0006518874008604"",
""metadata"":
    {
        ""id"":4840015100088201000,
        ""ofdId"":""ofd1"",
        ""address"":""188692,Россия,Ленинградская обл,Всеволожский м.р-н,Заневское г.п.,Кудрово г.,,Европейский пр-кт,,д. 8,,,пом. 49-Н,"",
        ""subtype"":""receipt"",
        ""receiveDate"":""2023-07-04T16:16:18Z""
    },
    ""operator"":""Кияев Алексей Игоревич"",
    ""totalSum"":154500,
    ""creditSum"":0,
    ""numberKkt"":""0494011062"",
    ""fiscalSign"":2699635242,
    ""prepaidSum"":0,
    ""retailPlace"":""магазин ПИВОПТТОРГ"",
    ""shiftNumber"":371,
    ""cashTotalSum"":0,
    ""provisionSum"":0,
    ""ecashTotalSum"":154500,
    ""operationType"":1,
    ""redefine_mask"":8,
    ""requestNumber"":6,
    ""fiscalDriveNumber"":""9960440302391392"",
    ""messageFiscalSign"":9297368881396023000,
    ""appliedTaxationType"":32,
    ""fiscalDocumentNumber"":20083,
    ""fiscalDocumentFormatVer"":2}
";
        var parser = new ExpenseJsonParser();
        var expenses = parser.Parse(text, "Еда", Currency.Rur).ToList();

        Assert.That(expenses.Count, Is.EqualTo(4));
        CollectionAssert.AreEquivalent(
            new []
            {
                new Money(){Currency = Currency.Rur, Amount = 334},
                new Money(){Currency = Currency.Rur, Amount = 580},
                new Money(){Currency = Currency.Rur, Amount = 312},
                new Money(){Currency = Currency.Rur, Amount = 319},
            }, expenses.Select(c => c.Amount));
    }
    
    [Test]
    public void Parse2()
    {
        var text = @"{
    ""messageFiscalSign"": 9297380857849405000,
    ""code"": 3,
    ""fiscalDocumentFormatVer"": 4,
    ""fiscalDriveNumber"": ""7281440500506263"",
    ""kktRegId"": ""0006365226040924    "",
    ""userInn"": ""7838471956  "",
    ""fiscalDocumentNumber"": 22465,
    ""dateTime"": ""2023-07-05T17:20:00"",
    ""fiscalSign"": 4049126880,
    ""shiftNumber"": 43,
    ""requestNumber"": 316,
    ""operationType"": 1,
    ""totalSum"": 44387,
    ""user"": ""ООО \""ТК Прогресс\"""",
    ""appliedTaxationType"": 1,
    ""operator"": ""Мамаджанов"",
    ""operatorInn"": ""780457521443"",
    ""retailPlaceAddress"": ""г.Санкт-Петербург, 192281, ул Будапештская, д. 83/39, литера А, пом. 2-Н"",
    ""retailPlace"": ""Магазин"",
    ""items"": [
        {
            ""paymentType"": 4,
            ""productType"": 33,
            ""name"": ""Вода мин. \""Псыж\"" л/с газ пэт 1л"",
            ""price"": 3999,
            ""quantity"": 1,
            ""nds"": 1,
            ""ndsSum"": 667,
            ""sum"": 3999,
            ""itemsQuantityMeasure"": 0,
            ""labelCodeProcesMode"": 0,
            ""productCodeNew"": {
                ""gs1m"": {
                    ""rawProductCode"": ""0104605035006964215NGjoAoE7VGqe"",
                    ""productIdType"": 6,
                    ""gtin"": ""04605035006964"",
                    ""sernum"": ""5NGjoAoE7VGqe""
                }
            },
            ""checkingProdInformationResult"": 15
        },
        {
            ""paymentType"": 4,
            ""productType"": 1,
            ""name"": ""Сухарики \""Кириешки\"" ржаные красная икра 40г"",
            ""price"": 1299,
            ""quantity"": 3,
            ""nds"": 2,
            ""ndsSum"": 354,
            ""sum"": 3897,
            ""itemsQuantityMeasure"": 0
        },
        {
            ""paymentType"": 4,
            ""productType"": 1,
            ""name"": ""***Соломка фри \""Русская картошка\"" со вкусом кетчупа и зелени 100г"",
            ""price"": 4999,
            ""quantity"": 1,
            ""nds"": 2,
            ""ndsSum"": 454,
            ""sum"": 4999,
            ""itemsQuantityMeasure"": 0
        },
        {
            ""paymentType"": 4,
            ""productType"": 1,
            ""name"": ""Сухарики \""Крутец\"" пшенично-ржаные со вкусом сметаны с зеленью 35г"",
            ""price"": 999,
            ""quantity"": 1,
            ""nds"": 2,
            ""ndsSum"": 91,
            ""sum"": 999,
            ""itemsQuantityMeasure"": 0
        },
        {
            ""paymentType"": 4,
            ""productType"": 1,
            ""name"": ""Чипсы \""Начос\"" кукурузные омар средиземноморский 100г, Деликадос"",
            ""price"": 4999,
            ""quantity"": 1,
            ""nds"": 2,
            ""ndsSum"": 454,
            ""sum"": 4999,
            ""itemsQuantityMeasure"": 0
        },
        {
            ""paymentType"": 4,
            ""productType"": 1,
            ""name"": ""Сухарики \""Барные снэки\"" ветчина/сыр 100г"",
            ""price"": 1999,
            ""quantity"": 1,
            ""nds"": 2,
            ""ndsSum"": 182,
            ""sum"": 1999,
            ""itemsQuantityMeasure"": 0
        },
        {
            ""paymentType"": 4,
            ""productType"": 1,
            ""name"": ""Суп \""Кнорр Чашка супа\"" грибной с сухариками б/п 15,5г"",
            ""price"": 2499,
            ""quantity"": 1,
            ""nds"": 1,
            ""ndsSum"": 417,
            ""sum"": 2499,
            ""itemsQuantityMeasure"": 0
        },
        {
            ""paymentType"": 4,
            ""productType"": 1,
            ""name"": ""Сухарики \""Хрустим\"" багет вкус корол. краб 60г"",
            ""price"": 3999,
            ""quantity"": 1,
            ""nds"": 2,
            ""ndsSum"": 364,
            ""sum"": 3999,
            ""itemsQuantityMeasure"": 0
        },
        {
            ""paymentType"": 4,
            ""productType"": 1,
            ""name"": ""Щупальца кальмара и мидии \""Балтийский Берег\"" в рассоле пл/б 180г"",
            ""price"": 8999,
            ""quantity"": 1,
            ""nds"": 2,
            ""ndsSum"": 818,
            ""sum"": 8999,
            ""itemsQuantityMeasure"": 0
        },
        {
            ""paymentType"": 4,
            ""productType"": 33,
            ""name"": ""Молоко \""Славянские традиции\"" стер. 1,5% т/п 1л БЗМЖ"",
            ""price"": 4999,
            ""quantity"": 1,
            ""nds"": 2,
            ""ndsSum"": 454,
            ""sum"": 4999,
            ""itemsQuantityMeasure"": 0,
            ""labelCodeProcesMode"": 0,
            ""productCodeNew"": {
                ""gs1m"": {
                    ""rawProductCode"": ""01048103190195102123HbpUjj"",
                    ""productIdType"": 6,
                    ""gtin"": ""04810319019510"",
                    ""sernum"": ""23HbpUjj""
                }
            },
            ""checkingProdInformationResult"": 15
        },
        {
            ""paymentType"": 4,
            ""productType"": 1,
            ""name"": ""Десерт зам. шербет \""Морозофф\"" яблочный с аром. черн. смородина бум.ст. 90г "",
            ""price"": 2999,
            ""quantity"": 1,
            ""nds"": 1,
            ""ndsSum"": 500,
            ""sum"": 2999,
            ""itemsQuantityMeasure"": 0
        }
    ],
    ""cashTotalSum"": 0,
    ""ecashTotalSum"": 44387,
    ""prepaidSum"": 0,
    ""creditSum"": 0,
    ""provisionSum"": 0,
    ""nds18"": 1584,
    ""nds10"": 3171,
    ""nds0"": 0,
    ""ndsNo"": 0,
    ""nds18118"": 0,
    ""nds10110"": 0,
    ""sellerAddress"": ""noreply@platformaofd.ru"",
    ""fnsUrl"": ""nalog.gov.ru"",
    ""checkingLabeledProdResult"": 0,
    ""region"": ""78"",
    ""numberKkt"": ""02005016561"",
    ""redefine_mask"": 0,
    ""metadata"": {
        ""id"": 4841348669442431000,
        ""ofdId"": ""ofd9"",
        ""receiveDate"": ""2023-07-05T17:20:00Z"",
        ""subtype"": ""receipt"",
        ""address"": ""192281,Россия,город федерального значения Санкт-Петербург,муниципальный округ Балканский вн.тер.г.,,,,Будапештская ул,,д. 83/39,литера А,,помещение 2-Н,""
    }
}
";
        var parser = new ExpenseJsonParser();
        var expenses = parser.Parse(text, "Еда", Currency.Rur).ToList();

        Assert.That(expenses.Count, Is.EqualTo(11));
        CollectionAssert.AreEquivalent(
            new []
            {
                new Money(){Currency = Currency.Rur, Amount = 39.99m},
                new Money(){Currency = Currency.Rur, Amount = 38.97m},
                new Money(){Currency = Currency.Rur, Amount = 49.99m},
                new Money(){Currency = Currency.Rur, Amount = 9.99m},
                new Money(){Currency = Currency.Rur, Amount = 49.99m},
                new Money(){Currency = Currency.Rur, Amount = 19.99m},
                new Money(){Currency = Currency.Rur, Amount = 24.99m},
                new Money(){Currency = Currency.Rur, Amount = 39.99m},
                new Money(){Currency = Currency.Rur, Amount = 89.99m},
                new Money(){Currency = Currency.Rur, Amount = 49.99m},
                new Money(){Currency = Currency.Rur, Amount = 29.99m},
            }, expenses.Select(c => c.Amount));
    }
}