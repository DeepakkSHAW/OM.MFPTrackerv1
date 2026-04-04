using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface IFundNavRepo
	{
		Task<(IReadOnlyList<FundNav> Items, int TotalCount)> GetAsync(
			int? fundId,
			DateTime? fromDate,
			DateTime? toDate,
			string? sortBy,
			bool sortDesc,
			int pageNumber,
			int pageSize);

		Task<FundNav?> GetByIdAsync(int id);

		Task AddAsync(FundNav nav);
		Task UpdateAsync(FundNav nav);
		Task DeleteAsync(int id);

		Task AddRangeAsync(IEnumerable<FundNav> navs);
	}

public sealed class FundNavRepo : IFundNavRepo
	{
		private readonly MFPTrackerDbContext _db;

		public FundNavRepo(MFPTrackerDbContext db)
		{
			_db = db;
		}

		public async Task<(IReadOnlyList<FundNav> Items, int TotalCount)> GetAsync(
			int? fundId,
			DateTime? fromDate,
			DateTime? toDate,
			string? sortBy,
			bool sortDesc,
			int pageNumber,
			int pageSize)
		{
			if (pageNumber < 1) pageNumber = 1;
			if (pageSize < 1) pageSize = 10;
			if (pageSize > 200) pageSize = 200;

			IQueryable<FundNav> q = _db.FundNavs
				.Include(n => n.Fund)
				.AsNoTracking();

			// -------- Filters ----------
			if (fundId.HasValue)
				q = q.Where(n => n.FundId == fundId.Value);

			if (fromDate.HasValue)
				q = q.Where(n => n.NavDate >= fromDate.Value);

			if (toDate.HasValue)
				q = q.Where(n => n.NavDate <= toDate.Value);

			// -------- Sorting ----------
			q = (sortBy, sortDesc) switch
			{
				("NavDate", false) =>
					q.OrderBy(n => n.NavDate)
					 .ThenBy(n => n.FundId)
					 .ThenBy(n => n.FundNavId),

				("NavDate", true) =>
					q.OrderByDescending(n => n.NavDate)
					 .ThenByDescending(n => n.FundId)
					 .ThenByDescending(n => n.FundNavId),

				("Fund", false) =>
					q.OrderBy(n => n.FundId == null)
					 .ThenBy(n => n.Fund!.FundName)
					 .ThenBy(n => n.NavDate)
					 .ThenBy(n => n.FundNavId),

				("Fund", true) =>
					q.OrderByDescending(n => n.FundId == null)
					 .ThenByDescending(n => n.Fund!.FundName)
					 .ThenByDescending(n => n.NavDate)
					 .ThenByDescending(n => n.FundNavId),

				("NavValue", false) =>
					q.OrderBy(n => n.NavValue)
					 .ThenBy(n => n.NavDate)
					 .ThenBy(n => n.FundNavId),

				("NavValue", true) =>
					q.OrderByDescending(n => n.NavValue)
					 .ThenByDescending(n => n.NavDate)
					 .ThenByDescending(n => n.FundNavId),

				_ =>
					q.OrderByDescending(n => n.NavDate)
					 .ThenByDescending(n => n.FundNavId),
			};

			var totalCount = await q.CountAsync();

			var items = await q
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, totalCount);
		}

		public Task<FundNav?> GetByIdAsync(int id)
		{
			return _db.FundNavs
				.Include(n => n.Fund)
				.FirstOrDefaultAsync(n => n.FundNavId == id);
		}

		public async Task AddAsync(FundNav nav)
		{
			nav.FetchedAt = DateTime.UtcNow;
			_db.FundNavs.Add(nav);
			await _db.SaveChangesAsync();
		}

		public async Task UpdateAsync(FundNav nav)
		{
			var existing = await _db.FundNavs
				.FirstOrDefaultAsync(n => n.FundNavId == nav.FundNavId);

			if (existing == null)
				throw new InvalidOperationException("NAV record not found.");

			existing.NavDate = nav.NavDate;
			existing.NavValue = nav.NavValue;
			existing.FundId = nav.FundId;

			await _db.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var nav = await _db.FundNavs.FindAsync(id);
			if (nav == null) return;

			_db.FundNavs.Remove(nav);
			await _db.SaveChangesAsync();
		}

		public async Task AddRangeAsync(IEnumerable<FundNav> navs)
		{
			_db.FundNavs.AddRange(navs);
			await _db.SaveChangesAsync();
		}
	}
}
