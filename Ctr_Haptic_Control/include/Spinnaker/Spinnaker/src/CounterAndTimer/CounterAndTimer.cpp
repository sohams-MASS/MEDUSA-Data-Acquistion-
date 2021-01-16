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
 *   @example CounterAndTimer.cpp
 *
 *   @brief CounterAndTimer.cpp shows how to setup a Pulse Width Modulation (PWM)
 *   signal using counters and timers. The camera will output the PWM signal via
 *   strobe, and capture images at a rate defined by the PWM signal as well.
 *   Users should take care to use a PWM signal within the camera's max
 *   framerate (by default, the PWM signal is set to 50 Hz).
 *
 *   Counter and Timer functionality is only available for BFS and Oryx Cameras.
 *   For details on the hardware setup, see our kb article, "Using Counter and
 *   Timer Control";
 * https://www.flir.com/support-center/iis/machine-vision/application-note/using-counter-and-timer-control
 */

#include "Spinnaker.h"
#include "SpinGenApi/SpinnakerGenApi.h"
#include <iostream>
#include <sstream>

using namespace Spinnaker;
using namespace Spinnaker::GenApi;
using namespace Spinnaker::GenICam;
using namespace std;

// This function prints the device information of the camera from the transport
// layer; please see NodeMapInfo example for more in-depth comments on printing
// device information from the nodemap.
int PrintDeviceInfo(INodeMap& nodeMap)
{
    int result = 0;

    cout << endl << "*** DEVICE INFORMATION ***" << endl << endl;

    try
    {
        FeatureList_t features;
        CCategoryPtr category = nodeMap.GetNode("DeviceInformation");
        if (IsAvailable(category) && IsReadable(category))
        {
            category->GetFeatures(features);

            FeatureList_t::const_iterator it;
            for (it = features.begin(); it != features.end(); ++it)
            {
                CNodePtr pfeatureNode = *it;
                cout << pfeatureNode->GetName() << " : ";
                CValuePtr pValue = (CValuePtr)pfeatureNode;
                cout << (IsReadable(pValue) ? pValue->ToString() : "Node not readable");
                cout << endl;
            }
        }
        else
        {
            cout << "Device control information not available." << endl;
        }
    }
    catch (Spinnaker::Exception& e)
    {
        cout << "Error: " << e.what() << endl;
        result = -1;
    }

    return result;
}

