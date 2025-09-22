namespace RESQ_API.Domain.Entities
{
    public class RegisterUserDto
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string PersonalPhoneNumber { get; set; }
        public string FamilyPhoneNumber { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
    }
}
