using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Domain.Enums;
using UserService.Domain.Exceptions;
using UserService.Domain.Repositories;

namespace UserService.Application.UseCases
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateUserRequest> _createUserValidator;

        public UserService(
            IUserRepository userRepository,
            IMapper mapper,
            IValidator<CreateUserRequest> createUserValidator)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _createUserValidator = createUserValidator;
        }

        public async Task<UserResponse> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new UserNotFoundException(id);
            }
            return _mapper.Map<UserResponse>(user);
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            // Validate input using FluentValidation
            var validationResult = await _createUserValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new DuplicateEmailException(request.Email);
            }

            var now = DateTime.UtcNow;
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Status = UserStatus.Active,
                KeycloakId = Guid.NewGuid().ToString(), // Placeholder: In real app, this comes from Identity Service
                CreatedAt = now,
                UpdatedAt = now
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Publish event (UserCreated) - logic to be added

            return _mapper.Map<UserResponse>(user);
        }
    }
}
