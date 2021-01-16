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
 *  @example Inference_CSharp.cs
 *
 *  @brief Inference_CSharp.cs shows how to get inference data from chunk
 *  data on an image.
 *
 *  Inference is only available for Firefly deep learning cameras.
 *
 *  It can also be helpful to familiarize yourself with the ChunkData example.
 */

using System;
using System.IO;
using SpinnakerNET.GenApi;
using SpinnakerNET;

namespace Inference_CSharp
{
    class Program
    {
        // Use the following enum and global constant to select whether inference network
        // type is Detection or Classification.
        enum InferenceNetworkType
        {
            Detection,
            Classification
        }

        static InferenceNetworkType chosenInferenceNetworkType = InferenceNetworkType.Detection;

        // This function enables or disables the given chunk data type based on
        // the specified entry name.
        static int SetChunkEnable(INodeMap nodeMap, string entryName, bool enable)
        {
            int result = 0;
            IEnum iChunkSelector = nodeMap.GetNode<IEnum>("ChunkSelector");
            IEnumEntry iEntry = iChunkSelector.GetEntryByName(entryName);
            if (iEntry == null || !iEntry.IsReadable)
            {
                return -1;
            }

            IBool iChunkEnable = nodeMap.GetNode<IBool>("ChunkEnable");
            if (iChunkEnable == null)
            {
                Console.WriteLine("{0} not available", entryName);
                return -1;
            }
            if (enable)
            {
                if (iChunkEnable.Value)
                {
                    Console.WriteLine("{0} enabled", entryName);
                }
                else if (iChunkEnable.IsWritable)
                {
                    iChunkEnable.Value = true;
                    Console.WriteLine("{0} enabled", entryName);
                }
                else
                {
                    Console.WriteLine("{0} not writable", entryName);
                    result = -1;
                }
            }
            else
            {
                if (!iChunkEnable.Value)
                {
                    Console.WriteLine("{0} disabled", entryName);
                }
                else if (iChunkEnable.IsWritable)
                {
                    iChunkEnable.Value = false;
                    Console.WriteLine("{0} disabled", entryName);
                }
                else
                {
                    Console.WriteLine("{0} not writable", entryName);
                    result = -1;
                }
            }

            return result;
        }