// This function configures the camera to setup a Pulse Width Modulation signal using
// Counter and Timer functionality.  By default, the PWM signal will be set to run at
// 50hz, with a duty cycle of 70%.
int SetupCounterAndTimer(INodeMap& nodeMap)
{
    int result = 0;

    cout << endl << "Configuring Pulse Width Modulation signal" << endl;

    try
    {
        // Set Counter Selector to Counter 0
        CEnumerationPtr ptrCounterSelector = nodeMap.GetNode("CounterSelector");

        // Check to see if camera supports Counter and Timer functionality
        if (!IsAvailable(ptrCounterSelector))
        {
            cout << endl << "Camera does not support Counter and Timer Functionality.  Aborting..." << endl;
            return -1;
        }

        if (!IsAvailable(ptrCounterSelector) || !IsWritable(ptrCounterSelector))
        {
            cout << "Unable to set Counter Selector (enum retrieval). Aborting..." << endl << endl;
            return -1;
        }

        CEnumEntryPtr ptrCounter0 = ptrCounterSelector->GetEntryByName("Counter0");
        if (!IsAvailable(ptrCounter0) || !IsReadable(ptrCounter0))
        {
            cout << "Unable to set Counter Selector (entry retrieval). Aborting..." << endl << endl;
            return -1;
        }

        int64_t counter0 = ptrCounter0->GetValue();

        ptrCounterSelector->SetIntValue(counter0);

        // Set Counter Event Source to MHzTick
        CEnumerationPtr ptrCounterEventSource = nodeMap.GetNode("CounterEventSource");
        if (!IsAvailable(ptrCounterEventSource) || !IsWritable(ptrCounterEventSource))
        {
            cout << "Unable to set Counter Event Source (enum retrieval). Aborting..." << endl << endl;
            return -1;
        }

        CEnumEntryPtr ptrCounterEventSourceMHzTick = ptrCounterEventSource->GetEntryByName("MHzTick");
        if (!IsAvailable(ptrCounterEventSourceMHzTick) || !IsReadable(ptrCounterEventSourceMHzTick))
        {
            cout << "Unable to set Counter Event Source (entry retrieval). Aborting..." << endl << endl;
            return -1;
        }

        int64_t counterEventSourceMHzTick = ptrCounterEventSourceMHzTick->GetValue();

        ptrCounterEventSource->SetIntValue(counterEventSourceMHzTick);

        // Set Counter Duration to 14000
        CIntegerPtr ptrCounterDuration = nodeMap.GetNode("CounterDuration");
        if (!IsAvailable(ptrCounterDuration) || !IsWritable(ptrCounterDuration))
        {
            cout << "Unable to set Counter Duration (integer retrieval). Aborting..." << endl << endl;
            return -1;
        }

        ptrCounterDuration->SetValue(14000);

        // Set Counter Delay to 6000
        CIntegerPtr ptrCounterDelay = nodeMap.GetNode("CounterDelay");
        if (!IsAvailable(ptrCounterDelay) || !IsWritable(ptrCounterDelay))
        {
            cout << "Unable to set Counter Delay (integer retrieval). Aborting..." << endl << endl;
            return -1;
        }

        ptrCounterDelay->SetValue(6000);

        // Determine Duty Cycle of PWM signal
        int64_t dutyCycle = (int64_t)(
            (float)ptrCounterDuration->GetValue() /
            ((float)ptrCounterDuration->GetValue() + (float)ptrCounterDelay->GetValue()) * 100);

        cout << endl << "The duty cycle has been set to " << dutyCycle << "%" << endl;

        // Determine pulse rate of PWM signal
        int64_t pulseRate =
            (int64_t)(1000000 / ((float)ptrCounterDuration->GetValue() + (float)ptrCounterDelay->GetValue()));

        cout << endl << "The pulse rate has been set to " << pulseRate << "Hz" << endl;

        // Set Counter Trigger Source to Frame Trigger Wait
        CEnumerationPtr ptrCounterTriggerSource = nodeMap.GetNode("CounterTriggerSource");
        if (!IsAvailable(ptrCounterTriggerSource) || !IsWritable(ptrCounterTriggerSource))
        {
            cout << "Unable to set Counter Trigger Source (enum retrieval). Aborting..." << endl << endl;
            return -1;
        }

        CEnumEntryPtr ptrCounterTriggerSourceFTW = ptrCounterTriggerSource->GetEntryByName("FrameTriggerWait");
        if (!IsAvailable(ptrCounterTriggerSourceFTW) || !IsReadable(ptrCounterTriggerSourceFTW))
        {
            cout << "Unable to set Counter Trigger Source (entry retrieval). Aborting..." << endl << endl;
            return -1;
        }

        int64_t counterTriggerSourceFTW = ptrCounterTriggerSourceFTW->GetValue();

        ptrCounterTriggerSource->SetIntValue(counterTriggerSourceFTW);

        // Set Counter Trigger Activation to Level High
        CEnumerationPtr ptrCounterTriggerActivation = nodeMap.GetNode("CounterTriggerActivation");
        if (!IsAvailable(ptrCounterTriggerActivation) || !IsWritable(ptrCounterTriggerActivation))
        {
            cout << "Unable to set Counter Trigger Activation (enum retrieval). Aborting..." << endl << endl;
            return -1;
        }

        CEnumEntryPtr ptrCounterTriggerActivationLH = ptrCounterTriggerActivation->GetEntryByName("LevelHigh");
        if (!IsAvailable(ptrCounterTriggerActivationLH) || !IsReadable(ptrCounterTriggerActivationLH))
        {
            cout << "Unable to set Counter Trigger Activation (entry retrieval). Aborting..." << endl << endl;
            return -1;
        }

        int64_t counterTriggerActivationLH = ptrCounterTriggerActivationLH->GetValue();

        ptrCounterTriggerActivation->SetIntValue(counterTriggerActivationLH);
    }
    catch (Spinnaker::Exception& e)
    {
        cout << "Error: " << e.what() << endl;
        result = -1;
    }

    return result;
}

