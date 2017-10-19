using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrotiNet;
using Nekoxy;

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

		/// <summary>
		/// SendResponseをoverrideし、リクエストデータを読み取る。
		/// </summary>
		protected override void SendRequest()
		{
			//HTTPリクエストヘッダ送信
			this.SocketPS.WriteBinary(Encoding.ASCII.GetBytes(
				$"{this.RequestLine.RequestLine}\r\n{this.RequestHeaders.HeadersInOrder}\r\n"));

			byte[] request = null;
			if (this.State.bRequestHasMessage)
			{
				if (this.State.bRequestMessageChunked)
				{
					//FIXME: chunked request のデータ読み取りは未対応
					this.SocketBP.TunnelChunkedDataTo(this.SocketPS);
				}
				else
				{
					//Requestデータを読み取って流す
					request = new byte[this.State.RequestMessageLength];
					this.SocketBP.TunnelDataTo(request, this.State.RequestMessageLength);
					this.SocketPS.TunnelDataTo(this.TunnelPS, request);
				}
			}

			//ReadResponseへ移行
			this.State.NextStep = this.ReadResponse;
		}
	}
}