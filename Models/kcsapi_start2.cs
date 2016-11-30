using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace ZSubtitle
{
	public class kcsapi_start2
	{
		public kcsapi_mst_shipgraph[] api_mst_shipgraph { get; set; }
	}

	public class kcsapi_mst_shipgraph
	{
		public int api_id { get; set; }
		public int api_sortno { get; set; }
		public string api_filename { get; set; }
	}
}
