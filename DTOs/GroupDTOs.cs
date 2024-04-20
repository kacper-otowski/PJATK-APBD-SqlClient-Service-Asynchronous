namespace SqlClientExample_Tasks.DTOs;

public record GroupDTO(string Name);

public record GroupDetailsDTO(int Id, string Name) : GroupDTO(Name)
{
    public GroupDetailsDTO(Group group) : this(group.Id, group.Name){}
}