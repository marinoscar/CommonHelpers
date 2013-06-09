/*

Copyright (C) 2007-2011 by Gustavo Duarte and Bernardo Vieira.
 
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 
*/
using System;
using System.Data.SqlClient;
using System.Linq;

namespace Common.Helpers {
	/// <summary>
	/// Represents SQL Error Codes
	/// </summary>
	public static class SqlErrorCodes {
		public static int[] InternalServerErrors = new[] {
			556, // An error occurred while writing an audit trace. SQL Server is shutting down. Check and correct error conditions such as insufficient disk space, and then restart SQL Server. (...)
			674, //Exception occurred in destructor of RowsetNewSS 0x%p. This error may indicate a problem related to releasing pre-allocated disk blocks used during bulk-insert operations. Restart the server to resolve this problem.
			708, // Server is running low on virtual address space or machine is running low on virtual memory. Reserved memory used %d times since startup. Cancel query and re-run, decrease server load, or cancel other applications. 
			844, // Time out occurred while waiting for buffer latch -- type %d, bp %p, page %d:%d, stat %#x, database id: %d, allocation unit id: %I64d%ls, task 0x%p : %d, waittime %d, flags 0x%I64x, owning task 0x%p.  Continuing to wait.
			847, // Timeout occurred while waiting for latch: class '%ls', id %p, type %d, Task 0x%p : %d, waittime %d, flags 0x%I64x, owning task 0x%p. Continuing to wait.
			945, // Database '%.*ls' cannot be opened due to inaccessible files or insufficient memory or disk space.  See the SQL Server errorlog for details.
			947, // Error while closing database '%.*ls'. Check for previous additional errors and retry the operation.
			1101, // Could not allocate a new page for database '%.*ls' because of insufficient disk space in filegroup '%.*ls'. Create the necessary space by dropping objects in the filegroup, adding additional files to the filegroup, or setting autogrowth on for existing fi
			1102, // IAM page %S_PGID for object ID %ld is incorrect. The %S_MSG ID on page is %ld; should be %ld. The entry in sysindexes may be incorrect or the IAM page may contain an error.
			1105, // Could not allocate space for object '%.*ls'%.*ls in database '%.*ls' because the '%.*ls' filegroup is full. Create disk space by deleting unneeded files, dropping objects in the filegroup, adding additional files to the filegroup, or setting autogrowth on
			1121, // Space allocator cannot allocate page in database %d.
			1122, // Table error: Page %S_PGID. Test (%hs) failed. Address 0x%x is not aligned.
			1123, // Table error: Page %S_PGID. Unexpected page type %d.
			1124, // Table error: Page %S_PGID. Test (%hs) failed. Slot %d, offset 0x%x is invalid.
			1125, // Table error: Page %S_PGID. Test (%hs) failed. Slot %d, row extends into free space at 0x%x.
			1126, // Table error: Page %S_PGID. Test (%hs) failed. Slot %d, offset 0x%x overlaps with the prior row.
			1127, // Table error: Page %S_PGID. Test (%hs) failed. Values are %ld and %ld.
			1128, // Table error: Page %S_PGID, row %d. Test (%hs) failed. Values are %ld and %ld.
			1129, // Could not cleanup deferred deallocations from filegroup '%.*ls'.
			1130, // Error while allocating extent for a worktable.  Extent %S_PGID in TEMPDB may have been lost.
			1203, // Process ID %d attempted to unlock a resource it does not own: %.*ls. Retry the transaction, because this error may be caused by a timing condition. If the problem persists, contact the database administrator.
			1204, // The instance of the SQL Server Database Engine cannot obtain a LOCK resource at this time. Rerun your statement when there are fewer active users. Ask the database administrator to check the lock and memory configuration for this instance, or to check for
			1221, // The Database Engine is attempting to release a group of locks that are not currently held by the transaction. Retry the transaction. If the problem persists, contact your support provider.
			1501, // Sort failure. Contact Technical Support.
			1509, // Row comparison failed during sort because of an unknown data type on a key column. Metadata might be corrupt. Contact Technical Support.
			1510, // Sort failed. Out of space or locks in database '%.*ls'.
			1511, // Sort cannot be reconciled with transaction log.
			1522, // Sort operation failed during an index build. The overwriting of the allocation page in database '%.*ls' was prevented by terminating the sort. Run DBCC CHECKDB to check for allocation and consistency errors. It may be necessary restore the database from b
			1523, // Sort failure. The incorrect extent could not be deallocated. Contact Technical Support.
			1528, // Character data comparison failure. An unrecognized Sort-Map-Element type (%d) was found in the server-wide default sort table at SMEL entry [%d].
			1529, // Character data comparison failure. A list of Sort-Map-Elements from the server-wide default sort table does not end properly. This list begins at SMEL entry [%d].
			1532, // New sort run starting on page %S_PGID found an extent not marked as shared. Retry the transaction. If the problem persists, contact Technical Support.
			1533, // Cannot share extent %S_PGID. The correct extents could not be identified. Retry the transaction.
			1534, // Extent %S_PGID not found in shared extent directory. Retry the transaction. If the problem persists, contact Technical Support.
			1535, // Cannot share extent %S_PGID. Shared extent directory is full. Retry the transaction. If the problem persists, contact Technical Support.
			1543, // Operating system error '%ls' resulted from attempt to read the following: sort run page %S_PGID, in file '%ls', in database with ID %d. Sort is retrying the read.
			1814, // Could not create tempdb. You may not have enough disk space available. Free additional disk space by deleting other files on the tempdb drive and then restart SQL Server. Check for additional errors in the event log that may indicate why the tempdb files 
			2502, // DBCC MEMOBJLIST failed due to temporary inconsistency in PMO structure. Please try again.
			2533, // Table error: page %S_PGID allocated to object ID %d, index ID %d, partition ID %I64d, alloc unit ID %I64d (type %.*ls) was not seen. The page may be invalid or may have an incorrect alloc unit ID in its header.
			2534, // Table error: page %S_PGID, whose header indicates that it is allocated to object ID %d, index ID %d, partition ID %I64d, alloc unit ID %I64d (type %.*ls), is allocated by another object.
			2537, // Table error: object ID %d, index ID %d, partition ID %I64d, alloc unit ID %I64d (type %.*ls), page %S_PGID, row %d. The record check (%hs) failed. The values are %ld and %ld.
			2540, // The system cannot self repair this error.
			2556, // There is insufficient space in the filegroup to complete the emptyfile operation.
			3140, // Could not adjust the space allocation for file '%ls'.
			3230, // Operation on device '%ls' exceeded retry count.
			3257, // There is insufficient free space on disk volume '%ls' to create the database. The database requires %I64u additional free bytes, while only %I64u bytes are available.
			3619, // Could not write a checkpoint record in database ID %d because the log is out of space. Contact the database administrator to truncate the log or allocate more space to the database log files.
			3620, // Automatic checkpointing is disabled in database '%.*ls' because the log is out of space. Automatic checkpointing will be enabled when the database owner successfully checkpoints the database. Contact the database owner to either truncate the log file or a
			3635, // An error occurred while processing '%ls' metadata for database id %d, file id %d, and transaction='%.*ls'. Additional Context='%ls'. Location='%hs'(%d). Retry the operation; if the problem persists, contact the database administrator to review locking and
			3953, // Snapshot isolation transaction failed in database '%.*ls' because the database was not recovered when the current transaction was started. Retry the transaction after the database has recovered.
			3957, // Snapshot isolation transaction failed in database '%.*ls' because the database did not allow snapshot isolation when the current transaction started. It may help to retry the transaction.
			3958, // Transaction aborted when accessing versioned row in table '%.*ls' in database '%.*ls'. Requested versioned row was not found. Your tempdb is probably out of space. Please refer to BOL on how to configure tempdb for versioning.
			3966, // Transaction is rolled back when accessing version store. It was earlier marked as victim when the version store was shrunk due to insufficient space in tempdb. This transaction was marked as a victim earlier because it may need the row version(s) that hav
			3967, // Insufficient space in tempdb to hold row versions.  Need to shrink the version store to free up some space in tempdb. Transaction (id=%I64d xsn=%I64d spid=%d elapsed_time=%d) has been marked as victim and it will be rolled back if it accesses the version 
			3973, // The database is currently being used by another thread under the same workspace in exclusive mode. The operation failed.
			3974, // The number of databases in exclusive mode usage under a workspace is limited. Because the limit has been exceeded, the operation failed.
			3984, // Cannot acquire a database lock during a transaction change.
			5128, // Write to sparse file '%ls' failed due to lack of disk space.
			5231, // Object ID %ld (object '%.*ls'): A deadlock occurred while trying to lock this object for checking. This object has been skipped and will not be processed.
			5245, // DBCC could not obtain a lock on object %ld because the lock request timeout period was exceeded.
			5249, // %.*ls: Page %d:%d could not be moved because shrink could not lock the page. 
			5252, // File ID %d of database ID %d cannot be shrunk to the expected size. The high concurrent workload is leading to too many deadlocks during the shrink operation.   Re-run the shrink operation when the workload is lower.
			5904, // Unable to issue checkpoint: there are not enough locks available. Background checkpoint process will remain suspended until locks are available. To free up locks, list transactions and their locks, and terminate transactions with the highest number of loc
			6292, // The transaction that is associated with this operation has been committed or rolled back. Retry with a different transaction.
			7151, // Insufficient buffer space to perform write operation.
			7622, // There is not sufficient disk space to complete this operation for the full-text catalog "%ls".
			7644, // Full-text crawl manager has not been initialized. Any crawl started before the crawl manager was fully initialized will need to be restarted. Please restart SQL Server and retry the command. You should also check the error log to fix any failures that mig
			7695, // Operation failed. Full-text catalog backup in progress. Retry after backup operation has completed.
			7926, // Check statement aborted. The database could not be checked as a database snapshot could not be created and the database or table could not be locked. See Books Online for details of when this behavior is expected and what workarounds exist. Also see previ
			8311, // Unable to map view of file mapping object '%ls' into SQL Server process address space. SQL Server performance counters are disabled.
			8914, // Incorrect PFS free space information for page %S_PGID in object ID %d, index ID %d, partition ID %I64d, alloc unit ID %I64d (type %.*ls). Expected value %hs, actual value %hs.
			8921, // Check terminated. A failure was detected while collecting facts. Possibly tempdb out of space or a system table is inconsistent. Check previous errors.
			8943, // Table error: Object ID %d, index ID %d, partition ID %I64d, alloc unit ID %I64d (type %.*ls), page %S_PGID. Test (%hs) failed. Slot %d, row extends into free space at 0x%x.
			9002, // The transaction log for database '%.*ls' is full. To find out why space in the log cannot be reused, see the log_reuse_wait_desc column in sys.databases
			17207, // %ls: Operating system error %ls occurred while creating or opening file '%ls'. Diagnose and correct the operating system error, and retry the operation.
			17888, // All schedulers on Node %d appear deadlocked due to a large number of worker threads waiting on %ls. Process Utilization %d%%.
			18812, // Can not lock the database object in article cache.
			18819, // Failed to lock current log record at LSN {%08lx:%08lx:%04lx}.
			18834, // Unexpected TIB log record encountered while processing TI block for offset %ld, last TIB processed : (textInfoFlags 0x%x, coloffset %ld, newSize %I64d, oldSize %I64d).
			20041, // Transaction rolled back. Could not execute trigger. Retry your transaction.
			21413, // Failed to acquire the application lock indicating the front of the queue.
			21414, // Unexpected failure acquiring application lock.
			21415, // Unexpected failure releasing application lock.
		};

