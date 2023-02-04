﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookReader.DAL.Interfaces;
using BookReader.DAL.Repositories;
using BookReader.Domain.Entity;
using BookReader.Domain.Response;
using BookReader.Service.Interfaces;
using BookReader.Domain.ViewModel;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using BookReader.Domain.Extensions;
using System.Security.Claims;

namespace BookReader.Service.Implementations
{
	public class AccountService : IAccountService
	{

		private readonly IBaseRepository<User> _userRepository;

		public AccountService(IBaseRepository<User> AccountRepository)
		{
			_userRepository= AccountRepository;
		}

		public async Task<IBaseResponse<ClaimsIdentity>> Register(RegisterViewModel userViewModel)
		{
			var baseResponse = new BaseResponse<ClaimsIdentity>();
			try
			{				
				var user_to_create = await _userRepository.GetAll().FirstOrDefaultAsync(x => x.Login == userViewModel.Login);
				if  (user_to_create != null)
				{
					baseResponse.Description = "User with this name already exist";
					baseResponse.statusCode = Domain.Enum.StatusCode.InternatlServiceError;
					return baseResponse;
				}

				user_to_create = new User()
				{
					Login = userViewModel.Login,
					Password = HashPasswordExtension.HashPassword(userViewModel.Password),
					UserRole = Domain.Enum.UserRole.user
				};
				bool request = await _userRepository.Create(user_to_create);

								
				if (!request)
				{
					baseResponse.statusCode = Domain.Enum.StatusCode.InternatlServiceError;
					baseResponse.Description = $"A try to create user with login {user_to_create.Login} failed";
					return baseResponse;

				}

				var result = Authenticate(user_to_create);

				baseResponse.Data = result;
				baseResponse.statusCode = Domain.Enum.StatusCode.ok;
				baseResponse.Description = $"A try to create user with login {user_to_create.Login} succesful";
				return baseResponse;

			}
			catch (Exception ex)
			{
				return new BaseResponse<ClaimsIdentity>()
				{
					Description = $"{ex.Message}"
				};
			}
		}

		private ClaimsIdentity Authenticate(User user)
		{
			var claimsIdentity = new List<Claim>()
			{
				new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
				new Claim(ClaimsIdentity.DefaultRoleClaimType, user.UserRole.ToString())
			};
			return new ClaimsIdentity(claimsIdentity, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
		}
       
    }
}