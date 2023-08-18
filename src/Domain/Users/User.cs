using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BogusStore.Domain.Users
{
	public class User
	{
		public string UserName { get; set; } = default!;
		public string Password { get; set; } = default!;
	}
}