		public static int[] TemporarySqlErrors = new[] {
			601, // Could not continue scan with NOLOCK due to data movement.
			847, // Timeout occurred while waiting for latch: class '%ls', id %p, type %d, Task 0x%p : %d, waittime %d, flags 0x%I64x, owning task 0x%p. Continuing to wait.
			1205, // Transaction (Process ID %d) was deadlocked on %.*ls resources with another process and has been chosen as the deadlock victim. Rerun the transaction.
			1206, // The Microsoft Distributed Transaction Coordinator (MS DTC) has cancelled the distributed transaction.
			1220, // No more lock classes available from transaction.
			1222, // Lock request time out period exceeded.
			1421, // Communications to the remote server instance '%.*ls' failed to complete before its timeout. The ALTER DATABASE command may have not completed. Retry the command.
			1807, // Could not obtain exclusive lock on database '%.*ls'. Retry the operation later.
			3928, // The marked transaction '%.*ls' failed. A Deadlock was encountered while attempting to place the mark in the log.
			5030, // The database could not be exclusively locked to perform the operation.
			5061, // ALTER DATABASE failed because a lock could not be placed on database '%.*ls'. Try again later.
			7604, // Full-text operation failed due to a time out.
			8628, // A time out occurred while waiting to optimize the query. Rerun the query.
			8645, // A time out occurred while waiting for memory resources to execute the query. Rerun the query.
			14355, // The MSSQLServerADHelper service is busy. Retry this operation later.
			17197, // Login failed due to timeout; the connection has been closed. This error may indicate heavy server load. Reduce the load on the server and retry login.%.*ls
			17830, // Network error code 0x%x occurred while establishing a connection; the connection has been closed. This may have been caused by client or server login timeout expiration. Time spent during login: total %d ms, enqueued %d ms, network writes %d ms, network r
			17889, // A new connection was rejected because the maximum number of connections on session ID %d has been reached. Close an existing connection on this session and retry.%.*ls
			18486, // Login failed for user '%.*ls' because the account is currently locked out. The system administrator can unlock it. %.*ls
		};

		public static int[] UniquenessViolationErrors = new[] { 2601, 2627 };

		public static ExceptionInformation AnalyzeException(Exception ex) {
			ArgumentValidator.ThrowIfNull(ex, "ex");

			var sqlException = (ex as SqlException);
			if (null == sqlException) {
				return null;
			}

			var error = (uint) sqlException.ErrorCode;

			// Error codes <= 32 come from DB lib and are related to connection problems
			if (error <= 32 || InternalServerErrors.Contains(sqlException.Number) || TemporarySqlErrors.Contains(sqlException.Number)) {
				return ExceptionInformation.Retry;
			}

			return ExceptionInformation.NoRetry;
		}

		public static bool IsUniquenessViolation(SqlException ex) {
			ArgumentValidator.ThrowIfNull(ex, "ex");

			return UniquenessViolationErrors.Contains(ex.Number);
		}
	}
}
