using System;
using System.Linq;
using System.Threading.Tasks;
using Api.Entities;
using Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Office4U.Articles.ImportExport.Api.Helpers;

namespace Api.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        public UserRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.Photos)
                .SingleOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<PagedList<AppUser>> GetUsersAsync(UserParams userParams)
        {
            var query = _context.Users
                .Include(u => u.Photos)
                .AsQueryable();

            query = FilterQuery(userParams, query);

            query = OrderQuery(userParams, query);

            return await PagedList<AppUser>.CreateAsync(
                query,
                userParams.PageNumber,
                userParams.PageSize);
        }

        private static IQueryable<AppUser> FilterQuery(UserParams userParams, IQueryable<AppUser> query)
        {
            query = query.Where(u => u.UserName != userParams.CurrentUsername);

            query = query.Where(u => u.Gender == userParams.Gender);

            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            return query;
        }

        private static IQueryable<AppUser> OrderQuery(UserParams userParams, IQueryable<AppUser> query)
        {
            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };
            return query;
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}