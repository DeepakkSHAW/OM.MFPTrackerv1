using Microsoft.EntityFrameworkCore;
using OM.MFPTrackerV1.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OM.MFPTrackerV1.Data.Services
{
	public interface IFolioHolderRepo
	{
		/// <summary>
		/// Case-insensitive filtering by name (first/last contains) and signature (contains),
		/// with server-side paging.
		/// </summary>
		Task<(IReadOnlyList<FolioHolder> Items, int TotalCount)> GetAsync(string? nameContains = null, string? signatureContains = null, int skip = 0, int take = 20, CancellationToken ct = default);
		Task<List<FolioHolder>> GetAllAsync(CancellationToken cancellationToken = default);
		Task<FolioHolder?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
		Task<int> AddAsync(FolioHolder entity, CancellationToken ct = default);
		Task UpdateAsync(FolioHolder person, CancellationToken cancellationToken = default);
		Task DeleteAsync(int id, CancellationToken cancellationToken = default);
	}
	public class FolioHolderRepo : IFolioHolderRepo
	{
		private readonly MFPTrackerDbContext _db;
		public FolioHolderRepo(MFPTrackerDbContext db) => _db = db;
		// Small shared validator for required fields and DOB
		private static void ValidateRequired(FolioHolder e)
		{
			if (string.IsNullOrWhiteSpace(e.FirstName))
				throw new ArgumentException("First name is required.", nameof(e.FirstName));
			if (string.IsNullOrWhiteSpace(e.LastName))
				throw new ArgumentException("Last name is required.", nameof(e.LastName));
			if (string.IsNullOrWhiteSpace(e.Signature))
				throw new ArgumentException("Signature is required.", nameof(e.Signature));

			// Your attribute enforces "in the past"; this is a server-side guard too.
			if (e.DateOfBirth.Date >= DateTime.UtcNow.Date)
				throw new ArgumentOutOfRangeException(nameof(e.DateOfBirth), "Date of Birth must be in the past.");
		}
		public async Task<(IReadOnlyList<FolioHolder> Items, int TotalCount)> GetAsync(string? nameContains = null, string? signatureContains = null, int skip = 0, int take = 20,
			CancellationToken ct = default)
		{
			var q = _db.Set<FolioHolder>().AsNoTracking();

			// ---- Case-insensitive filters (SQLite NOCASE) ----
			if (!string.IsNullOrWhiteSpace(nameContains))
			{
				var pattern = $"%{nameContains}%";
				q = q.Where(x =>
					(x.FirstName != null && EF.Functions.Like(EF.Functions.Collate(x.FirstName, "NOCASE"), pattern)) ||
					(x.LastName != null && EF.Functions.Like(EF.Functions.Collate(x.LastName, "NOCASE"), pattern))
				);
			}

			if (!string.IsNullOrWhiteSpace(signatureContains))
			{
				var pattern = $"%{signatureContains}%";
				q = q.Where(x => x.Signature != null &&
								 EF.Functions.Like(EF.Functions.Collate(x.Signature, "NOCASE"), pattern));
			}

			var total = await q.CountAsync(ct);

			// Case-insensitive, stable ordering
			q = q.OrderBy(x => EF.Functions.Collate(x.LastName, "NOCASE"))
				 .ThenBy(x => EF.Functions.Collate(x.FirstName, "NOCASE"))
				 .ThenBy(x => x.FolioHolderId);

			var items = await q.Skip(Math.Max(skip, 0))
							   .Take(Math.Max(take, 1))
							   .ToListAsync(ct);

			return (items, total);
		}

		// READ: All
		public async Task<List<FolioHolder>> GetAllAsync(CancellationToken cancellationToken = default)
		{
			return await _db.FolioHolders
							.AsNoTracking()
							.ToListAsync(cancellationToken);
		}

		// READ: By Id
		public Task<FolioHolder?> GetByIdAsync(int id, CancellationToken ct = default) =>
			_db.Set<FolioHolder>()
	   .AsNoTracking()
	   .FirstOrDefaultAsync(x => x.FolioHolderId == id, ct);
		// CREATE
		public async Task<int> AddAsync(FolioHolder entity, CancellationToken ct = default)
		{
			ValidateRequired(entity);

			// Unique: Signature (case-insensitive)
			var dupSig = await _db.Set<FolioHolder>()
				.AsNoTracking()
				.AnyAsync(x => x.Signature != null &&
							   EF.Functions.Collate(x.Signature, "NOCASE") == entity.Signature, ct);
			if (dupSig)
				throw new InvalidOperationException("Signature already exists. Please choose a unique signature.");

			// Unique: First+Last+DOB (names case-insensitive)
			var dupPerson = await _db.Set<FolioHolder>()
				.AsNoTracking()
				.AnyAsync(x =>
					EF.Functions.Collate(x.FirstName, "NOCASE") == entity.FirstName &&
					EF.Functions.Collate(x.LastName, "NOCASE") == entity.LastName &&
					x.DateOfBirth.Date == entity.DateOfBirth.Date, ct);
			if (dupPerson)
				throw new InvalidOperationException("A folio holder with the same name and date of birth already exists.");

			var now = DateTime.UtcNow;
			entity.InDate = now;
			entity.UpdateDate = now;

			_db.Set<FolioHolder>().Add(entity);
			await _db.SaveChangesAsync(ct);
			return entity.FolioHolderId;
		}
		// UPDATE
		public async Task UpdateAsync(FolioHolder entity, CancellationToken ct = default)
		{
			// Load the tracked row. This avoids "instance already tracked" errors.
			var existing = await _db.Set<FolioHolder>()
									.FirstOrDefaultAsync(x => x.FolioHolderId == entity.FolioHolderId, ct);
			if (existing is null) return; // or throw new KeyNotFoundException(...);

			// Minimal domain validation (complements DataAnnotations)
			ValidateRequired(entity);

			// ---- Uniqueness checks (EXCLUDE self) ----
			// Unique: Signature (case-insensitive)
			if (!string.IsNullOrWhiteSpace(entity.Signature))
			{
				var dupSig = await _db.Set<FolioHolder>()
					.AsNoTracking()
					.AnyAsync(x =>
						x.FolioHolderId != entity.FolioHolderId &&
						x.Signature != null &&
						EF.Functions.Collate(x.Signature, "NOCASE") == entity.Signature,
						ct);

				if (dupSig)
					throw new InvalidOperationException("Signature already exists. Please choose a unique signature.");
			}

			// Unique: the same person (FirstName + LastName + DateOfBirth), names case-insensitive
			var dupPerson = await _db.Set<FolioHolder>()
				.AsNoTracking()
				.AnyAsync(x =>
					x.FolioHolderId != entity.FolioHolderId &&
					EF.Functions.Collate(x.FirstName, "NOCASE") == entity.FirstName &&
					EF.Functions.Collate(x.LastName, "NOCASE") == entity.LastName &&
					x.DateOfBirth.Date == entity.DateOfBirth.Date, ct);

			if (dupPerson)
				throw new InvalidOperationException("A folio holder with the same name and date of birth already exists.");

			// ---- Map fields (do not change InDate; it's the creation timestamp) ----
			existing.FirstName = entity.FirstName.Trim();
			existing.LastName = entity.LastName.Trim();
			existing.DateOfBirth = entity.DateOfBirth;  // assumed valid (in the past)
			existing.Signature = entity.Signature.Trim();
			// Keep existing.InDate as is
			existing.UpdateDate = DateTime.UtcNow;

			await _db.SaveChangesAsync(ct);
		}
		// DELETE
		public async Task DeleteAsync(int id, CancellationToken ct = default)
		{
			var row = await _db.Set<FolioHolder>().FindAsync(new object?[] { id }, ct);
			if (row is null) return;

			_db.Set<FolioHolder>().Remove(row);
			await _db.SaveChangesAsync(ct);
		}
	}
}
