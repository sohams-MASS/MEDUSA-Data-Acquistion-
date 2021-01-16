#include <iterator>
#include <sstream>

#include "flir.h"


Flir::Flir(CameraPtr pCam_) : 
		pCam{ pCam_ },
		nodeMap{ pCam->GetTLDeviceNodeMap() },
		nodeMapTLDevice{ pCam->GetTLDeviceNodeMap() }
	{
	printDeviceInformation(nodeMapTLDevice);
	pCam->Init();
	nodeMap = pCam->GetNodeMap();
	CEnumerationPtr ptrAcquisitionMode = pCam->GetNodeMap().GetNode("AcquisitionMode");
	CEnumEntryPtr ptrAcquisitionModeContinuous = ptrAcquisitionMode->GetEntryByName("Continuous");
	const int64_t acquisitionModeContinuous = ptrAcquisitionModeContinuous->GetValue();
	ptrAcquisitionMode->SetIntValue(acquisitionModeContinuous);
}

Flir::~Flir() {
	
}

vector<char> Flir::acquireImage() {
	pCam->BeginAcquisition();
	ImagePtr pResultImage = pCam->GetNextImage(1000);
	if (pResultImage->IsIncomplete()) {
		return vector<char>();
	}
	else {
		ImagePtr convertedImage = pResultImage->Convert(PixelFormat_Mono8, EDGE_SENSING);
		//ostringstream filename{ "test.png" };
		//convertedImage->Save(filename.str().c_str());
		char* data = (char*)convertedImage->GetData();
		return vector<char>(data, data + strlen(data));
	}
}

const void Flir::printDeviceInformation(INodeMap& nodeMap) {
	cout << endl << "*** DEVICE INFORMATION ***" << endl << endl;

	try {
		FeatureList_t features;
		const CCategoryPtr category = nodeMap.GetNode("DeviceInformation");
		if (IsAvailable(category) && IsReadable(category)) {
			category->GetFeatures(features);

			for (auto it = features.begin(); it != features.end(); ++it)
			{
				const CNodePtr pfeatureNode = *it;
				cout << pfeatureNode->GetName() << " : ";
				CValuePtr pValue = static_cast<CValuePtr>(pfeatureNode);
				cout << (IsReadable(pValue) ? pValue->ToString() : "Node not readable");
				cout << endl;
			}
		}
		else {
			cout << "Device control information not available." << endl;
		}
	}
	catch (Spinnaker::Exception& e)
	{
		cout << "Error: " << e.what() << endl;
	}

}