namespace Test3
{
    public class UniqueId
    {
        private Guid id;
        public UniqueId()
        {
            id = Guid.NewGuid();
        }
        public Guid Id => id;
    }
    public class Log
    {
        private Guid _id;
        Log(UniqueId uniqueId)
        {
            _id = uniqueId.Id;
        }
        public string LogMessage()
        {
            return $"Журнал: {_id}";
        }
    }
    public class Report
    {
        private Guid _id;
        Report(UniqueId uniqueId)
        {
            _id = uniqueId.Id;
        }
        public string ReportMessage()
        {
            return $"Отчет: {_id}";
        }
    }
}
