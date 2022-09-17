namespace Customer.Microservice.Entities;

public class Customer : EntityBase
{
    public string Name { get; set; }
    public string Contact { get; set; }
    public string City { get; set; }
    public string Email { get; set; }
}