// Configure GPIO to output the PWM signal
int ConfigureDigitalIO(INodeMap& nodeMap)
{
    int result = 0;
    const gcstring cameraFamilyBFS = "BFS";
    const gcstring cameraFamilyOryx = "ORX";

    cout << endl << "Configuring GPIO strobe output" << endl;

    try
    {
        // Determine camera family
        CStringPtr ptrDeviceModelName = nodeMap.GetNode("DeviceModelName");
        if (!IsAvailable(ptrDeviceModelName) || !IsReadable(ptrDeviceModelName))
        {
            cout << "Unable to determine camera family. Aborting..." << endl << endl;
            return -1;
        }

        gcstring cameraModel = ptrDeviceModelName->GetValue();

        // Set Line Selector
        CEnumerationPtr ptrLineSelector = nodeMap.GetNode("LineSelector");
        if (!IsAvailable(ptrLineSelector) || !IsWritable(ptrLineSelector))
        {
            cout << "Unable to set Line Selector (enum retrieval). Aborting..." << endl << endl;
            return -1;
        }

        if (cameraModel.find(cameraFamilyBFS) != std::string::npos)
        {
            CEnumEntryPtr ptrLineSelectorLine1 = ptrLineSelector->GetEntryByName("Line1");
            if (!IsAvailable(ptrLineSelectorLine1) || !IsReadable(ptrLineSelectorLine1))
            {
                cout << "Unable to set Line Selector (entry retrieval). Aborting..." << endl << endl;
                return -1;
            }

            int64_t lineSelectorLine1 = ptrLineSelectorLine1->GetValue();

            ptrLineSelector->SetIntValue(lineSelectorLine1);
        }
        else if (cameraModel.find(cameraFamilyOryx) != std::string::npos)
        {
            CEnumEntryPtr ptrLineSelectorLine2 = ptrLineSelector->GetEntryByName("Line2");
            if (!IsAvailable(ptrLineSelectorLine2) || !IsReadable(ptrLineSelectorLine2))
            {
                cout << "Unable to set Line Selector (entry retrieval). Aborting..." << endl << endl;
                return -1;
            }

            int64_t lineSelectorLine2 = ptrLineSelectorLine2->GetValue();

            ptrLineSelector->SetIntValue(lineSelectorLine2);

            // Set Line Mode to output
            CEnumerationPtr ptrLineMode = nodeMap.GetNode("LineMode");
            if (!IsAvailable(ptrLineMode) || !IsWritable(ptrLineMode))
            {
                cout << "Unable to set Line Mode (enum retrieval). Aborting..." << endl << endl;
                return -1;
            }

            CEnumEntryPtr ptrLineModeOutput = ptrLineMode->GetEntryByName("Output");
            if (!IsAvailable(ptrLineModeOutput) || !IsReadable(ptrLineModeOutput))
            {
                cout << "Unable to set Line Mode (entry retrieval). Aborting..." << endl << endl;
                return -1;
            }

            int64_t lineModeOutput = ptrLineModeOutput->GetValue();

            ptrLineMode->SetIntValue(lineModeOutput);
        }

        // Set Line Source for Selected Line to Counter 0 Active
        CEnumerationPtr ptrLineSource = nodeMap.GetNode("LineSource");
        if (!IsAvailable(ptrLineSource) || !IsWritable(ptrLineSource))
        {
            cout << "Unable to set Line Source (enum retrieval). Aborting..." << endl << endl;
            return -1;
        }

        CEnumEntryPtr ptrLineSourceCounter0Active = ptrLineSource->GetEntryByName("Counter0Active");
        if (!IsAvailable(ptrLineSourceCounter0Active) || !IsReadable(ptrLineSourceCounter0Active))
        {
            cout << "Unable to set Line Source (entry retrieval). Aborting..." << endl << endl;
            return -1;
        }

        int64_t LineSourceCounter0Active = ptrLineSourceCounter0Active->GetValue();

        ptrLineSource->SetIntValue(LineSourceCounter0Active);

        if (cameraModel.find(cameraFamilyBFS) != std::string::npos)
        {
            // Change Line Selector to Line 2 and Enable 3.3 Voltage Rail
            CEnumEntryPtr ptrLineSelectorLine2 = ptrLineSelector->GetEntryByName("Line2");
            if (!IsAvailable(ptrLineSelectorLine2) || !IsReadable(ptrLineSelectorLine2))
            {
                cout << "Unable to set Line Selector (entry retrieval). Aborting..." << endl << endl;
                return -1;
            }

            int64_t lineSelectorLine2 = ptrLineSelectorLine2->GetValue();

            ptrLineSelector->SetIntValue(lineSelectorLine2);

            CBooleanPtr ptrVoltageEnable = nodeMap.GetNode("V3_3Enable");
            if (!IsAvailable(ptrVoltageEnable) || !IsWritable(ptrVoltageEnable))
            {
                cout << "Unable to set Voltage Enable (boolean retrieval). Aborting..." << endl << endl;
                return -1;
            }

            ptrVoltageEnable->SetValue(true);
        }
    }
    catch (Spinnaker::Exception& e)
    {
        cout << "Error: " << e.what() << endl;
        result = -1;
    }

    return result;
}

