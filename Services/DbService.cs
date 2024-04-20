using System.Data;
using System.Data.SqlClient;

namespace SqlClientExample_Tasks.Services;

public interface IDbService
{
    Task<Student?> GetStudentDetailsById(int id);
    Task<Group?> GetGroupById(int id);
    Task<Student> AddStudentWithGroupsAssignments(Student student);
}

public class DbService(IConfiguration configuration) : IDbService
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

        var command = new SqlCommand(
            @"select s.id, s.firstname, s.lastname, s.phone, s.birthdate, g.id, g.name
                     from students s
                         join groupassignments ga on s.id = ga.students_id
                         join groups g on ga.groups_id = g.id where s.id = @1",
            connection
        );

        command.Parameters.AddWithValue("@1", id);

        var reader = await command.ExecuteReaderAsync();

        // If there are no rows, it means that the student with this ID does not exist
        if (!reader.HasRows)
        {
            return null;
        }

        var groups = new List<Group>();
        Student? response = null;

        await reader.ReadAsync();
        do
        {
            groups.Add(new Group
            {
                Id = reader.GetInt32(5),
                Name = reader.GetString(6)
            });
            if (response is null)
            {
                response = new Student
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Phone = reader.GetString(3),
                    Birthdate = reader.GetDateTime(4),
                    Groups = groups
                };
            }
        } while (await reader.ReadAsync());

        return response;
    }

    public async Task<Group?> GetGroupById(int id)
    {
        await using var connection = await GetConnection();
        var c1 = new SqlCommand(
            @"select * from groups where id = @1",
            connection
        );

        c1.Parameters.AddWithValue("@1", id);
        var reader = await c1.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new Group
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1)
        };
    }

    public async Task<Student> AddStudentWithGroupsAssignments(Student student)
    {
        await using var connection = await GetConnection();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            // Generate a new student and get his ID
            var c2 = new SqlCommand(
                @"Insert into students values (@1, @2, @3, @4);
                         select cast(scope_identity() as int)",
                connection,
                (SqlTransaction)transaction
            );
            c2.Parameters.AddWithValue("@1", student.FirstName);
            c2.Parameters.AddWithValue("@2", student.LastName);
            c2.Parameters.AddWithValue("@3", student.Phone);
            c2.Parameters.AddWithValue("@4", student.Birthdate);

            var studentId = (int)(await c2.ExecuteScalarAsync())!;

            // Assign a student to each group
            foreach (var group in student.Groups)
            {
                var c3 = new SqlCommand(
                    "Insert into groupassignments values (@1, @2)",
                    connection,
                    (SqlTransaction)transaction
                );
                c3.Parameters.AddWithValue("@1", studentId);
                c3.Parameters.AddWithValue("@2", group.Id);

                await c3.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();

            student.Id = studentId;

            return student;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}