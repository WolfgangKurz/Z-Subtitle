using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrotiNet;

namespace ZSubtitle
{
	internal delegate void SoundRequested(string PathAndQuery);

	internal class NoCacheProxy : ProxyLogic
	{
		public NoCacheProxy(HttpSocket clientSocket) : base(clientSocket) { }
		static new public NoCacheProxy CreateProxy(HttpSocket clientSocket) => new NoCacheProxy(clientSocket);

		public static event SoundRequested OnSoundRequested;

		protected override void OnReceiveResponse()
		{
			if (!RequestLine.RequestLine.StartsWith("GET /kcs/sound/")) return;

			// from Nekoxy
			string PathAndQuery = this.RequestLine.URI.Contains("://") && Uri.IsWellFormedUriString(this.RequestLine.URI, UriKind.Absolute)
				? new Uri(this.RequestLine.URI).PathAndQuery
				: this.RequestLine.URI.Contains("/")
					? this.RequestLine.URI
					: string.Empty;

			NoCacheProxy.OnSoundRequested?.Invoke(PathAndQuery);

			ResponseHeaders.CacheControl = "must-revalidate, no-cache"; // , no-store
			// 캐싱은 하되 항상 확인을 (302 Not modified 로 처리)

			// ResponseHeaders.Expires = "Fri, 01 Jan 1990 00:00:00 GMT";
			// ResponseHeaders.Pragma = "no-cache";
		}
	}
}