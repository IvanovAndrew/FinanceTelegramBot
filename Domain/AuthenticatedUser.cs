namespace Domain;

public record AuthenticatedUser(long TelegramChatId, bool IsAdmin);
