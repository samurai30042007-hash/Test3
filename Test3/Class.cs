namespace Test3
{
    public class ToDo
    {
        private int _id;
        private string _title;
        private bool _isCompleted;
        public ToDo(int id, string title, bool iscompleted)
        {
            Title = title;
            Id = id;
            IsCompleted = iscompleted;
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
        public ToDo Copy()
        {
            return new ToDo(_id, _title, _isCompleted);
        }
    }
    public class Storage
    {
        private Dictionary<int, ToDo> _ToDos;
        private int _nextId = 0;

        private object _lock = new object();

        public Storage()
        {
            _ToDos = new();
        }
        public Storage(Dictionary<int, ToDo> ToDos)
        {
            _ToDos = ToDos;
        }

        private int NextId
        {
            get
            {
                return Interlocked.Increment(ref _nextId);
            }
        }

        public List<ToDo> ToDos // хотел использовать что-то типо Copy но не нашел
        {
            get
            {
                lock (_lock)
                {
                    if (_ToDos is null)
                    {
                        return new List<ToDo>();
                    }
                    List<ToDo> ToDosCopy = new();
                    foreach (var toDo in _ToDos)
                    {
                        ToDosCopy.Add(toDo.Value.Copy());
                    }
                    return ToDosCopy;
                }
            }

        }
        public ToDo? FindToDo(int id) // Я только что понял что я должен был делать не через null, а через TryFind.... Так было бы лучше 
        {
            lock (_lock)
            {
                ToDo toDo;
                bool isFind = _ToDos.TryGetValue(id, out toDo);
                if (isFind)
                {
                    return toDo.Copy();
                }
                return null;
            }
        }
        public int Add(string title, bool isComplete = false)
        {
            lock (_lock)
            {
                if (string.IsNullOrWhiteSpace(title))
                {
                    throw new ArgumentNullException("Null value title");
                }
                int id = NextId;
                _ToDos.Add(id, new ToDo(id, title, isComplete));
                return id;
            }
        }
        public void TryPatchTitle(int id, string title)
        {
            lock (_lock)
            {
                ToDo toDo;
                bool isFind = _ToDos.TryGetValue(id, out toDo);
                if (string.IsNullOrWhiteSpace(title))
                {
                    throw new ArgumentNullException("Null value title");
                }
                if (toDo is null)
                {
                    throw new ArgumentException("No such id");
                }
                toDo.Title = title;
            }
        }
        public void TryPatchIsComplete(int id, bool isComplete)
        {
            lock (_lock)
            {
                ToDo toDo;
                bool isFind = _ToDos.TryGetValue(id, out toDo);

                if (toDo is null)
                {
                    throw new ArgumentException("No such id1");
                }
                toDo.IsCompleted = isComplete;
            }
        }
        public void TryPut(int id, string title, bool isComplete)
        {
            lock (_lock)
            {
                ToDo toDo;
                bool isFind = _ToDos.TryGetValue(id, out toDo);

                if (string.IsNullOrWhiteSpace(title)) 
                {
                throw new ArgumentNullException("Null value title");
                }

                if (toDo is null)
                {
                    throw new ArgumentException("No such id");
                }
                toDo.IsCompleted = isComplete;
                toDo.Title = title;
            }
        }

        public bool TryDelete(int id)
        {
            lock (_lock)
            {
                return _ToDos.Remove(id);
            }
        }


    }
    public class CreateTaskRequest
    {
        public string? Title { get; set; }
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
    }
}
