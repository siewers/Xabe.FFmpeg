namespace Xabe.FFmpeg;

using System.Collections.Generic;

internal interface IFilterable
{
    IEnumerable<IFilterConfiguration> GetFilters();
}
