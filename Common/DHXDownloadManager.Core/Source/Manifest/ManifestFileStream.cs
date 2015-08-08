﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Runtime.Serialization;
namespace DHXDownloadManager
{ 
    /// <summary>
    /// The FileStream implementation of ManifestStream
    /// </summary>
    [System.Serializable]
    [DataContract]
    public class ManifestFileStream : ManifestStream
    {
        string _WritePath;
        public string WritePath { get { return _WritePath;} }
        public ManifestFileStream()
            : base()
        {

        }


        // FIXME: This will break on Unity iOS when building different versions
        // write path somehow needs to be a static ref to Application.persistentDataPath
        public ManifestFileStream(string url, string writePath, Flags flag)
            : base(url, flag)
        {
            _WritePath = writePath;
        }

        public static void GetPaths(ManifestFileStream metadata, out string realPath, out string tmpPath)
        {
            realPath = string.Concat(metadata.WritePath, metadata.RelativePath);
            tmpPath = string.Concat(realPath, ".tmp");
        }

        override protected Stream CreateStream()
        {
            Stream stream = null;

            try
            {

                string realSaveLocation, tmpSaveLocation;
                GetPaths(this, out realSaveLocation, out tmpSaveLocation);
                string path = Path.GetDirectoryName(tmpSaveLocation);

                if (Directory.Exists(path) == false)
                {
                    Directory.CreateDirectory(path);
                }
                stream = File.Open(tmpSaveLocation, FileMode.Create);

            }
            catch (IOException e)
            {
                SetError(ManifestErrors.IOException);
                Logger.Log(e);
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }
            }
            catch (Exception e)
            {
                SetError(ManifestErrors.Unknown);
                Logger.Log(e);
                if (stream != null)
                { 
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }
            }
            finally
            {

            }

            return stream;
        }


        override protected int FinalizeStreamSuccess()
        {

            try
            {

                string realSaveLocation, tmpSaveLocation;
                GetPaths(this, out realSaveLocation, out tmpSaveLocation);
                if (File.Exists(realSaveLocation)) 
                {
                    File.Delete(realSaveLocation);
                }
                File.Move(tmpSaveLocation, realSaveLocation);

            }
            catch (IOException e)
            {
                SetError(ManifestErrors.IOException);
                Logger.Log(e);
                return 1;
            }
            catch (Exception e)
            {
                SetError(ManifestErrors.Unknown);
                Logger.Log(e);
                return 1;
            }

            return 0;
        }

        override protected int FinalizeStreamFailure()
        {

            try
            {
                string realSaveLocation, tmpSaveLocation;
                GetPaths(this, out realSaveLocation, out tmpSaveLocation);
                if (File.Exists(tmpSaveLocation))
                    File.Delete(tmpSaveLocation);

            }
            catch (Exception e)
            {
                SetError(ManifestErrors.Unknown);
                Logger.Log(e);
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Same as Manifest.Destroy and cleans up any files it has written to
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();

            try
            {
                string realSaveLocation, tmpSaveLocation;
                GetPaths(this, out realSaveLocation, out tmpSaveLocation);
                if (File.Exists(realSaveLocation) == true)
                {
                    File.Delete(realSaveLocation);
                }
                if (File.Exists(tmpSaveLocation) == true)
                {
                    File.Delete(tmpSaveLocation);
                }
            }
            catch(System.Exception e)
            {
                Logger.Log(e);
            }
            finally
            {

            }
            
        }
    }
}