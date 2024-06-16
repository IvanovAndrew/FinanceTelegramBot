using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using StateMachine;

namespace Infrastructure;

public abstract class TelegramCommand
{
    public abstract IExpenseInfoState Execute(IExpenseInfoState state, IStateFactory stateFactory);

    public static List<CommandAttribute> GetAllCommands()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        
        return assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(CommandAttribute), true).Length > 0)
            .Select(t => (CommandAttribute)t.GetCustomAttributes(typeof(CommandAttribute), true).First())
            .ToList();
    }
    
    public static bool TryGetCommand(string text, [NotNullWhen(true)] out TelegramCommand? telegramCommand)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        
        var type = assembly.GetTypes()
            .FirstOrDefault(t => t.IsSubclassOf(typeof(TelegramCommand)) && 
                                 t.GetCustomAttributes(typeof(CommandAttribute), true).Length > 0 && 
                                string.Equals(((CommandAttribute)t.GetCustomAttributes(typeof(CommandAttribute), true).First()).Command, text, StringComparison.InvariantCultureIgnoreCase));

        if (type == null)
        {
            telegramCommand = null;
            return false;
        }

        telegramCommand = (TelegramCommand) Activator.CreateInstance(type)!;
        return true;
    }
}