namespace Project1
{
    class BirdEventArg
    {
        public string Message { get; }
        public double TimeOfFligth { get; }
        public double MaxY { get; }
        public double Range { get; }
        public BirdEventArg(string message, double timeOfFligth, double maxY, double range)
        {
            Message = message;
            TimeOfFligth = timeOfFligth;
            MaxY = maxY;
            Range = range;
        }
    }
}
