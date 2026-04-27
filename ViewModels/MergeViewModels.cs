using DonorTrackingSystem.Models;

namespace DonorTrackingSystem.ViewModels
{
    // ViewModel for merging congregants
    public class MergeCongregantViewModel
    {
        public List<Congregant> Congregants { get; set; } = new();
        public int SourceId { get; set; }
        public int TargetId { get; set; }
    }

    // ViewModel for merging non-congregants
    public class MergeNonCongregantViewModel
    {
        public List<NonCongregant> NonCongregants { get; set; } = new();
        public int SourceId { get; set; }
        public int TargetId { get; set; }
    }
}
