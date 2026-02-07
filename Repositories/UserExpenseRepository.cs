using AccountingApp.Data;
using AccountingApp.Interfaces;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Repositories
{
    public class UserExpenseRepository : Repository<UserExpense>, IUserExpenseRepository
    {
        private readonly ApplicationDbContext _context;

        public UserExpenseRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserExpense>> GetByUserIdAsync(string userId)
        {
            try
            {
                return await _context
                    .UserExpenses.Where(ue => ue.UserId == userId)
                    .Include(ue => ue.Expense)
                    .OrderByDescending(ue => ue.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<UserExpense>();
            }
        }

        public async Task<IEnumerable<UserExpense>> GetByExpenseIdAsync(string expenseId)
        {
            try
            {
                return await _context
                    .UserExpenses.Where(ue => ue.ExpenseId == expenseId)
                    .Include(ue => ue.User)
                    .OrderByDescending(ue => ue.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception)
            {
                return new List<UserExpense>();
            }
        }

        public async Task<(
            IEnumerable<UserExpense> Items,
            int TotalCount
        )> GetByUserIdPaginatedAsync(string userId, int page, int pageSize)
        {
            try
            {
                var result = await _context
                    .UserExpenses.Where(ue => ue.UserId == userId)
                    .OrderByDescending(ue => ue.CreatedAt)
                    .Select(ue => new
                    {
                        // Ambil data utama
                        Data = ue,
                        // Paksa ambil data Expense di sini agar tidak null
                        ExpenseData = ue.Expense,
                        // Hitung total dalam query yang sama
                        TotalCount = _context.UserExpenses.Count(x => x.UserId == userId),
                    })
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                if (!result.Any())
                {
                    return (new List<UserExpense>(), 0);
                }

                var totalCount = result[0].TotalCount;

                // Pasangkan kembali ExpenseData ke properti Expense di objek utama
                var items = result
                    .Select(r =>
                    {
                        r.Data.Expense = r.ExpenseData;
                        return r.Data;
                    })
                    .ToList();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                // Log error agar tahu jika ada masalah lain
                Console.WriteLine($"Error: {ex.Message}");
                return (new List<UserExpense>(), 0);
            }
        }
    }
}
