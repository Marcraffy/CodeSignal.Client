using Abp.AppFactory.Interfaces;
using System.Collections.Generic;

namespace CodeSignal.Models
{
    public class Assessment: IAssessment
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string IntroMessage { get; set; }
        public string OutroMessage { get; set; }
        public int? Duration { get; set; }
        public IList<IAssessmentReport> Reports { get; set; }
    }
}