namespace Andgasm.HoundDog.API.Interfaces
{
    public class FieldValidationErrorDTO
    {
        public string Key { get; set; }
        public string Description { get; set; }

        public FieldValidationErrorDTO()
        {
        }

        public FieldValidationErrorDTO(string key, string err)
        {
            Key = key;
            Description = err;
        }
    }
}
