﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections;

using System.Collections.Generic;
using System.Linq;
namespace DHXDownloadManager.Tests
{

    class GroupLedgerTest02<T> : Test<T> where T : IDownloadEngine, new()
    {

        public GroupLedgerTest02(Tests<T> parent) : base(parent) { }
        int downloadCount = 0;
        override protected IEnumerator _DoTest()
        {
            yield return base._DoTest();

            Start();

            Manifest metadata1 = new ManifestFileStream("https://s3.amazonaws.com/piko_public/Test.png", System.IO.Path.GetTempPath(), ManifestFileStream.Flags.None);
            Manifest metadata2 = new ManifestFileStream("https://s3.amazonaws.com/piko_public/Test2.png", System.IO.Path.GetTempPath(), ManifestFileStream.Flags.None);
            Manifest metadata3 = new ManifestFileStream("https://s3.amazonaws.com/piko_public/Test3.png", System.IO.Path.GetTempPath(), ManifestFileStream.Flags.None);
            List<Manifest> manifests = new List<Manifest>();
            manifests.Add(metadata1);
            manifests.Add(metadata2);
            manifests.Add(metadata3);

            Group group = _Parent._GroupLedger.AddGetGroup("GroupLedgerTest02");
            _Parent._GroupLedger.AddDownloads(group, manifests);
            group.OnDownloadGroupStartDownload += group_OnDownloadGroupStartDownload;
            group.OnDownloadGroupEndDownload += group_OnDownloadGroupEndDownload;
            for (int i = 0; i < manifests.Count; i++)
            {
                Manifest manifest = manifests[i];
                _Parent._Manager.AddDownload(manifest);
                manifests[i] = manifest;
            }


            while (downloadCount != 0 || group.Status != Group.StatusFlags.Complete)
            {
                //Logger.Log("WaitForFixedUpdate(): " + group.Status + " " + downloadCount);
                yield return 0;
            }

            manifests[0].Destroy();

            if (group.Status == Group.StatusFlags.Destroyed)
                Success();
            else
                Fail();
            _Parent._GroupLedger.Write();
            yield return null;
        }

        void group_OnDownloadGroupEndDownload(Group obj)
        {
            downloadCount--;
            Logger.Log("group_OnDownloadGroupEndDownload");
        }

        void group_OnDownloadGroupStartDownload(Group obj)
        {
            downloadCount++;
            Logger.Log("group_OnDownloadGroupStartDownload");
        }

    }
}