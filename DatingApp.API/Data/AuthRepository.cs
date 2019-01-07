// Interface Repository for Authentication
using System;
using System.Threading.Tasks;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
  public class AuthRepository : IAuthRepository
  {
    private readonly DataContext _context;

    //constructor
    public AuthRepository(DataContext context)
    {
      _context = context;

    }

    //User login
    //if VerifyPassword is true than login success else incorrect password.
    public async Task<User> Login(string username, string password)
    {
      var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);

      if (user == null)
        return null;

      if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
        return null;

      return user;
    }

    //Verify entered password with the stored password
    //Computes Hashed password using the stored Salted password and compares cumputed hashed password with the stored hashed password.
    //if both computed password and hashed password matches the returns true else false.
    private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
      using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
      {
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        for (int i = 0; i < computedHash.Length; i++)
        {
          if (computedHash[i] != passwordHash[i]) return false;
        }
      }
      return true;
    }

    //User Registrations
    //Creates a hashed and salted password before adding users.
    public async Task<User> Register(User user, string password)
    {

      byte[] passwordHash, passwordSalt;
      CreatePasswordHash(password, out passwordHash, out passwordSalt);

      user.PasswordHash = passwordHash;
      user.PasswordSalt = passwordSalt;

      await _context.Users.AddAsync(user);
      await _context.SaveChangesAsync();

      return user;
    }

    //Creating a Hashed and Salted password using System.Security.Cryptography
    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
      using (var hmac = new System.Security.Cryptography.HMACSHA512())
      {
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
      }
    }

    //Check if User is Exists or not
    public async Task<bool> UserExists(string username)
    {
      if (await _context.Users.AnyAsync(x => x.Username == username))
        return true;
      return false;
    }
  }
}