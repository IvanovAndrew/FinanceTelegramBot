namespace Infrastructure;

public interface IMessage
{
    int Id { get; }
    long ChatId { get; }
    DateTime Date { get; }
    string Text { get; }
    bool Edited { get; }
}