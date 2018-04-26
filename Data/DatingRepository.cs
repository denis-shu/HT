using System;
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

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes
            .FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipientId);

        }

        public async Task<Photo> GetMainPhoto(int userId)
        {
            return await _context.Photos
            .Where(u => u.UserId == userId)
            .FirstOrDefaultAsync(p => p.IsMainPhoto);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos
            .FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var sdf = await _context.Users.Include(p => p.Photos)
            .Include(x => x.Liker)
            .Include(x => x.Likee)
            .FirstOrDefaultAsync(f => f.Id == id);
            return sdf;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.Include(u => u.Photos)
            .OrderByDescending(u => u.LastActive).AsQueryable();

            users = users.Where(x => x.Id != userParams.UserId);

            if (userParams.Likees != true && userParams.Likers != true)
                users = users.Where(x => x.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUsersLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Any(x => x.LikerId == u.Id));
            }
            if (userParams.Likees)
            {
                var userLikees = await GetUsersLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Any(x => x.LikeeId == u.Id));
            }

            if (userParams.MinAge != 18 && userParams.MaxAge != 99)
            {
                // users = users.Where(u => u.DateOfBirth.CalculateAge() >= userParams.MinAge
                //   && u.DateOfBirth.CalculateAge() <= userParams.MaxAge);
                var min = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var max = DateTime.Today.AddYears(-userParams.MinAge - 1);
                users = users.Where(u => u.DateOfBirth > min && u.DateOfBirth < max);
                
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;

                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageSize, userParams.PageNumber);
        }

        private async Task<IEnumerable<Like>> GetUsersLikes(int id, bool likers)
        {
            var user = await _context.Users
            .Include(x => x.Liker)
            .Include(x => x.Likee)
            .FirstOrDefaultAsync(x => x.Id == id);

            if (likers)
            {
                return user.Likee.Where(x => x.LikeeId == id);
            }
            else
            {
                return user.Liker.Where(x => x.LikerId == id);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<Message>> GetMessageForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
            .Include(x => x.Sender)
            .ThenInclude(p => p.Photos)
            .Include(x => x.Recipient)
            .ThenInclude(p => p.Photos)
            .AsQueryable();

            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(x => x.RecipientId == messageParams.UserId
                    && x.RecipientDeleted == false);
                    break;

                case "Outbox":
                    messages = messages.Where(x => x.SenderId == messageParams.UserId
                    && x.SenderDeleted == false);
                    break;
                default:
                    messages = messages.Where(x => x.RecipientId == messageParams.UserId
                    && x.RecipientDeleted == false
                    && x.IsRead == false);
                    break;
            }
            messages = messages.OrderByDescending(x => x.MessageSent);

            return await PagedList<Message>.CreateAsync(messages, messageParams.PageSize, messageParams.PageNumber);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
           .Include(x => x.Sender)
           .ThenInclude(p => p.Photos)
           .Include(x => x.Recipient)
           .ThenInclude(p => p.Photos)
           .Where(m => (m.RecipientId == userId
           && m.RecipientDeleted == false
           && m.SenderId == recipientId)
           || (m.RecipientId == recipientId
           && m.SenderId == userId
           && m.SenderDeleted == false))
           .OrderByDescending(m => m.MessageSent)
           .ToListAsync();

            return messages;
        }
    }
}