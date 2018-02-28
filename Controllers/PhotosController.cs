using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.DTOS;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    public class PhotosController : Controller
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repo, IMapper mapper,
        IOptions<CloudinarySettings> cloudConfig)
        {
            _mapper = mapper;
            _cloudConfig = cloudConfig;
            _repo = repo;

            Account account = new Account(
                cloudConfig.Value.CloudName,
                cloudConfig.Value.ApiKey,
                cloudConfig.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }
        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _repo.GetPhoto(id);
            var photo = _mapper.Map<PhotoForReturn>(photoFromRepo);
            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, PhotoDTO photoDTO)
        {
            var user = await _repo.GetUser(userId);

            if (user == null)
                return BadRequest("Could not find");

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            if (currentUserId != user.Id)
                return Unauthorized();

            var file = photoDTO.File;

            var uplodaRes = new ImageUploadResult();

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
                    uplodaRes = _cloudinary.Upload(uploadParams);
                }
            }

            photoDTO.Url = uplodaRes.Uri.ToString();
            photoDTO.PublicId = uplodaRes.PublicId;

            var photo = _mapper.Map<Photo>(photoDTO);

            photo.User = user;

            if (!user.Photos.Any(po => po.IsMainPhoto))
                photo.IsMainPhoto = true;

            user.Photos.Add(photo);


            if (await _repo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturn>(photo);

                return Ok(photoToReturn);
            }
            return BadRequest("CDNT add the photo");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo == null)
                return NotFound();

            if (photoFromRepo.IsMainPhoto)
                return BadRequest("This is already the main photo");

            var currentMainPhoto = await _repo.GetMainPhoto(userId);

            if (currentMainPhoto != null)
                currentMainPhoto.IsMainPhoto = false;

            photoFromRepo.IsMainPhoto = true;

            if (await _repo.SaveAll())
                return NoContent();

            return BadRequest("cdnt save p to main");

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var photoFromRepo = await _repo.GetPhoto(id);

            if (photoFromRepo == null)
                return NotFound();

            if (photoFromRepo.IsMainPhoto)
                return BadRequest("Caannot del main photo");

            if (photoFromRepo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);

                var res = _cloudinary.Destroy(deleteParams);

                if (res.Result == "ok")
                    _repo.Delete(photoFromRepo);
            }

            if (photoFromRepo.PublicId == null)
                _repo.Delete(photoFromRepo);

            if (await _repo.SaveAll())
                return Ok();

            return BadRequest("Failed to delete a photo");
        }
    }
}