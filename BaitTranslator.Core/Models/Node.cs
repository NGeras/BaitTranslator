namespace BaitTranslator.Core.Models
{
    public class Node
    {
        public string sourceNode { get; set; }

        public string targetNode { get; set; }

        public Node(string source, string target)
        {
            sourceNode = source;
            targetNode = target;
        }

        public override string ToString()
        {
            return $"{sourceNode} -- {targetNode}";
        }
    }
}
