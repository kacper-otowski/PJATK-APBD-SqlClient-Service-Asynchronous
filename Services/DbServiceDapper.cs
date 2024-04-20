using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace SqlClientExample_Tasks.Services;

public interface IDbServiceDapper
{
    Task<Student?> GetStudentDetailsById(int id);
    Task<Group?> GetGroupById(int id);
    Task<Student> AddStudentWithGroupsAssignments(Student student);
}

public class DbServiceDapperDapper(IConfiguration configuration) : IDbServiceDapper
{
    // Helper method for creating and opening connection
    private async Task<SqlConnection> GetConnection()
    {
        var connection = new SqlConnection(configuration.GetConnectionString("Default"));
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        return connection;
    }

    public async Task<Student?> GetStudentDetailsById(int id)
    {
        await using var connection = await GetConnection();
        
        // Dapper many-many relationship : https://www.learndapper.com/relationships#dapper-many-to-many-relationships
        var students = await connection.QueryAsync<Student, Group, Student>(
            @"select s.id, s.firstname, s.lastname, s.phone, s.birthdate, g.id, g.name 
                from students s
                    join groupassignments ga on s.id = ga.students_id
                    join groups g on ga.groups_id = g.id 
                where s.id = @Id",
            (student, group) =>
            {
                student.Groups.Add(group);
                return student;
            },
            new { Id = id },
            splitOn: "id"
        );

        var result = students.GroupBy(e => e.Id).Select(e =>
        {
            var groupedStudents = e.First();
            groupedStudents.Groups = e.Select(student => student.Groups.Single()).ToList();
            return groupedStudents;
        });

        return result.FirstOrDefault();
    }

    public async Task<Group?> GetGroupById(int id)
    {
        await using var connection = await GetConnection();
        var result = await connection.QueryFirstOrDefaultAsync<Group>("select * from groups where id = @id", new {Id = id});
        return result;
    }

    public async Task<Student> AddStudentWithGroupsAssignments(Student student)
    {
        await using var connection = await GetConnection();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            // Generate a new student and get his ID
            var studentId = await connection.ExecuteScalarAsync<int>(
                @"Insert into students values (@fn, @ln, @ph, @bd); 
                        select cast(scope_identity() as int)",
                new
                {
                    Fn = student.FirstName,
                    Ln = student.LastName,
                    Ph = student.Phone,
                    Bd = student.Birthdate
                },
                transaction: transaction
            );

            // Assign a student to each group
            foreach (var group in student.Groups)
            {
                await connection.ExecuteAsync(
                    @"Insert into groupassignments values (@si, @gi)",
                    new { Si = studentId, Gi = group.Id },
                    transaction: transaction
                );    
            }

            student.Id = studentId;
            student.Groups.AddRange(student.Groups);
            await transaction.CommitAsync();

            return student;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}