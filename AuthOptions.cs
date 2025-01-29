using Microsoft.IdentityModel.Tokens;
using System.Text;

public class AuthOptions
{
    public const string ISSUER = "UserUser";
    public const string AUDIENCE = "MachineAPI";
    private const string KEY = "myasujrsecret_856307secretsecretkey!123";

    public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
}