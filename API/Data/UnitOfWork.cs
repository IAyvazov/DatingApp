using API.Interfaces;
using AutoMapper;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IMapper mapper;
        private readonly DataContext dataContext;

        public UnitOfWork(DataContext dataContext, IMapper mapper)
        {
            this.dataContext = dataContext;
            this.mapper = mapper;
            
        }

        public IUserRepository UserRepository => new UserRepository(this.dataContext, this.mapper);

        public IMessageRepository MessageRepository => new MessageRepository(this.dataContext, this.mapper);

        public ILikesRepository LikesRepository => new LikesRepository(this.dataContext);

        public IPhotoRepository PhotoRepository =>  new PhotoRepository(this.dataContext);

        public async Task<bool> Complete()
        {
            return await this.dataContext.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return this.dataContext.ChangeTracker.HasChanges();
        }
    }
}