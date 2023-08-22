using System.Text;

namespace FakeAuth.Server.Services;

public class JwtConfig
{
    public string Audience { get; set; }

    /**
     * Duration in seconds
     */
    public int Duration { get; set; }

    public string Issuer { get; set; }
    public string Key { get; set; }

    public byte[] GetKeyAsBytes()
    {
        return Encoding.ASCII.GetBytes(Key);
    }
}
