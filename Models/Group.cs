namespace SqlClientExample_Tasks;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    
    public List<Student> Students { get; set; } = []; // Required only for dapper mapping
}