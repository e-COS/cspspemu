﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CSPspEmu.Hle.Formats.video
{
	public interface IDemuxer
	{
		/// <summary>
		/// Name of the demuxer.
		/// </summary>
		String Name { get; }

		/// <summary>
		/// 
		/// </summary>
		String LongName { get; }

		/// <summary>
		/// Gets a fuzzy-logic score that determines if the file could be of this kind.
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="Data"></param>
		/// <returns>The score</returns>
		float Probe(String FileName, Stream ProbeStream);


		// ReadHeader
		// ReadPacket
		// ReadTimestamp
		// Flags
	}
}
