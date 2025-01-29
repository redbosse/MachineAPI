namespace MachineAPI.Model
{
    public interface IPersonDataBase
    {
        public bool ValidateUser(Person user);

        public bool ValidatePassword(Person user);

        public bool ValidateAdminUser(Person user);

        public bool AddNewUser(Person user);

        public string GetUserlist();
    }
}