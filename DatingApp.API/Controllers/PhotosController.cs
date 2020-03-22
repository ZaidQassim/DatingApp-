using AutoMapper;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using CloudinaryDotNet;
using System.Threading.Tasks;
using DatingApp.API.Dtos;
using DatingApp.API.Data;
using System.Security.Claims;
using CloudinaryDotNet.Actions;
using DatingApp.API.Models;
using System.Linq;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private readonly Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinaryConfig = cloudinaryConfig;
            _mapper = mapper;
            _repo = repo;
            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(acc);
        }


        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);
            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm] PhotoForCreationDto photoForCreationDto)
        {
            // to valadation of user is same edit of my data
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var userForRepo = await _repo.GetUser(userId);

            var file = photoForCreationDto.File;

            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation()
                        .Width(500).Height(500).Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDto);

            if (!userForRepo.Photos.Any(u => u.IsMain))
                photo.IsMain = true;

            userForRepo.Photos.Add(photo);

            if (await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
            }

            return BadRequest("Cloud not add the Photo");

        }


        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            // to valadation of user is same edit (loggedIn) of my data
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId);  // get the info user

            // to valdation of photo is exist for user
            if (!user.Photos.Any(p => p.Id == id))  // 
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id); // get the info photo

            // if photo of user is main 
            if (photoFromRepo.IsMain)
                return BadRequest("This is already the main photo");

            // to  get photo old is main for user and remove of main
            var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
            currentMainPhoto.IsMain = false;

            // to set  photo is main
            photoFromRepo.IsMain = true;

            if (await _repo.SaveAll())
                return NoContent();

            return BadRequest("Could not set photo to main");

        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            // to valadation of user is same edit (loggedIn) of my data
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId);  // get the info user

            // to valdation of photo is exist for user
            if (!user.Photos.Any(p => p.Id == id))  // 
                return Unauthorized();

            // get the info photo
            var photoFromRepo = await _repo.GetPhoto(id);

            // if photo of user is main 
            if (photoFromRepo.IsMain)
                return BadRequest("You Can not delete your main photo !!");

            // to check of photo is storage in  cloudinary as delete from database and cloudinary
            if (photoFromRepo.PublicId != null)
            {
                // to  delete the photo of cloudinary
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);
                var result = _cloudinary.Destroy(deleteParams);

                // if delete photo of  cloudinary
                if (result.Result == "ok")
                    _repo.Delete(photoFromRepo);  // delete photo of data base
            }

            // if photo just storage in database  delete from database 
            if (photoFromRepo.PublicId == null)
                _repo.Delete(photoFromRepo);  // delete photo of data base


            if (await _repo.SaveAll())  // if successful  save infor return ok
                return Ok();

            return BadRequest("Falied to delete the photo");

        }

    }
}