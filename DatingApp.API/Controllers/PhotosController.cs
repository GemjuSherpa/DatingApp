using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
  [Authorize]
  [Route("api/users/{userId}/photos")]
  public class PhotosController : ControllerBase
  {
    //Private Fields
    private readonly IDatingRepository _repo;
    private readonly IMapper _mapper;
    private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
    private Cloudinary _cloudinary;

    //Constructor
    public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudinaryConfig)
    {
      _cloudinaryConfig = cloudinaryConfig;
      _mapper = mapper;
      _repo = repo;

      //Create new Account
      Account acc = new Account(
          _cloudinaryConfig.Value.CloudName,
          _cloudinaryConfig.Value.ApiKey,
          _cloudinaryConfig.Value.ApiSecret
      );

      //Create a new instance of Cloudinary with new account instance.
      _cloudinary = new Cloudinary(acc);
    }

    /* 
        @route    GET api/users/{userId}/photos
        @desc     Get photos
        @access   Private
    */

    [HttpGet("{id}", Name = "GetPhoto")]
    public async Task<IActionResult> GetPhoto(int id)
    {
      var photoFromRepo = await _repo.GetPhoto(id);
      var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

      return Ok(photo);

    }


    //Photo Upload route
    /* 
        @route    POST api/users/{userId}/photos
        @desc     Uploads photos
        @access   Private
    */
    [HttpPost]
    public async Task<IActionResult> AddPhotoForUser(int userId, PhotoForCreationDto photoForCreationDto)
    {
      //Validate Authenticated userid
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }
      //get the user details
      var userFromRepo = await _repo.GetUser(userId);
      //File to store
      var file = photoForCreationDto.File;
      //store the response back from Cloudinary
      var uploadResult = new ImageUploadResult();

      /*
          OpenReadStream(): Opens the Read stream for uploaded file.
       */
      if (file.Length > 0)
      {
        using (var stream = file.OpenReadStream())
        {
          var uploadParams = new ImageUploadParams()
          {
            File = new FileDescription(file.Name, stream),
            Transformation = new Transformation().Width("500").Height("500").Crop("fill").Gravity("face")
          };
          //return response for uploaded file
          uploadResult = _cloudinary.Upload(uploadParams);
        }
      }

      photoForCreationDto.Url = uploadResult.Uri.ToString();
      photoForCreationDto.PublicId = uploadResult.PublicId;

      //Map the photo object to upload
      var photo = _mapper.Map<Photo>(photoForCreationDto);

      // If the user doesn't have their main profile set the photos to their main profile.
      if (!userFromRepo.Photos.Any(u => u.IsMain))
      {
        photo.IsMain = true;
      }

      //add the photos
      userFromRepo.Photos.Add(photo);

      //save the changes
      if (await _repo.SaveAll())
      {
        //Photo to return
        var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);

        return CreatedAtRoute("GetPhoto", new { id = photo.Id }, photoToReturn);
      }
      return BadRequest("Could not Upload Photos."); //Unsuccess
    }

    //set profile photo
    /* 
        @route    POST api/users/{userId}/photos/{id}/setMain
        @desc     Set profile photos
        @access   Private
    */
    [HttpPost("{id}/setMain")]
    public async Task<IActionResult> SetMainPhoto(int userId, int id)
    {
      //Validate Authenticated userid
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }
      //get the user details
      var user = await _repo.GetUser(userId);

      //validate photo id
      if (!user.Photos.Any(p => p.Id == id))
      {
        return Unauthorized();
      }

      //get the photo and check if it is the main photo
      var photoFromRepo = await _repo.GetPhoto(id);
      if (photoFromRepo.IsMain)
      {
        return BadRequest("This is already your main photo");
      }

      //get current main photo
      var currentMainPhoto = await _repo.GetMainPhotoForUser(userId);
      currentMainPhoto.IsMain = false;

      photoFromRepo.IsMain = true;

      //Save the changes
      if (await _repo.SaveAll())
      {
        return NoContent();
      }

      //if fails
      return BadRequest("could not set main photo");
    }


    //delete photos
    /* 
        @route    DELETE api/users/{userId}/photos/{id}
        @desc     Delete photos
        @access   Private
    */
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePhoto(int userId, int id)
    {
      //Validate Authenticated userid
      if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
      {
        return Unauthorized();
      }
      //get the user details
      var user = await _repo.GetUser(userId);

      //validate photo id
      if (!user.Photos.Any(p => p.Id == id))
      {
        return Unauthorized();
      }

      //get the photo and check if it is the main photo
      var photoFromRepo = await _repo.GetPhoto(id);
      if (photoFromRepo.IsMain)
      {
        return BadRequest("You can not delete your main photo");
      }

      //delete photo
      if (photoFromRepo.PublicId != null)
      {
        var deleteParams = new DeletionParams(photoFromRepo.PublicId);
        var result = _cloudinary.Destroy(deleteParams);

        if (result.Result == "ok")
        {
          _repo.Delete(photoFromRepo);
        }
      }
      else
      {
        _repo.Delete(photoFromRepo);
      }

      //save changes
      if (await _repo.SaveAll())
      {
        return Ok();
      }

      //otherwise unsuccess.
      return BadRequest("Could not delete the photo");
    }
  }
}