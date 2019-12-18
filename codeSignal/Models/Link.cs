using Abp.AppFactory.Interfaces;

namespace CodeSignal.Models
{
    public class Link : IAssessmentLink
    {
        public string Id { get; set; }
        public string InvitationUrl { get; set; }
    }
}