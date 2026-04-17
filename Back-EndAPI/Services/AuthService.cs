using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace Back_EndAPI.Services
{
    public class AuthService
    {
        

        public string? LoginSimple(string username, string password) {

            var hasher = new PasswordHasher<object>();
            var hashedPassword = hasher.HashPassword(null, password);

            //pretend to get hashed pw from fb 

            string dbPasswrod = "password123"; //whatever is in the db
            var example = _dbcontext.Authorize
                .Where(char => char.Username == username);
      

            var result = hasher.VerifyHashedPassword(null, hashedPassword, dbPasswrod);

            if (result == PassswordVerificationResult.Success) {

                Console.WriteLine("Success!");
            }

            return GenerateTokensimple(username)
        }
}
