using System.Net.Mime;
using Domain;
using Infrastructure;
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
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        
        // Assert
        CollectionAssert.AreEquivalent(new []{"Outcome", "Statistics"}, lastMessage.TelegramKeyboard?.Buttons.SelectMany(b => b.Select(b1 => b1)).Select(c => c.Text));
    }
    
    [TestCase("Outcome")]
    [TestCase("Statistics")]
    public async Task AfterPressingOnAnyButtonInGreetingState_TheGreetingMessageIsDeleted(string pressedButton)
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new ExpenseRepositoryStub();
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
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);

        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        lastMessage = await botEngine.Proceed("By myself");
        
        // Assert
        CollectionAssert.AreEquivalent(new []{"Today", "Yesterday", "Other"}, lastMessage.TelegramKeyboard?.Buttons.SelectMany(b => b.Select(b1 => b1)).Select(c => c.Text));
    }
    
    [TestCase("today")]
    [TestCase("yesterday")]
    public async Task AfterEnteringDateWeChooseACategory(string answer)
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);

        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("outcome");
        await botEngine.Proceed("By myself");
        var lastMessage = await botEngine.Proceed(answer);
        
        // Assert
        Assert.That(lastMessage.Text, Is.EqualTo("Enter the category"));
        CollectionAssert.AreEquivalent(new []{"Food", "Cats"}, lastMessage.TelegramKeyboard?.Buttons.SelectMany(c => c.Select(b => b.Text)));
    }

    [Test]
    public async Task WhenACategoryWithoutSubcategoryIsChosenTheDescriptionWillBeAsked()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
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
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
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
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
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
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
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

        var savedExpenses = await expenseRepository.Read(default);
        var savedExpense = savedExpenses.First();
        
        // Assert
        Assert.That(() => new DateOnly(2023, 6, 29) == savedExpense.Date);
        Assert.That(savedExpense.Category, Is.EqualTo("Cats"));
        Assert.That(savedExpense.SubCategory, Is.EqualTo(null));
        Assert.That(savedExpense.Description, Is.EqualTo("royal canin"));
        Assert.That(savedExpense.Amount, Is.EqualTo(new Money(){Amount = 20_000, Currency = Currency.Amd}));
    }
    
    [Test]
    public async Task WhenBackCommandIsExecutedThenLastBotMessageWillBeRemoved()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
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
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
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

        var savedExpenses = await expenseRepository.Read(default);
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
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub() {DelayTime = TimeSpan.FromMinutes(1)};
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

        var savedExpenses = await expenseRepository.Read(default);
        
        // Assert
        Assert.That(savedExpenses.Count, Is.EqualTo(0));
    }
    
    [Test]
    public async Task ThereAreThreeOptionsInStatisticsState()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        var lastMessage = await botEngine.Proceed("statistics");

        // Assert
        CollectionAssert.AreEquivalent(new []{"For a day", "For a month", "For a category"}, lastMessage.TelegramKeyboard?.Buttons.SelectMany(b => b.Select(b1 => b1)).Select(c => c.Text));
    }
    
    [Test]
    public async Task AfterClickingOnOutcomesFromJsonTheFileIsRequired()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);

        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        lastMessage = await botEngine.Proceed("From check");
        
        // Assert
        CollectionAssert.AreEquivalent("Paste json file", lastMessage.Text);
    }
    
    [TestCase(MediaTypeNames.Application.Pdf)]
    [TestCase(MediaTypeNames.Application.Xml)]
    [TestCase(MediaTypeNames.Text.Plain)]
    public async Task PastedFileShouldHaveJsonFormat(string mimeType)
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);

        var telegramFile = new FileInfoStub() { FileId = "1", FileName = "test.json", MimeType = mimeType };
        telegramBot.SavedFiles["1"] = new FileStub(){Text = "{\"dateTime\": \"2023-06-29T20:00:00\", \"items\":[{\"sum\": 100000,\"name\":\"Молоко\"}, {\"sum\": 78000, \"name\":\"Макароны\"}]}"};
        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        lastMessage = await botEngine.Proceed("From check");
        lastMessage = await botEngine.ProceedFile(telegramFile);
        
        // Assert
        CollectionAssert.AreEquivalent("Paste json file", lastMessage.Text);
    }
    

    [Test]
    public async Task PastedFileShouldHaveJsonFormat()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 6, 29));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        var expenseRepository = new ExpenseRepositoryStub();
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);

        var telegramFile = new FileInfoStub() { FileId = "1", FileName = "test.json", MimeType = MediaTypeNames.Application.Json };
        telegramBot.SavedFiles["1"] = new FileStub(){Text = "{\"dateTime\": \"2023-06-29T20:00:00\", \"items\":[{\"sum\": 100000,\"name\":\"Молоко\"}, {\"sum\": 78000, \"name\":\"Макароны\"}]}"};
        
        // Act
        var lastMessage = await botEngine.Proceed("/start");
        lastMessage = await botEngine.Proceed("outcome");
        lastMessage = await botEngine.Proceed("From check");
        lastMessage = await botEngine.ProceedFile(telegramFile);
        
        // Assert
        Assert.That(telegramBot.SentMessages.Any(c => c.Text.Contains("All expenses are saved", StringComparison.InvariantCultureIgnoreCase)));
    }
    
    [Test]
    public async Task StatisticForADay()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        await expenseRepository.SaveAll(
            new List<IExpense>()
            {
                new Expense(){Date = new DateOnly(2023, 7, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("For a day");
        var lastMessage = await botEngine.Proceed("23 July 2023");

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
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        await expenseRepository.SaveAll(
            new List<IExpense>()
            {
                new Expense(){Date = new DateOnly(2023, 7, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        var response = await botEngine.Proceed("For a day");
        

        // Assert
        var buttons = response.TelegramKeyboard?.Buttons?.SelectMany(c => c.Select(_ => _.Text));
        CollectionAssert.Contains(buttons, "24 July 2023");
        CollectionAssert.Contains(buttons, "23 July 2023");
        CollectionAssert.Contains(buttons, "Another day");
    }
    
    [Test]
    public async Task StatisticForACustomDay()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        await expenseRepository.SaveAll(
            new List<IExpense>()
            {
                new Expense(){Date = new DateOnly(2023, 7, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("For a day");
        await botEngine.Proceed("Another day");
        var lastMessage = await botEngine.Proceed("22 July 2023");
        
        // Assert
        StringAssert.Contains("22 July 2023", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Cats", lastMessage.Text);
        StringAssert.Contains("֏10,000", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForAMonthAllowsToChooseBetweenCurrentPreviousAndEnterCustomMonth()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        await expenseRepository.SaveAll(
            new List<IExpense>()
            {
                new Expense(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        var response = await botEngine.Proceed("For a month");
        

        // Assert
        var buttons = response.TelegramKeyboard?.Buttons?.SelectMany(c => c.Select(_ => _.Text));
        CollectionAssert.Contains(buttons, "July 2023");
        CollectionAssert.Contains(buttons, "June 2023");
        CollectionAssert.Contains(buttons, "Another month");
    }
    
    [Test]
    public async Task StatisticForACustomMonth()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        await expenseRepository.SaveAll(
            new List<IExpense>()
            {
                new Expense(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("For a month");
        await botEngine.Proceed("Another month");
        var lastMessage = await botEngine.Proceed("May 2023");
        

        // Assert
        StringAssert.Contains("May 2023", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Cats", lastMessage.Text);
        StringAssert.Contains("֏10,000", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForACategory()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        await expenseRepository.SaveAll(
            new List<IExpense>()
            {
                new Expense(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("For a category");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("For the period");
        var lastMessage = await botEngine.Proceed("July 2022");
        

        // Assert
        StringAssert.Contains("July 2023", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Food", lastMessage.Text);
        StringAssert.Contains("֏6,000", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForACategoryWithCustomDateRange()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        await expenseRepository.SaveAll(
            new List<IExpense>()
            {
                new Expense(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("For a category");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("For the period");
        await botEngine.Proceed("Another");
        var lastMessage = await botEngine.Proceed("January 2022");
        

        // Assert
        StringAssert.Contains("July 2023", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Food", lastMessage.Text);
        StringAssert.Contains("֏6,000", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForASubcategory()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        await expenseRepository.SaveAll(
            new List<IExpense>()
            {
                new Expense(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("For a category");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("Subcategory");
        var lastMessage = await botEngine.Proceed("July 2022");
        

        // Assert
        StringAssert.Contains("Expenses from July 2022", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Subcategory", lastMessage.Text);
        StringAssert.Contains("Food", lastMessage.Text);
        StringAssert.Contains("Snacks", lastMessage.Text);
        StringAssert.Contains("Products", lastMessage.Text);
        StringAssert.Contains("֏1,000", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }
    
    [Test]
    public async Task StatisticForASubcategoryCategoryWithCustomDateRange()
    {
        // Arrange
        var telegramBot = new TelegramBotMock();
        var dateTimeService = new DateTimeServiceStub(new DateOnly(2023, 7, 24));
        var categories = new Category[]
        {
            new()
            {
                Name = "Food",
                SubCategories = new[] { new SubCategory() { Name = "Snacks" }, new SubCategory() { Name = "Products" } }
            },
            new()
            {
                Name = "Cats",
            }
        };
        
        var expenseRepository = new ExpenseRepositoryStub();
        await expenseRepository.SaveAll(
            new List<IExpense>()
            {
                new Expense(){Date = new DateOnly(2023, 5, 22), Category = "Cats", Amount = new Money(){Amount = 10_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 6, 23), Category = "Cats", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 23), Category = "Food", SubCategory = "Snacks", Amount = new Money(){Amount = 1_000m, Currency = Currency.Amd}},
                new Expense(){Date = new DateOnly(2023, 7, 24), Category = "Food", SubCategory = "Products", Amount = new Money(){Amount = 5_000m, Currency = Currency.Amd}},
            }, default);
        var botEngine = CreateBotEngineWrapper(categories, expenseRepository, dateTimeService, telegramBot);
        
        // Act
        await botEngine.Proceed("/start");
        await botEngine.Proceed("Statistics");
        await botEngine.Proceed("For a category");
        await botEngine.Proceed("Food");
        await botEngine.Proceed("Subcategory");
        await botEngine.Proceed("Another");
        var lastMessage = await botEngine.Proceed("March 2022");
        

        // Assert
        StringAssert.Contains("Expenses from March 2022", lastMessage.Text);
        StringAssert.Contains("Category", lastMessage.Text);
        StringAssert.Contains("Food", lastMessage.Text);
        StringAssert.Contains("Snacks", lastMessage.Text);
        StringAssert.Contains("Products", lastMessage.Text);
        StringAssert.Contains("֏1,000", lastMessage.Text);
        StringAssert.Contains("Total", lastMessage.Text);
    }

    private StateFactory CreateStateFactory(Category[] categories, IExpenseRepository expenseRepository, IDateTimeService dateTimeService, ILogger<StateFactory> logger)
    {
        return new StateFactory(dateTimeService, new MoneyParser(new CurrencyParser()), categories, fnsService, expenseRepository, logger);
    }

    private BotEngine CreateBotEngine(Category[] categories, IFnsService fnsService, IExpenseRepository expenseRepository, IDateTimeService dateTimeService)
    {
        var logger = new LoggerStub<StateFactory>();
        var stateFactory = CreateStateFactory(categories, fnsService, expenseRepository, dateTimeService, logger);

        return new BotEngine(stateFactory, logger);
    }
    
    private BotEngineWrapper CreateBotEngineWrapper(Category[] categories, IExpenseRepository expenseRepository, IDateTimeService dateTimeService, TelegramBotMock telegramBot)
    {
        var botEngine = CreateBotEngine(categories, new FnsServiceStub(), expenseRepository, dateTimeService);
        return new BotEngineWrapper(botEngine, telegramBot);
    }
}