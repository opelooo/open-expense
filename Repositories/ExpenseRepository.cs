using AccountingApp.Data;
using AccountingApp.Interfaces;
using AccountingApp.Models;

namespace AccountingApp.Repositories
{
    public class ExpenseRepository : Repository<Expense>, IExpenseRepository
    {
        public ExpenseRepository(ApplicationDbContext dbContext)
            : base(dbContext) { }

        public async Task<IEnumerable<Expense>> GetExpensesByDateAsync(DateTime date)
        {
            return await FindAsync(e => e.ExpenseDate.Date == date.Date);
        }

        public async Task<IEnumerable<Expense>> GetExpensesByDateRangeAsync(
            DateTime startDate,
            DateTime endDate
        )
        {
            return await FindAsync(e =>
                e.ExpenseDate.Date >= startDate.Date && e.ExpenseDate.Date <= endDate.Date
            );
        }

        public async Task<decimal> GetTotalExpensesByDateAsync(DateTime date)
        {
            var expenses = await GetExpensesByDateAsync(date);
            return expenses.Sum(e => e.Amount);
        }

        public async Task<decimal> GetTotalExpensesByDateRangeAsync(
            DateTime startDate,
            DateTime endDate
        )
        {
            var expenses = await GetExpensesByDateRangeAsync(startDate, endDate);
            return expenses.Sum(e => e.Amount);
        }
    }
}
