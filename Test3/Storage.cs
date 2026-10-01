using Npgsql;

namespace Test3
{
    public class ToDo
    {
        private int _id;
        private string _title;
        private bool _isCompleted;
        private int _ownerId;
        private DateTime _createAt;
        public ToDo(int id, string title, bool iscompleted, int ownerId, DateTime createAt)
        {
            Title = title;
            Id = id;
            IsCompleted = iscompleted;
            OwnerId = ownerId;
            CreateAt = createAt;
        }

        public int Id
        {
            get { return _id; }
            private set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Wrong id value");
                }
                _id = value;
            }
        }
        public string Title
        {
            get { return _title; }
            set
            {
                if (value is null)
                {
                    throw new ArgumentNullException("Null value title");
                }
                _title = value;
            }
        }
        public bool IsCompleted
        {
            get { return _isCompleted; }

            set { _isCompleted = value; }
        }
        public int OwnerId
        {
            get { return _ownerId; }
            private set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("Wrong owner id value");
                }
                _ownerId = value;
            }
        }
        public DateTime CreateAt
        {
            get { return _createAt; }
            private set
            {
                _createAt = value;
            }
        }
        public ToDo Copy()
        {
            return new ToDo(_id, _title, _isCompleted, _ownerId, _createAt);
        }
        
    }
    public class Storage
    {
        private NpgsqlDataSource _npgsqlDataSource;

        public Storage(NpgsqlDataSource npgsqlDataSource)
        {
            _npgsqlDataSource = npgsqlDataSource;
        }

        public async Task<List<ToDo>> ToDos(CancellationToken cancellation) //планировал делать через ofset
        {
            await using var connection = await _npgsqlDataSource.OpenConnectionAsync(cancellation);
            await using var command = new NpgsqlCommand("""
                SELECT id, title, iscompleted, owner_id, created_at
                FROM public.tasks
                """, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellation);
            List<ToDo> toDos = new();
            while (await reader.ReadAsync(cancellation))
            {
                toDos.Add(readDbLine(reader));
            }
            return toDos;

        }
        public async Task<List<ToDo>> ToDos(CancellationToken cancellation, int ownerId) 
        {
            await using var connection = await _npgsqlDataSource.OpenConnectionAsync(cancellation);
            await using var command = new NpgsqlCommand("""
                SELECT id, title, iscompleted, owner_id, created_at
                FROM public.tasks
                WHERE owner_id = $1
                """, connection);
            command.Parameters.Add(new NpgsqlParameter { Value = ownerId });
            await using var reader = await command.ExecuteReaderAsync(cancellation);
            List<ToDo> toDos = new();
            while (await reader.ReadAsync(cancellation))
            {
                toDos.Add(readDbLine(reader));
            }
            return toDos;

        }
        public async Task<ToDo?> FindToDo(int id, CancellationToken cancellationToken) // Я только что понял что я должен был делать не через null, а через TryFind.... Так было бы лучше 
        {
            await using var conection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand("""
                SELECT id, title, iscompleted, owner_id, created_at
                FROM public.tasks
                WHERE id = $1
                """, conection);
            command.Parameters.Add(new NpgsqlParameter { Value = id});
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            bool isRead = await reader.ReadAsync(cancellationToken);
            if (!isRead)
            {
                return null;
            }
            return readDbLine(reader);
        }
        public async Task<int?> Add(string title, int ownerId, CancellationToken cancellationToken, bool isComplete = false)
        {
            await using var conection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand("""
                INSERT INTO public.tasks (title, iscompleted, owner_id)
                VALUES ($1, $2, $3)
                RETURNING id
                """, conection);
            command.Parameters.Add(new NpgsqlParameter { Value = title });
            command.Parameters.Add(new NpgsqlParameter { Value = isComplete });
            command.Parameters.Add(new NpgsqlParameter { Value = ownerId });
            int id;
            try
            {
                 id = (int)(await command.ExecuteScalarAsync(cancellationToken))!;
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {

                return null;
            }

            return id;

        }
        public async Task<bool> TryPatchTitle(int id, string title, CancellationToken cancellationToken)
        {
            await using var conection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand("""
                UPDATE public.tasks
                SET title = $1
                WHERE id = $2
                """, conection);
            command.Parameters.Add(new NpgsqlParameter { Value = title });
            command.Parameters.Add(new NpgsqlParameter { Value = id });
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            return result > 0;
        }
        public async Task<bool> TryPatchIsComplete(int id, bool isComplete, CancellationToken cancellationToken)
        {
            await using var conection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand("""
                UPDATE public.tasks
                SET iscompleted = $1
                WHERE id = $2
                """, conection);
            command.Parameters.Add(new NpgsqlParameter { Value = isComplete });
            command.Parameters.Add(new NpgsqlParameter { Value = id });
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            return result > 0;
        }
        public async Task<MessegStoreg> TryPut(int id, string title, bool isComplete,int ownerId, CancellationToken cancellationToken)
        {
            await using var conection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand("""
                UPDATE public.tasks
                SET iscompleted = $1, title = $2, owner_id = $3
                WHERE id = $4
                """, conection);

            command.Parameters.Add(new NpgsqlParameter { Value = isComplete });
            command.Parameters.Add(new NpgsqlParameter { Value = title });
            command.Parameters.Add(new NpgsqlParameter { Value = ownerId });
            command.Parameters.Add(new NpgsqlParameter { Value = id });
            int result;
            try
            {
                result = await command.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {

                return MessegStoreg.ForeignKeyViolation;
            }
            if (result > 0){
                return MessegStoreg.Ok;
            }
            return MessegStoreg.NotFound;
        }

        public async Task<bool> TryDelete(int id, CancellationToken cancellationToken)
        {
            await using var conection = await _npgsqlDataSource.OpenConnectionAsync(cancellationToken);
            await using var command = new NpgsqlCommand("""
                DELETE FROM public.tasks
                WHERE id = $1
                RETURNING id
                """, conection);
            command.Parameters.Add(new NpgsqlParameter { Value = id });
            var result = await command.ExecuteNonQueryAsync(cancellationToken);
            return result > 0;
        }

        private ToDo readDbLine(NpgsqlDataReader reader) => new ToDo(reader.GetInt32(0), reader.GetString(1), reader.GetBoolean(2), reader.GetInt32(3), reader.GetDateTime(4));
        

    }
    public class CreateTaskRequest
    {
        public string? Title { get; set; }
        public int OwnerId {  get; set; }
    }
    public class UpdateTitleTaskRequest
    {
        public string? Title { get; set; }
    }
    public class UpdateIsCompletedTaskRequest
    {
        public bool IsCompleted { get; set; }
    }
    public class UpdateTaskRequest
    {
        public string? Title { get; set; }
        public bool IsCompleted { get; set; }
        public int  OwnerId { get; set; }
    }

    public enum MessegStoreg
    {
        NotFound,
        ForeignKeyViolation,
        Ok


    }
}
