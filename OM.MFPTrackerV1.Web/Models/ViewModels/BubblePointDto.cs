namespace OM.MFPTrackerV1.Web.Models.ViewModels
{
	public class BubbleChartPoint
	{
		public DateTime X { get; set; }          // Transaction date
		public decimal Y { get; set; }           // NAV
		public decimal Investment { get; set; }  // Total Investment (₹)

		public string Fund { get; set; } = "";
		public string Holder { get; set; } = "";
		public int ColorKey { get; set; }         // Used to generate stable color
	}
}
