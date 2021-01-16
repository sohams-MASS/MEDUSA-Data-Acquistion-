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
 *  @example NodeMapInfo_CSharp_QuickSpin.cs
 *
 *  @brief NodeMapInfo_CSharp_QuickSpin.cs shows how to interact with nodes
 *  using the QuickSpin API. QuickSpin is a subset of the Spinnaker library
 *  that allows for simpler node access and control.
 *
 *  This example demonstrates the retrieval of information from both the
 *  transport layer and the camera. Because the focus of this example is node
 *  access, which is where QuickSpin and regular Spinnaker differ, this
 *  example differs from NodeMapInfo_CSharp quite a bit.
 *
 *  A much wider range of topics is covered in the full Spinnaker examples than
 *  in the QuickSpin ones. There are only enough QuickSpin examples to
 *  demonstrate node access and to get started with the API; please see full
 *  Spinnaker examples for further or specific knowledge on a topic.
 */

using System;
using System.Collections.Generic;
using SpinnakerNET;
using SpinnakerNET.GenApi;

namespace NodeMapInfo_CSharp_QuickSpin
{
    class Program
    {
        // This function prints device information from the transport layer.
        static int PrintTransportLayerDeviceInfo(IManagedCamera camera)
        {
            int result = 0;

            try
            {
                //
                // Print device information from the transport layer
                //
                // *** NOTES ***
                // In QuickSpin, accessing device information on the transport
                // layer is accomplished via a camera's TLDevice property. The
                // TLDevice property houses nodes related to general device
                // information such as the three demonstrated below, device
                // access status, XML and GUI paths and locations, and GEV
                // information to name a few. The TLDevice property allows access
                // to nodes that would generally be retrieved through the TL
                // device nodemap in full Spinnaker.
                //
                // Notice that each node is checked for availability and
                // readability prior to value retrieval. Checking for
                // availability and readability (or writability when applicable)
                // whenever a node is accessed is important in terms of error
                // handling. If a node retrieval error occurs but remains
                // unhandled, an exception is thrown.
                //
                // Print device serial number
                Console.Write("Device serial number: ");

                if (camera.TLDevice.DeviceSerialNumber != null && camera.TLDevice.DeviceSerialNumber.IsReadable)
                {
                    Console.WriteLine("{0}", camera.TLDevice.DeviceSerialNumber.ToString());
                }
                else
                {
                    Console.WriteLine("unavailable");
                    result = -1;
                }

                // Print device vendor name
                Console.Write("Device vendor name: ");

                if (camera.TLDevice.DeviceVendorName != null && camera.TLDevice.DeviceVendorName.IsReadable)
                {
                    Console.WriteLine("{0}", camera.TLDevice.DeviceVendorName.ToString());
                }
                else
                {
                    Console.WriteLine("unavailable");
                    result = -1;
                }

                // Print device display name
                Console.Write("Device display name: ");

                if (camera.TLDevice.DeviceDisplayName != null && camera.TLDevice.DeviceDisplayName.IsReadable)
                {
                    Console.WriteLine("{0}\n", camera.TLDevice.DeviceDisplayName.ToString());
                }
                else
                {
                    Console.WriteLine("unavailable\n");
                    result = -1;
                }
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                result = -1;
            }

            return result;
        }

        // This function prints stream information from the transport layer.
        static int PrintTransportLayerStreamInfo(IManagedCamera camera)
        {
            int result = 0;

            try
            {
                //
                // Print stream information from the transport layer
                //
                // *** NOTES ***
                // In QuickSpin, accessing stream information on the transport
                // layer is accomplished via a camera's TLStream property. The
                // TLStream property
                // houses nodes related to streaming such as the two demonstrated
                // below, buffer information, and GEV packet information to name
                // a few. The TLStream property allows access to nodes that would
                // generally be retrieved through the TL stream nodemap in full
                // Spinnaker.
                //
                // Print stream ID
                Console.Write("Stream ID: ");

                if (camera.TLStream.StreamID != null && camera.TLStream.StreamID.IsReadable)
                {
                    Console.WriteLine("{0}", camera.TLStream.StreamID.ToString());
                }
                else
                {
                    Console.WriteLine("unavailable");
                    result = -1;
                }

                // Print stream type
                Console.Write("Stream type: ");

                if (camera.TLStream.StreamType != null && camera.TLStream.StreamType.IsReadable)
                {
                    Console.WriteLine("{0}\n", camera.TLStream.StreamType.ToString());
                }
                else
                {
                    Console.WriteLine("unavailable\n");
                    result = -1;
                }
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                result = -1;
            }

            return result;
        }

