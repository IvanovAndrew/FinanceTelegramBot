namespace TelegramBot.Contracts;

public class ShopExpensesDto
{
    public string Shop { get; set; }
    public DateOnly Date { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; }
    public List<MoneyTransferDTO> Expenses { get; set; }
}