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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mooege.Common.Helpers;

namespace Mooege.Common.MPQ
{
    public static class MPQStorage
    {
        private static readonly Logger Logger = LogManager.CreateLogger();
        private readonly static string MpqRoot = Core.Common.Storage.Config.Instance.MPQRoot;

        public static readonly List<string> MPQList;
        public static readonly CoreData CoreData;

        static MPQStorage()
        {
            if (!Directory.Exists(MpqRoot)) Logger.Error("MPQ root folder does not exist: {0}", MpqRoot);
            MPQList = FileHelpers.GetFilesByExtensionRecursive(MpqRoot, ".mpq");

            CoreData = new CoreData();
            if (CoreData.Loaded) CoreData.Init();
        }

        public static string GetMPQFile(string name)
        {
            return MPQList.FirstOrDefault(file => file.Contains(name));
        }

        public static void Init()
        {
            
        }
    }
}
