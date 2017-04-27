using System.Windows.Controls;

namespace Client
{
    public class Room
    {
        public int Id { get; set; }
        public string Header { get; set; }
        public TabItem Tab { get; set; }
        public TextBox ChatBox { get; set; }
    }
}
