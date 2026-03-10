using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OM.MFPTrackerV1.Data.Models
{
	public class DateInPastOrTodayAttribute : ValidationAttribute
	{
		public override bool IsValid(object? value)
		{
			if (value is DateTime date)
			{
				return date.Date <= DateTime.Today;
			}
			return true; // Not a date? Let Required handle it
		}
	}
	public class DateInPastAttribute : ValidationAttribute
	{
		public override bool IsValid(object? value)
		{
			if (value == null)
				return true; // let [Required] handle nulls

			if (value is DateTime date)
			{
				return date.Date < DateTime.Today;
			}

			return false;
		}
	}
}
