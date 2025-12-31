using AutoMapper;
using Entities;
using Repositories;
using System.Threading.Tasks;
using Zxcvbn;
using DTOs;


namespace Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IPasswordService _passwordService;
        private readonly IMapper _mapper;
        public UserService(IUserRepository repository, IPasswordService passwordService, IMapper mapper)
        {
            _repository = repository;
            _passwordService = passwordService;
            _mapper = mapper;
        }
        

        public async Task<UserDTO> GetUserById(int id) {
            User user= await _repository.GetUserById(id); 
            UserDTO dto=_mapper.Map<User, UserDTO>(user);
            return dto;
        }

        public async Task<UserDTO> AddUserToFile(UserDTO user)
        {
            Password password= _passwordService.PasswordHardness(user.Password);
            if (password.Level < 3) return null;
            User user2=_mapper.Map<UserDTO, User>(user);
            User userres=await _repository.AddUserToFile(user2);
            UserDTO dto=_mapper.Map<User, UserDTO>(userres);
            return dto;
        }

        public async Task<UserDTO?> Loginto(UserLoginDTO oldUser)
        {
            User user = _mapper.Map<UserLoginDTO, User>(oldUser);
            User? userres= await _repository.Loginto(user);
            UserDTO dto = _mapper.Map<User, UserDTO>(userres);
            return dto;
        }

        public async Task<List<UserDTO>> GetAllUsers() 
        {
            List<User> users=await _repository.GetAllUsers();
            List<UserDTO> dtos=_mapper.Map<List<User>, List<UserDTO>>(users);
            return dtos;

        }

        public async Task<UserDTO> UpdateUserDetails(int id, UserDTO userToUp) {

            Password password = _passwordService.PasswordHardness(userToUp.Password);
            if (password.Level <3) return null;
            User user = _mapper.Map<UserDTO, User>(userToUp);
            await _repository.UpdateUserDetails(id, user);
            return userToUp;
        }

        
    }
}
