namespace SqlClientExample_Tasks;

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
    public DateTime Birthdate { get; set; }

    
    public List<Group> Groups { get; set; } = []; // Required only for dapper mapping
}