using System.Net.Mime;
using Domain;
using Infrastructure;
using Infrastructure.Fns;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using StateMachine;

namespace UnitTest;

public class StateTest
{
    [Test]
    public async Task ThereAreTwoOptionsInGreetingState()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        
        // Assert
        CollectionAssert.AreEquivalent(new []{"Outcome", "Income", "Statistics"}, lastMessage.TelegramKeyboard?.Buttons.SelectMany(b => b.Select(b1 => b1)).Select(c => c.Text));
    }
    
    [Ignore("Reimpement test")]
    [TestCase("Outcome")]
    [TestCase("Income")]
    [TestCase("Statistics")]
    public async Task AfterPressingOnAnyButtonInGreetingState_TheGreetingMessageIsDeleted(string pressedButton)
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        var greetingMessage = await botEngine.Proceed("/start");
        var message = await botEngine.Proceed(pressedButton);

        // Assert
        CollectionAssert.DoesNotContain(telegramBot.SentMessages, greetingMessage);
    }
    
    [Test]
    public async Task ThereAreThreeDaysForOutcome()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);

        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        lastMessage = await botEngine.Proceed("By myself");
        
        // Assert
        CollectionAssert.AreEquivalent(new []{"Today", "Yesterday", "Another day"}, lastMessage.TelegramKeyboard?.Buttons.SelectMany(b => b.Select(b1 => b1)).Select(c => c.Text));
    }
    
    [TestCase("today")]
    [TestCase("yesterday")]
    public async Task AfterEnteringDateWeChooseACategory(string date)
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);

        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        var lastMessage = await botEngine.Proceed(date);
        
        // Assert
        Assert.That(lastMessage.Text, Is.EqualTo("Enter the category"));
        CollectionAssert.AreEquivalent(new []{"Food", "Cats"}, lastMessage.TelegramKeyboard?.Buttons.SelectMany(c => c.Select(b => b.Text)));
    }

    [Test]
    public async Task WhenACategoryWithoutSubcategoryIsChosenTheDescriptionWillBeAsked()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        await botEngine.Proceed("today");
        var lastMessage = await botEngine.Proceed("cats");
        
        // Assert
        Assert.That(lastMessage.Text, Is.EqualTo("Write a description"));
    }
    
    [Test]
    public async Task WhenDescriptionIsAddedThePriceWillBeAsked()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        await botEngine.Proceed("today");
        await botEngine.Proceed("cats");
        var lastMessage = await botEngine.Proceed("royal canin");
        
        // Assert
        Assert.That(lastMessage.Text, Is.EqualTo("Enter the price"));
    }
    
    [TestCase("1 рубль")]
    [TestCase("10 рублей")]
    [TestCase("100 rur")]
    [TestCase("50 amd")]
    [TestCase("50 драм")]
    [TestCase("50 драмов")]
    public async Task WhenThePriceIsAddedThenSaveWillBeAsked(string price)
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        await botEngine.Proceed("today");
        await botEngine.Proceed("cats");
        await botEngine.Proceed("royal canin");
        var lastMessage = await botEngine.Proceed(price);
        
        // Assert
        StringAssert.EndsWith("save it?", lastMessage.Text);
        CollectionAssert.AreEquivalent(new []{"Save", "Cancel"}, lastMessage.TelegramKeyboard?.Buttons?.SelectMany(row => row.Select(b => b.Text)));
    }
    
    
    [Test]
    public async Task ClickOnSaveButtonSavesTheExpense()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        await botEngine.Proceed("today");
        await botEngine.Proceed("cats");
        await botEngine.Proceed("royal canin");
        await botEngine.Proceed("20000 amd");
        var lastMessage = await botEngine.Proceed("Save");

        var savedExpenses = await expenseRepository.ReadOutcomes(new FinanceFilter(), default);
        var savedExpense = savedExpenses.First();
        
        // Assert
        Assert.That(() => new DateOnly(2023, 6, 29) == savedExpense.Date);
        Assert.That(savedExpense.Category, Is.EqualTo("Cats"));
        Assert.That(savedExpense.SubCategory, Is.EqualTo(null));
        Assert.That(savedExpense.Description, Is.EqualTo("royal canin"));
        Assert.That(savedExpense.Amount, Is.EqualTo(new Money(){Amount = 20_000, Currency = Currency.Amd}));
    }
    
    [Test]
    public async Task ClickOnSaveButtonSavesTheIncome()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2024, 9, 14));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var finanseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, finanseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("income");
        await botEngine.Proceed("Another date");
        await botEngine.Proceed("08.09.2024");
        await botEngine.Proceed("Прочее");
        await botEngine.Proceed("Improvisation class");
        await botEngine.Proceed("8000 amd");
        var lastMessage = await botEngine.Proceed("Save");

        var savedIncomes = await finanseRepository.ReadIncomes(new FinanceFilter(), default);
        var savedIncome = savedIncomes.First();
        
        // Assert
        StringAssert.EndsWith("Saved", lastMessage.Text);
        Assert.That(() => new DateOnly(2024, 9, 8) == savedIncome.Date);
        Assert.That(savedIncome.Category, Is.EqualTo("Прочее"));
        Assert.That(savedIncome.Description, Is.EqualTo("Improvisation class"));
        Assert.That(savedIncome.Amount, Is.EqualTo(new Money(){Amount = 8_000, Currency = Currency.Amd}));
    }
    
    [Ignore("Reimplement test")]
    [Test]
    public async Task WhenBackCommandIsExecutedThenLastBotMessageWillBeRemoved()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        await botEngine.Proceed("today");
        var lastMessage = await botEngine.Proceed("/back");

        // Assert
        Assert.That(telegramBot.SentMessages.Select(c => c.Text), Is.Not.Contains("Enter the category"));
    }
    
    [Test]
    public async Task IfWrongPriceIsEnteredItWillBePossibleToReenterIt()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        await botEngine.Proceed("today");
        await botEngine.Proceed("cats");
        await botEngine.Proceed("royal canin");
        await botEngine.Proceed("20000 dam");
        await botEngine.Proceed("1999");
        await botEngine.Proceed("10000 amd");
        var lastMessage = await botEngine.Proceed("Save");

        var savedExpenses = await expenseRepository.ReadOutcomes(new FinanceFilter(), default);
        var savedExpense = savedExpenses.First();
        
        // Assert
        Assert.That(() => new DateOnly(2023, 6, 29) == savedExpense.Date);
        Assert.That(savedExpense.Category, Is.EqualTo("Cats"));
        Assert.That(savedExpense.SubCategory, Is.EqualTo(null));
        Assert.That(savedExpense.Description, Is.EqualTo("royal canin"));
        Assert.That(savedExpense.Amount, Is.EqualTo(new Money(){Amount = 10_000, Currency = Currency.Amd}));
    }
    
    [Test]
    public async Task ClickOnCancelButtonCancelsLongTermOperation()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub() {DelayTime = TimeSpan.FromMinutes(1)};
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        await botEngine.Proceed("today");
        await botEngine.Proceed("cats");
        await botEngine.Proceed("royal canin");
        await botEngine.Proceed("20000 amd");
        
        // Act
        var savingTask = botEngine.Proceed("Save");
        Thread.Sleep(TimeSpan.FromSeconds(1));
        var cancellingTask = botEngine.Proceed("/cancel");

        await Task.WhenAll(savingTask, cancellingTask);

        var savedExpenses = await expenseRepository.ReadOutcomes(new FinanceFilter(), default);
        
        // Assert
        Assert.That(savedExpenses.Count, Is.EqualTo(0));
    }
    
    [Test]
    public async Task ThereAreFiveOptionsInStatisticsState()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        var lastMessage = await botEngine.Proceed("statistics");

        // Assert
        CollectionAssert.AreEquivalent(new []{"Balance", "Day expenses (by categories)", "Month expenses (by categories)", "Category expenses (by months)", "Subcategory expenses (overall)", "Subcategory expenses (by months)"}, lastMessage.TelegramKeyboard?.Buttons.SelectMany(b => b.Select(b1 => b1)).Select(c => c.Text));
    }
    
    [Test]
    public async Task AfterClickingOnOutcomesFromJsonTheFileIsRequired()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);

        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        lastMessage = await botEngine.Proceed("From check");
        lastMessage = await botEngine.Proceed("json");
        
        // Assert
        CollectionAssert.AreEquivalent("Paste a json file", lastMessage.Text);
    }
    
    [TestCase(MediaTypeNames.Application.Pdf)]
    [TestCase(MediaTypeNames.Application.Xml)]
    [TestCase(MediaTypeNames.Text.Plain)]
    public async Task PastedFileShouldHaveJsonFormat(string mimeType)
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);

        var telegramFile = new FileInfoStub() { FileId = "1", FileName = "test.json", MimeType = mimeType };
        telegramBot.SavedFiles["1"] = new FileStub(){Text = "{\"dateTime\": \"2023-06-29T20:00:00\", \"items\":[{\"sum\": 100000,\"name\":\"Молоко\"}, {\"sum\": 78000, \"name\":\"Макароны\"}]}"};
        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        lastMessage = await botEngine.Proceed("From check");
        lastMessage = await botEngine.Proceed("json");
        lastMessage = await botEngine.ProceedFile(telegramFile);
        
        // Assert
        CollectionAssert.AreEquivalent("Paste a json file", lastMessage.Text);
    }
    

    [Test]
    public async Task PastedFileShouldHaveJsonFormat()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new FinanceRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);

        var telegramFile = new FileInfoStub() { FileId = "1", FileName = "test.json", MimeType = MediaTypeNames.Application.Json };
        telegramBot.SavedFiles["1"] = new FileStub(){Text = "{\"dateTime\": \"2023-06-29T20:00:00\", \"items\":[{\"sum\": 100000,\"name\":\"Молоко\"}, {\"sum\": 78000, \"name\":\"Макароны\"}]}"};
        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        lastMessage = await botEngine.Proceed("From check");
        lastMessage = await botEngine.Proceed("json");
        lastMessage = await botEngine.ProceedFile(telegramFile);
        
        // Assert
        Assert.That(telegramBot.SentMessages.Any(c => c.Text.Contains("All expenses are saved", StringComparison.InvariantCultureIgnoreCase)));
    }
    
    [Test]
    public async Task StatisticForADay()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Day expenses (by categories)");
        await botEngine.Proceed("Yesterday");
        var lastMessage = await botEngine.Proceed("All");

        // Assert
        StringAssert.Contains("23 July 2023", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Cats", lastMessage.Text);
        StringAssert.Contains("Food", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForADayAllowsToChooseBetweenTodayYesterdayAndEnterCustomDate()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        var response = await botEngine.Proceed("Day expenses (by categories)");
        

        // Assert
        var buttons = response.TelegramKeyboard?.Buttons?.SelectMany(c => c.Select(_ => _.Text));
        CollectionAssert.Contains(buttons, "Today");
        CollectionAssert.Contains(buttons, "Yesterday");
        CollectionAssert.Contains(buttons, "Another day");
    }
    
    [Test]
    public async Task StatisticForACustomDay()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 7, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Day expenses (by categories)");
        await botEngine.Proceed("Another day");
        await botEngine.Proceed("22 July 2023");
        var lastMessage = await botEngine.Proceed("All");
        
        // Assert
        StringAssert.Contains("22 July 2023", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Cats", lastMessage.Text);
        StringAssert.Contains($"10{TestConstants.NBSP}000 ֏", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForAMonthAllowsToChooseBetweenCurrentPreviousAndEnterCustomMonth()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        var response = await botEngine.Proceed("Month expenses (by categories)");
        

        // Assert
        var buttons = response.TelegramKeyboard?.Buttons?.SelectMany(c => c.Select(_ => _.Text));
        CollectionAssert.Contains(buttons, "This month");
        CollectionAssert.Contains(buttons, "Previous month");
        CollectionAssert.Contains(buttons, "Another month");
    }
    
    [Test]
    public async Task StatisticForACustomMonth()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Month expenses (by categories)");
        await botEngine.Proceed("Another month");
        await botEngine.Proceed("May 2023");
        var lastMessage = await botEngine.Proceed("All");
        

        // Assert
        StringAssert.Contains("May 2023", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Cats", lastMessage.Text);
        StringAssert.Contains($"10{TestConstants.NBSP}000 ֏", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForACategoryByAPeriod()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Category expenses (by months)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("July 2022");
        var lastMessage = await botEngine.Proceed("All");
        

        // Assert
        StringAssert.Contains("July 2023", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Food", lastMessage.Text);
        StringAssert.Contains($"6{TestConstants.NBSP}000 ֏", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForACategoryWithACustomDateRange()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Category expenses (by months)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("Another");
        await botEngine.Proceed("January 2022");
        var lastMessage = await botEngine.Proceed("All");
        

        // Assert
        StringAssert.Contains("July 2023", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Food", lastMessage.Text);
        StringAssert.Contains($"6{TestConstants.NBSP}000 ֏", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForASubcategory()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Subcategory expenses (overall)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("July 2022");
        var lastMessage = await botEngine.Proceed("AMD");
        

        // Assert
        StringAssert.Contains("Expenses from July 2022", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Subcategory", lastMessage.Text);
        StringAssert.Contains("Food", lastMessage.Text);
        StringAssert.Contains("Snacks", lastMessage.Text);
        StringAssert.Contains("Products", lastMessage.Text);
        StringAssert.Contains($"1{TestConstants.NBSP}000 ֏", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForASubcategoryWithCustomDateRange()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Subcategory expenses (overall)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("Another");
        await botEngine.Proceed("March 2022");
        var lastMessage = await botEngine.Proceed("All");
        

        // Assert
        StringAssert.Contains("Expenses from March 2022", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Food", lastMessage.Text);
        StringAssert.Contains("Snacks", lastMessage.Text);
        StringAssert.Contains("Products", lastMessage.Text);
        StringAssert.Contains($"1{TestConstants.NBSP}000 ֏", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForACategory2()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Subcategory expenses (by months)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("Snacks");
        await botEngine.Proceed("Another period");
        await botEngine.Proceed("July 2023");
        var lastMessage = await botEngine.Proceed("All");
        

        // Assert
        StringAssert.Contains("July 2023", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Food", lastMessage.Text);
        StringAssert.Contains($"1{TestConstants.NBSP}000 ֏", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForASubCategoryWithCustomDateRange()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Subcategory expenses (by months)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("Snacks");
        await botEngine.Proceed("Another period");
        await botEngine.Proceed("January 2022");
        var lastMessage = await botEngine.Proceed("All");
        

        // Assert
        StringAssert.Contains("July 2023", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Food", lastMessage.Text);
        StringAssert.Contains($"1{TestConstants.NBSP}000 ֏", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForASubCategoryByMonthsIsSortedChronologically()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateTime(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                Subcategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new FinanceRepositoryStub();
        await expenseRepository.SaveAllOutcomes(
            new List<IMoneyTransfer>()
            {
                new Outcome(){Date = new DateOnly(2023, 5, 22), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 6, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Outcome(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("Subcategory expenses (by months)");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("Snacks");
        await botEngine.Proceed("January 2023");
        var lastMessage = await botEngine.Proceed("AMD");
        
        // Assert
        StringAssertExtension.AssertOrder(lastMessage.Text, "May 2023", "June 2023", "July 2023");
    }

    private StateFactory CreateStateFactory(Category[] categories, IFnsService fnsService, IFinanceRepository financeRepository, IDateTimeService dateTimeService, ILogger<StateFactory> logger)
    {
        return new StateFactory(dateTimeService, categories, fnsService, financeRepository, logger);
    }

    private BotEngine CreateBotEngine(Category[] categories, IFnsService fnsService, IFinanceRepository financeRepository, IDateTimeService dateTimeService)
    {
        var logger = new LoggerStub<StateFactory>();
        var stateFactory = CreateStateFactory(categories, fnsService, financeRepository, dateTimeService, logger);

        return new BotEngine(stateFactory, logger);
    }
    
    private BotEngineWrapper CreateBotEngineWrapper(Category[] categories, IFinanceRepository financeRepository, IDateTimeService dateTimeService, TelegramBotMock telegramBot)
    {
        var botEngine = CreateBotEngine(categories, new FnsServiceStub(), financeRepository, dateTimeService);
        return new BotEngineWrapper(botEngine, telegramBot);
    }
}