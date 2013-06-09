using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Helpers
{
    public class MySqlStatement : SqlStatement
    {
        public MySqlStatement()
            : base(DatabaseProviderType.MySql)
        { }

        public override string GetUtcDate()
        {
            return "SELECT UTC_TIMESTAMP()";
        }

        public override string GetDate()
        {
            return "SELECT NOW()";
        }
        
        public override string GetUtcOffset()
        {
            return "SELECT TIMESTAMPDIFF(MINUTE, UTC_TIMESTAMP(), NOW())";
        }

        public override string GetTopStatement(int recordCount)
        {
            return "LIMIT {0}".Fi(recordCount);
        }

        public override string GetCharIndexFunction(string substring, string stringValue, int startIndex)
        {
            return "LOCATE ({0}, {1})".Fi(substring, stringValue);
        }
    }
}
