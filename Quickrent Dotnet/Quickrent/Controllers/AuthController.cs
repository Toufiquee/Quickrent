using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Quickrent.DTO.UserDTO;
using Quickrent.Model;
using Quickrent.Service.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Quickrent.Controllers
{
    //[Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _service;
        private readonly IEmailService emailService;
        private readonly IDistributedCache _cache;

        public AuthController(ILogger<AuthController> logger, IAuthService service, IEmailService emailService, IDistributedCache cache)
        {
            _logger = logger;
            _service = service;
            this.emailService = emailService;
            _cache = cache;
        }

        [Route("auth/admin")]
        [HttpPost]
        public IActionResult AdminLogin([FromBody] ReqUserLoginDto dto)
        {
            string token = _service.AuthAdminDetails(dto);
            return Ok(token);
        }

        /*
        [HttpPost]
        public IActionResult RegisterAdmin(){

        }
        */

        private async Task<string> GenerateOTP(string userEmail)
        {
            Random random = new Random();
            string randomno = random.Next(0, 1000000).ToString("D6");
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync(userEmail, randomno, options);
            return randomno;
        }

        private void SendOtpMail(string useremail, string OtpText, string Name)
        {
            var mailrequest = new Mailrequest();
            mailrequest.Email = useremail;
            mailrequest.Subject = "Thanks for registering: OTP";
            mailrequest.EmailBody = GenerateEmailBody(Name, OtpText);
            this.emailService.SendEmail(mailrequest);
        }

        private string GenerateEmailBody(string name, string otptext)
        {
            string emailbody = "<div style='width: 100%; background-color:grey'>";
            emailbody += "<h1>Hi " + name + ", Thanks for registering</h1>";
            emailbody += "<h2>Please enter OTP text and complete the registeration</h2>";
            emailbody += "<h2>OTP Text is: " + otptext + "</h2>";
            emailbody += "</div>";
            return emailbody;
        }

        [Route("auth/signup")]
        [HttpPost]
        public async Task<IActionResult> RegisterUser([FromBody] ReqRegisterUserDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("A valid email is required to register.");

            string otp = await GenerateOTP(dto.Email);
            SendOtpMail(dto.Email, otp, dto.FirstName ?? string.Empty);

            string userData = JsonConvert.SerializeObject(dto);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };
            await _cache.SetStringAsync("data_" + dto.Email, userData, options);
            return Ok("OTP Generated");
        }

        [Route("auth/verify")]
        [HttpPost]
        public IActionResult ValidateEmail([FromBody] ReqValidateEmailDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Otp))
                return BadRequest("Email and OTP are required.");

            var cachedOtp = _cache.GetString(dto.Email);
            if (string.IsNullOrWhiteSpace(cachedOtp))
                return BadRequest("OTP expired or not found.");

            if (!int.TryParse(cachedOtp, out var genOtp))
                return BadRequest("Invalid cached OTP format.");

            if (!int.TryParse(dto.Otp, out var userOtp))
                return BadRequest("Invalid OTP format.");

            if (userOtp != genOtp)
                return BadRequest("Invalid OTP.");

            var cachedUserData = _cache.GetString("data_" + dto.Email);
            if (string.IsNullOrWhiteSpace(cachedUserData))
                return BadRequest("Registration data expired or not found.");

            var userData = JsonConvert.DeserializeObject<ReqRegisterUserDto>(cachedUserData);
            if (userData == null)
                return BadRequest("Failed to read registration data.");

            string response = _service.AddUser(userData);
            if (response.Contains("Failed"))
                return StatusCode(500, response);

            _cache.Remove(dto.Email);
            _cache.Remove("data_" + dto.Email);
            return Ok(response);
        }


        [Route("auth/login")]
        [HttpPost]
        public IActionResult AuthUser([FromBody] ReqUserLoginDto dto ){
            try
            {
                string token = _service.AuthUserDetails(dto);
                return Ok(token);
            }
            catch (InvalidCredentialException e)
            {
                return Unauthorized(e.Message);
            }
            catch(Exception e)
            {
                return StatusCode(500, new { error = e.Message });
            }
        }

    }
}