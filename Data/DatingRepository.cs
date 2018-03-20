using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Photo> GetMainPhoto(int userId)
        {
            return await _context.Photos
            .Where(u => u.UserId == userId)
            .FirstOrDefaultAsync(p => p.IsMainPhoto);
        }

        public Task<Photo> GetPhoto(int id)
        {
            var photo = _context.Photos
            .FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            return await _context.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
        var users =  _context.Users.Include(u => u.Photos);
        
        return await PagedList<User>.CreateAsync(users, userParams.PageSize, userParams.PageNumber); 
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }


    }
}