class User
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Role { get; set; }
    
    
    public static readonly List<User> Users = new()
    {
        new()
        {
            Username = "admin",
            PasswordHash = "21232F297A57A5A743894A0E4A801FC3",
            Role = "admin"
        },
        new()
        {
            Username = "user1",
            PasswordHash = "24C9E15E52AFC47C225B757E7BEE1F9D",
            Role = "HR"
        }
    };
}


class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}