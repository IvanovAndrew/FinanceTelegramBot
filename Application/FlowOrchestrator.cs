using Application.AddMoneyTransfer;
using Application.AddMoneyTransferByRequisites;
using Application.Events;
using Application.Statistic.StatisticBalance;
using Application.Statistic.StatisticByCategory;
using Application.Statistic.StatisticByDay;
using Application.Statistic.StatisticByMonth;
using Application.Statistic.StatisticBySubcategory;
using Application.Statistic.StatisticBySubcategoryByMonth;
using Domain;
using MediatR;

namespace Application;

public class FlowOrchestrator(IUserSessionService sessions, IMediator mediator) : INotificationHandler<DraftUpdatedEvent>
{
    public async Task Handle(DraftUpdatedEvent e, CancellationToken ct)
    {
        var session = sessions.GetUserSession(e.SessionId);
        var flow = session.ActiveFlow;

        flow.ComputeStep();
        var step = flow.CurrentStep;

        if (step == FlowStep.Completed)
        {
            await ExecuteFlow(flow, session, ct);
            return;
        }

        var extraData = flow.GetExtraData();

        await RequestNextInput(step, session, extraData, ct);
    }
    
    private async Task ExecuteFlow(
        UserFlow flow,
        UserSession session,
        CancellationToken ct)
    {
        switch (flow)
        {
            case AddMoneyTransferFlow addMoneyTransferFlow:
                await mediator.Send(
                    new SaveMoneyTransferCommand(){SessionId = session.Id, LastSentMessageId = session.LastSentMessageId!.Value, MoneyTransfer = addMoneyTransferFlow.ToEntity()}, ct);
                break;

            case CheckRequisiteFlow checkRequisiteFlow:
                await mediator.Send(new DownloadExpenseFromFNSServiceCommand(){SessionId = session.Id, CheckRequisite = checkRequisiteFlow.ToEntity()}, ct);
                break;
            
            case StatisticsFlow statisticsFlow:

                switch (statisticsFlow.Draft.Mode)
                {
                    case StatisticsQueryMode.DailyExpenses:
                        await mediator.Send(new StatisticDayRequestCommand()
                        {
                            SessionId = session.Id, LastSentMessageId = session.LastSentMessageId.Value,
                            Query = statisticsFlow.ToEntity()
                        }, ct);
                        break;
                        break;
                    case StatisticsQueryMode.BalanceFromMonth:
                        await mediator.Send(new GetBalanceStatisticCommand()
                        {
                            SessionId = session.Id, LastSentMessageId = session.LastSentMessageId,
                            Query = statisticsFlow.ToEntity()
                        }, ct);
                        break;
                    case StatisticsQueryMode.MonthlyExpenses:
                        await mediator.Send(new GetStatisticMonthRequestCommand()
                        {
                            SessionId = session.Id, LastSentMessageId = session.LastSentMessageId.Value,
                            Query = statisticsFlow.ToEntity()
                        }, ct);
                        break;
                    case StatisticsQueryMode.CategoryByMonths:
                        await mediator.Send(new GetStatisticCategoryRequestCommand()
                        {
                            SessionId = session.Id, LastSentMessageId = session.LastSentMessageId.Value,
                            Query = statisticsFlow.ToEntity()
                        }, ct);
                        break;
                    case StatisticsQueryMode.SubcategoryByMonth:
                        await mediator.Send(new StatisticSubcategoryMonthRequestCommand()
                        {
                            SessionId = session.Id, LastSentMessageId = session.LastSentMessageId.Value,
                            Query = statisticsFlow.ToEntity()
                        }, ct);
                        break;
                    case StatisticsQueryMode.SubcategoryTotal:
                        await mediator.Send(new StatisticSubcategoryRequestCommand()
                        {
                            SessionId = session.Id, LastSentMessageId = session.LastSentMessageId.Value,
                            Query = statisticsFlow.ToEntity()
                        }, ct);
                        break;
                }
                break;
        }
    }
    
    private async Task RequestNextInput(
        FlowStep step,
        UserSession session,
        ExtraData extraData,
        CancellationToken ct)
    {
        switch (step)
        {
            case FlowStep.AskDay:
                await mediator.Publish(
                    new EnterTheDayEvent(){SessionId = session.Id, LastSentMessageId = session.LastSentMessageId},
                    ct);
                break;
            
            case FlowStep.AskCustomDay:
                await mediator.Publish(
                    new EnterTheCustomDayEvent(){SessionId = session.Id, LastSentMessageId = session.LastSentMessageId},
                    ct);
                break;
            
            case FlowStep.AskOutcomeCategory:
                
                await mediator.Publish(new EnterCategoryEvent(){SessionId = session.Id, LastSentMessageId = session.LastSentMessageId.Value, Categories = Categories.Outcome.All},
                    ct);
                break;
            
            case FlowStep.AskIncomeCategory:
                
                await mediator.Publish(new EnterCategoryEvent(){SessionId = session.Id, LastSentMessageId = session.LastSentMessageId.Value, Categories = Categories.Income.All},
                    ct);
                break;
            
            case FlowStep.AskSubCategory:
                await mediator.Publish(new EnterSubCategoryEvent(){SessionId = session.Id, LastSentMessageId = session.LastSentMessageId, Category = extraData.Category},
                    ct);
                break;
            
            case FlowStep.AskDescription:
                await mediator.Publish(new EnterDescriptionEvent(){SessionId = session.Id, },
                    ct);
                break;
            
            case FlowStep.AskAmount:
                await mediator.Publish(
                    new EnterThePriceEvent(){SessionId = session.Id},
                    ct);
                break;
            
            case FlowStep.Confirm:
                await mediator.Publish(
                    new ConfirmEvent(){SessionId = session.Id},
                    ct);
                break;
            
            case FlowStep.AskFiscalNumber:
                await mediator.Publish(new AskFiscalNumberEvent() { SessionId = session.Id }, ct);
                break;
            
            case FlowStep.AskFiscalDocumentNumber:
                await mediator.Publish(new AskFiscalDocumentNumberEvent() { SessionId = session.Id }, ct);
                break;
            
            case FlowStep.AskFiscalDocumentSign:
                await mediator.Publish(new AskFiscalDocumentSignEvent() { SessionId = session.Id }, ct);
                break;

            
            case FlowStep.AskMonth:
                await mediator.Publish(
                    new EnterTheMonthEvent(){SessionId = session.Id, LastSentMessageId = session.LastSentMessageId},
                    ct);
                break;
            
            case FlowStep.AskCustomMonth:
                await mediator.Publish(
                    new EnterTheCustomMonthEvent(){SessionId = session.Id, LastSentMessageId = session.LastSentMessageId},
                    ct);
                break;
            
            case FlowStep.AskCurrency:
                await mediator.Publish(
                    new EnterTheCurrencyEvent(){SessionId = session.Id, LastSentMessageId = session.LastSentMessageId},
                    ct);
                break;
            
            case FlowStep.AskDateTime:
            case FlowStep.Completed:
            default:
                throw new InvalidOperationException(
                    $"Unsupported flow step: {step}");
        }
    }

}

internal record ExtraData
{
    public Category? Category { get; init; }
}