        // This function configures the camera to add chunk data to each image.
        // It does this by enabling each type of chunk data before enabling
        // chunk data mode. When chunk data is turned on, the data is made
        // available in both the nodemap and each image.
        static int ConfigureChunkData(INodeMap nodeMap)
        {
            int result = 0;

            Console.WriteLine("\n*** CONFIGURING CHUNK DATA INFERENCE ***\n");

            try
            {
                // Enable inference
                IBool iInferenceEnable = nodeMap.GetNode<IBool>("InferenceEnable");
                if (iInferenceEnable == null || !iInferenceEnable.IsWritable)
                {
                    Console.WriteLine("Cannot enable inference. Aborting...");
                    return -1;
                }

                iInferenceEnable.Value = true;
                Console.WriteLine("Inference enabled...");

                // Set Network Type to Detection
                IEnum iInferenceNetworkTypeSelector = nodeMap.GetNode<IEnum>("InferenceNetworkTypeSelector");
                if (iInferenceNetworkTypeSelector == null || !iInferenceNetworkTypeSelector.IsReadable)
                {
                    Console.WriteLine("Inference network type selector not available. Aborting...");
                    return -1;
                }

                string networkTypeString =
                    (chosenInferenceNetworkType == InferenceNetworkType.Detection) ? "Detection" : "Classification";

                IEnumEntry iInferenceNetworkType = iInferenceNetworkTypeSelector.GetEntryByName(networkTypeString);
                if (iInferenceNetworkType == null || !iInferenceNetworkType.IsReadable)
                {
                    Console.WriteLine(
                        "Unable to set inference network type to {0} (enum entry retrieval). Aborting...\n",
                        networkTypeString);
                    return -1;
                }
                iInferenceNetworkTypeSelector.Value = iInferenceNetworkType.Symbolic;

                //
                // Activate chunk mode
                //
                // *** NOTES ***
                // Once enabled, chunk data will be available at the end of the
                // payload of every image captured until it is disabled. Chunk
                // data can also be retrieved from the nodemap.
                //
                IBool iChunkModeActive = nodeMap.GetNode<IBool>("ChunkModeActive");
                if (iChunkModeActive == null || !iChunkModeActive.IsWritable)
                {
                    Console.WriteLine("Cannot active chunk mode. Aborting...");
                    return -1;
                }

                iChunkModeActive.Value = true;

                Console.WriteLine("Chunk mode activated...");

                // Enable inference related chunks in chunk data

                // Retrieve selector node
                IEnum iChunkSelector = nodeMap.GetNode<IEnum>("ChunkSelector");
                if (iChunkSelector == null || !iChunkSelector.IsReadable)
                {
                    Console.WriteLine("Chunk selector not available. Aborting...");
                    return -1;
                }

                // Enable chunk data inference Frame Id
                result = SetChunkEnable(nodeMap, "InferenceFrameId", true);
                if (result == -1)
                {
                    Console.Write("Unable to enable Inference Frame Id chunk data. Aborting...");
                    return result;
                }

                if (chosenInferenceNetworkType == InferenceNetworkType.Detection)
                {
                    // Detection network type

                    // Enable chunk data inference bounding box
                    result = SetChunkEnable(nodeMap, "InferenceBoundingBoxResult", true);
                    if (result == -1)
                    {
                        Console.Write("Unable to enable Inference Bounding Box chunk data. Aborting...");
                        return result;
                    }
                }
                else
                {
                    // Classification network type

                    // Enable chunk data inference result
                    result = SetChunkEnable(nodeMap, "InferenceResult", true);
                    if (result == -1)
                    {
                        Console.Write("Unable to enable Inference Result chunk data. Aborting...");
                        return result;
                    }

                    // Enable chunk data inference confidence
                    result = SetChunkEnable(nodeMap, "InferenceConfidence", true);
                    if (result == -1)
                    {
                        Console.Write("Unable to enable Inference Confidence chunk data. Aborting...");
                        return result;
                    }
                }

                Console.WriteLine();
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                result = -1;
            }

            return result;
        }

        // This function displays a select amount of chunk data from the image.
        // Unlike accessing chunk data via the nodemap, there is no way to loop
        // through all available data.
        static int DisplayChunkData(IManagedImage managedImage)
        {
            int result = 0;

            Console.WriteLine("Printing chunk data from image...");

            try
            {
                //
                // Retrieve chunk data from image
                //
                // *** NOTES ***
                // When retrieving chunk data from an image, the data is stored
                // in a a ChunkData object and accessed with getter functions.
                //
                ManagedChunkData managedChunkData = managedImage.ChunkData;

                long inferenceFrameID = managedChunkData.FrameID;
                Console.WriteLine("\tInference frame ID: {0}", inferenceFrameID);

                if (chosenInferenceNetworkType == InferenceNetworkType.Detection)
                {
                    ManagedInferenceBoundingBoxResult boxResult = managedChunkData.InferenceBoundingBoxResult;
                    Console.WriteLine("\tInference bounding box result");

                    short boxCount = boxResult.BoxCount;
                    if (boxCount == 0)
                    {
                        Console.WriteLine("\tNo bounding box");
                    }

                    for (short i = 0; i < boxResult.BoxCount; ++i)
                    {
                        ManagedInferenceBoundingBox box = boxResult.get_BoxAt(i);
                        switch (box.BoxType)
                        {
                            case InferenceBoundingBoxType.RECTANGLE:
                                Console.WriteLine(
                                    "\t\tBox[{0}]: Class {1} - {2} - Rectangle (X={3}, Y={4}, W={5}, H={6})",
                                    i,
                                    box.ClassId,
                                    box.Confidence * 100,
                                    box.RectangleTopLeftX,
                                    box.RectangleTopLeftY,
                                    box.RectangleBottomRightX - box.RectangleTopLeftX,
                                    box.RectangleBottomRightY - box.RectangleTopLeftY);
                                break;
                            case InferenceBoundingBoxType.CIRCLE:
                                Console.WriteLine(
                                    "\t\tBox[{0}]: Class {1} - {2} Circle (X={3}, Y={4}, R={5})",
                                    i,
                                    box.ClassId,
                                    box.Confidence * 100,
                                    box.CircleCenterX,
                                    box.CircleCenterY,
                                    box.CircleRadius);
                                break;
                            case InferenceBoundingBoxType.ROTATED_RECTANGLE:
                                Console.WriteLine(
                                    "\t\tBox[{0}]: Class {1} - {2}Rotated Rectangle (X1={3}, Y1={4}, X2={5}, Y2={6}, angle={7})",
                                    i,
                                    box.ClassId,
                                    box.Confidence * 100,
                                    box.RotatedRectangleTopLeftX,
                                    box.RotatedRectangleTopLeftY,
                                    box.RotatedRectangleBottomRightX,
                                    box.RotatedRectangleBottomRightY,
                                    box.RotatedRectangleRotationAngle);
                                break;
                            default:
                                Console.WriteLine(
                                    "\t\tBox[{0}]: Class {1} - {2} Unknown bounding box type (not supported)",
                                    i,
                                    box.ClassId,
                                    box.Confidence * 100);
                                break;
                        }
                    }
                }
                else
                {
                    long inferenceResult = managedChunkData.InferenceResult;
                    Console.WriteLine("\tInference result: {0}", inferenceResult);

                    double inferenceConfidence = managedChunkData.InferenceConfidence;
                    Console.WriteLine("\tInference confidence: {0} %", inferenceConfidence * 100);
                }
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                result = -1;
            }

            return result;
        }

