using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using API.Entities;
using API.Interfaces;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IUnitOfWork unitOfWork;
        private readonly IPhotoService photoService;
        
        public AdminController(UserManager<AppUser> userManager, IUnitOfWork unitOfWork, IPhotoService photoService)
        {
            this.photoService = photoService;
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoId}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
            var photo = await this.unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if(photo == null) return NotFound("Could not find photo");

            photo.IsApproved = true;

            var user = await this.unitOfWork.UserRepository.GetUserByPhotoId(photoId);

            if(!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;

            await this.unitOfWork.Complete();

            return Ok();
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            var photo = await this.unitOfWork.PhotoRepository.GetPhotoById(photoId);

            if(photo.PublicId != null)
            {
                var result = await this.photoService.DeletePhotoAsync(photo.PublicId);

                if(result.Result == "ok")
                {
                    this.unitOfWork.PhotoRepository.RemovePhoto(photo);
                }
            }
            else
            {
                this.unitOfWork.PhotoRepository.RemovePhoto(photo);
            }

            await this.unitOfWork.Complete();

            return Ok();
        }


        [Authorize(Policy= "RequiredAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {

            var users = await this.userManager.Users
            .OrderBy(u => u.UserName)
            .Select(u=> new 
            {
                u.Id,
                Username = u.UserName,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            }).ToListAsync();

            return Ok(users);
        }

        [Authorize(Policy = "RequiredAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRole(string username, [FromQuery]string roles)
        {
            if(string.IsNullOrEmpty(roles)) return BadRequest("You must select at least one role");

            var selectedRoles = roles.Split(",").ToArray();

            var user = await this.userManager.FindByNameAsync(username);

            if(user == null) return NotFound();

            var userRoles = await this.userManager.GetRolesAsync(user);

            var result = await this.userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if(!result.Succeeded) return BadRequest("Failed to add to roles");

            result = await this.userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if(!result.Succeeded) return BadRequest("Failed to remove form roles");

            return Ok(await this.userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosFromModeration()
        {
            var photos = await this.unitOfWork.PhotoRepository.GetUnapprovedPhotos();

            return Ok(photos);
        }
    }
}