using backend.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace backend.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User Create(User user);
    }

    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly AppSettings _appSettings;
        private readonly PasswordSalt _passwordSalt;

        public UserService(IConfiguration config)
        {
            _appSettings = new AppSettings();
            _appSettings.Secret = config.GetSection("AppSettings")["Secret"]; ;
            _passwordSalt = new PasswordSalt();
            _passwordSalt.Salt = config.GetSection("PasswordHash")["Salt"];

            var client = new MongoClient(config.GetConnectionString("WrautomatenDb"));
            var database = client.GetDatabase("wrautomaten");
            _users = database.GetCollection<User>("Users");
        }

        public User Get(string id)
        {
            User user = _users.Find<User>(x => x.Id == id).FirstOrDefault();
            user.Password = null;
            user.Token = null;
            return user;
        }

        public User GetByUserName(string id)
        {
            User user = _users.Find<User>(x => x.Id == id).FirstOrDefault();
            user.Password = null;
            user.Token = null;
            return user;
        }

        public User Authenticate(string username, string password)
        {
            string passwordHash = CreateHash(password, _passwordSalt.Salt);
            User user = _users.Find<User>(x => x.Username == username && x.Password == passwordHash).FirstOrDefault();

            // return null if user not found
            if (user == null)
                return null;

            user = CreateToken(user);

            user.Expires = DateTime.Now.AddDays(30);

            if (user == null)
            {
                user = new User
                {
                    Id = "-2"
                };        
                return user;
            }

            _users.ReplaceOne(x => x.Id == user.Id, user);

            // remove password before returning
            user.Password = null;

            return user;
        }

        public User Create(User user)
        {
            if (_users.Find(User => true).ToList().Where(x => x.Username == user.Username).FirstOrDefault() != null)
                return null;

            user.Password = CreateHash(user.Password, _passwordSalt.Salt);
            user = CreateToken(user);
            if (user == null)
            {
                user = new User
                {
                    Id = "-2"
                };
                return user;
            }
            _users.InsertOne(user);
            user.Password = null;
            return user;
        }

        public User Update(string id, User user)
        {
            user.Password = CreateHash(user.Password, _passwordSalt.Salt);
            user = CreateToken(user);
            _users.ReplaceOne(x => x.Id == id, user);
            user.Password = null;
            return user;
        }

        public User Patch(User user, bool newPassword)
        {
            if(newPassword)
            {
                user.Password = CreateHash(user.Password, _passwordSalt.Salt);
                user = CreateToken(user);
            }
            _users.ReplaceOne(x => x.Id == user.Id, user);
            user.Password = null;
            return user;
        }

        public User CreateToken(User user)
        {
            try
            {
                // authentication successful so generate jwt token
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Id),
                    new Claim(ClaimTypes.Role, user.Role)
                };
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);

                user.Token = tokenHandler.WriteToken(token);

                return user;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static string CreateHash(string value, string salt)
        {
            var valueBytes = KeyDerivation.Pbkdf2(
                                password: value,
                                salt: Encoding.UTF8.GetBytes(salt),
                                prf: KeyDerivationPrf.HMACSHA512,
                                iterationCount: 10000,
                                numBytesRequested: 256 / 8);

            return Convert.ToBase64String(valueBytes);
        }

        public IEnumerable<User> GetAll()
        {
            // return users without passwords
            List<User> users = _users.Find(User => true).ToList();
            return users.Select(x =>
            {
                x.Password = null;
                x.Token = null;
                return x;
            });
        }
    }
}