        // This function prints information about the interface.
        static int PrintTransportLayerInterfaceInfo(IManagedInterface iface)
        {
            int result = 0;

            try
            {
                //
                // Print stream information from the transport layer
                //
                // *** NOTES ***
                // In QuickSpin, accessing interface information is accomplished
                // via an interface's TLInterface property. The TLInterface
                // property houses nodes that hold information about the
                // interface such as the three demonstrated below, other general
                // interface information, and GEV addressing information. The
                // TLInterface property allows access to nodes that would
                // generally be retrieved through the interface nodemap in full
                // Spinnaker.
                //
                // Interface nodes should also always be checked for availability
                // and readability (or writability when applicable). If a node
                // retrieval error occurs but remains unhandled, an exception is
                // thrown.
                //
                // Print interface display name
                Console.Write("Interface display name: ");

                if (iface.TLInterface.InterfaceDisplayName != null && iface.TLInterface.InterfaceDisplayName.IsReadable)
                {
                    Console.WriteLine("{0}", iface.TLInterface.InterfaceDisplayName.ToString());
                }
                else
                {
                    Console.WriteLine("unavailable");
                    result = -1;
                }

                // Print interface ID
                Console.Write("Interface ID: ");

                if (iface.TLInterface.InterfaceID != null && iface.TLInterface.InterfaceID.IsReadable)
                {
                    Console.WriteLine("{0}", iface.TLInterface.InterfaceID.ToString());
                }
                else
                {
                    Console.WriteLine("unavailable");
                    result = -1;
                }

                // Print interface type
                Console.Write("Interface type: ");

                if (iface.TLInterface.InterfaceType != null && iface.TLInterface.InterfaceType.IsReadable)
                {
                    Console.WriteLine("{0}\n", iface.TLInterface.InterfaceType.ToString());
                }
                else
                {
                    Console.WriteLine("unavailable\n");
                    result = -1;
                }
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                result = -1;
            }

            return result;
        }

