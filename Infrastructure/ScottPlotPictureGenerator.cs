using Application;
using Domain;
using ScottPlot;

namespace Infrastructure;

public class ScottPlotPictureGenerator : IPictureGenerator
{
    public byte[] GeneratePlot(IReadOnlyList<MonthlyBalance> data, Currency currency, PictureOptions options)
    {
        double step = CalculateStepSize(data);
        
        // ScottPlot работает с DateTime
        double[] dates = data.Select(d => d.Month.ToDateTime().ToOADate()).ToArray();
        double[] incomes = data.Select(d => (double) d.Balance.Income.Amount).ToArray();
        double[] outcomes = data.Select(d => (double) d.Balance.Outcome.Amount).ToArray();

        var plot = new Plot();

        var incomesScatter = plot.Add.Scatter(dates, incomes, Colors.Green);
        incomesScatter.LegendText = "Income";
        incomesScatter.LineWidth = 5;
        incomesScatter.MarkerSize = 10f;

        var outcomesScatter = plot.Add.Scatter(dates, outcomes, Colors.Red);
        outcomesScatter.LegendText = "Outcome";
        outcomesScatter.LineWidth = 3;
        outcomesScatter.MarkerSize = 10f;
        
        // month interval
        plot.Axes.Bottom.TickGenerator = CreateMonthTickGenerator(data.Select(c => c.Month).ToList());
        plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperCenter; // Текст центрируется под тиком
        
        // 4. Настройка ОСИ Y
        // Определяем максимальное значение для сетки
        double maxVal = Math.Max(incomes.Max(), outcomes.Max());
        int tickCount = (int)(maxVal / step) + 2;

        double[] tickPositions = Enumerable.Range(0, tickCount).Select(i => i * step).ToArray();
        string[] tickLabels = tickPositions.Select(v => $"{v:N0}{currency.Name}").ToArray();
        
        plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual(tickPositions, tickLabels);
        plot.Axes.Left.Label.Text = $"Amount ({currency.Symbol})";

        plot.Axes.Margins(horizontal: 0, vertical:0);
        plot.Axes.SetLimitsX(
            data.Select(d => new DateTime(d.Month.Year, d.Month.Month, 1)).Min().ToOADate(), 
             data.Select(d => new DateTime(d.Month.Year, d.Month.Month, 1)).Max().AddDays(1).ToOADate());
        
        plot.Title(options.Title);
        plot.XLabel(options.xLable);
        plot.YLabel(options.yLable);
        
        plot.ShowLegend(Alignment.UpperRight);

        return plot.GetImageBytes(1200, 800, ImageFormat.Png);
    }

    public byte[] GeneratePlot(IReadOnlyList<(YearMonth date, decimal value)> data, Currency currency, PictureOptions options)
    {
        double step = CalculateStepSize(data.Select(c => c.value));
        
        // ScottPlot работает с DateTime
        //double[] dates = data.Select(d => d.date.ToDateTime().ToOADate()).ToArray();
        var dates = data.Select(c => c.date.ToDateTime()).ToArray();
        double[] outcomes = data.Select(d => (double) d.value).ToArray();

        var plot = new Plot();
        
        var outcomesScatter = plot.Add.Scatter(dates, outcomes, Colors.Blue);
        outcomesScatter.LegendText = "Outcome";
        outcomesScatter.LineWidth = 3;
        outcomesScatter.MarkerSize = 10f;
        
        // month interval
        plot.Axes.Bottom.TickGenerator = CreateMonthTickGenerator(data.Select(c => c.date).ToList());
        plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperCenter;
        
        // 4. Настройка ОСИ Y
        // Определяем максимальное значение для сетки
        double maxVal = outcomes.Max();
        int tickCount = (int)(maxVal / step) + 2;

        double[] tickPositions = Enumerable.Range(0, tickCount).Select(i => i * step).ToArray();
        string[] tickLabels = tickPositions.Select(v => $"{v:N0}{currency.Name}").ToArray();
        
        plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual(tickPositions, tickLabels);
        plot.Axes.Left.Label.Text = $"Amount ({currency.Symbol})";
        
        plot.Title(options.Title);
        plot.XLabel(options.xLable);
        plot.YLabel(options.yLable);
        
        plot.ShowLegend(Alignment.UpperRight);

        return plot.GetImageBytes(1200, 800, ImageFormat.Png);
    }

    private static ScottPlot.TickGenerators.NumericManual CreateMonthTickGenerator(ICollection<YearMonth> data)
    {
        int monthInterval = data.Count > 15 ? 3 : 1;
        var manualTicks = data
            .Where((_, index) => index % monthInterval == 0)
            .Select(d => {
                double pos = d.ToDateTime().ToOADate();
                string label = $"{d.ToDateTime():MMMM}{Environment.NewLine}{d.ToDateTime():yyyy}";
                return new Tick(pos, label);
            }).ToArray();

        return new ScottPlot.TickGenerators.NumericManual(manualTicks);
    }

    private int CalculateStepSize(IReadOnlyList<MonthlyBalance> data)
    {
        var numbers = data.Select(d => Math.Max(d.Balance.Income.Amount, d.Balance.Outcome.Amount));
        return CalculateStepSize(numbers);
    }

    private int CalculateStepSize(IEnumerable<decimal> data)
    {
        var maxValue = (int) data.Max();

        if (maxValue >= 1_000_000)
            return 100_000;

        if (maxValue >= 100_000)
            return (maxValue / 100_000) * 10_000;

        if (maxValue >= 10_000)
            return (maxValue / 10_000) * 1_000;

        if (maxValue >= 1_000)
            return (maxValue / 1_000) * 100;

        return 1;
    }
}