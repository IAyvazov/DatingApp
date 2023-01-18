using Microsoft.EntityFrameworkCore;

using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using API.Helpers;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<MemberDto> GetMemberAsync(string username, bool isCurrentUser)
        {
           var query = this.context.Users
           .Where(x => x.UserName == username)
           .ProjectTo<MemberDto>(this.mapper.ConfigurationProvider)
           .AsQueryable();

           if(isCurrentUser) query = query.IgnoreQueryFilters();

           return await query.FirstOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = this.context.Users.AsQueryable();

            query = query.Where(u => u.UserName != userParams.CurrentUsername);
            query = query.Where(u => u.Gender == userParams.Gender);

            var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
            var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge));

            query = query.Where(u=>u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u=> u.Created),
                _ => query.OrderByDescending(u=> u.LastActive)
            };

            return await PagedList<MemberDto>.CreateAsync(
                query
                .AsNoTracking()
                .ProjectTo<MemberDto>(this.mapper.ConfigurationProvider),
                userParams.PageNumber,
                userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id) =>
            await this.context.Users
            .FindAsync(id);

        public async Task<AppUser> GetUserByPhotoId(int photoId)
        => await this.context.Users
            .Include(p => p.Photos)
            .IgnoreQueryFilters()
            .Where(p => p.Photos.Any(p => p.Id == photoId))
            .FirstOrDefaultAsync();

        public async Task<AppUser> GetUserByUsernameAsync(string username) =>
            await this.context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == username);

        public async Task<string> GetUserGender(string username) =>
         await this.context.Users
            .Where(x=> x.UserName == username)
            .Select(x=> x.Gender)
            .FirstOrDefaultAsync();
        

        public async Task<IEnumerable<AppUser>> GetUsersAsync() =>
            await this.context.Users
            .Include(p => p.Photos)
            .ToListAsync();

        public void Update(AppUser user)
        {
            this.context.Entry(user).State = EntityState.Modified;
        }
    }
}