// This function configures the camera to set a manual exposure value and enables
// camera to be triggered by the PWM signal.
int ConfigureExposureandTrigger(INodeMap& nodeMap)
{
    int result = 0;

    cout << endl << "Configuring Exposure and Trigger" << endl;

    try
    {
        // Turn off auto exposure
        CEnumerationPtr ptrExposureAuto = nodeMap.GetNode("ExposureAuto");
        if (!IsAvailable(ptrExposureAuto) || !IsWritable(ptrExposureAuto))
        {
            cout << "Unable to set Exposure Auto (enumeration retrieval). Aborting..." << endl << endl;
            return -1;
        }

        CEnumEntryPtr ptrExposureAutoOff = ptrExposureAuto->GetEntryByName("Off");
        if (!IsAvailable(ptrExposureAutoOff) || !IsReadable(ptrExposureAutoOff))
        {
            cout << "Unable to set Exposure Auto (entry retrieval). Aborting..." << endl << endl;
            return -1;
        }

        int64_t exposureAutoOff = ptrExposureAutoOff->GetValue();

        ptrExposureAuto->SetIntValue(exposureAutoOff);

        // Set Exposure Time to less than 1/50th of a second (5000 us is used as an example)
        CFloatPtr ptrExposureTime = nodeMap.GetNode("ExposureTime");
        if (!IsAvailable(ptrExposureTime) || !IsWritable(ptrExposureTime))
        {
            cout << "Unable to set Exposure Time (float retrieval). Aborting..." << endl << endl;
            return -1;
        }

        ptrExposureTime->SetValue(5000);

        // Ensure trigger mode is off
        //
        // *** NOTES ***
        // The trigger must be disabled in order to configure
        //
        CEnumerationPtr ptrTriggerMode = nodeMap.GetNode("TriggerMode");
        if (!IsAvailable(ptrTriggerMode) || !IsReadable(ptrTriggerMode))
        {
            cout << "Unable to disable trigger mode (node retrieval). Aborting..." << endl;
            return -1;
        }

        CEnumEntryPtr ptrTriggerModeOff = ptrTriggerMode->GetEntryByName("Off");
        if (!IsAvailable(ptrTriggerModeOff) || !IsReadable(ptrTriggerModeOff))
        {
            cout << "Unable to disable trigger mode (enum entry retrieval). Aborting..." << endl;
            return -1;
        }

        ptrTriggerMode->SetIntValue(ptrTriggerModeOff->GetValue());

        // Set Trigger Source to Counter 0 Start
        CEnumerationPtr ptrTriggerSource = nodeMap.GetNode("TriggerSource");
        if (!IsAvailable(ptrTriggerSource) || !IsWritable(ptrTriggerSource))
        {
            cout << "Unable to set trigger mode (node retrieval). Aborting..." << endl;
            return -1;
        }

        CEnumEntryPtr ptrTriggerSourceCounter0Start = ptrTriggerSource->GetEntryByName("Counter0Start");
        if (!IsAvailable(ptrTriggerSourceCounter0Start) || !IsReadable(ptrTriggerSourceCounter0Start))
        {
            cout << "Unable to set trigger mode (enum entry retrieval). Aborting..." << endl;
            return -1;
        }

        ptrTriggerSource->SetIntValue(ptrTriggerSourceCounter0Start->GetValue());

        // Set Trigger Overlap to Readout
        CEnumerationPtr ptrTriggerOverlap = nodeMap.GetNode("TriggerOverlap");
        if (!IsAvailable(ptrTriggerOverlap) || !IsWritable(ptrTriggerOverlap))
        {
            cout << "Unable to set Trigger Overlap (enum retrieval). Aborting..." << endl << endl;
            return -1;
        }

        CEnumEntryPtr ptrTriggerOverlapRO = ptrTriggerOverlap->GetEntryByName("ReadOut");
        if (!IsAvailable(ptrTriggerOverlapRO) || !IsReadable(ptrTriggerOverlapRO))
        {
            cout << "Unable to set TriggerOverlap (entry retrieval). Aborting..." << endl << endl;
            return -1;
        }

        int64_t TriggerOverlapRO = ptrTriggerOverlapRO->GetValue();

        ptrTriggerOverlap->SetIntValue(TriggerOverlapRO);

        // Turn trigger mode on
        CEnumEntryPtr ptrTriggerModeOn = ptrTriggerMode->GetEntryByName("On");
        if (!IsAvailable(ptrTriggerModeOn) || !IsReadable(ptrTriggerModeOn))
        {
            cout << "Unable to enable trigger mode (enum entry retrieval). Aborting..." << endl;
            return -1;
        }

        ptrTriggerMode->SetIntValue(ptrTriggerModeOn->GetValue());
    }
    catch (Spinnaker::Exception& e)
    {
        cout << "Error: " << e.what() << endl;
        result = -1;
    }

    return result;
}

