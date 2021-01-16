#pragma once

#include <iostream>
#include <sstream>
#include <string>

#include "Spinnaker.h"
#include "SpinGenApi/SpinnakerGenApi.h"

using namespace Spinnaker;
using namespace Spinnaker::GenApi;
using namespace Spinnaker::GenICam;
using namespace std;


class Flir {

private:
	CameraPtr pCam;
	INodeMap& nodeMap;
	INodeMap& nodeMapTLDevice;
public:
	Flir(CameraPtr pCam_);
	~Flir();
	
	const void printDeviceInformation(INodeMap& nodeMap);
	vector<char> acquireImage();
};