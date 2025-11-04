namespace API
{
    public class Order
    {

        private async Task<(bool ShouldBypass, List<string> ProjectIds)> GetQuoteUpdateStatusFilterAsync(
            Dictionary<string, string> filtersDictionary)
        {
            if (!filtersDictionary.TryGetValue("OrderStatusName", out var statusFilter) ||
                string.IsNullOrEmpty(statusFilter))
            {
                return (false, new List<string>());
            }

            try
            {
                var statusList = statusFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                // Get all QuoteUpdate statuses and their ProjectIds
                var quoteUpdateData = await portalContext.QuoteUpdateSelections
                    .Where(qu => !string.IsNullOrEmpty(qu.ProjectUpdate) &&
                                 statusList.Contains(qu.ProjectUpdate))
                    .Select(qu => new { qu.ProjectId, qu.ProjectUpdate })
                    .ToListAsync();

                if (quoteUpdateData.Any())
                {
                    var projectIds = quoteUpdateData.Select(qu => qu.ProjectId).Distinct().ToList();
                    return (true, projectIds);
                }

                return (false, new List<string>());
            }
            catch (Exception ex)
            {
                await logService.LogExceptionAsync(
                    nameof(LogModule.OrderService),
                    nameof(GetQuoteUpdateStatusFilterAsync),
                    ex
                );
                return (false, new List<string>());
            }
        }

        private async Task<List<string>> GetQuoteUpdateStatusesAsync(string accountNumber)
        {
            try
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
                {
                    { "AccountNumber", accountNumber }
                };
                var response = await GetQuoteUpdateStatusFilterAsync(keyValuePairs);
                if (!response.ShouldBypass)
                {
                    return new List<string> {};
                }

                // Get all QuoteUpdateSelections that have ProjectUpdate values
                var quoteUpdateStatuses = await portalContext.QuoteUpdateSelections
                    .Where(qu => !string.IsNullOrEmpty(qu.ProjectUpdate) && response.ProjectIds.Contains(qu.ProjectId))
                    .Select(qu => qu.ProjectUpdate!)
                    .Distinct()
                    .ToListAsync();

                return quoteUpdateStatuses;
            }
            catch (Exception ex)
            {
                await logService.LogExceptionAsync(
                    nameof(LogModule.OrderService),
                    nameof(GetQuoteUpdateStatusesAsync),
                    ex
                );
                return new List<string>();
            }
        }

        

    }
}
