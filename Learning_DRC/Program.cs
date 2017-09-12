using System.IO;

namespace Learning_DRC
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var drcInstance = new LearningDRC(new DirectoryInfo("."));
            drcInstance.ExecuteBasedOnMode(LearningDRC.GetExecutionMode());
        }
    }
}