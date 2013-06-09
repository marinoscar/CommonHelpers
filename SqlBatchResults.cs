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
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Common.Helpers {
	public class SqlBatchResults {
		public List<SqlError> SqlInfoMessages { get; private set; }
		public List<SqlResultSet> ResultSets { get; private set; }

		public SqlBatchResults(SqlCommand commandToBeExecutedAsDataReader) {
			ArgumentValidator.ThrowIfNull(commandToBeExecutedAsDataReader, "commandToBeExecutedAsDataReader");

			this.SqlInfoMessages = new List<SqlError>();
			this.ResultSets = new List<SqlResultSet>();

			commandToBeExecutedAsDataReader.Connection.InfoMessage += this.AddInfoMessage;
			
			try {
				using (SqlDataReader dr = commandToBeExecutedAsDataReader.ExecuteReader()) {
					do {
						this.ResultSets.Add(new SqlResultSet(dr));
					} while (dr.NextResult());
				}				
			} finally {
				commandToBeExecutedAsDataReader.Connection.InfoMessage -= this.AddInfoMessage;	
			}
		}

		// add it up, add it up, you gotta add it up...
		internal void AddInfoMessage(object sender, SqlInfoMessageEventArgs e) {
			SqlInfoMessages.AddRange(e.Errors.OfType<SqlError>());
		}
	}
}
