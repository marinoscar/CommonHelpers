using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public class SqlServerStatement : SqlStatement
    {

        public SqlServerStatement():base(DatabaseProviderType.SqlServer){ } 

        public override string GetUtcDate()
        {
            return "SELECT GETUTCDATE()";
        }

        public override string GetDate()
        {
            return "SELECT GETDATE()";
        }

        public override string GetUtcOffset()
        {
            return "SELECT DATEDIFF(mi, GETUTCDATE(), GETDATE())";
        }

        public override string GetTopStatement(int recordCount)
        {
            return "TOP {0}".Fi(recordCount);
        }

        public override string GetCharIndexFunction(string substring, string stringValue, int startIndex)
        {
            return "CHARINDEX({0}, {1}, {2})".Fi(substring, stringValue, startIndex);
        }
    }
}
