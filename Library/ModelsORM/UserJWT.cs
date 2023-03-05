namespace Library.ModelsORM
{
    public class UserJWT
    {
            public string Username { get; set; } = string.Empty;
            public byte[] PasswordHash { get; set; }
            public byte[] PasswordSalt { get; set; }
    }
}
