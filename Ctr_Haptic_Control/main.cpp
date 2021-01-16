#include <stdio.h>
#include <tchar.h>
#include <thread>
#include <iostream>
#include <sstream>
#include <iterator>
#include <future>

#include "A3200.h"
#include "Spinnaker.h"
#include "SpinGenApi/SpinnakerGenApi.h"
#include "flir.h"

using namespace Spinnaker;
using namespace Spinnaker::GenApi;
using namespace Spinnaker::GenICam;
using namespace std;

int main(int argc, char* argv[]) {

	SystemPtr system = System::GetInstance();
	const LibraryVersion spinnakerLibraryVersion = system->GetLibraryVersion();
	cout << "Spinnaker library version: " << spinnakerLibraryVersion.major << "." << spinnakerLibraryVersion.minor
		 << "." << spinnakerLibraryVersion.type << "." << spinnakerLibraryVersion.build << endl
		 << endl;
	CameraList camList = system->GetCameras();
	unsigned int numCameras = camList.GetSize();
	
	cout << "Number of cameras detected: " << numCameras << endl << endl;
	
	vector<Flir> flirCameras;
	vector<future<vector<char>>> flirFutures;
	for (unsigned int i = 0; i < numCameras; i++) {
		flirCameras.push_back(Flir(camList.GetByIndex(i)));	
		flirFutures.push_back(async(launch::async, flirCameras.back().acquireImage));
	}

	
	vector<vector<char>> images;
	for (auto& flirFuture : flirFutures) {
		images.push_back(flirFuture.get());
	}



/*	A3200Handle handle{ nullptr };
	DOUBLE positionFeedback;
	AXISMASK axisMask{ (AXISMASK)(AXISMASK_00 | AXISMASK_01) };

	cout << "Connecting to A3200. Initializing if necessary." << endl;
	if (!A3200Connect(&handle)) { cout << "Failed to connect." << endl << endl; }
	else {
		cout << "Retrieving position feedback for the first axis." << endl;
		while (1) {
			if (!A3200StatusGetItem(handle, 0, STATUSITEM_PositionFeedback, 0, &positionFeedback)) {
				cout << "error" << endl;
				Sleep(1000);
			}
			else {
				cout << positionFeedback << endl;
				Sleep(1000);
			}
		};
	};
*/
}