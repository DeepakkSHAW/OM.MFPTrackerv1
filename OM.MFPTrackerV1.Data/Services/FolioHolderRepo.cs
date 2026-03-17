using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OM.MFPTrackerV1.Data;
using OM.MFPTrackerV1.Data.Models;
using System.Diagnostics.CodeAnalysis;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface IFolioOwnerRepository
	{
		Task<(IReadOnlyList<FolioOwner> Items, int TotalCount)> GetAsync(
	string? search,
	string? sortBy,
	bool sortDesc,
	int pageNumber,
	int pageSize);

		Task<FolioOwner?> GetByIdAsync(int id);
		Task<bool> ExistsByNameAsync(string firstName, int? excludeId = null);
		Task<bool> ExistsBySignatureAsync(string Signature, int? excludeId = null);
		Task<bool> EnsureUniqueAsync(FolioOwner e, bool isUpdate, CancellationToken ct = default);
		Task<FolioOwner> AddAsync(FolioOwner entity);
		Task<FolioOwner> UpdateAsync(FolioOwner entity);
		Task DeleteAsync(int id);
		Task<int> CountAsync();
	}
	public class FolioOwnerRepository : IFolioOwnerRepository
	{
		private readonly MFPTrackerDbContext _db;

		public FolioOwnerRepository(MFPTrackerDbContext db)
		{
			_db = db;
		}

		public async Task<(IReadOnlyList<FolioOwner> Items, int TotalCount)> GetAsync(
			string? search, string? sortBy, bool sortDesc, int pageNumber, int pageSize)
		{
			// Guardrails
			if (pageNumber < 1) pageNumber = 1;
			if (pageSize < 1) pageSize = 10;
			if (pageSize > 200) pageSize = 200; // optional cap for safety

			IQueryable<FolioOwner> q = _db.Set<FolioOwner>().AsNoTracking();

			// Filter (case-insensitive; SQLite column uses NOCASE which helps)
			if (!string.IsNullOrWhiteSpace(search))
			{
				var term = search.Trim();
				static string EscapeLike(string input) => input.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

				var escaped = EscapeLike(term);
				var pattern = $"%{escaped}%";

				//var pattern = $"%{search}%";
				q = q.Where(x =>
					(x.FirstName != null && EF.Functions.Like(EF.Functions.Collate(x.FirstName, "NOCASE"), pattern))||
					(x.LastName != null && EF.Functions.Like(EF.Functions.Collate(x.LastName, "NOCASE"), pattern)) ||
					(x.Signature != null && EF.Functions.Like(EF.Functions.Collate(x.Signature, "NOCASE"), pattern))
				);
			}


			// Sorting
			//q = (sortBy?.ToLowerInvariant()) switch
			//{
			//	"firstname" => (sortDesc ? q.OrderByDescending(x => x.FirstName)
			//							 : q.OrderBy(x => x.FirstName)),
			//	_ => (sortDesc ? q.OrderByDescending(x => x.FolioOwnerId)
			//				   : q.OrderBy(x => x.FolioOwnerId))
			//};

			// ------------------------------
			// Sorting
			// ------------------------------
			// Normalize the incoming sort key
			var key = (sortBy ?? "LastName").Trim();
			q = (key, sortDesc) switch
			{
				// FirstName
				(nameof(FolioHolder.FirstName), true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.FirstName ?? string.Empty, "NOCASE"))
					 .ThenBy(x => x.FolioOwnerId),

				(nameof(FolioHolder.FirstName), false) =>
					q.OrderBy(x => EF.Functions.Collate(x.FirstName ?? string.Empty, "NOCASE"))
					 .ThenBy(x => x.FolioOwnerId),

				// LastName
				(nameof(FolioHolder.LastName), true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.LastName ?? string.Empty, "NOCASE"))
					 .ThenBy(x => x.FolioOwnerId),

				(nameof(FolioHolder.LastName), false) =>
					q.OrderBy(x => EF.Functions.Collate(x.LastName ?? string.Empty, "NOCASE"))
					 .ThenBy(x => x.FolioOwnerId),

				// DateOfBirth (example of non-string)
				(nameof(FolioHolder.DateOfBirth), true) =>
					q.OrderByDescending(x => x.DateOfBirth)
					 .ThenBy(x => x.FolioOwnerId),

				(nameof(FolioHolder.DateOfBirth), false) =>
					q.OrderBy(x => x.DateOfBirth)
					 .ThenBy(x => x.FolioOwnerId),
				// Signature
				(nameof(FolioHolder.Signature), true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.Signature ?? string.Empty, "NOCASE"))
					 .ThenBy(x => x.FolioOwnerId),

				(nameof(FolioHolder.Signature), false) =>
					q.OrderBy(x => EF.Functions.Collate(x.Signature ?? string.Empty, "NOCASE"))
					  .ThenBy(x => x.FolioOwnerId),
				// Fallback: LastName ascending (stable)
				(_, true) =>
					q.OrderByDescending(x => EF.Functions.Collate(x.LastName ?? string.Empty, "NOCASE"))
					 .ThenBy(x => x.FolioOwnerId),

				_ =>
					q.OrderBy(x => EF.Functions.Collate(x.LastName ?? string.Empty, "NOCASE"))
					 .ThenBy(x => x.FolioOwnerId),
			};
			var total = await q.CountAsync();

			var items = await q
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, total);
		}

		public async Task<FolioOwner?> GetByIdAsync(int id)
		{
			return await _db.Set<FolioOwner>().FirstOrDefaultAsync(x => x.FolioOwnerId == id);
		}

		public async Task<bool> ExistsByNameAsync(string firstName, int? excludeId = null)
		{
			if (string.IsNullOrWhiteSpace(firstName))
				return false;

			var query = _db.Set<FolioOwner>().AsQueryable();

			if (excludeId.HasValue)
				query = query.Where(x => x.FolioOwnerId != excludeId.Value);

			// SQLite column collation NOCASE aids here, Contains/Equals are case-insensitive per collation
			return await query.AnyAsync(x => x.FirstName == firstName.Trim());
		}
		public async Task<bool> ExistsBySignatureAsync(string Signature, int? excludeId = null)
		{
			if (string.IsNullOrWhiteSpace(Signature))
				return false;

			var query = _db.Set<FolioOwner>().AsQueryable();

			if (excludeId.HasValue)
				query = query.Where(x => x.FolioOwnerId != excludeId.Value);

			// SQLite column collation NOCASE aids here, Contains/Equals are case-insensitive per collation
			return await query.AnyAsync(x => x.Signature == Signature.Trim());
		}

		public async Task<bool> EnsureUniqueAsync(FolioOwner e, bool isUpdate, CancellationToken ct = default)
		{
			var q = _db.Set<FolioOwner>().AsNoTracking().Where(x => !isUpdate || x.FolioOwnerId != e.FolioOwnerId);

			var dupPerson = await q.AnyAsync(x =>
				EF.Functions.Collate(x.FirstName, "NOCASE") == e.FirstName &&
				EF.Functions.Collate(x.LastName, "NOCASE") == e.LastName &&
				x.DateOfBirth.Date == e.DateOfBirth.Date, ct);
			return dupPerson;
		}

		public async Task<FolioOwner> AddAsync(FolioOwner entity)
		{
			_db.Set<FolioOwner>().Add(entity);
			await _db.SaveChangesAsync();
			return entity;
		}

		public async Task<FolioOwner> UpdateAsync(FolioOwner entity)
		{
			var existing = await _db.Set<FolioOwner>().FirstOrDefaultAsync(x => x.FolioOwnerId == entity.FolioOwnerId);
			if (existing == null)
				throw new InvalidOperationException($"FolioOwner id {entity.FolioOwnerId} not found.");

			// Only update allowed fields
			existing.FirstName = entity.FirstName;
			existing.LastName = entity.LastName;
			existing.DateOfBirth = entity.DateOfBirth;
			existing.Signature = entity.Signature;
			await _db.SaveChangesAsync();
			return existing;
		}

		public async Task DeleteAsync(int id)
		{
			var existing = await _db.Set<FolioOwner>().FirstOrDefaultAsync(x => x.FolioOwnerId == id);
			if (existing == null)
				return;

			_db.Set<FolioOwner>().Remove(existing);
			await _db.SaveChangesAsync();
		}

		public Task<int> CountAsync() => _db.Set<FolioOwner>().CountAsync();
	}


	///////////END FolioOwnerRepository //////////////
	//public interface IFolioHolderRepo
	//{
	//	Task<(IReadOnlyList<FolioHolder> Items, int TotalCount)> GetAsync(
	//		string? nameContains = null,
	//		string? signatureContains = null,
	//		int skip = 0,
	//		int take = 25,
	//		string sortBy = "LastName",
	//		bool desc = false,
	//		CancellationToken ct = default);

	//	Task<FolioHolder?> GetByIdAsync(int id, CancellationToken ct = default);
	//	Task<int> AddAsync(FolioHolder entity, CancellationToken ct = default);
	//	Task UpdateAsync(FolioHolder entity, CancellationToken ct = default);
	//	Task DeleteAsync(int id, CancellationToken ct = default);
	//	Task<Dictionary<int, int>> GetFolioCountsAsync(CancellationToken ct = default);
	//}

	//public class FolioHolderRepo : IFolioHolderRepo
	//{
	//	private readonly MFPTrackerDbContext _db;
	//	public FolioHolderRepo(MFPTrackerDbContext db) => _db = db;

	//	public async Task<(IReadOnlyList<FolioHolder> Items, int TotalCount)> GetAsync(
	//		string? nameContains = null,
	//		string? signatureContains = null,
	//		int skip = 0,
	//		int take = 25,
	//		string sortBy = "LastName",
	//		bool desc = false,
	//		CancellationToken ct = default)
	//	{
	//		// Base query (no tracking for read-only listing)
	//		IQueryable<FolioHolder> q = _db.Set<FolioHolder>().AsNoTracking();

	//		// ------------------------------
	//		// Filtering (case-insensitive on SQLite using NOCASE collation)
	//		// ------------------------------
	//		if (!string.IsNullOrWhiteSpace(nameContains))
	//		{
	//			var pattern = $"%{nameContains}%";
	//			q = q.Where(x =>
	//				(x.FirstName != null && EF.Functions.Like(EF.Functions.Collate(x.FirstName, "NOCASE"), pattern)) ||
	//				(x.LastName != null && EF.Functions.Like(EF.Functions.Collate(x.LastName, "NOCASE"), pattern))
	//			);
	//		}

	//		if (!string.IsNullOrWhiteSpace(signatureContains))
	//		{
	//			var pattern = $"%{signatureContains}%";
	//			q = q.Where(x =>
	//				x.Signature != null && EF.Functions.Like(EF.Functions.Collate(x.Signature, "NOCASE"), pattern)
	//			);
	//		}

	//		// Total count BEFORE paging
	//		var total = await q.CountAsync(ct);

	//		// ------------------------------
	//		// Sorting
	//		// ------------------------------
	//		// Normalize the incoming sort key
	//		var key = (sortBy ?? "LastName").Trim();

	//		// Apply ordering. Strings use NOCASE; dates/numbers sort directly.
	//		q = (key, desc) switch
	//		{
	//			// FirstName
	//			(nameof(FolioHolder.FirstName), true) =>
	//				q.OrderByDescending(x => EF.Functions.Collate(x.FirstName ?? string.Empty, "NOCASE"))
	//				 .ThenBy(x => x.FolioHolderId),

	//			(nameof(FolioHolder.FirstName), false) =>
	//				q.OrderBy(x => EF.Functions.Collate(x.FirstName ?? string.Empty, "NOCASE"))
	//				 .ThenBy(x => x.FolioHolderId),

	//			// LastName
	//			(nameof(FolioHolder.LastName), true) =>
	//				q.OrderByDescending(x => EF.Functions.Collate(x.LastName ?? string.Empty, "NOCASE"))
	//				 .ThenBy(x => x.FolioHolderId),

	//			(nameof(FolioHolder.LastName), false) =>
	//				q.OrderBy(x => EF.Functions.Collate(x.LastName ?? string.Empty, "NOCASE"))
	//				 .ThenBy(x => x.FolioHolderId),

	//			// Signature
	//			(nameof(FolioHolder.Signature), true) =>
	//				q.OrderByDescending(x => EF.Functions.Collate(x.Signature ?? string.Empty, "NOCASE"))
	//				 .ThenBy(x => x.FolioHolderId),

	//			(nameof(FolioHolder.Signature), false) =>
	//				q.OrderBy(x => EF.Functions.Collate(x.Signature ?? string.Empty, "NOCASE"))
	//				 .ThenBy(x => x.FolioHolderId),

	//			// DateOfBirth (example of non-string)
	//			(nameof(FolioHolder.DateOfBirth), true) =>
	//				q.OrderByDescending(x => x.DateOfBirth)
	//				 .ThenBy(x => x.FolioHolderId),

	//			(nameof(FolioHolder.DateOfBirth), false) =>
	//				q.OrderBy(x => x.DateOfBirth)
	//				 .ThenBy(x => x.FolioHolderId),

	//			// Fallback: LastName ascending (stable)
	//			(_, true) =>
	//				q.OrderByDescending(x => EF.Functions.Collate(x.LastName ?? string.Empty, "NOCASE"))
	//				 .ThenBy(x => x.FolioHolderId),

	//			_ =>
	//				q.OrderBy(x => EF.Functions.Collate(x.LastName ?? string.Empty, "NOCASE"))
	//				 .ThenBy(x => x.FolioHolderId),
	//		};

	//		var items = await q.Skip(Math.Max(0, skip)).Take(Math.Max(1, take)).ToListAsync(ct);
	//		return (items, total);
	//	}

	//	public Task<FolioHolder?> GetByIdAsync(int id, CancellationToken ct = default) =>
	//		_db.Set<FolioHolder>().AsNoTracking().FirstOrDefaultAsync(x => x.FolioHolderId == id, ct);

	//	public async Task<int> AddAsync(FolioHolder entity, CancellationToken ct = default)
	//	{
	//		Normalize(entity);
	//		Validate(entity);

	//		await EnsureUniqueAsync(entity, false, ct);

	//		var now = DateTime.UtcNow;
	//		entity.InDate = now;
	//		entity.UpdateDate = now;

	//		_db.Set<FolioHolder>().Add(entity);
	//		await _db.SaveChangesAsync(ct);
	//		return entity.FolioHolderId;
	//	}

	//	public async Task UpdateAsync(FolioHolder entity, CancellationToken ct = default)
	//	{
	//		var existing = await _db.Set<FolioHolder>().FirstOrDefaultAsync(x => x.FolioHolderId == entity.FolioHolderId, ct);
	//		if (existing is null) return;

	//		Normalize(entity);
	//		Validate(entity);
	//		await EnsureUniqueAsync(entity, true, ct);

	//		existing.FirstName = entity.FirstName;
	//		existing.LastName = entity.LastName;
	//		existing.DateOfBirth = entity.DateOfBirth;
	//		existing.Signature = entity.Signature;
	//		existing.UpdateDate = DateTime.UtcNow;

	//		await _db.SaveChangesAsync(ct);
	//	}

	//	public async Task DeleteAsync(int id, CancellationToken ct = default)
	//	{
	//		var hasFolios = await _db.Set<Folio>().AsNoTracking().AnyAsync(f => f.FolioHolderId == id, ct);
	//		if (hasFolios) throw new InvalidOperationException("Cannot delete holder: folios are linked.");

	//		var row = await _db.Set<FolioHolder>().FindAsync(new object?[] { id }, ct);
	//		if (row is null) return;

	//		_db.Set<FolioHolder>().Remove(row);
	//		await _db.SaveChangesAsync(ct);
	//	}

	//	public async Task<Dictionary<int, int>> GetFolioCountsAsync(CancellationToken ct = default)
	//	{
	//		return await _db.Set<Folio>().AsNoTracking()
	//			.GroupBy(f => f.FolioHolderId)
	//			.Select(g => new { g.Key, Cnt = g.Count() })
	//			.ToDictionaryAsync(x => x.Key, x => x.Cnt, ct);
	//	}
	//	// Helpers
	//	private static void Normalize(FolioHolder e)
	//	{
	//		e.FirstName = (e.FirstName ?? "").Trim();
	//		e.LastName = (e.LastName ?? "").Trim();
	//		e.Signature = (e.Signature ?? "").Trim();
	//	}

	//	private static void Validate(FolioHolder e)
	//	{
	//		if (string.IsNullOrWhiteSpace(e.FirstName)) throw new ArgumentException("First name is required.", nameof(e.FirstName));
	//		if (string.IsNullOrWhiteSpace(e.LastName)) throw new ArgumentException("Last name is required.", nameof(e.LastName));
	//		if (string.IsNullOrWhiteSpace(e.Signature)) throw new ArgumentException("Signature is required.", nameof(e.Signature));
	//		if (e.DateOfBirth.Date >= DateTime.UtcNow.Date) throw new ArgumentOutOfRangeException(nameof(e.DateOfBirth), "Date of Birth must be in the past.");
	//	}

	//	private async Task EnsureUniqueAsync(FolioHolder e, bool isUpdate, CancellationToken ct)
	//	{
	//		var q = _db.Set<FolioHolder>().AsNoTracking().Where(x => !isUpdate || x.FolioHolderId != e.FolioHolderId);

	//		var dupSig = await q.AnyAsync(x => x.Signature != null && EF.Functions.Collate(x.Signature, "NOCASE") == e.Signature, ct);
	//		if (dupSig) throw new InvalidOperationException("Signature already exists.");

	//		var dupPerson = await q.AnyAsync(x =>
	//			EF.Functions.Collate(x.FirstName, "NOCASE") == e.FirstName &&
	//			EF.Functions.Collate(x.LastName, "NOCASE") == e.LastName &&
	//			x.DateOfBirth.Date == e.DateOfBirth.Date, ct);
	//		if (dupPerson) throw new InvalidOperationException("A holder with same name and DOB already exists.");
	//	}
	//}


		public interface IFolioHolderRepo
		{
			Task<(IReadOnlyList<FolioHolder> Items, int TotalCount)> GetAsync(
				string? search,
				string? sortBy,
				bool sortDesc,
				int pageNumber,
				int pageSize);

			Task<FolioHolder?> GetByIdAsync(int id);

			Task<bool> ExistsBySignatureAsync(string signature, int? excludeId = null);

			/// <summary>
			/// Checks uniqueness of (FirstName + LastName + DateOfBirth)
			/// </summary>
			Task<bool> EnsureUniqueAsync(FolioHolder entity, bool isUpdate, CancellationToken ct = default);

			Task<FolioHolder> AddAsync(FolioHolder entity);
			Task<FolioHolder> UpdateAsync(FolioHolder entity);
			Task DeleteAsync(int id);
			Task<int> CountAsync();
		}

		public class FolioHolderRepo : IFolioHolderRepo
		{
			private readonly MFPTrackerDbContext _db;

			public FolioHolderRepo(MFPTrackerDbContext db)
			{
				_db = db;
			}

			// ---------------------------------------------------------
			// GET WITH SEARCH + SORT + PAGING
			// ---------------------------------------------------------
			public async Task<(IReadOnlyList<FolioHolder> Items, int TotalCount)> GetAsync(
				string? search,
				string? sortBy,
				bool sortDesc,
				int pageNumber,
				int pageSize)
			{
				if (pageNumber < 1) pageNumber = 1;
				if (pageSize < 1) pageSize = 10;
				if (pageSize > 200) pageSize = 200;

			//IQueryable<FolioHolder> q = _db.Set<FolioHolder>().AsNoTracking();
			IQueryable<FolioHolder> q = _db.Set<FolioHolder>().Include(x => x.Folios).AsNoTracking();

			// ---------------------- Filtering ----------------------
			if (!string.IsNullOrWhiteSpace(search))
				{
					var term = search.Trim();

					static string Esc(string s) =>
						s.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

					var pattern = $"%{Esc(term)}%";

					q = q.Where(x =>
						EF.Functions.Like(EF.Functions.Collate(x.FirstName, "NOCASE"), pattern) ||
						EF.Functions.Like(EF.Functions.Collate(x.LastName, "NOCASE"), pattern) ||
						EF.Functions.Like(EF.Functions.Collate(x.Signature, "NOCASE"), pattern)
					);
				}

				// ---------------------- Sorting ------------------------
				var key = (sortBy ?? nameof(FolioHolder.LastName)).Trim();

				q = (key, sortDesc) switch
				{
					// FirstName
					(nameof(FolioHolder.FirstName), false) =>
						q.OrderBy(x => EF.Functions.Collate(x.FirstName, "NOCASE"))
						 .ThenBy(x => x.FolioHolderId),

					(nameof(FolioHolder.FirstName), true) =>
						q.OrderByDescending(x => EF.Functions.Collate(x.FirstName, "NOCASE"))
						 .ThenBy(x => x.FolioHolderId),

					// LastName
					(nameof(FolioHolder.LastName), false) =>
						q.OrderBy(x => EF.Functions.Collate(x.LastName, "NOCASE"))
						 .ThenBy(x => x.FolioHolderId),

					(nameof(FolioHolder.LastName), true) =>
						q.OrderByDescending(x => EF.Functions.Collate(x.LastName, "NOCASE"))
						 .ThenBy(x => x.FolioHolderId),

					// Date of Birth
					(nameof(FolioHolder.DateOfBirth), false) =>
						q.OrderBy(x => x.DateOfBirth).ThenBy(x => x.FolioHolderId),

					(nameof(FolioHolder.DateOfBirth), true) =>
						q.OrderByDescending(x => x.DateOfBirth).ThenBy(x => x.FolioHolderId),

					// Signature
					(nameof(FolioHolder.Signature), false) =>
						q.OrderBy(x => EF.Functions.Collate(x.Signature, "NOCASE"))
						 .ThenBy(x => x.FolioHolderId),

					(nameof(FolioHolder.Signature), true) =>
						q.OrderByDescending(x => EF.Functions.Collate(x.Signature, "NOCASE"))
						 .ThenBy(x => x.FolioHolderId),

					// Default: LastName ASC
					_ =>
						q.OrderBy(x => EF.Functions.Collate(x.LastName, "NOCASE"))
						 .ThenBy(x => x.FolioHolderId)
				};

				// Count + paging
				var total = await q.CountAsync();
				var items = await q
					.Skip((pageNumber - 1) * pageSize)
					.Take(pageSize)
					.ToListAsync();

				return (items, total);
			}

			// ---------------------------------------------------------
			public Task<int> CountAsync() =>
				_db.Set<FolioHolder>().CountAsync();

			// ---------------------------------------------------------
			public async Task<FolioHolder?> GetByIdAsync(int id)
			{
				return await _db.Set<FolioHolder>()
					.Include(x => x.Folios) // if you need folios loaded
					.FirstOrDefaultAsync(x => x.FolioHolderId == id);
			}

			// ---------------------------------------------------------
			// SIGNATURE uniqueness check
			// ---------------------------------------------------------
			public async Task<bool> ExistsBySignatureAsync(string signature, int? excludeId = null)
			{
				if (string.IsNullOrWhiteSpace(signature))
					return false;

				var query = _db.Set<FolioHolder>().AsQueryable();

				if (excludeId.HasValue)
					query = query.Where(x => x.FolioHolderId != excludeId.Value);

				return await query.AnyAsync(
					x => EF.Functions.Collate(x.Signature, "NOCASE") == signature.Trim()
				);
			}

			// ---------------------------------------------------------
			// FirstName + LastName + DOB uniqueness check
			// ---------------------------------------------------------
			public async Task<bool> EnsureUniqueAsync(FolioHolder e, bool isUpdate, CancellationToken ct = default)
			{
				var q = _db.Set<FolioHolder>()
						   .AsNoTracking()
						   .Where(x => !isUpdate || x.FolioHolderId != e.FolioHolderId);

				return await q.AnyAsync(x =>
					EF.Functions.Collate(x.FirstName, "NOCASE") == e.FirstName &&
					EF.Functions.Collate(x.LastName, "NOCASE") == e.LastName &&
					x.DateOfBirth.Date == e.DateOfBirth.Date,
					ct
				);
			}

			// ---------------------------------------------------------
			// ADD
			// ---------------------------------------------------------
			public async Task<FolioHolder> AddAsync(FolioHolder entity)
			{
				entity.InDate = DateTime.UtcNow;
				entity.UpdateDate = DateTime.UtcNow;

				_db.Set<FolioHolder>().Add(entity);
				await _db.SaveChangesAsync();
				return entity;
			}

			// ---------------------------------------------------------
			// UPDATE
			// ---------------------------------------------------------
			public async Task<FolioHolder> UpdateAsync(FolioHolder entity)
			{
				var existing = await _db.Set<FolioHolder>()
					.FirstOrDefaultAsync(x => x.FolioHolderId == entity.FolioHolderId);

				if (existing == null)
					throw new InvalidOperationException($"FolioHolder id {entity.FolioHolderId} not found.");

				existing.FirstName = entity.FirstName;
				existing.LastName = entity.LastName;
				existing.DateOfBirth = entity.DateOfBirth;
				existing.Signature = entity.Signature;

				existing.UpdateDate = DateTime.UtcNow;

				await _db.SaveChangesAsync();
				return existing;
			}

			// ---------------------------------------------------------
			// DELETE
			// ---------------------------------------------------------
			public async Task DeleteAsync(int id)
			{
				var existing = await _db.Set<FolioHolder>()
					.FirstOrDefaultAsync(x => x.FolioHolderId == id);

				if (existing == null)
					return;

				_db.Set<FolioHolder>().Remove(existing);
				await _db.SaveChangesAsync();
			}
		}
	}
