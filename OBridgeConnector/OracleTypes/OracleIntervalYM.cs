using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OBridgeConnector.OracleTypes;

public class OracleIntervalYM
{
	public int Years;
	public int Months;

	public override string ToString()
	{
		int totalMonths = Years * 12 + Months;
		int absMonths = Math.Abs(totalMonths);
		int y = absMonths / 12;
		int m = absMonths % 12;

		var isNegative = totalMonths < 0 ? "-" : "";
		return $"{isNegative}{y:D4}-{m:D2}";
	}
}