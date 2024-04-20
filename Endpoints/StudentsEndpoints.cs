using System.Data.SqlClient;
using FluentValidation;
using SqlClientExample_Tasks.DTOs;
using SqlClientExample_Tasks.Services;

namespace SqlClientExample_Tasks.Endpoints;

public static class StudentsEndpoints
{
    public static void RegisterStudentsEndpoints(this WebApplication app)
    {
        var students = app.MapGroup("students");
        students.MapGet("{id:int}", GetStudentDetails);
        students.MapPost("", AddStudentWithGroupAssignment);
    }
    
    private static async Task<IResult> GetStudentDetails(int id, IConfiguration configuration, IDbService db)
    {
        var student = await db.GetStudentDetailsById(id);

        return student is null
            ? Results.NotFound($"Student with id:{id} does not exist")
            : Results.Ok(new StudentDetailsDTO(student));
    }

    private static async Task<IResult> AddStudentWithGroupAssignment(
        StudentWithGroupsIdsDTO request,
        IConfiguration configuration,
        IValidator<StudentWithGroupsIdsDTO> validator,
        IDbService db
    )
    {
        // Validate a request
        var validate = await validator.ValidateAsync(request);
        if (!validate.IsValid)
        {
            return Results.ValidationProblem(validate.ToDictionary());
        }

        // Check if all groups exist
        var groups = new List<Group>();
        foreach (var groupId in request.GroupsIds)
        {
            var group = await db.GetGroupById(groupId);
            if (group is null)
            {
                return Results.NotFound($"Group with id:{groupId} does not exist");
            }

            groups.Add(group);
        }

        // Create a new student
        var student = await db.AddStudentWithGroupsAssignments(new Student
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            Birthdate = request.Birthdate,
            Groups = groups
        });
        
        return Results.Created(
            $"students/{student.Id}",
            new StudentDetailsDTO(student)
        );
    }
}