        // This function prints information about the interface's host adapter.
        static int PrintHostAdapterInfo(IManagedInterface iface)
        {
            int result = 0;

            try
            {
                //
                // Print stream information from the transport layer
                //
                // *** NOTES ***
                // In QuickSpin, accessing interface information is accomplished
                // via an interface's TLInterface property. The TLInterface
                // property houses nodes that hold information about the
                // interface such as the three demonstrated below, other general
                // interface information, and GEV addressing information. The
                // TLInterface property allows access to nodes that would
                // generally be retrieved through the interface nodemap in full
                // Spinnaker.
                //
                // Interface nodes should also always be checked for availability
                // and readability (or writability when applicable). If a node
                // retrieval error occurs but remains unhandled, an exception is
                // thrown.
                //

                // Print host adapter name
                if (iface.TLInterface.HostAdapterName != null && iface.TLInterface.HostAdapterName.IsImplemented)
                {
                    Console.Write("Host adapter name: ");

                    if (iface.TLInterface.HostAdapterName.IsReadable)
                    {
                        Console.WriteLine("{0}", iface.TLInterface.HostAdapterName.ToString());
                    }
                    else
                    {
                        Console.WriteLine("unavailable");
                        result = -1;
                    }
                }

                // Print host adapter vendor
                if (iface.TLInterface.HostAdapterVendor != null && iface.TLInterface.HostAdapterVendor.IsImplemented)
                {
                    Console.Write("Host adapter vendor: ");

                    if (iface.TLInterface.HostAdapterVendor.IsReadable)
                    {
                        Console.WriteLine("{0}", iface.TLInterface.HostAdapterVendor.ToString());
                    }
                    else
                    {
                        Console.WriteLine("unavailable");
                        result = -1;
                    }
                }
                // Print host adapter driver version
                if (iface.TLInterface.HostAdapterDriverVersion != null &&
                    iface.TLInterface.HostAdapterDriverVersion.IsImplemented)
                {
                    Console.Write("Host adapter driver version: ");

                    if (iface.TLInterface.HostAdapterDriverVersion.IsReadable)
                    {
                        Console.WriteLine("{0}\n", iface.TLInterface.HostAdapterDriverVersion.ToString());
                    }
                    else
                    {
                        Console.WriteLine("unavailable\n");
                        result = -1;
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

        // This function prints device information from the GenICam nodemap.
        static int PrintGenICamDeviceInfo(IManagedCamera camera)
        {
            int result = 0;

            try
            {
                //
                // Print device information from the camera
                //
                // *** NOTES ***
                // Most camera interaction happens through GenICam nodes. The
                // advantages of these nodes is that there is a lot more of them,
                // they allow for a much deeper level of interaction with a
                // camera, and no intermediate property (i.e. TLDevice or
                // TLStream) is required. The disadvantage is that they require
                // initialization.
                //
                // Print exposure time
                Console.Write("Exposure time: ");

                if (camera.ExposureTime != null && camera.ExposureTime.IsReadable)
                {
                    Console.WriteLine("{0}", camera.ExposureTime.ToString());
                }
                else
                {
                    Console.WriteLine("unavailable");
                    result = -1;
                }

                // Print black level
                Console.Write("Black level: ");

                if (camera.BlackLevel != null && camera.BlackLevel.IsReadable)
                {
                    Console.WriteLine("{0}", camera.BlackLevel.ToString());
                }
                else
                {
                    Console.WriteLine("unavailable");
                    result = -1;
                }

                // Print height
                Console.Write("Height: ");

                if (camera.Height != null && camera.Height.IsReadable)
                {
                    Console.WriteLine("{0}\n", camera.Height.ToString());
                }
                else
                {
                    Console.WriteLine("unavailable\n");
                    result = -1;
                }
            }
            catch (SpinnakerException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                result = -1;
            }

            return result;
        }

        // Example entry point; please see Enumeration_CSharp_QuickSpin
        // example for more in-depth comments on preparing and cleaning up
        // the system.
        static int Main(string[] args)
        {
            int result = 0;

            Program program = new Program();

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

            // Retrieve list of interfaces from the system
            ManagedInterfaceList interfaceList = system.GetInterfaces();

            Console.WriteLine("Number of interfaces detected: {0}\n", interfaceList.Count);

            // Retrieve list of cameras from the system
            ManagedCameraList camList = system.GetCameras();

            Console.WriteLine("Number of cameras detected: {0}\n", camList.Count);

            //
            // Print information on each interface
            //
            // *** NOTES ***
            // All USB 3 Vision and GigE Vision interfaces should enumerate for
            // Spinnaker.
            //
            Console.WriteLine("\n*** PRINTING INTERFACE INFORMATION ***\n");

            foreach(var iface in interfaceList)
            {
                result = result | PrintTransportLayerInterfaceInfo(iface);
                result = result | PrintHostAdapterInfo(iface);
            }

            //
            // Print general device information on each camera from transport
            // layer
            //
            // *** NOTES ***
            // Transport layer nodes do not require initialization in order to
            // interact with them.
            //
            Console.WriteLine("\n*** PRINTING TRANSPORT LAYER DEVICE INFORMATION ***\n");

            foreach(var cam in camList)
            {
                result = result | PrintTransportLayerDeviceInfo(cam);
            }

            //
            // Print streaming information on each camera from transport layer
            //
            // *** NOTES ***
            // Again, initialization is not required to print information from
            // the transport layer; this is equally true of streaming information.
            //
            Console.WriteLine("\n*** PRINTING TRANSPORT LAYER STREAMING INFORMATION ***\n");

            foreach(var cam in camList)
            {
                result = result | PrintTransportLayerStreamInfo(cam);
            }

            //
            // Print device information on each camera from GenICam nodemap
            //
            // *** NOTES ***
            // GenICam nodes require initialization in order to interact with
            // them; as such, this loop initializes the camera, prints some
            // information from the GenICam nodemap, and then deinitializes it.
            // If the camera were not initialized, node availability would fail.
            //
            Console.WriteLine("\n*** PRINTING GENICAM INFORMATION ***\n");

            foreach(var cam in camList)
            {
                // Initialize camera
                cam.Init();

                // Print information
                result = result | PrintGenICamDeviceInfo(cam);

                // Deinitialize camera
                cam.DeInit();
            }

            // Clear camera list before releasing system
            camList.Clear();

            // Clear interface list before releasing system
            interfaceList.Clear();

            // Release system
            system.Dispose();

            Console.WriteLine("\nDone! Press Enter to exit...");
            Console.ReadLine();

            return result;
        }
    }
}