// courtesy of http://stackoverflow.com/questions/647533
using System.Collections.Generic;

namespace Common.Helpers {
	public class RecursiveDictionary : Dictionary<string, RecursiveDictionary> { }
}