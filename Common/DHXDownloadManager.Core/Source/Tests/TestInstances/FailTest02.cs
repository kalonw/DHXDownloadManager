﻿﻿/* This Source Code Form is subject to the terms of the Mozilla Public
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
    class FailTest02<T> : Test<T> where T : IDownloadEngine, new()
    {
        public FailTest02(Tests<T> parent) : base(parent) { }

        override protected IEnumerator _DoTest()
        {
            yield return base._DoTest();

            Start();

            int succeed = -1;
            Manifest metadata = new Manifest("httpfs://s3.amazonaws.com/piko_public/Test.png", 0);
            metadata.OnDownloadFailed += delegate(Manifest m)
            {
                m.OnDownloadFailed += (m2) => succeed = 1;
                _Parent._Manager.AddDownload(m);
            };
            _Parent._Manager.AddDownload(metadata);



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