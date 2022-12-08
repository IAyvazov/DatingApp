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

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await this.context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(this.mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = this.context.Users
            .ProjectTo<MemberDto>(this.mapper.ConfigurationProvider)
            .AsNoTracking();

            return await PagedList<MemberDto>.CreateAsync(query, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id) =>
            await this.context.Users
            .FindAsync(id);

        public async Task<AppUser> GetUserByUsernameAsync(string username) =>
            await this.context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == username);

        public async Task<IEnumerable<AppUser>> GetUsersAsync() =>
            await this.context.Users
            .Include(p => p.Photos)
            .ToListAsync();

        public async Task<bool> SaveAllAsync() =>
            await this.context.SaveChangesAsync() > 0;

        public void Update(AppUser user)
        {
            this.context.Entry(user).State = EntityState.Modified;
        }
    }
}