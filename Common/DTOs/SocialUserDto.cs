namespace Common.DTOs
{
    public class SocialUserDto
    {
        required public int UserID { get; set; }
        required public string Cnp { get; set; }
        required public string FirstName { get; set; }
        required public string LastName { get; set; }
        required public string Email { get; set; }
        required public string Username { get; set; }
        required public int ReportedCount { get; set; }

    }

}