        // This function displays all available chunk data by looping through
        // the chunk data category node on the nodemap.
        static int DisplayChunkData(INodeMap nodeMap)
        {
            int result = 0;

            Console.WriteLine("Printing chunk data from nodemap...");

            try
            {
                //
                // Retrieve chunk data information nodes
                //
                // *** NOTES ***
                // As well as being written into the payload of the image, chunk
                // data is accessible on the GenICam nodemap. Insofar as chunk
                // data is enabled, it is available from both sources.
                //
                ICategory iChunkDataControl = nodeMap.GetNode<ICategory>("ChunkDataControl");
                if (iChunkDataControl == null || !iChunkDataControl.IsReadable)
                {
                    Console.WriteLine("Chunk selector not available. Aborting...");
                    return -1;
                }

                // Retrieve entries
                var features = iChunkDataControl.Features;

                // Iterate through entries
                for (int i = 0; i < features.Length; i++)
                {
                    IValue currentFeature = features[i];

                    if (currentFeature == null || !currentFeature.IsReadable)
                    {
                        Console.WriteLine("\tNode not available");
                    }
                    //
                    // Print boolean node type value
                    //
                    // *** NOTES ***
                    // Boolean information is manipulated to output the
                    // more -easily identifiable 'true' and 'false' as opposed to
                    // '1' and '0'.
                    //
                    else if (currentFeature.GetType() == typeof (IBool))
                    {
                        IBool boolFeature = (IBool) currentFeature;

                        Console.WriteLine(
                            "\t{0}: {1}",
                            currentFeature.DisplayName,
                            (boolFeature.Value ? "true"
                             : "false"));
                    }
                    //
                    // Print non-boolean node type value
                    //
                    // *** NOTES ***
                    // Features are retrieved from category nodes as value nodes.
                    // Value nodes (or 'IValue') can have their values accessed
                    // and returned as a string using the ToString() method.
                    //
                    else
                    {
                        Console.WriteLine("\t{0}: {1}", currentFeature.DisplayName, currentFeature.ToString());
                    }
                }
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                result = -1;
            }

            return result;
        }

