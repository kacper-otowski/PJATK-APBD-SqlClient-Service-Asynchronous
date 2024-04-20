namespace SqlClientExample_Tasks.DTOs;

public record StudentDTO(string FirstName, string LastName, string Phone, DateTime Birthdate)
{
    public StudentDTO(Student student) : this(student.FirstName, student.LastName, student.Phone, student.Birthdate)
    { }
}

public record StudentWithGroupsIdsDTO(
    string FirstName,
    string LastName,
    string Phone,
    DateTime Birthdate,
    List<int> GroupsIds
    ) : StudentDTO(FirstName, LastName, Phone, Birthdate);

public record StudentDetailsDTO(
    int Id,
    string FirstName,
    string LastName,
    string Phone,
    DateTime Birthdate,
    List<GroupDetailsDTO> Groups) : StudentDTO(FirstName, LastName, Phone, Birthdate)
{
    public StudentDetailsDTO(Student student) : this(
        student.Id,
        student.FirstName,
        student.LastName,
        student.Phone,
        student.Birthdate,
        student.Groups.Select(e => new GroupDetailsDTO(e.Id, e.Name)).ToList()
        ){}
}