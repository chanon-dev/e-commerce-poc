using System;
using System.Threading.Tasks;
using UserService.Application.DTOs.Requests;
using UserService.Application.DTOs.Responses;

namespace UserService.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> GetUserByIdAsync(Guid id);
        Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    }
}
