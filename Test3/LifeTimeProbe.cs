namespace Test3
{
    public class LifeTimeProbe
    {
        public Guid Id { get; } = Guid.NewGuid();
    }
    public class ReadProbe
    {
        private LifeTimeProbe _id;

        public ReadProbe(LifeTimeProbe id)
        {
             _id = id;
        }

        public Guid Read()
        {
            return _id.Id; 
        }
    }
}
