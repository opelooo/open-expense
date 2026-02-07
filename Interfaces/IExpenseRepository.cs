using AccountingApp.Models;

namespace AccountingApp.Interfaces
{
    public interface IExpenseRepository : IRepository<Expense>
    {
        Task<IEnumerable<Expense>> GetExpensesByDateAsync(DateTime date);
        Task<IEnumerable<Expense>> GetExpensesByDateRangeAsync(
            DateTime startDate,
            DateTime endDate
        );
        Task<decimal> GetTotalExpensesByDateAsync(DateTime date);
        Task<decimal> GetTotalExpensesByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}
