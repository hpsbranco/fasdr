﻿using System;

namespace Fasdr.Backend
{
	public struct Entry
	{
		public static readonly char Separator = '|';

		public Entry(string fullPath,Int64 frequency,DateTime lastAccessTime,bool isLeaf) :
		this(fullPath,frequency,lastAccessTime.ToFileTimeUtc(),isLeaf) {
		}

		public Entry(string fullPath,Int64 frequency,Int64 lastAccessTimeUtc,bool isLeaf)
		{
			FullPath = fullPath;
			SplitPath = FullPath.Split(new char[] {'\\'},StringSplitOptions.RemoveEmptyEntries);
			Frequency = frequency;
			LastAccessTimeUtc = lastAccessTimeUtc;
			IsLeaf = isLeaf;
		}

		public static Entry FromString(string line)
		{
			var split = line.Split(new char[] {Separator}, StringSplitOptions.RemoveEmptyEntries);

			if (split==null || split.Length!=4)
			{
				throw new Exception("Failed to parse line '" + line + "'");
			}

			var path = split[0];

			Int64 frequency;
			if (!Int64.TryParse(split[1], out frequency))
			{
				throw new Exception ($"Failed to parse frequency from string '{split[1]}'");
			}

			Int64 lastAccessTimeUtc;
			if (!Int64.TryParse(split[2], out lastAccessTimeUtc))
			{
				throw new Exception ($"Failed to parse last access time from string '{split[2]}'");
			}

			bool isLeaf;
			if (!Boolean.TryParse(split[3], out isLeaf))
			{
				throw new Exception ($"Failed to parse isLeaf flag from string '{split[3]}'");
			}

			return new Entry (path,frequency,lastAccessTimeUtc,isLeaf);
		}

		public override string ToString()
		{
			return $"{FullPath}{Separator}" + 
				$"{Frequency}{Separator}" +
				$"{LastAccessTimeUtc}{Separator}" +
				$"{IsLeaf}";
		}

		public double CalculateFrecency()
		{
			// borrowed from https://github.com/JannesMeyer/z.ps/blob/master/z.psm1
			var now = DateTime.Now;
			var last = DateTime.FromFileTimeUtc(LastAccessTimeUtc);
			double factor = 0.25;
			if (last.AddHours (1) > now) {
				factor = 4.0;
			} else if (last.AddDays(1) > now) {
				factor = 2.0;
			} else if (last.AddDays(7) > now) {
				factor = 0.5;
			}

			return factor * Frequency;
		}

		public string FullPath { get; }
		public string[] SplitPath { get; }
		public Int64 Frequency { get; }
		public Int64 LastAccessTimeUtc { get; }
		public DateTime LastAccessTime {
			get {
				return DateTime.FromFileTimeUtc(LastAccessTimeUtc);
			}
		}
		public bool IsLeaf { get; }

		public static readonly Int64 cInvalid = -1;

		public bool IsValid { get { return Frequency >= 0; } }
	}
}

