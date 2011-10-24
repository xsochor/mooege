﻿/*
 * Copyright (C) 2011 mooege project
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CrystalMpq;
using CrystalMpq.Utility;
using Wintellect.PowerCollections;

namespace Mooege.Common.MPQ
{    
    public class MPQPatchChain
    {
        protected static readonly Logger Logger = LogManager.CreateLogger();

        public bool Loaded { get; private set; }
        public readonly MpqFileSystem FileSystem = new MpqFileSystem();
        public List<string> BaseMPQFiles = new List<string>();
        public string PatchPattern { get; private set; }
        public readonly OrderedMultiDictionary<int, string> MPQFileList = new OrderedMultiDictionary<int, string>(false);

        protected MPQPatchChain(IEnumerable<string> baseFiles, string patchPattern=null)
        {
            this.Loaded = false;

            foreach(var file in baseFiles)
            {
                var mpqFile = MPQStorage.GetMPQFile(file);
                if(mpqFile == null)
                {
                    Logger.Error("Can not find base-mpq file: {0} for patch chain: {1}.", file, this.GetType().Name);
                    return;
                }
                this.BaseMPQFiles.Add(mpqFile);
            }

            Logger.Info("Reading MPQ patch-chain: {0}", this.GetType().Name);
            
            this.PatchPattern = patchPattern;
            this.ConstructChain();
            this.Loaded = true;
        }

        private void ConstructChain()
        {            
            // add base mpq files;
            foreach(var mpqFile in this.BaseMPQFiles)
            {
                MPQFileList.Add(0, mpqFile);
            }

            if (PatchPattern == null) return;

            /* match the mpq files for the patch chain */
            var patchRegex = new Regex(this.PatchPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            foreach(var file in MPQStorage.MPQList)
            {
                var match = patchRegex.Match(file);
                if (!match.Success) continue;
                if (!match.Groups["version"].Success) continue;

                MPQFileList.Add(Int32.Parse(match.Groups["version"].Value), file);
            }

            /* add mpq's to mpq-file system in reverse-order (highest version first) */
            foreach(var pair in this.MPQFileList.Reverse())
            {
                foreach(var mpq in pair.Value)
                {
                    this.FileSystem.Archives.Add(new MpqArchive(mpq));    
                }
            }
        }

        public List<string> FindMatchingFiles(string mask)
        {
            var list = new List<string>();
            foreach(var archive in this.FileSystem.Archives)
            {
                foreach(var file in archive.Files)
                {
                    if (!file.Name.Contains(mask)) continue;
                    if (list.Contains(file.Name)) continue;

                    list.Add(file.Name);
                }
            }

            return list;
        }
    }
}
