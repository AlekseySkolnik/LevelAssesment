namespace Patterns.Structural
{
    // The 'Flyweight' interface
    interface IRobot
    {
        void Print();
    }

    // A 'ConcreteFlyweight' class
    class SmallRobot : IRobot
    {
        public void Print() =>
            Console.WriteLine(" This is a small Robot");
    }

    // A 'ConcreteFlyweight' class
    class LargeRobot : IRobot
    {
        public void Print() =>
            Console.WriteLine(" I am a large Robot");
    }

    // The 'FlyweightFactory' class
    class RobotFactory
    {
        readonly Dictionary<string, IRobot> _shapes = new();

        public int TotalObjectsCreated => _shapes.Count;

        public IRobot GetRobotFromFactory(string robotType)
        {
            if (_shapes.TryGetValue(robotType, out var robotCategory))
            {
                return robotCategory;
            }

            switch (robotType)
            {
                case "Small":
                    robotCategory = new SmallRobot();
                    _shapes.Add("Small", robotCategory);
                    break;

                case "Large":
                    robotCategory = new LargeRobot();
                    _shapes.Add("Large", robotCategory);
                    break;

                default:
                    throw new Exception(" Robot Factory can create only small and large robots");
            }

            return robotCategory;
        }

    }

    internal static class FlyweightPattern
    {
        public static void Demo()
        {
            Console.WriteLine("***Flyweight Pattern Demo***\n");
            var myfactory = new RobotFactory();
            IRobot shape = myfactory.GetRobotFromFactory("Small");
            shape.Print();

            for (int i = 0; i < 2; i++)
            {
                shape = myfactory.GetRobotFromFactory("Small");
                shape.Print();
            }
            int NumOfDistinctRobots = myfactory.TotalObjectsCreated;
            Console.WriteLine("\n Now, total numbers of distinct robot objects is = {0}\n", NumOfDistinctRobots);

            /*Here we are trying to get the 5 more Large robots.
            Note that: now onwards we need not create additional small robots because
            we have already created one of this category */
            for (int i = 0; i < 5; i++)
            {
                shape = myfactory.GetRobotFromFactory("Large");
                shape.Print();
            }

            NumOfDistinctRobots = myfactory.TotalObjectsCreated;
            Console.WriteLine("\n Distinct Robot objects created till now = {0}", NumOfDistinctRobots);
        }
    }
}
