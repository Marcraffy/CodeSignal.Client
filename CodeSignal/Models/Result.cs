using Abp.AppFactory.Interfaces;
using System.Collections.Generic;

namespace CodeSignal.Models
{
    public class Result: IAssessmentResult
    {
        public int Score { get; set; }
        public IList<IAssessmentTask> Tasks { get; set; }
    }
}