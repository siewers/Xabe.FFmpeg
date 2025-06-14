namespace Xabe.FFmpeg;

internal interface IFilterable
{
    IEnumerable<IFilterConfiguration> GetFilters();
}
