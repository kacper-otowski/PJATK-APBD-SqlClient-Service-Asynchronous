using FluentValidation;
using SqlClientExample_Tasks.DTOs;
using SqlClientExample_Tasks.Services;

namespace SqlClientExample_Tasks.Endpoints;

public static class StudentsEndpointsWithDapper
{
    public static void RegisterStudentsEndpoints2(this WebApplication app)
    {
        var students = app.MapGroup("students-dapper");
        students.MapGet("{id:int}", GetStudentDetails);
        students.MapPost("", AddStudentWithGroupAssignment);
    }

    private static async Task<IResult> GetStudentDetails(int id, IDbServiceDapper db)
    {
        var student = await db.GetStudentDetailsById(id);
        return student is null
            ? Results.NotFound($"Student with id:{id} does not exist")
            : Results.Ok(new StudentDetailsDTO(student));
    }

    private static async Task<IResult> AddStudentWithGroupAssignment(
        StudentWithGroupsIdsDTO request,
        IDbServiceDapper db,
        IValidator<StudentWithGroupsIdsDTO> validator
    )
    {
        var validate = await validator.ValidateAsync(request);
        if (!validate.IsValid)
        {
            return Results.ValidationProblem(validate.ToDictionary());
        }

        var groups = new List<Group>();
        foreach (var groupId in request.GroupsIds)
        {
            var group = await db.GetGroupById(groupId);
            if (group is null) return Results.NotFound($"Group with id:{groupId} does not exist");
            groups.Add(group);
        }

        var result = await db.AddStudentWithGroupsAssignments(
            new Student
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                Birthdate = request.Birthdate,
                Groups = groups
            }
        );

        return Results.Created($"students/{result.Id}", new StudentDetailsDTO(result));
    }
}