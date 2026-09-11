using Application.Bot.Flows;
using Application.Core;
using Application.Core.Services;
using Domain;

namespace Application.Bot;

public interface IFlowStepRenderer
{
    Task Render(long sessionId, FlowStep step, ExtraData extraData, CancellationToken ct);
}

public class FlowStepRenderer(IConversation conversation, IDateTimeService dateTimeService) : IFlowStepRenderer
{
    public async Task Render(long sessionId, FlowStep step, ExtraData extraData, CancellationToken ct)
    {
        var screen = step switch
        {
            FlowStep.AskDay => Screens.SelectDay(dateTimeService.Today()),
            FlowStep.AskCustomDay => Screens.SelectCustomDay(dateTimeService.Today()),
            FlowStep.AskDateTime => Screens.EnterDateTime("Enter date and time"),
            FlowStep.AskMonth => Screens.SelectMonth(YearMonth.From(dateTimeService.Today())),
            FlowStep.AskCustomMonth => Screens.SelectCustomMonth(YearMonth.From(dateTimeService.Today())),

            FlowStep.AskOutcomeCategory => Screens.SelectCategory(
                extraData.ShowAllCategories
                    ? Categories.Outcome.Actual.OrderBy(c => c.ShortName ?? c.Name).ToList()
                    : Categories.Outcome.Popular,
                includeShowAll: !extraData.ShowAllCategories),

            FlowStep.AskCurrency => Screens.SelectCurrency(
                extraData.ShowAllCurrencies
                    ? Currency.GetAvailableCurrencies()
                    : Currency.GetAvailableCurrencies().Where(c => c.IsPopular).ToList(),
                includeShowAll: !extraData.ShowAllCurrencies),

            FlowStep.AskSubCategory => Screens.SelectSubCategory(extraData.Category?.Subcategories ?? []),
            FlowStep.AskDescription => Screens.EnterDescription(),
            FlowStep.AskAmount => Screens.EnterPrice(),
            FlowStep.Confirm => Screens.Confirm(extraData.ConfirmationText?? string.Empty),
            FlowStep.AskFiscalNumber => Screens.EnterFiscalNumber(),
            FlowStep.AskFiscalDocumentNumber => Screens.EnterFiscalDocumentNumber(),
            FlowStep.AskFiscalDocumentSign => Screens.EnterFiscalDocumentSign(),
            FlowStep.AskUrlLink => Screens.EnterText("Enter the url"),
            FlowStep.AskOrderId => Screens.EnterAskOrderId(),
            FlowStep.Completed => null,
            FlowStep.AskIncomeCategory => Screens.SelectCategory(Categories.Income.All.OrderBy(c => c.ShortName ?? c.Name).ToList(), false),
            _ => throw new InvalidOperationException($"Unsupported flow step: {step}")
        };

        if (screen != null)
            await conversation.Update(sessionId, screen, ct);
    }
}