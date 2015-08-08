﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections;

using System.Collections.Generic;
using System.Linq;
namespace DHXDownloadManager.Tests
{
    /// <summary>
    /// Tests whether an invalid URL throws a fail
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class FailTest<T> : Test<T> where T : IDownloadEngine, new()
    {
        public FailTest(Tests<T> parent) : base(parent) { }

        override protected IEnumerator _DoTest()
        {
            yield return base._DoTest();

            Start();

            int succeed = -1;
            Manifest metadata = new Manifest("httpfs://s3.amazonaws.com/piko_public/Test.png", 0);
            metadata.OnDownloadFinished += (m) => succeed = 0;
            metadata.OnDownloadFailed += (m) => succeed = 1;
            metadata.OnAbort += (m) => { succeed = 1; Logger.Log("FailTest::Abort"); };
            metadata.OnFinalizeMetadata += () => succeed = 1;
            try
            { 
                _Parent._Manager.AddDownload(metadata);
            }
            catch
            {
                succeed = 1;
            }

            while (succeed == -1)
            {
                yield return null;
            }
            Finish();
            if (succeed == 1)
                Success();
            else
                Fail();

            yield return null;
        }
    }

}