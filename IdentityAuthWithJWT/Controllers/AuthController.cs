﻿using AutoMapper;
using IdentityAuthWithJWT.Data;
using IdentityAuthWithJWT.DTOs;
using IdentityAuthWithJWT.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace IdentityAuthWithJWT.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		//private readonly UserManager<ApiUser> _userManager;
		private readonly IMapper _mapper;
		private readonly IAuthService _authService;
		private IResponseFactory _successFactory;
		private IResponseFactory _failureFactory;

		public AuthController(
			IAuthService authService,
			IMapper mapper)
		{
			_authService = authService;
			_mapper = mapper;
		}

		[HttpPost("Register")]
		public async Task<IActionResult> Register([FromBody] UserDto userDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var result = await _authService.RegisterAsync(userDto);
				if (!result.IsAuthed)
				{
					return BadRequest(result.Message);
				}



				return Ok(result);
			}
			catch (Exception)
			{
				return Problem("Something went wrong , please try again later !!", statusCode: 500);
			}
		}


		[HttpPost("RegisterAdmin")]
		public async Task<IActionResult> RegisterAdmin([FromBody] UserDto userDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var result = await _authService.RegisterAsync(userDto, true);
				if (!result.IsAuthed)
				{
					return BadRequest(result.Message);
				}



				return Ok(result);
			}
			catch (Exception)
			{
				return Problem("Something went wrong , please try again later !!", statusCode: 500);
			}
		}

		[HttpPost("Login")]
		public async Task<IActionResult> Login([FromBody] UserLoginDto userDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{
				var result = await _authService.Login(userDto);

				if (!result.IsAuthed)
				{
					return BadRequest(result.Message);
				}

				return Ok(result);
			}
			catch (Exception)
			{
				return Problem("Something went wrong with Login !!");
			}
		}


		[HttpPost("AddUserToRole")]
		public async Task<IActionResult> AddToRole([FromBody] AddUserToRoleRequestDto model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await _authService.AddToRoleAsync(model);

			if (result.IsNullOrEmpty())
				return Ok(model);

			return BadRequest(result);
		}


		[HttpPost("Update")]
		public async Task<IActionResult> Update([FromBody] UpdateUserDto newUser)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			try
			{

				var updatedUser = await _authService.UpdateUserNameAsync(newUser);
				return Ok(updatedUser);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		//[Authorize]     // for forcing the user/client to send token for it
		[Authorize(Roles = "Admin")]   // for determine specific rule to use this end point
		[HttpGet]
		public  IActionResult GetAllUsersAsync()
		{

			var users = _authService.GetAllUsers();
			return Ok(_mapper.Map<List<UserDto>>(users));
		}


		[Authorize(Policy = "RequireAdminAndManagerRoles")]
		[HttpGet("GetAllUsersWithPolicy")]
		public IActionResult GetAllUsersWithPolicyAsync()
		{
			return Ok(_mapper.Map<List<UserDto>>(_authService.GetAllUsers().ToList()));
		}
	}
}
