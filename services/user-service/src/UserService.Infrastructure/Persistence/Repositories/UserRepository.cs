using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Addresses)
                .Include(u => u.Preference)
                .Include(u => u.Statistics)
                .FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Addresses)
                .Include(u => u.Preference)
                .Include(u => u.Statistics)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email, cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
        }

        public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(user);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Remove(user);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                // Handle unique constraint violation (duplicate email)
                if (ex.InnerException?.Message.Contains("duplicate key") == true ||
                    ex.InnerException?.Message.Contains("unique constraint") == true ||
                    ex.InnerException?.Message.Contains("IX_Users_Email") == true)
                {
                    // Extract email from the failing entity
                    var entry = ex.Entries.FirstOrDefault();
                    if (entry?.Entity is User user)
                    {
                        throw new DuplicateEmailException(user.Email);
                    }
                    throw new DomainException("A user with this email already exists.");
                }
                throw;
            }
        }
    }
}
