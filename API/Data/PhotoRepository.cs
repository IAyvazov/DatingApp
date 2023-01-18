using Microsoft.EntityFrameworkCore;

using API.DTOs;
using API.Entities;
using API.Interfaces;

namespace API.Data
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DataContext context;

        public PhotoRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<Photo> GetPhotoById(int id)
         => await this.context.Photos
         .IgnoreQueryFilters()
         .SingleOrDefaultAsync(x => x.Id == id);
        

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
        => await this.context.Photos
        .IgnoreQueryFilters()
        .Where(p => p.IsApproved == false)
        .Select( u => new PhotoForApprovalDto 
        {
            Id = u.Id,
            Username = u.AppUser.UserName,
            Url = u.Url,
            IsApproved = u.IsApproved
        })
        .ToListAsync();

        public void RemovePhoto(Photo photo)
        {
           this.context.Photos.Remove(photo);
        }
    }
}