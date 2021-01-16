//=============================================================================
// Copyright (c) 2001-2019 FLIR Systems, Inc. All Rights Reserved.
//
// This software is the confidential and proprietary information of FLIR
// Integrated Imaging Solutions, Inc. ("Confidential Information"). You
// shall not disclose such Confidential Information and shall use it only in
// accordance with the terms of the license agreement you entered into
// with FLIR Integrated Imaging Solutions, Inc. (FLIR).
//
// FLIR MAKES NO REPRESENTATIONS OR WARRANTIES ABOUT THE SUITABILITY OF THE
// SOFTWARE, EITHER EXPRESSED OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE, OR NON-INFRINGEMENT. FLIR SHALL NOT BE LIABLE FOR ANY DAMAGES
// SUFFERED BY LICENSEE AS A RESULT OF USING, MODIFYING OR DISTRIBUTING
// THIS SOFTWARE OR ITS DERIVATIVES.
//=============================================================================

/**
 *  @example AcquisitionMultipleThread_CSharp.cs
 *
 *  @brief AcquisitionMultipleThread_CSharp.cs shows how to capture images from
 *  multiple cameras simultaneously on multiple threads. It relies on information
 *  provided in the Enumeration_CSharp, Acquisition_CSharp, and NodeMapInfo_CSharp examples.
 *
 *  This example reads similarly to the Acquisition_CSharp example, except that
 *  loops and vectors are used to allow for simultaneous acquisitions.
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SpinnakerNET;
using SpinnakerNET.GenApi;
using System.ComponentModel;

namespace AcquisitionMultipleThread_CSharp
{
    class Program
    {
        List<BackgroundWorker>bgws = new List<BackgroundWorker>();
        AutoResetEvent workersDoneEvent = new AutoResetEvent(false);
        bool workerHasError = false;

        void DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = 0;

            BackgroundWorker wk = (BackgroundWorker) sender;
            IManagedCamera cam = (IManagedCamera) e.Argument; // the 'argument' parameter resurfaces here

            // Retrieve TL device nodemap
            INodeMap nodeMapTLDevice = cam.GetTLDeviceNodeMap();

            String deviceSerialNumber = "";
            IString iDeviceSerialNumber = nodeMapTLDevice.GetNode<IString>("DeviceSerialNumber");
            if (iDeviceSerialNumber != null && iDeviceSerialNumber.IsReadable)
            {
                deviceSerialNumber = iDeviceSerialNumber.Value;
            }

            Console.WriteLine("Camera {0} - BeginAcquisition()", deviceSerialNumber);

            // Print device information
            PrintDeviceInfo(nodeMapTLDevice);

            // Initialize camera
            cam.Init();

            // Set acquisition mode to continuous
            IEnum iAcquisitionMode = cam.GetNodeMap().GetNode<IEnum>("AcquisitionMode");
            if (iAcquisitionMode == null || !iAcquisitionMode.IsWritable)
            {
                Console.WriteLine(
                    "Unable to set acquisition mode to continuous (node retrieval camera {0}). Aborting...\n",
                    deviceSerialNumber);
                return;
            }

            IEnumEntry iAcquisitionModeContinuous = iAcquisitionMode.GetEntryByName("Continuous");
            if (iAcquisitionModeContinuous == null || !iAcquisitionMode.IsReadable)
            {
                Console.WriteLine(
                    "Unable to set acquisition mode to continuous (enum entry retrieval camera {0}). Aborting...\n",
                    deviceSerialNumber);
                return;
            }

            iAcquisitionMode.Value = iAcquisitionModeContinuous.Symbolic;

            Console.WriteLine("Camera {0} acquisition mode set to continuous...", deviceSerialNumber);

#if DEBUG
            Console.WriteLine("\n\n*** DEBUG ***\n\n");

            // If using a GEV camera and debugging, should disable heartbeat first to prevent further issues
            if (DisableHeartbeat(cam, cam.GetNodeMap(), cam.GetTLDeviceNodeMap()) != 0)
            {
                return;
            }

            Console.WriteLine("\n\n*** END OF DEBUG ***\n\n");
#endif

            try
            {
                // Begin capturing images
                cam.BeginAcquisition();
            }
            catch (SpinnakerException se)
            {
                if (se.ErrorCode == Error.SPINNAKER_ERR_RESOURCE_IN_USE)
                {
                    // Expected case where camera is already streaming
                    Console.WriteLine(
                        "Camera {0} - Expected Camera is already streaming : {1}", deviceSerialNumber, se.Message);
                }
                else
                {
                    Console.WriteLine("Camera {0} - Unexpected Exception : {1}", deviceSerialNumber, se.Message);
                    e.Result = -1;
                }
            }

            Console.WriteLine("Camera {0} - GetNextImage()", deviceSerialNumber);
            int numImages = 100;
            for (int i = 0; i < numImages; i++)
            {
                try
                {
                    using(IManagedImage rawImage = cam.GetNextImage(1000))
                    {
                        // Check Inconsistency Errors
                        if (rawImage.IsIncomplete)
                        {
                            Console.WriteLine(
                                "Camera {0} - incomplete image : {1}",
                                deviceSerialNumber,
                                ManagedImage.GetImageStatusDescription(rawImage.ImageStatus));
                        }

                        Console.WriteLine("Camera {0} - Grabbed image {1}", deviceSerialNumber, i);
                    }
                }
                catch (SpinnakerException se)
                {
                    if (se.ErrorCode == Error.SPINNAKER_ERR_IO)
                    {
                        // If a thread has already called EndAcquisition() when other threads try to GetNextImage() it
                        // is expected to throw the message: camera is not streaming
                        if (!cam.IsStreaming())
                        {
                            Console.WriteLine(
                                "Camera {0} - expected SPINNAKER_ERR_IO because End acquisition was called",
                                deviceSerialNumber);
                        }
                        else
                        {
                            Console.WriteLine("Camera {0} - unexpected SPINNAKER_ERR_IO", deviceSerialNumber);
                            e.Result = -1;
                        }
                    }
                    else if (se.ErrorCode == Error.SPINNAKER_ERR_TIMEOUT)
                    {
                        // If a thread has already called EndAcquisition() and another thread was in the process of
                        // getting an image event, the event will time out
                        if (!cam.IsStreaming())
                        {
                            Console.WriteLine(
                                "Camera {0} - expected SPINNAKER_ERR_TIMEOUT because End acquisition was called",
                                deviceSerialNumber);
                        }
                        else
                        {
                            Console.WriteLine("Camera {0} - unexpected SPINNAKER_ERR_TIMEOUT", deviceSerialNumber);
                            e.Result = -1;
                        }
                    }
                    else if (se.ErrorCode == Error.SPINNAKER_ERR_INVALID_BUFFER)
                    {
                        // If a thread has already called EndAcquisition() then images may have already been released
                        if (!cam.IsStreaming())
                        {
                            Console.WriteLine(
                                "Camera {0} - expected SPINNAKER_ERR_INVALID_BUFFER because End acquisition was called",
                                deviceSerialNumber);
                        }
                        else
                        {
                            Console.WriteLine(
                                "Camera {0} - unexpected SPINNAKER_ERR_INVALID_BUFFER", deviceSerialNumber);
                            e.Result = -1;
                        }
                    }
                    else
                    {
                        Console.WriteLine(
                            "Camera {0} - unexpected expectation retrieving image {1}", deviceSerialNumber, se.Message);
                    }

                    if ((int) e.Result == -1)
                    {
                        break;
                    }
                }
            }

            Console.WriteLine("Camera {0} - EndAcquisition()", deviceSerialNumber);
            try
            {
                cam.EndAcquisition();
            }
            catch (SpinnakerException se)
            {
                if (se.ErrorCode == Error.SPINNAKER_ERR_NOT_INITIALIZED)
                {
                    // If a thread has already called EndAcquisition() when other threads try to call EndAcquisition()
                    // it will be expected to throw the message: camera is not started
                    Console.WriteLine("Camera {0} - Trying to End Acquisition: {1}", deviceSerialNumber, se.Message);
                }
                else
                {
                    e.Result = -1;
                    Console.WriteLine(
                        "Camera {0} - Unexpected error Stopping Acquisition: {1}", deviceSerialNumber, se.Message);
                    return;
                }
            }

            try
            {
                cam.DeInit();
            }
            catch (SpinnakerException se)
            {
                Console.WriteLine("Camera {0} - Trying to DeInit: {1}", deviceSerialNumber, se.Message);
            }

            Console.WriteLine("Camera {0} - End Grab", deviceSerialNumber);
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((int) e.Result == -1)
            {
                workerHasError = true;
            }

            BackgroundWorker bgw = (BackgroundWorker) sender;
            bgws.Remove(bgw);
            bgw.Dispose();

            if (bgws.Count == 0)
            {
                workersDoneEvent.Set();
            }
        }

#if DEBUG
        // Disables heartbeat on GEV cameras so debugging does not incur timeout errors
        static int DisableHeartbeat(IManagedCamera cam, INodeMap nodeMap, INodeMap nodeMapTLDevice)
        {
            Console.WriteLine("Checking device type to see if we need to disable the camera's heartbeat...\n\n");

            //
            // Write to boolean node controlling the camera's heartbeat
            //
            // *** NOTES ***
            // This applies only to GEV cameras and only applies when in DEBUG mode.
            // GEV cameras have a heartbeat built in, but when debugging applications the
            // camera may time out due to its heartbeat. Disabling the heartbeat prevents
            // this timeout from occurring, enabling us to continue with any necessary debugging.
            // This procedure does not affect other types of cameras and will prematurely exit
            // if it determines the device in question is not a GEV camera.
            //
            // *** LATER ***
            // Since we only disable the heartbeat on GEV cameras during debug mode, it is better
            // to power cycle the camera after debugging. A power cycle will reset the camera
            // to its default settings.
            //
            IEnum iDeviceType = nodeMapTLDevice.GetNode<IEnum>("DeviceType");
            IEnumEntry iDeviceTypeGEV = iDeviceType.GetEntryByName("GigEVision");
            // We first need to confirm that we're working with a GEV camera
            if (iDeviceType != null && iDeviceType.IsReadable)
            {
                if (iDeviceType.Value == iDeviceTypeGEV.Value)
                {
                    Console.WriteLine(
                        "Working with a GigE camera. Attempting to disable heartbeat before continuing...\n\n");
                    IBool iGEVHeartbeatDisable = nodeMap.GetNode<IBool>("GevGVCPHeartbeatDisable");
                    if (iGEVHeartbeatDisable == null || !iGEVHeartbeatDisable.IsWritable)
                    {
                        Console.WriteLine(
                            "Unable to disable heartbeat on camera. Continuing with execution as this may be non-fatal...");
                    }
                    else
                    {
                        iGEVHeartbeatDisable.Value = true;
                        Console.WriteLine("WARNING: Heartbeat on GigE camera disabled for the rest of Debug Mode.");
                        Console.WriteLine(
                            "         Power cycle camera when done debugging to re-enable the heartbeat...");
                    }
                }
                else
                {
                    Console.WriteLine("Camera does not use GigE interface. Resuming normal execution...\n\n");
                }
            }
            else
            {
                Console.WriteLine("Unable to access TL device nodemap. Aborting...");
                return -1;
            }

            return 0;
        }
#endif

        static int PrintDeviceInfo(INodeMap nodeMap)
        {
            int result = 0;

            try
            {
                Console.WriteLine("Printing device information for camera...\n");

                ICategory category = nodeMap.GetNode<ICategory>("DeviceInformation");
                if (category != null && category.IsReadable)
                {
                    for (int i = 0; i < category.Children.Length; i++)
                    {
                        Console.WriteLine(
                            "{0}: {1}",
                            category.Children[i].Name,
                            (category.Children[i].IsReadable ? category.Children[i].ToString()
                             : "Node not available"));
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Device control information not available.");
                }
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                result = -1;
            }

            return result;
        }

        // This function acts as the body of the example; please see
        // NodeMapInfo_CSharp example for more in-depth comments on setting up
        // cameras.
        int RunMultipleCameras(ManagedCameraList cameraList)
        {
            int result = 0;
            int camListSize = cameraList.Count;

            // Initialize camera worker threads
            for (int index = 0; index < camListSize; index++)
            {
                // Assign Each Task with GrabImage helper
                BackgroundWorker bgw = new BackgroundWorker();
                bgw.DoWork += DoWork;
                bgw.RunWorkerCompleted += RunWorkerCompleted;
                bgws.Add(bgw);
            }

            // Start all task
            Console.WriteLine("Starting Tasks");
            for (int index = 0; index < camListSize; index++)
            {
                bgws[index].RunWorkerAsync(argument : cameraList[index]);
            }

            // Wait for all threads to finish
            Console.WriteLine("Waiting for tasks");
            workersDoneEvent.WaitOne();

            // Check thread return code for each camera
            Console.WriteLine("Checking error code");
            if (workerHasError)
            {
                Console.WriteLine(
                    "There are errors found with the grab threads.\nPlease check on-screen print outs for error details.");
                result = -1;
            }

            return result;
        }

        // Example entry point; please see Enumeration_CSharp example for more
        // in-depth comments on preparing and cleaning up the system.
        static int Main(string[] args)
        {
            int result = 0;

            Program program = new Program();

            // Since this application saves images in the current folder
            // we must ensure that we have permission to write to this folder.
            // If we do not have permission, fail right away.
            FileStream fileStream;
            try
            {
                fileStream = new FileStream(@"test.txt", FileMode.Create);
                fileStream.Close();
                File.Delete("test.txt");
            }
            catch
            {
                Console.WriteLine("Failed to create file in current folder. Please check permissions.");
                Console.WriteLine("Press enter to exit...");
                Console.ReadLine();
                return -1;
            }

            // Retrieve singleton reference to system object
            ManagedSystem system = new ManagedSystem();

            // Print out current library version
            LibraryVersion spinVersion = system.GetLibraryVersion();
            Console.WriteLine(
                "Spinnaker library version: {0}.{1}.{2}.{3}\n\n",
                spinVersion.major,
                spinVersion.minor,
                spinVersion.type,
                spinVersion.build);

            // Retrieve list of cameras from the system
            ManagedCameraList cameraList = system.GetCameras();

            Console.WriteLine("Number of cameras detected: {0}\n\n", cameraList.Count);

            // Finish if there are no cameras
            if (cameraList.Count == 0)
            {
                // Clear camera list before releasing system
                cameraList.Clear();

                // Release system
                system.Dispose();

                Console.WriteLine("Not enough cameras!");
                Console.WriteLine("Done! Press Enter to exit...");
                Console.ReadLine();

                return -1;
            }

            // Run example on all cameras
            Console.WriteLine("Running example for all cameras...");

            try
            {
                result = program.RunMultipleCameras(cameraList);
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                result = -1;
            }

            Console.WriteLine("Example complete...\n");

            // Clear camera list before releasing system
            cameraList.Clear();

            // Release system
            system.Dispose();

            Console.WriteLine("\nDone! Press Enter to exit...");
            Console.ReadLine();

            return result;
        }
    }
}
