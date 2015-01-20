using System;
using System.ComponentModel;

//http://stackoverflow.com/a/9236605
//Mangesh Pimpalkar http://stackoverflow.com/users/503786

namespace LolBoostNotifier {
	public static class AttributesHelperExtension {
		public static string ToDescription(this Enum value) {
			var da = (DescriptionAttribute[])(value.GetType().GetField(value.ToString())).GetCustomAttributes(typeof(DescriptionAttribute), false);
			return da.Length > 0 ? da[0].Description : value.ToString();
		}
	}
}

