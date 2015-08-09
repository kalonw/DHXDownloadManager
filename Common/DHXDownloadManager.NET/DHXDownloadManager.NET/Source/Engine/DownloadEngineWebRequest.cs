﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;

namespace DHXDownloadManager
{

    public class RequestState
    {
        const int BufferSize = 1024;
        public byte[] BufferRead;
        public WebRequest Request;
        public Stream ResponseStream;
        public Manifest Manifest;
        public DownloadEngineWebRequest Parent;
        public RequestState()
        {
            BufferRead = new byte[BufferSize];
            Request = null;
            ResponseStream = null;
        }
    }
    /// <summary>
    /// C# WebRequest implementation of the DownloadEngine
    /// </summary>
    public class DownloadEngineWebRequest : IDownloadEngine
    {
        const int BUFFER_SIZE = 1024;

        public event OnDownloadedFinishedDelegate OnEngineDownloadFailed;
        public event OnDownloadedFinishedDelegate OnEngineDownloadFinished;
        
        public void PerformDownload(Manifest manifest)
        {

            manifest.__Start();
            manifest.Ping();

            string url = manifest.URL;
            string realURL = System.Uri.EscapeUriString(url);
            Uri httpSite = null;
            bool success = Uri.TryCreate(realURL, UriKind.Absolute, out httpSite) && (httpSite.Scheme == Uri.UriSchemeHttp || httpSite.Scheme == Uri.UriSchemeHttps);
            if(success == false)
            {
                Logger.Log("Invalid URI");
                manifest.Attempts = -1;
                if (OnEngineDownloadFailed != null)
                    OnEngineDownloadFailed(manifest);
                return;
            }
            WebRequest wreq = null;
            // Create the request object.
            try
            {
                wreq = WebRequest.Create(httpSite);
            }
            catch (System.Exception e)
            {
                Logger.Log("EXCEPTION");
                manifest.Attempts = -1;
                if (OnEngineDownloadFailed != null)
                    OnEngineDownloadFailed(manifest);
                return;
            }
            finally { }

            // Create the state object.
            RequestState rs = new RequestState();

            // Put the request into the state object so it can be passed around.
            rs.Request = wreq;
            rs.Manifest = manifest;
            rs.Parent = this;
            // Issue the async request.
            IAsyncResult r = (IAsyncResult)wreq.BeginGetResponse(
               new AsyncCallback(RespCallback), rs);

            
            /*
            HTTPMethods methodType = HTTPMethods.Get;

            if (manifest.POSTFieldKVP.Count > 0)
            {
                methodType = HTTPMethods.Post;
            }
            if (manifest.POSTFieldKVP.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in manifest.POSTFieldKVP)
                {
                    bestHTTPRequest.AddField(kvp.Key, kvp.Value);
                }
            }*/

            manifest.EngineInstance = rs;
        }



        private static void RespCallback(IAsyncResult ar)
        {
            // Get the RequestState object from the async result.
            RequestState rs = (RequestState)ar.AsyncState;

            // Get the WebRequest from RequestState.
            WebRequest req = rs.Request;

            // Call EndGetResponse, which produces the WebResponse object
            //  that came from the request issued above.
            try
            {
                WebResponse resp = req.EndGetResponse(ar);
                //  Start reading data from the response stream.
                Stream ResponseStream = resp.GetResponseStream();
                
                // Store the response stream in RequestState to read 
                // the stream asynchronously.
                rs.ResponseStream = ResponseStream;

                //  Pass rs.BufferRead to BeginRead. Read data into rs.BufferRead
                IAsyncResult iarRead = ResponseStream.BeginRead(rs.BufferRead, 0,
                   BUFFER_SIZE, new AsyncCallback(ReadCallBack), rs);
            }
            catch (System.Net.WebException e)
            {
                
                Manifest manifest = rs.Manifest;
                if (manifest != null)
                {
                    if(e.Response != null)
                    { 
                        int code = (int)((HttpWebResponse)e.Response).StatusCode;
                        manifest.ResponseCode = code;
                    }
                    manifest.Abort();
                }
                return;
            }
            catch (System.Exception e)
            {
                Manifest manifest = rs.Manifest;
                if (manifest != null)
                {
                    manifest.Abort();
                }
                return;
            }
            finally { }

        }


        private static void ReadCallBack(IAsyncResult asyncResult)
        {
            // Get the RequestState object from AsyncResult.
            RequestState rs = (RequestState)asyncResult.AsyncState;
            Manifest manifest = rs.Manifest;


            if (manifest == null)
            {
                rs.Request.Abort();
                return;
            }
            
            // TODO: fix error codes
            /*
            if (resp != null)
                metadata.ResponseCode = resp.StatusCode;
            if (resp == null || resp.StatusCode >= 400 || req.Exception != null || req.Response == null)
            {
                metadata.SetError(ManifestErrors.UnknownDownloadError);
                if (OnEngineDownloadFailed != null)
                    OnEngineDownloadFailed(metadata);
                metadata.EngineInstance = null;

                req.Tag = null;
                return;
            }
            */
            // Retrieve the ResponseStream that was set in RespCallback. 
            Stream responseStream = rs.ResponseStream;

            // Read rs.BufferRead to verify that it contains data. 
            manifest.Ping();
            int read = responseStream.EndRead(asyncResult);
            if (read > 0)
            {
				int readSize = (int)rs.Request.ContentLength;
				manifest.__UpdateBytes(read, readSize);
                List<byte[]> bytes = new List<byte[]>();


				// kind of crappy, have to copy out the right amount of bytes
				byte[] resizedBytes = new byte[read];
				for(int i = 0; i < read; i++)
				{
					resizedBytes[i] = rs.BufferRead[i];
				}
				bytes.Add(resizedBytes);
                manifest.__Update(bytes);

                // Continue reading data until 
                // responseStream.EndRead returns –1.
                try
                {
                IAsyncResult ar = responseStream.BeginRead(
                   rs.BufferRead, 0, BUFFER_SIZE,
                   new AsyncCallback(ReadCallBack), rs);
                }
                catch (System.Net.WebException e)
                {
                    if (manifest != null)
                    {
                        if(e.Response != null)
                        { 
                            int code = (int)((HttpWebResponse)e.Response).StatusCode;
                            manifest.ResponseCode = code;
                        }
                        manifest.Abort();
                    }
                    return;
                }
                catch (System.Exception e)
                {
                    if (manifest != null)
                    {
                        manifest.Abort();
                    }
                    return;
                }
                finally { }
            }
            else
            {
                rs.Manifest = null;
                manifest.EngineInstance = null;
                if (rs.Parent.OnEngineDownloadFinished != null)
                    rs.Parent.OnEngineDownloadFinished(manifest);

                // Close down the response stream.
                responseStream.Close();
            }
            return;
        }    


        public void Abort(Manifest manifest)
        {
            RequestState req = (RequestState)manifest.EngineInstance;

            if (req != null)
            {
                req.Request.Abort();
                req.Manifest = null;
            }
            manifest.EngineInstance = null;
        }


    }
}