        // This function prints the device information of the camera from the
        // transport layer; please see NodeMapInfo_CSharp example for more
        // in-depth comments on printing device information from the nodemap.
        static int PrintDeviceInfo(INodeMap nodeMap)
        {
            int result = 0;

            try
            {
                Console.WriteLine("\n*** DEVICE INFORMATION ***\n");

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

        // This function acquires and saves 10 images from a device; please see
        // Acquisition_CSharp example for more in-depth comments on the
        // acquisition of images.
        static int AcquireImages(IManagedCamera cam, INodeMap nodeMap, INodeMap nodeMapTLDevice)
        {
            int result = 0;

            Console.WriteLine("\n*** IMAGE ACQUISITION ***\n");

            try
            {
                // Set acquisition mode to continuous
                IEnum iAcquisitionMode = nodeMap.GetNode<IEnum>("AcquisitionMode");
                if (iAcquisitionMode == null || !iAcquisitionMode.IsWritable)
                {
                    Console.WriteLine("Unable to set acquisition mode to continuous (node retrieval). Aborting...\n");
                    return -1;
                }

                IEnumEntry iAcquisitionModeContinuous = iAcquisitionMode.GetEntryByName("Continuous");
                if (iAcquisitionModeContinuous == null || !iAcquisitionMode.IsReadable)
                {
                    Console.WriteLine(
                        "Unable to set acquisition mode to continuous (enum entry retrieval). Aborting...\n");
                    return -1;
                }

                iAcquisitionMode.Value = iAcquisitionModeContinuous.Symbolic;

                Console.WriteLine("Acquisition mode set to continuous...");

                // Begin acquiring images
                cam.BeginAcquisition();

                Console.WriteLine("Acquiring images...");

                // Retrieve device serial number for filename
                String deviceSerialNumber = "";

                IString iDeviceSerialNumber = nodeMapTLDevice.GetNode<IString>("DeviceSerialNumber");
                if (iDeviceSerialNumber != null && iDeviceSerialNumber.IsReadable)
                {
                    deviceSerialNumber = iDeviceSerialNumber.Value;

                    Console.WriteLine("Device serial number retrieved as {0}...", deviceSerialNumber);
                }
                Console.WriteLine();

                // Retrieve, convert, and save images
                const int NumImages = 10;

                for (int imageCnt = 0; imageCnt < NumImages; imageCnt++)
                {
                    try
                    {
                        // Retrieve next received image and ensure image completion
                        using(IManagedImage rawImage = cam.GetNextImage(1000))
                        {
                            if (rawImage.IsIncomplete)
                            {
                                Console.WriteLine("Image incomplete with image status {0}...", rawImage.ImageStatus);
                            }
                            else
                            {
                                // Print image information
                                Console.WriteLine(
                                    "Grabbed image {0}, width = {1}, height = {1}",
                                    imageCnt,
                                    rawImage.Width,
                                    rawImage.Height);

                                // Convert image to mono 8
                                using(IManagedImage convertedImage = rawImage.Convert(PixelFormatEnums.Mono8))
                                {
                                    // Create unique file name
                                    String filename = "ChunkData-CSharp-";
                                    if (deviceSerialNumber != "")
                                    {
                                        filename = filename + deviceSerialNumber + "-";
                                    }
                                    filename = filename + imageCnt + ".jpg";

                                    // Save image
                                    convertedImage.Save(filename);

                                    Console.WriteLine("Image saved at {0}", filename);

                                    // Display chunk data
                                    result = DisplayChunkData(rawImage);
                                }
                            }
                        }
                        Console.WriteLine();
                    }
                    catch (SpinnakerException ex)
                    {
                        Console.WriteLine("Error: {0}", ex.Message);
                        result = -1;
                    }
                }

                // End acquisition
                cam.EndAcquisition();
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                result = -1;
            }

            return result;
        }

        // This function disables each type of chunk data before disabling chunk data mode.
        static int DisableChunkData(INodeMap nodeMap)
        {
            int result = 0;

            try
            {
                // Retrieve selector node
                IEnum iChunkSelector = nodeMap.GetNode<IEnum>("ChunkSelector");
                if (iChunkSelector == null || !iChunkSelector.IsReadable)
                {
                    Console.WriteLine("Chunk selector not available. Aborting...");
                    return -1;
                }

                result = SetChunkEnable(nodeMap, "InferenceFrameId", false);
                if (result == -1)
                {
                    Console.WriteLine("Unable to disable Inference Bounding Box chunk data. Aborting...");
                    return -1;
                }

                if (chosenInferenceNetworkType == InferenceNetworkType.Detection)
                {
                    // Detection network type

                    // Disable chunk data inference bounding box
                    result = SetChunkEnable(nodeMap, "InferenceBoundingBoxResult", false);
                    if (result == -1)
                    {
                        Console.WriteLine("Unable to disable Inference Bounding Box chunk data. Aborting...");
                        return result;
                    }
                }
                else
                {
                    // Disable chunk data inference result
                    result = SetChunkEnable(nodeMap, "InferenceResult", false);
                    if (result == -1)
                    {
                        Console.WriteLine("Unable to disable Inference Result chunk data. Aborting...");
                        return result;
                    }

                    // Disable chunk data inference confidence
                    result = SetChunkEnable(nodeMap, "InferenceConfidence", false);
                    if (result == -1)
                    {
                        Console.WriteLine("Unable to disable Inference Confidence chunk data. Aborting...");
                        return result;
                    }
                }

                Console.WriteLine();

                // Deactivate ChunkMode
                IBool iChunkModeActive = nodeMap.GetNode<IBool>("ChunkModeActive");
                if (iChunkModeActive == null || !iChunkModeActive.IsWritable)
                {
                    Console.WriteLine("Cannot deactive chunk mode. Aborting...");
                    result = -1;
                }
                iChunkModeActive.Value = false;
                Console.WriteLine("Chunk mode deactivated...");

                // Deactivate Inference
                IBool iInferenceEnable = nodeMap.GetNode<IBool>("InferenceEnable");
                if (iInferenceEnable == null || !iInferenceEnable.IsWritable)
                {
                    Console.WriteLine("Cannot disable inference. Aborting...");
                    result = -1;
                }
                iChunkModeActive.Value = false;
                Console.WriteLine("Inference disabled...");
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
        int RunSingleCamera(IManagedCamera cam)
        {
            int result = 0;
            int err = 0;

            try
            {
                // Retrieve TL device nodemap and print device information
                INodeMap nodeMapTLDevice = cam.GetTLDeviceNodeMap();

                result = PrintDeviceInfo(nodeMapTLDevice);

                // Initialize camera
                cam.Init();

                // Retrieve GenICam nodemap
                INodeMap nodeMap = cam.GetNodeMap();

                // Configure chunk data
                err = ConfigureChunkData(nodeMap);
                if (err < 0)
                {
                    return err;
                }

                // Acquire images and display chunk data
                result = result | AcquireImages(cam, nodeMap, nodeMapTLDevice);

                // Disable chunk data
                err = DisableChunkData(nodeMap);
                if (err < 0)
                {
                    return err;
                }

                // Deinitialize camera
                cam.DeInit();
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
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
            ManagedCameraList camList = system.GetCameras();

            Console.WriteLine("Number of cameras detected: {0}\n\n", camList.Count);

            // Finish if there are no cameras
            if (camList.Count == 0)
            {
                // Clear camera list before releasing system
                camList.Clear();

                // Release system
                system.Dispose();

                Console.WriteLine("Not enough cameras!");
                Console.WriteLine("Done! Press Enter to exit...");
                Console.ReadLine();

                return -1;
            }

            // Run example on each camera
            int i = 0;

            foreach(IManagedCamera managedCamera in camList) using(managedCamera)
            {
                Console.WriteLine("Running example for camera {0}...", i);

                try
                {
                    // Run example
                    result = result | program.RunSingleCamera(managedCamera);
                }
                catch (SpinnakerException ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                    result = -1;
                }

                Console.WriteLine("Camera {0} example complete...\n", i++);
            }

            // Clear camera list before releasing system
            camList.Clear();

            // Release system
            system.Dispose();

            Console.WriteLine("\nDone! Press Enter to exit...");
            Console.ReadLine();

            return result;
        }
    }
}
