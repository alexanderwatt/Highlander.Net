/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System.Collections.Generic;
using System.IO;
using System.Threading;
using Orion.Util.Expressions;
using Orion.Util.Serialisation;

namespace Core.Common
{
    public class DataTransferJob
    {
        public ICoreClient SourceClient;
        public StreamReader SourceStream;
        public IExpression Filter;
        public string DataTypeName;
        public bool ReadOnly;
        public ICoreClient TargetClient;
        public StreamWriter TargetStream;
    }

    public class BackupTools
    {
        public static void TransferObjects(DataTransferJob job, ref long cancelFlag)
        {
            int sourceSizeInBytes = 0;
            int sourceSizeInChars = 0;
            int targetSizeInBytes = 0;
            int targetSizeInChars = 0;
            var sourceItems = new List<RawItem>();
            if ((job.SourceClient != null) || (job.SourceStream != null))
            {
                //logger.LogDebug("Loading ...");
                if (job.SourceStream != null)
                {
                    // source is a file
                    int loadCount = 0;
                    while (!job.SourceStream.EndOfStream)
                    {
                        string itemAsString = job.SourceStream.ReadLine();
                        var item = (RawItem)BinarySerializerHelper.DeserializeFromString(itemAsString);
                        if (item.ItemKind == (int)ItemKind.Object)
                        {
                            sourceSizeInBytes += item.YData?.Length ?? 0;
                            if (itemAsString != null) sourceSizeInChars += itemAsString.Length;
                            sourceItems.Add(item);
                            loadCount++;
                            //if (loadCount % 2500 == 0)
                            //    logger.LogDebug("Loaded {0} objects ...", loadCount);
                        }
                        if (Interlocked.Add(ref cancelFlag, 0) > 0)
                        {
                            //logger.LogDebug("Transfer cancelled! ({0} objects loaded)", loadCount);
                            return;
                        }
                    }
                }
                else
                {
                    // source is a server
                    if (job.SourceClient != null)
                    {
                        List<ICoreItem> list = job.SourceClient.LoadUntypedItems(job.DataTypeName, ItemKind.Object, job.Filter, false);
                        foreach (ICoreItem item in list)
                        {
                            sourceSizeInBytes += item.SysProps.GetValue(SysPropName.ZLen, 0);
                            sourceSizeInChars += item.SysProps.GetValue(SysPropName.TLen, 0);
                            sourceItems.Add(new RawItem(item));
                        }
                    }
                }
            }
            if ((job.TargetClient != null) || (job.TargetStream != null))
            {
                int saveCount = 0;
                if (job.TargetStream != null)
                {
                    // to stream
                    foreach (RawItem item in sourceItems)
                    {
                        string itemAsString = BinarySerializerHelper.SerializeToString(item);
                        job.TargetStream.WriteLine(itemAsString);
                        targetSizeInBytes = sourceSizeInBytes;
                        targetSizeInChars += itemAsString.Length;
                        saveCount++;
                        //if (saveCount % 2500 == 0)
                        //    logger.LogDebug("Saved {0} objects ...", saveCount);
                        if (Interlocked.Add(ref cancelFlag, 0) > 0)
                        {
                            //logger.LogDebug("Transfer cancelled! ({0} objects saved)", saveCount);
                            return;
                        }
                    }
                }
                else
                {
                    // to server
                    targetSizeInBytes = sourceSizeInBytes;
                    targetSizeInChars = sourceSizeInChars;
                    //
                    //This is where we can replace the appScope
                    //
                    //foreach(var item in sourceItems)
                    //{
                    //    item.AppScope = AppScopeNames.Current;
                    //}
                    job.TargetClient?.SaveRawItems(sourceItems);
                    saveCount += sourceItems.Count;
                }
                job.TargetStream?.Flush();
                //logger.LogDebug("Saved {0} objects ({1} chars, {2} bytes)", saveCount, targetSizeInChars, targetSizeInBytes);
            }
            else
            {
                //if(!job.ReadOnly)
                //    logger.LogError("No target specified!");
            }
        }
    }
}
