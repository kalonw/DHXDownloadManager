﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DHXDownloadManager;
namespace DHXDownloadManager.Tests
{ 
    class Test<T> where T : IDownloadEngine, new()
    {
        public event System.Action OnTestFail;
        public event System.Action OnTestSucceed;
        protected Tests<T> _Parent;
        public Test(Tests<T> parent)
        {
            _Parent = parent;
        }

        virtual protected IEnumerator _DoTest()
        {
            yield return null;
        }

        virtual public IEnumerator DoTest()
        {
            return _DoTest();
        }

        protected void Start()
        {
            Logger.Log(GetType().Name + "::Start");
        }

        protected void Finish()
        {
            Logger.Log(GetType().Name + "::Finish");
        }

        protected void Fail()
        {
            Logger.Log(GetType().Name + "::Fail");
            if (OnTestFail != null)
                OnTestFail();
        }

        protected void Success()
        {
            Logger.Log("<color=green>" + GetType().Name + "::Success</color>");
            if (OnTestSucceed != null)
                OnTestSucceed();
        }
    }


    public class Tests<T> where T : IDownloadEngine, new()
    {
        public Manager<T> _Manager;
        public Ledger _Ledger;
        public GroupLedger _GroupLedger;
        List<Test<T>> _Tests = new List<Test<T>>();


        public Tests(Manager<T> manager, Ledger ledger, GroupLedger groupLedger)
        {
            _Manager = manager;
            _Ledger = ledger;
            _GroupLedger = groupLedger;
        }

	    // Use this for initialization
	    public IEnumerator Start()
        {
            yield return null;
            _Ledger.Clear();
            _GroupLedger.Clear();

            _Tests.Add(new BaseTest<T>(this));
            _Tests.Add(new PostTest<T>(this));
            _Tests.Add(new FailTest<T>(this));
            _Tests.Add(new FailTest02<T>(this));
            _Tests.Add(new FileStreamTest<T>(this));
            _Tests.Add(new MemoryStreamTest<T>(this));
            _Tests.Add(new SerializationTest<T>(this));
            _Tests.Add(new SortingTest<T>(this));
            _Tests.Add(new ProgressTest<T>(this));
            _Tests.Add(new StatusCodeTest<T>(this));
            _Tests.Add(new LedgerTest01<T>(this));
            _Tests.Add(new LedgerTest02<T>(this));
            _Tests.Add(new GroupLedgerTest01<T>(this));
            _Tests.Add(new GroupLedgerTest02<T>(this));
            _Tests.Add(new GroupLedgerTest03<T>(this));
            _Tests.Add(new GroupLedgerTest04<T>(this));
            _Tests.Add(new AbortTest<T>(this));
            _Tests.Add(new DestroyTest01<T>(this));
            _Tests.Add(new DestroyTest02<T>(this));
            
            int succeedCount = 0;
            int failCount = 0;
            for(int i = 0; i < _Tests.Count; i++)
            {
                _Tests[i].OnTestFail += () => failCount++;
                _Tests[i].OnTestSucceed += () => succeedCount++;
                yield return _Tests[i].DoTest();
            }
            Logger.Log(failCount + "/" + _Tests.Count + " Failures");
            Logger.Log(succeedCount + "/" + _Tests.Count + " Successes");

	    }


    }
}