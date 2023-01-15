using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using AutoMapper;

using API.Entities;
using API.Interfaces;
using API.DTOs;
using API.Extensions;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IMapper mapper;
        private readonly IPhotoService photService;
        private readonly IUnitOfWork unitOfWork;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.photService = photService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var gender = await this.unitOfWork.UserRepository.GetUserGender(User.GetUsername());

            userParams.CurrentUsername = User.GetUsername();

            if(string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender == "male" ? "female" : "male";
            }
            
            var users = await this.unitOfWork.UserRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username) =>
         await this.unitOfWork.UserRepository.GetMemberAsync(username);

         [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(this.User.GetUsername());

            if(user == null) return NotFound();

            this.mapper.Map(memberUpdateDto,user);

            if(await this.unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(this.User.GetUsername());

            if(user == null) return NotFound();

             var result = await this.photService.AddPhotoAsync(file);

             if(result.Error != null) return BadRequest(result.Error.Message);

             var photo = new Photo
             {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
             };

             if(user.Photos.Count == 0) photo.IsMain = true;

             user.Photos.Add(photo);

             if(await this.unitOfWork.Complete()) return CreatedAtAction(nameof(GetUser), new {username = user.UserName}, this.mapper.Map<PhotoDto>(photo));

             return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(this.User.GetUsername());

            if(user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if( photo == null) return NotFound();

            if(photo.IsMain) return BadRequest("This is already your main photo");

            var currentMainPhoto = user.Photos.FirstOrDefault(x => x.IsMain);

            if(currentMainPhoto != null) currentMainPhoto.IsMain = false;
            photo.IsMain = true;

            if(await this.unitOfWork.Complete()) return NoContent();

            return BadRequest("Problem setting the main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(this.User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x=>x.Id == photoId);

            if(photo == null) return NotFound();

            if(photo.IsMain) return BadRequest("You cannot delete your main photo");

            if(photo.PublicId != null) 
            {
                var result = await this.photService.DeletePhotoAsync(photo.PublicId);

                if(result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if(await this.unitOfWork.Complete()) return Ok();

            return BadRequest("Problem deleting photo");
        }
    }
}