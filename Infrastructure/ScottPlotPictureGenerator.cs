using Application.Bot;
using Application.Core;
using Domain;
using ScottPlot;

namespace Infrastructure;

public class ScottPlotPictureGenerator : IPictureGenerator
{
    private static readonly string CustomFontName = "DejaVu Sans";

    static ScottPlotPictureGenerator()
    {
        var fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "DejaVuSans.ttf");
        Fonts.AddFontFile(CustomFontName, fontPath);
        Fonts.Default = CustomFontName;
    }
    
    public byte[] GeneratePlot(IReadOnlyList<(ChartBucket bucket, decimal value)> data, Currency currency, PictureOptions options)
    {
        if (data == null || data.Count == 0)
            return Array.Empty<byte>();

        double step = CalculateStepSize(data.Select(x => x.value));

        double[] xs = data.Select(d => d.bucket.Date.ToDateTime(TimeOnly.MinValue).ToOADate()).ToArray();
        double[] ys = data.Select(d => (double)d.value).ToArray();

        var plot = new Plot();

        var scatter = plot.Add.Scatter(xs, ys, Colors.RoyalBlue);
        scatter.LegendText = options.Title ?? "Daily Expenses";
        scatter.LineWidth = 2;
        scatter.MarkerSize = 8f;

        var dayTicks = CreateBucketTickGenerator(data.Select(d => d.bucket).ToList());
        plot.Axes.Bottom.TickGenerator = dayTicks;
        plot.Axes.Bottom.TickLabelStyle.ForeColor = Colors.Black;
        plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
        plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
        plot.Axes.Bottom.FrameLineStyle.Color = Colors.Black;

        double maxVal = ys.Max();
        int tickCount = Math.Max(2, (int)(maxVal / step) + 2);
        double[] tickPositions = Enumerable.Range(0, tickCount).Select(i => i * step).ToArray();
        string[] tickLabels = tickPositions.Select(v => $"{v:N0} {currency.Name}").ToArray();

        plot.Axes.Left.TickGenerator = new ScottPlot.TickGenerators.NumericManual(tickPositions, tickLabels);
        plot.Axes.Left.TickLabelStyle.ForeColor = Colors.Black;
        plot.Axes.Left.FrameLineStyle.Color = Colors.Black;

        if (!string.IsNullOrEmpty(options.Title))
        {
            plot.Title(options.Title);
            plot.Axes.Title.Label.ForeColor = Colors.Black;
        }
        
        if (!string.IsNullOrEmpty(options.xLable))
        {
            plot.XLabel(options.xLable);
            plot.Axes.Bottom.Label.ForeColor = Colors.Black;
        }
        
        plot.Axes.Margins(bottom: 0.03, left:0.01);

        plot.YLabel(options.yLable);
        plot.Axes.Left.Label.ForeColor = Colors.Black;

        plot.ShowLegend(Alignment.UpperRight);

        plot.Layout.Fixed(new PixelPadding(left: 125, right: 25, bottom: 80, top: 25));

        return plot.GetImageBytes(1200, 800, ImageFormat.Png);
    }
    
    private static ScottPlot.TickGenerators.NumericManual CreateBucketTickGenerator(IReadOnlyList<ChartBucket> buckets)
    {
        int interval = buckets.Count > 14 ? (buckets.Count / 10) : 1;
        if (interval < 1) interval = 1;

        var ticks = buckets
            .Where((_, index) => index % interval == 0)
            .Select(b => {
                DateTime dt = b.Date.ToDateTime(TimeOnly.MinValue);
                double pos = dt.ToOADate();
                return new Tick(pos, b.Label);
            }).ToArray();

        return new ScottPlot.TickGenerators.NumericManual(ticks);
    }
    
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
        
        plot.Axes.Bottom.TickGenerator = CreateMonthTickGenerator(data.Select(c => c.Month).ToList());
        plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperCenter;
        
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