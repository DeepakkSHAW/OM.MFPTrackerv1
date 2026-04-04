using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface ISpecialEventRepo
	{
		// Main page query (search + filter + sort + pagination)
		Task<(IReadOnlyList<SpecialEvent> Items, int TotalCount)> GetAsync(
			string? searchText,
			string? eventType,
			string? severity,
			int? fundId,
			DateTime? fromDate,
			DateTime? toDate,
			string? sortBy,
			bool sortDesc,
			int pageNumber,
			int pageSize);

		// Direct lookups
		Task<SpecialEvent?> GetByIdAsync(int id);

		// ✅ Explicit helpers (requested)
		Task<IReadOnlyList<SpecialEvent>> GetForFundAsync(int fundId);
		Task<IReadOnlyList<SpecialEvent>> GetMarketWideAsync();

		// Mutations
		Task<SpecialEvent> AddAsync(SpecialEvent entity);
		Task<SpecialEvent> UpdateAsync(SpecialEvent entity);
		Task DeleteAsync(int id);
	}




	public sealed class SpecialEventRepo : ISpecialEventRepo
	{
		private readonly MFPTrackerDbContext _db;

		public SpecialEventRepo(MFPTrackerDbContext db)
		{
			_db = db;
		}

		// -------------------------------------------------
		// Search + Filter + Sort + Pagination
		// -------------------------------------------------
		public async Task<(IReadOnlyList<SpecialEvent> Items, int TotalCount)> GetAsync(
			string? searchText,
			string? eventType,
			string? severity,
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

			IQueryable<SpecialEvent> q = _db.SpecialEvents
				.Include(e => e.Fund)
				.AsNoTracking();

			// -------- Filtering --------
			if (!string.IsNullOrWhiteSpace(searchText))
			{
				var txt = $"%{searchText.Trim()}%";
				q = q.Where(e =>
					EF.Functions.Like(e.Title, txt) ||
					(e.Description != null && EF.Functions.Like(e.Description, txt))
				);
			}

			if (!string.IsNullOrWhiteSpace(eventType))
				q = q.Where(e => e.EventType == eventType);

			if (!string.IsNullOrWhiteSpace(severity))
				q = q.Where(e => e.Severity == severity);

			if (fundId.HasValue)
				q = q.Where(e => e.FundId == fundId.Value);

			if (fromDate.HasValue)
				q = q.Where(e => e.EventDate >= fromDate.Value);

			if (toDate.HasValue)
				q = q.Where(e => e.EventDate <= toDate.Value);

			// -------- Sorting --------
			//q = (sortBy, sortDesc) switch
			//{
			//	("Title", false) => q.OrderBy(e => e.Title),
			//	("Title", true) => q.OrderByDescending(e => e.Title),

			//	("EventType", false) => q.OrderBy(e => e.EventType),
			//	("EventType", true) => q.OrderByDescending(e => e.EventType),

			//	("Severity", false) => q.OrderBy(e => e.Severity),
			//	("Severity", true) => q.OrderByDescending(e => e.Severity),

			//	("EventDate", false) => q.OrderBy(e => e.EventDate),
			//	("EventDate", true) => q.OrderByDescending(e => e.EventDate),

			//	_ => q.OrderByDescending(e => e.EventDate)
			//};
			// -------- Sorting --------
			q = (sortBy, sortDesc) switch
			{
				// ---------- Event Date ----------
				("EventDate", false) =>
					q.OrderBy(e => e.EventDate)
					 .ThenBy(e => e.SpecialEventId),

				("EventDate", true) =>
					q.OrderByDescending(e => e.EventDate)
					 .ThenByDescending(e => e.SpecialEventId),

				// ---------- Title ----------
				("Title", false) =>
					q.OrderBy(e => e.Title)
					 .ThenBy(e => e.EventDate)
					 .ThenBy(e => e.SpecialEventId),

				("Title", true) =>
					q.OrderByDescending(e => e.Title)
					 .ThenByDescending(e => e.EventDate)
					 .ThenByDescending(e => e.SpecialEventId),

				// ---------- ✅ Fund (NULL-safe, deterministic) ----------
				("Fund", false) =>
					q.OrderBy(e => e.FundId == null)                // market-wide LAST
					 .ThenBy(e => e.Fund != null ? e.Fund.FundName : string.Empty)
					 .ThenBy(e => e.EventDate)
					 .ThenBy(e => e.SpecialEventId),

				("Fund", true) =>
					q.OrderByDescending(e => e.FundId == null)     // market-wide FIRST
					 .ThenByDescending(e => e.Fund != null ? e.Fund.FundName : string.Empty)
					 .ThenByDescending(e => e.EventDate)
					 .ThenByDescending(e => e.SpecialEventId),

				// ---------- Event Type ----------
				("EventType", false) =>
					q.OrderBy(e => e.EventType)
					 .ThenBy(e => e.EventDate)
					 .ThenBy(e => e.SpecialEventId),

				("EventType", true) =>
					q.OrderByDescending(e => e.EventType)
					 .ThenByDescending(e => e.EventDate)
					 .ThenByDescending(e => e.SpecialEventId),

				// ---------- Severity ----------
				("Severity", false) =>
					q.OrderBy(e => e.Severity)
					 .ThenBy(e => e.EventDate)
					 .ThenBy(e => e.SpecialEventId),

				("Severity", true) =>
					q.OrderByDescending(e => e.Severity)
					 .ThenByDescending(e => e.EventDate)
					 .ThenByDescending(e => e.SpecialEventId),

				// ---------- Default ----------
				_ =>
					q.OrderByDescending(e => e.EventDate)
					 .ThenByDescending(e => e.SpecialEventId)
			};
			var totalCount = await q.CountAsync();

			var items = await q
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, totalCount);
		}

		// -------------------------------------------------
		// Get by Id
		// -------------------------------------------------
		public async Task<SpecialEvent?> GetByIdAsync(int id)
		{
			return await _db.SpecialEvents
				.Include(e => e.Fund)
				.FirstOrDefaultAsync(e => e.SpecialEventId == id);
		}

		// -------------------------------------------------
		// ✅ Fund‑specific events (no paging)
		// -------------------------------------------------
		public async Task<IReadOnlyList<SpecialEvent>> GetForFundAsync(int fundId)
		{
			return await _db.SpecialEvents
				.AsNoTracking()
				.Where(e => e.FundId == fundId)
				.OrderBy(e => e.EventDate)
				.ToListAsync();
		}

		// -------------------------------------------------
		// ✅ Market‑wide events (no paging)
		// -------------------------------------------------
		public async Task<IReadOnlyList<SpecialEvent>> GetMarketWideAsync()
		{
			return await _db.SpecialEvents
				.AsNoTracking()
				.Where(e => e.FundId == null)
				.OrderBy(e => e.EventDate)
				.ToListAsync();
		}

		// -------------------------------------------------
		// Add
		// -------------------------------------------------
		public async Task<SpecialEvent> AddAsync(SpecialEvent entity)
		{
			entity.InDate = DateTime.UtcNow;
			_db.SpecialEvents.Add(entity);
			await _db.SaveChangesAsync();
			return entity;
		}

		// -------------------------------------------------
		// Update
		// -------------------------------------------------
		public async Task<SpecialEvent> UpdateAsync(SpecialEvent entity)
		{
			var existing = await _db.SpecialEvents
				.FirstOrDefaultAsync(e => e.SpecialEventId == entity.SpecialEventId);

			if (existing == null)
				throw new InvalidOperationException("SpecialEvent not found.");

			existing.Title = entity.Title;
			existing.Description = entity.Description;
			existing.EventDate = entity.EventDate;
			existing.FundId = entity.FundId;
			existing.EventType = entity.EventType;
			existing.Severity = entity.Severity;

			await _db.SaveChangesAsync();
			return existing;
		}

		// -------------------------------------------------
		// Delete
		// -------------------------------------------------
		public async Task DeleteAsync(int id)
		{
			var entity = await _db.SpecialEvents.FindAsync(id);
			if (entity == null)
				return;

			_db.SpecialEvents.Remove(entity);
			await _db.SaveChangesAsync();
		}
	}
}
