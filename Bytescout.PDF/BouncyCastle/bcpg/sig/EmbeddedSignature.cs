﻿using System;

namespace Org.BouncyCastle.Bcpg.Sig
{
	/**
	 * Packet embedded signature
	 */
	internal class EmbeddedSignature
		: SignatureSubpacket
	{
		public EmbeddedSignature(
			bool	critical,
            bool    isLongLength,
			byte[]	data)
			: base(SignatureSubpacketTag.EmbeddedSignature, critical, isLongLength, data)
		{
		}
	}
}