// This function acquires and saves 10 images from a device; please see
// Acquisition example for more in-depth comments on acquiring images.
int AcquireImages(CameraPtr pCam, INodeMap& nodeMap, INodeMap& nodeMapTLDevice)
{
    int result = 0;

    cout << endl << "*** IMAGE ACQUISITION ***" << endl << endl;

    try
    {
        // Set acquisition mode to continuous
        CEnumerationPtr ptrAcquisitionMode = nodeMap.GetNode("AcquisitionMode");
        if (!IsAvailable(ptrAcquisitionMode) || !IsWritable(ptrAcquisitionMode))
        {
            cout << "Unable to set acquisition mode to continuous (node retrieval). Aborting..." << endl << endl;
            return -1;
        }

        CEnumEntryPtr ptrAcquisitionModeContinuous = ptrAcquisitionMode->GetEntryByName("Continuous");
        if (!IsAvailable(ptrAcquisitionModeContinuous) || !IsReadable(ptrAcquisitionModeContinuous))
        {
            cout << "Unable to set acquisition mode to continuous (entry 'continuous' retrieval). Aborting..." << endl
                 << endl;
            return -1;
        }

        int64_t acquisitionModeContinuous = ptrAcquisitionModeContinuous->GetValue();

        ptrAcquisitionMode->SetIntValue(acquisitionModeContinuous);

        cout << "Acquisition mode set to continuous..." << endl;

        // Begin acquiring images
        pCam->BeginAcquisition();

        cout << "Acquiring images..." << endl;

        // Retrieve device serial number for filename
        gcstring deviceSerialNumber("");

        CStringPtr ptrStringSerial = nodeMapTLDevice.GetNode("DeviceSerialNumber");
        if (IsAvailable(ptrStringSerial) && IsReadable(ptrStringSerial))
        {
            deviceSerialNumber = ptrStringSerial->GetValue();

            cout << "Device serial number retrieved as " << deviceSerialNumber << "..." << endl;
        }
        cout << endl;

        // Retrieve, convert, and save images
        const unsigned int k_numImages = 10;

        for (unsigned int imageCnt = 0; imageCnt < k_numImages; imageCnt++)
        {
            try
            {
                // Retrieve next received image and ensure image completion
                ImagePtr pResultImage = pCam->GetNextImage(1000);

                if (pResultImage->IsIncomplete())
                {
                    cout << "Image incomplete with image status " << pResultImage->GetImageStatus() << "..." << endl
                         << endl;
                }
                else
                {
                    // Print image information
                    cout << "Grabbed image " << imageCnt << ", width = " << pResultImage->GetWidth()
                         << ", height = " << pResultImage->GetHeight() << endl;

                    // Convert image to mono 8
                    ImagePtr convertedImage = pResultImage->Convert(PixelFormat_Mono8, HQ_LINEAR);

                    // Create a unique filename
                    ostringstream filename;

                    filename << "CounterAndTimer-";
                    if (deviceSerialNumber != "")
                    {
                        filename << deviceSerialNumber.c_str() << "-";
                    }
                    filename << imageCnt << ".jpg";

                    // Save image
                    convertedImage->Save(filename.str().c_str());

                    cout << "Image saved at " << filename.str() << endl;
                }

                // Release image
                pResultImage->Release();

                cout << endl;
            }
            catch (Spinnaker::Exception& e)
            {
                cout << "Error: " << e.what() << endl;
                result = -1;
            }
        }

        // End acquisition
        pCam->EndAcquisition();
    }
    catch (Spinnaker::Exception& e)
    {
        cout << "Error: " << e.what() << endl;
        result = -1;
    }

    return result;
}

