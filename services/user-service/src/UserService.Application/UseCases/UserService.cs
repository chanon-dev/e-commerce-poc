using System;
using System.Threading.Tasks;
using AutoMapper;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Domain.Repositories;

namespace UserService.Application.UseCases
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserResponse> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;
            return _mapper.Map<UserResponse>(user);
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new Exception("User with this email already exists.");
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Status = UserStatus.Active,
                KeycloakId = Guid.NewGuid().ToString() // Placeholder: In real app, this comes from Identity Service
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Publish event (UserCreated) - logic to be added

            return _mapper.Map<UserResponse>(user);
        }
    }
}
