using AutoMapper;
using Entities;
using Repositories;
using System.Threading.Tasks;
using Zxcvbn;
using DTOs;
using Microsoft.Extensions.Logging;


namespace Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IPasswordService _passwordService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        public UserService(IUserRepository repository, IPasswordService passwordService, IMapper mapper, ILogger<UserService> logger)
        {
            _repository = repository;
            _passwordService = passwordService;
            _mapper = mapper;
            _logger = logger;
        }
        

        public async Task<UserDTO> GetUserById(int id)
        {
            _logger.LogInformation("GetUserById called with id={Id}", id);
            User user = await _repository.GetUserById(id);
            if (user == null)
                _logger.LogWarning("User not found for id={Id}", id);
            UserDTO dto = _mapper.Map<User, UserDTO>(user);
            return dto;
        }

        public async Task<UserDTO> AddUserToFile(UserRequestDTO user)
        {
            _logger.LogInformation("AddUser called for username={UserName}", user?.UserName);
            Password password = _passwordService.PasswordHardness(user.Password);
            if (password.Level < 3)
            {
                _logger.LogWarning("AddUser rejected for username={UserName}: password too weak (level={Level})", user.UserName, password.Level);
                return null;
            }
            User user2 = _mapper.Map<UserRequestDTO, User>(user);
            User userres = await _repository.AddUserToFile(user2);
            if (userres == null)
            {
                _logger.LogWarning("AddUser rejected: username already exists for username={UserName}", user.UserName);
                throw new InvalidOperationException($"A user with the email '{user.UserName}' already exists.");
            }
            _logger.LogInformation("User registered successfully with id={Id}, username={UserName}", userres.UserId, user.UserName);
            UserDTO dto = _mapper.Map<User, UserDTO>(userres);
            return dto;
        }

        public async Task<UserDTO?> Loginto(UserLoginDTO oldUser)
        {
            _logger.LogInformation("Login attempted for username={UserName}", oldUser?.UserName);
            User user = _mapper.Map<UserLoginDTO, User>(oldUser);
            User? userres = await _repository.Loginto(user);
            if (userres == null)
            {
                _logger.LogWarning("Login failed for username={UserName}: invalid credentials", oldUser?.UserName);
                throw new UnauthorizedAccessException("Invalid username or password.");
            }
            _logger.LogInformation("Login successful for username={UserName}", oldUser?.UserName);
            UserDTO dto = _mapper.Map<User, UserDTO>(userres);
            return dto;
        }

        public async Task<List<UserDTO>> GetAllUsers()
        {
            _logger.LogInformation("GetAllUsers called");
            List<User> users = await _repository.GetAllUsers();
            _logger.LogInformation("GetAllUsers returned {Count} users", users?.Count ?? 0);
            List<UserDTO> dtos = _mapper.Map<List<User>, List<UserDTO>>(users);
            return dtos;
        }

        public async Task<UserDTO> UpdateUserDetails(int id, UserRequestDTO userToUp)
        {
            _logger.LogInformation("UpdateUserDetails called for id={Id}", id);
            if (!string.IsNullOrEmpty(userToUp.Password))
            {
                Password password = _passwordService.PasswordHardness(userToUp.Password);
                if (password.Level < 3)
                {
                    _logger.LogWarning("UpdateUserDetails rejected for id={Id}: new password too weak (level={Level})", id, password.Level);
                    return null;
                }
            }
            User user = _mapper.Map<UserRequestDTO, User>(userToUp);
            User existing = await _repository.GetUserById(id);
            if (existing == null)
            {
                _logger.LogWarning("UpdateUserDetails: user not found for id={Id}", id);
                throw new KeyNotFoundException($"User with id {id} was not found.");
            }
            await _repository.UpdateUserDetails(id, user);
            User updatedUser = await _repository.GetUserById(id);
            _logger.LogInformation("User updated successfully for id={Id}", id);
            return _mapper.Map<User, UserDTO>(updatedUser);
        }

        
    }
}
