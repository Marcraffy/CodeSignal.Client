using Abp.AppFactory.Interfaces;
using System;

namespace CodeSignal.Models
{
    public class Session : Link
    {
        public TestTaker TestTaker { get; set; }
        public AssessmentStatus Status { get; set; }
        public int MaxScore { get; set; }
        public long? StartDate { get; set; }
        public long? FinishDate { get; set; }
        public Result Result { get; set; }
    }
}