/*

This class was initially based on code by Stefan Delmarco, http://www.fotia.co.uk/

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
using System.Data;
using System.Data.SqlClient;

namespace Common.Helpers {
	public class VarBinaryHelper : IDisposable {
		private readonly SqlConnection m_connection;
		public bool ColumnIsNull { get; private set; }

		private readonly SqlCommand m_readCommand;
		private readonly SqlCommand m_writeCommand;
		private readonly SqlCommand m_initializeCommand;

		public VarBinaryHelper(SqlConnection connection, string tableName, string columnName, string whereClause) {
			ArgumentValidator.ThrowIfNullOrEmpty(tableName, "tableName");
			ArgumentValidator.ThrowIfNullOrEmpty(columnName, "columnName");
			ArgumentValidator.ThrowIfNullOrEmpty(whereClause, "whereClause");
			if (connection.State != ConnectionState.Open) {
				throw new ArgumentException("The connection must be open", "connection");
			}

			this.m_connection = connection;
			this.Length = GetLength(tableName, columnName, whereClause);

			this.m_readCommand = CreateReadCommand(tableName, columnName, whereClause);
			this.m_writeCommand = CreateWriteCommand(tableName, columnName, whereClause);
			this.m_initializeCommand = CreateInitializeCommand(tableName, columnName, whereClause);
		}

		public long Length { private set; get;}

		private SqlCommand CreateReadCommand(string tableName, string columnName, string whereClause) {
			var readCommand = this.m_connection.CreateCommand();
			readCommand.CommandText = @"
SELECT substring([{0}], @offset, @length)
FROM [{1}]
WHERE {2}".Fi(columnName, tableName, whereClause);

			readCommand.Parameters.Add("@length", SqlDbType.BigInt);
			readCommand.Parameters.Add("@offset", SqlDbType.BigInt);

            return readCommand;
		}

		private SqlCommand CreateWriteCommand(string tableName, string columnName, string whereClause) {
			var writecommand = this.m_connection.CreateCommand();
			writecommand.CommandText = @"
UPDATE {0}
SET {1}.write(@buffer, @offset, @length)
WHERE {2}".Fi(tableName, columnName, whereClause);

			writecommand.Parameters.Add("@offset", SqlDbType.BigInt);
			writecommand.Parameters.Add("@length", SqlDbType.BigInt);
			writecommand.Parameters.Add("@buffer", SqlDbType.VarBinary);

			return writecommand;
		}

		private SqlCommand CreateInitializeCommand(string tableName, string columnName, string whereClause) {
			var initializeCommand = this.m_connection.CreateCommand();
			initializeCommand.CommandText = @"UPDATE {0} SET {1} = 0x WHERE {2}".Fi(tableName, columnName, whereClause);
			return initializeCommand;
		}
        
		private long GetLength(string tableName, string columnName, string whereClause) {
			using (var command = this.m_connection.CreateCommand()) {
				command.CommandText = string.Format(@"
SELECT @length = CAST(datalength({0}) as bigint), @exists = 1
FROM {1}
WHERE {2}", columnName, tableName, whereClause);

				var length = command.Parameters.Add("@length", SqlDbType.BigInt);
				length.Direction = ParameterDirection.Output;

				var exists = command.Parameters.Add("@exists", SqlDbType.Int);
				exists.Direction = ParameterDirection.Output;

				command.ExecuteNonQuery();

				if (DBNull.Value == exists.Value) {
					throw new ArgumentException("No row satisfies WHERE clause: '{0}'".Fi(whereClause));
				}

				this.ColumnIsNull = (length.Value == DBNull.Value);
				return this.ColumnIsNull ? 0 : (long) length.Value;
			}
		}

		public byte[] Read(long offset, long length) {
			if (this.ColumnIsNull) {
				return new byte[0];
			}

			// substring is 1-based.
			this.m_readCommand.Parameters["@offset"].Value = offset + 1;
			this.m_readCommand.Parameters["@length"].Value = length;
			this.m_readCommand.ExecuteScalar();

			return (byte[]) this.m_readCommand.ExecuteScalar();
		}

		public void Write(byte[] buffer, long offset) {
			this.Write(buffer, buffer.Length, offset, buffer.Length);
		}

		public void Write(byte[] buffer, long offset, long length) {
			this.Write(buffer, buffer.Length, offset, length);
		}

		public void Write(byte[] buffer, int cntToWrite, long offset, long length) {
			ArgumentValidator.ThrowIfNull(buffer, "buffer");

			// We can't take 0 for cntToWrite because the SqlCommand will interpret the 0 as no limit on the varbinary array
			if (cntToWrite <= 0 || cntToWrite > buffer.Length) {
				var msg = "cntToWrite must be between 1 and the length of buffer ({0} in this case), but it was {1}".Fi(buffer.Length, cntToWrite);
				throw new ArgumentOutOfRangeException(msg);
			}

			if(this.ColumnIsNull) {
				this.m_initializeCommand.ExecuteNonQuery();
				this.ColumnIsNull = false;
			}

			this.m_writeCommand.Parameters["@buffer"].Value = buffer;
			this.m_writeCommand.Parameters["@offset"].Value = offset;
			this.m_writeCommand.Parameters["@length"].Value = length;
            this.m_writeCommand.Parameters["@buffer"].Size = cntToWrite;

			this.m_writeCommand.ExecuteNonQuery();

            this.Length = Math.Max(this.Length, offset + cntToWrite);
			if (length > cntToWrite) {
				var shrinkAmount = ((length - cntToWrite) - (offset + length - this.Length));
				this.Length -= shrinkAmount;
			}
		}

		public void Dispose() {
			this.m_readCommand.Dispose();
			this.m_writeCommand.Dispose();
			this.m_initializeCommand.Dispose();
			this.m_connection.Dispose();
		}
	}
}