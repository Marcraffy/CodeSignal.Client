using Abp.AppFactory.Interfaces;

namespace CodeSignal.Models
{
    internal class Session : Link
    {
        public TestTaker TestTaker { get; set; }
        public AssessmentStatus Status { get; set; }
    }
}