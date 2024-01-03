class Program
{
    private static readonly int NodesCount = 3;

    private static List<ComputingNode> ComputingNodeList = new();
    private static List<FibonacciNode> AllFibonacciComputingValueList = new();
    private static SemaphoreSlim BufferLock = new(1);
    private static FibonacciNode LastAddedFibonacciComputingValue = new FibonacciNode();
    private static long MaxComputingFibonacci;
    private static bool IsFoundAllFibonacciNumbers = false;

    static void Main()
    {
        for (int i = 1; i <= NodesCount; i++)
        {
            ComputingNodeList.Add(new ComputingNode(i));
        }

        if (ComputingNodeList.Count == 0)
        {
            Console.WriteLine($"Failed to create compute node");

            return;
        }
        var minComputingNode = ComputingNodeList.MinBy(x => x.ClosestFibonacciNumber);
        if (minComputingNode == null)
        {
            Console.WriteLine($"Minimum value could not be found");

            return;
        }
        MaxComputingFibonacci = ComputingNodeList.Max(x => x.ClosestFibonacciNumber);

        AddFibonacciComputingValue(new FibonacciNode()
        {
            FibonaciNumber = minComputingNode.ClosestFibonacciNumber,
            NodeNumber = minComputingNode.NodeNumber
        });

        Task.WaitAll(
            Task.Run(() => Run(ComputingNodeList[0])),
            Task.Run(() => Run(ComputingNodeList[1])),
            Task.Run(() => Run(ComputingNodeList[2]))
        );

        foreach (var computingNode in ComputingNodeList)
        {
            Console.WriteLine($"Node - {computingNode.NodeNumber}, Random number {computingNode.PseudoRandomNumber}");
            Console.WriteLine($"Node - {computingNode.NodeNumber}, Closest Fibonacci Number {computingNode.ClosestFibonacciNumber}");
        }

        foreach (var value in AllFibonacciComputingValueList)
        {
            Console.WriteLine($"Node - {value.NodeNumber}, {value.FibonaciNumber}");
        }
    }

    private static void Run(ComputingNode computingNode)
    {
        while (!IsFoundAllFibonacciNumbers)
        {
            ComputeMissingFibonacciNumbers(computingNode);
        }
    }

    private static void ComputeMissingFibonacciNumbers(ComputingNode currentNode)
    {
        if (LastAddedFibonacciComputingValue.NodeNumber != currentNode.NodeNumber)
        {
            var nextFibonacci = FindNextFibonacciInRange(LastAddedFibonacciComputingValue.FibonaciNumber, MaxComputingFibonacci);

            if (nextFibonacci == MaxComputingFibonacci)
            {
                IsFoundAllFibonacciNumbers = true;
            }

            if (nextFibonacci > LastAddedFibonacciComputingValue.FibonaciNumber)
            {
                BufferLock.Wait();
                var lastAddedNode = AllFibonacciComputingValueList.MaxBy(x => x.FibonaciNumber) ?? new FibonacciNode();
                if (lastAddedNode.FibonaciNumber < nextFibonacci)
                {
                    AddFibonacciComputingValue(new FibonacciNode
                    {
                        NodeNumber = currentNode.NodeNumber,
                        FibonaciNumber = nextFibonacci
                    });
                }
                BufferLock.Release();
            }

            return;
        }
    }

    private static void AddFibonacciComputingValue(FibonacciNode fibonacciNode)
    {
        LastAddedFibonacciComputingValue = fibonacciNode;
        AllFibonacciComputingValueList.Add(fibonacciNode);
    }

    public static long FindNextFibonacciInRange(long start, long end)
    {
        long a = 0, b = 1;

        while (b <= end)
        {
            long temp = a;
            a = b;
            b = temp + b;

            if (a > start && a <= end)
            {
                return a;
            }
        }
        return end;
    }
}

class ComputingNode
{
    public readonly int NodeNumber;
    private readonly Random Random;
    public readonly int PseudoRandomNumber;
    public readonly long ClosestFibonacciNumber;

    public ComputingNode(int nodeNumber)
    {
        NodeNumber = nodeNumber;
        Random = new Random();
        PseudoRandomNumber = Random.Next(1, 1000);
        ClosestFibonacciNumber = FindClosestFibonacci(PseudoRandomNumber);
    }

    private static int FindClosestFibonacci(int number)
    {
        int a = 0, b = 1;

        while (b < number)
        {
            int temp = a;
            a = b;
            b = temp + b;
        }

        return (b - number) < (number - a) ? b : a;
    }
}

class FibonacciNode
{
    public int NodeNumber { get; set; }
    public long FibonaciNumber { get; set; }
}