// This function returns the camera to a normal state by turning off trigger
// mode.
//
// *** NOTES ***
// This function turns off trigger mode, but does not change the trigger
// source.
//
int ResetTrigger(INodeMap& nodeMap)
{
    int result = 0;

    try
    {
        // Turn trigger mode back off
        CEnumerationPtr ptrTriggerMode = nodeMap.GetNode("TriggerMode");
        if (!IsAvailable(ptrTriggerMode) || !IsReadable(ptrTriggerMode))
        {
            cout << "Unable to disable trigger mode (node retrieval). Non-fatal error..." << endl;
            return -1;
        }

        CEnumEntryPtr ptrTriggerModeOff = ptrTriggerMode->GetEntryByName("Off");
        if (!IsAvailable(ptrTriggerModeOff) || !IsReadable(ptrTriggerModeOff))
        {
            cout << "Unable to disable trigger mode (enum entry retrieval). Non-fatal error..." << endl;
            return -1;
        }

        ptrTriggerMode->SetIntValue(ptrTriggerModeOff->GetValue());
    }
    catch (Spinnaker::Exception& e)
    {
        cout << "Error: " << e.what() << endl;
        result = -1;
    }

    return result;
}

// This function acts as the body of the example; please see the NodeMapInfo example
// for more in-depth comments on setting up cameras.
int RunSingleCamera(CameraPtr pCam)
{
    int result = 0;
    int err = 0;
    try
    {
        // Retrieve TL device nodemap and print device information
        INodeMap& nodeMapTLDevice = pCam->GetTLDeviceNodeMap();

        result = PrintDeviceInfo(nodeMapTLDevice);

        // Initialize camera
        pCam->Init();

        // Retrieve GenICam nodemap
        INodeMap& nodeMap = pCam->GetNodeMap();

        // Configure Counter and Timer setup
        err = SetupCounterAndTimer(nodeMap);
        if (err < 0)
        {
            return err;
        }

        // Configure DigitalIO (GPIO output)
        err = ConfigureDigitalIO(nodeMap);
        if (err < 0)
        {
            return err;
        }

        // Configure Exposure and Trigger
        err = ConfigureExposureandTrigger(nodeMap);
        if (err < 0)
        {
            return err;
        }

        // Acquire images
        result = result | AcquireImages(pCam, nodeMap, nodeMapTLDevice);

        // Reset trigger
        result = result | ResetTrigger(nodeMap);

        // Deinitialize camera
        pCam->DeInit();
    }
    catch (Spinnaker::Exception& e)
    {
        cout << "Error: " << e.what() << endl;
        result = -1;
    }

    return result;
}

