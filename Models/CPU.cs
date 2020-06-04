using System.Text.RegularExpressions;

namespace HPI.BBB.Autoscaler.Models
{
    public class CPU
    {
        public CPU()
        {
        }

        public string Id { get; internal set; }
        public decimal IdleTime { get; internal set; }
    }
}