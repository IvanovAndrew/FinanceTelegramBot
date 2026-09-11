namespace Application.Contracts;

public interface IYerevanCityAPI
{
    Task<string?> DownloadRawJson(DateOnly date, string code, CancellationToken cancellationToken);
}