// Example entry point; please see Enumeration example for more in-depth
// comments on preparing and cleaning up the system.
int main(int /*argc*/, char** /*argv*/)
{
    // Since this application saves images in the current folder
    // we must ensure that we have permission to write to this folder.
    // If we do not have permission, fail right away.
    FILE* tempFile = fopen("test.txt", "w+");
    if (tempFile == nullptr)
    {
        cout << "Failed to create file in current folder.  Please check "
                "permissions."
             << endl;
        cout << "Press Enter to exit..." << endl;
        getchar();
        return -1;
    }
    fclose(tempFile);
    remove("test.txt");

    int result = 0;

    // Print application build information
    cout << "Application build date: " << __DATE__ << " " << __TIME__ << endl << endl;

    // Retrieve singleton reference to system object
    SystemPtr system = System::GetInstance();

    // Retrieve list of cameras from the system
    CameraList camList = system->GetCameras();

    unsigned int numCameras = camList.GetSize();

    cout << "Number of cameras detected: " << numCameras << endl << endl;

    // Finish if there are no cameras
    if (numCameras == 0)
    {
        // Clear camera list before releasing system
        camList.Clear();

        // Release system
        system->ReleaseInstance();

        cout << "Not enough cameras!" << endl;
        cout << "Done! Press Enter to exit..." << endl;
        getchar();

        return -1;
    }

    //
    // Create shared pointer to camera
    //
    // *** NOTES ***
    // The CameraPtr object is a shared pointer, and will generally clean itself
    // up upon exiting its scope. However, if a shared pointer is created in the
    // same scope that a system object is explicitly released (i.e. this scope),
    // the reference to the shared point must be broken manually.
    //
    // *** LATER ***
    // Shared pointers can be terminated manually by assigning them to nullptr.
    // This prevents an exception from being thrown when releasing the system.
    //
    CameraPtr pCam = nullptr;

    // Run example on each camera
    for (unsigned int i = 0; i < numCameras; i++)
    {
        // Select camera
        pCam = camList.GetByIndex(i);

        cout << endl << "Running example for camera " << i << "..." << endl;

        // Run example
        result = result | RunSingleCamera(pCam);

        cout << endl << "Camera " << i << " example complete..." << endl << endl;
    }

    //
    // Release reference to the camera
    //
    // *** NOTES ***
    // Had the CameraPtr object been created within the for-loop, it would not
    // be necessary to manually break the reference because the shared pointer
    // would have automatically cleaned itself up upon exiting the loop.
    //
    pCam = nullptr;

    // Clear camera list before releasing system
    camList.Clear();

    // Release system
    system->ReleaseInstance();

    cout << endl << "Done! Press Enter to exit..." << endl;
    getchar();

    return result;
}
